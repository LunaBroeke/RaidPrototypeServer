using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RaidPrototypeServer
{
    public class Server
    {
        public const string version = "2410d10b";
        public static TcpListener server;
        public static List<ServerPlayer> players = new List<ServerPlayer>();
        public static ServerInfo serverInfo = new ServerInfo();
        public static Logger logger = new Logger() { name = "Server" };
        public static IPEndPoint localEP = null;
        public static Settings settings = Settings.LoadSettings();
        public bool serverActive;

        public void StartServer()
        {
            if (serverActive == false)
            {
                localEP = new IPEndPoint(IPAddress.Parse(settings.address), settings.port);

                try
                {
                    server = new TcpListener(localEP);
                    server.Start();
                    serverActive = true;
                    logger.Log($"Listening on {localEP}");
                    logger.Log("Server Started. waiting for connections");
#if DEBUG
                    logger.Log($"Debug Date: {DateTime.Now}");
#else
                    logger.Log($"Current version: {version}");
#endif
                    AccountManager.GetAccountDatabase();
                    Thread sendloop = new Thread( () => { SendPlayerInfoLoop(); }) { IsBackground = true };
                    sendloop.Start();
                    while (serverActive)
                    {
                        TcpClient client = server.AcceptTcpClient();
                        IPEndPoint endPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        ServerPlayer player = new ServerPlayer() { tcpClient = client, logger = new Logger() { name = $"{endPoint}" } };
                        player.logger.Log($"Client connected!");
                        Thread clientThread = new Thread(() => HandleClient(player)) { IsBackground = true };
                        clientThread.Start();
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"General Error: {e}");
                }
                finally
                {
                    StopServer();
                }
            }
        }

        private void HandleClient(ServerPlayer player)
        {
            player.thread = Thread.CurrentThread;
            NetworkStream stream = player.tcpClient.GetStream();
            IPEndPoint endPoint = player.tcpClient.Client.RemoteEndPoint as IPEndPoint;
            string s = null;
            PlayerInfo pi = null;
            try
            {
                player.name = endPoint.ToString();
                players.Add(player);
                PlayerListMonitor.WritePlayerList();
                AccountManager.AwaitAccount(player);
            }
            catch (Exception ex)
            {
#if DEBUG
                player.logger.LogError(ex.ToString());
#elif RELEASE
                player.logger.LogError(ex.Message);
#endif
                Disconnect(player);
            }
        }
        public static void HandlePlayer(ServerPlayer player)
        {
            player.thread = Thread.CurrentThread;
            NetworkStream stream = player.tcpClient.GetStream();
            IPEndPoint endPoint = player.tcpClient.Client.RemoteEndPoint as IPEndPoint;
            string s = null;
            PlayerInfo pi = null;
            try
            {
                s = PacketHandler.ReadStream(stream, 1024);
                player.logger.Log($"{s}");
                pi = JsonConvert.DeserializeObject<PlayerInfo>(s);
                if (!ValidatePacket(pi)) throw new Exception("PlayerInfo is missing");

                player.player = JsonConvert.DeserializeObject<PlayerInfo>(s);
                player.player.puppetID = AssignPuppetID();
                player.logger.name = $"{player.player.name}({player.player.puppetID})";
                player.logger.Log($"Obtained name {player.player.name} from {endPoint} and assigned ID {player.player.puppetID}");
                PacketHandler.WriteStream(stream, player.player);
                PlayerListMonitor.WritePlayerList();
            }
            catch (Exception ex)
            {
#if DEBUG
                player.logger.LogError(ex.ToString());
#elif RELEASE
                player.logger.LogError(ex.Message);
#endif
                Disconnect(player);
            }
            try
            {
                while (player.tcpClient.Connected)
                {
                    s = null;
                    try
                    {
                        s = PacketHandler.ReadStream(stream, 1024);
                    }
                    catch (Exception ex) { player.logger.LogWarning(ex.ToString()); continue; }
                    string[] strings = s.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str in strings)
                    {
                        ProcessMessage(player, str);
                        stream.Flush();
                    }
                    //SendPlayerInfo();
                }
                throw new Exception("Player disconnected");
            }
            catch (Exception ex)
            {
#if DEBUG
                player.logger.LogError(ex.ToString());
#elif RELEASE
                player.logger.LogError(ex.Message);
#endif
                Disconnect(player);
            }
        }
        public static void HandleSpectator(ServerPlayer player)
        {

        }
        public static void HandleAdminPanel(ServerPlayer player)
        {

        }
        private static void ProcessMessage(ServerPlayer player, string s)
        {
            string check = ValidatePacket(s);
            switch (check)
            {
                case "PlayerInfo":
                    HandlePlayerInfo(player, s);
                    break;
                case "Command":
                    HandleClientCommand(player, s);
                    break;
                default:
                    break;
            }
        }
        #region Packet Validating
        public static bool ValidatePacket(PlayerInfo pi)
        {
            if (pi.type != "PlayerInfo") return false;
            if (pi == null) return false;
            if (pi.name == null) return false;
            return true;
        }

        public static bool ValidatePacket(Command c)
        {
            if (c.type != "Command") return false;
            if (c.command == null) return false;
            return true;
        }
        public static bool ValidatePacket(PlayerInfoList pil)
        {
            if (pil.type != "PlayerInfoList") return false;
            if (pil.players == null) return false;
            return true;
        }

        public static string ValidatePacket(string s)
        {
            if (ValidatePacket(JsonConvert.DeserializeObject<PlayerInfo>(s))) return "PlayerInfo";
            if (ValidatePacket(JsonConvert.DeserializeObject<PlayerInfoList>(s))) return "PlayerInfoList";
            if (ValidatePacket(JsonConvert.DeserializeObject<Command>(s))) return "Command";
            return "invalid";
        }
        #endregion
        private static void SendPlayerInfo()
        {
            PlayerInfoList playerInfoList = new PlayerInfoList();
            lock (players)
            {
                foreach (ServerPlayer sp in players)
                {
                    if (sp.player != null)
                        playerInfoList.players.Add(sp.player);
                }
                string s = JsonConvert.SerializeObject(playerInfoList);
                foreach (ServerPlayer sp in players)
                {
                    if (sp.loggedIn)
                    {
                        TcpClient client = sp.tcpClient;
                        NetworkStream stream = client.GetStream();
                        PacketHandler.WriteStream(stream, s);
                    }
                }
            }
        }
        private static void SendPlayerInfoLoop()
        {
            while (true)
            {

                PlayerInfoList playerInfoList = new PlayerInfoList();
                lock (players)
                {
                    foreach (ServerPlayer sp in players)
                    {
                        if (sp.player != null)
                            playerInfoList.players.Add(sp.player);
                    }
                    string s = JsonConvert.SerializeObject(playerInfoList);
                    foreach (ServerPlayer sp in players)
                    {
                        if (sp.loggedIn)
                        {
                            TcpClient client = sp.tcpClient;
                            NetworkStream stream = client.GetStream();
                            PacketHandler.WriteStream(stream, s);
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }
        #region Packet Handling
        public static void HandlePlayerInfo(ServerPlayer player, string s)
        {
            PlayerInfo playerInfo = JsonConvert.DeserializeObject<PlayerInfo>(s);
            lock (players)
            {
                IPEndPoint ip = player.tcpClient.Client.RemoteEndPoint as IPEndPoint;
                PlayerInfo existingPlayer = FindPlayerByID(player.player.puppetID).player;
                if (existingPlayer != null)
                {
                    existingPlayer.position = playerInfo.position;
                    existingPlayer.rotation = playerInfo.rotation;
                    existingPlayer.health = playerInfo.health;
                }
            }
        }
        public static void HandleClientCommand(ServerPlayer player, string s)
        {
            Command command = JsonConvert.DeserializeObject<Command>(s);
            if (command.command == "Disconnect") Disconnect(player);
            if (command.command == "PONG") GetPing(player, command);
        }
        #endregion
        private static void GetPing(ServerPlayer player, Command c)
        {
            DateTime t1 = DateTime.Parse(c.arguments[0]);
            DateTime t2 = DateTime.Parse(c.arguments[1]);
            TimeSpan ping = t2 - t1;
            player.logger.Log($"Ping: {ping.TotalMilliseconds} ms");
        }
        public static int AssignPuppetID()
        {
            int id;
            Random random = new Random();
            do { id = random.Next(1, 99); } while (players.Contains(FindPlayerByID(id)));
            return id;
        }
        public static ServerPlayer FindPlayerByID(int playerID)
        {
            foreach (ServerPlayer player in players)
            {
                if (player.player.puppetID == playerID) { return player; }
            }
            return null;
        }

        public static void Disconnect(ServerPlayer player)
        {
            players.Remove(player);
            player.logger.LogError($"{player.tcpClient.Client.RemoteEndPoint} Disconnected");
            player.tcpClient.Close();
            PlayerListMonitor.WritePlayerList();
            player.thread.Join();
        }
        public void StopServer()
        {
            if (serverActive == true)
            {
                server.Stop();
                serverActive = false;
                logger.Log("Server stopped");
            }
        }
    }
}
