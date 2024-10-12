using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
        public static DateTime launchTime = DateTime.Now;
        public const string version = "2410d12a";
        public static TcpListener server;
        public static List<ServerPlayer> players = new List<ServerPlayer>();
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
                    logger.Log($"Debug Date: {launchTime}");
#else
                    logger.Log($"Current version: {version}");
#endif
                    AccountManager.GetAccountDatabase();
                    Thread sendloop = new Thread( () => { SendPlayerInfoLoop(); }) { IsBackground = true };
                    Thread sendPingLoop = new Thread( () => { SendPingLoop(); }) { IsBackground= true };
                    sendloop.Start();
                    sendPingLoop.Start();
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
                player.loggedIn = true;
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
            player.name += "[Spectator]";
        }
        public static void HandleAdminPanel(ServerPlayer admin)
        {
            admin.name += "[AdminPanel]";
            admin.logger.name = admin.name;
            admin.thread = Thread.CurrentThread;
            NetworkStream stream = admin.tcpClient.GetStream();
            IPEndPoint endPoint = admin.tcpClient.Client.RemoteEndPoint as IPEndPoint;
            List<ServerPlayer> playerCopy;
            Thread readThread = new Thread(() => AdminPanelRead(admin, stream)) { IsBackground = true};
            lock (players)
            {
                playerCopy = new List<ServerPlayer>(players);
            }

            PlayerListMonitor.WritePlayerList();
            admin.loggedIn = true;
            readThread.Start();
            while (admin.tcpClient.Connected)
            {
                try
                {
                    ServerInfo si = new ServerInfo()
                    {
                        players = players,
                        settings = settings,
                    };
                    PacketHandler.WriteStream(stream, si);
                    Thread.Sleep(3000);
                }
                catch (Exception ex)
                {
                    admin.logger.LogWarning(ex.Message);
                    Disconnect(admin);
                }
            }
        }
        private static void AdminPanelRead(ServerPlayer admin, NetworkStream stream)
        {
            try
            {
                while (admin.tcpClient.Connected)
                {
                    string s = PacketHandler.ReadStream(stream, 1024);
                    Command c = JsonConvert.DeserializeObject<Command>(s);
                    switch (c.command)
                    {
                        case "PONG":
                            GetPing(admin, c);
                            break;
                        case "RequestAccounts":
                            s = JsonConvert.SerializeObject(AccountManager.GetAccountRoot(),Formatting.None);
                            PacketHandler.WriteStream(stream, s);
                            break;
                    }
                }
            }
            catch (Exception e) { admin.logger.Log(e.Message); Disconnect(admin); }
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
                try
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
                            if (sp.loggedIn && sp.userType != UserClient.AdminPanel)
                            {
                                TcpClient client = sp.tcpClient;
                                NetworkStream stream = client.GetStream();
                                PacketHandler.WriteStream(stream, s);
                            }
                        }
                    }
                }
                catch (InvalidOperationException ex) { }
                Thread.Sleep(50);
            }
        }
        #region Packet Handling
        public static void HandlePlayerInfo(ServerPlayer player, string s)
        {
            PlayerInfo playerInfo = JsonConvert.DeserializeObject<PlayerInfo>(s);
            try
            {
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
            catch (NullReferenceException ex)
            {
                player.logger.LogWarning($"PlayerInfo:{playerInfo == null }");
                player.logger.LogWarning($"Player.player{player.player == null}");
                player.logger.LogWarning($"Player.player.puppetID {player.player.puppetID == null}");
                player.logger.LogWarning($"FindPlayerById{FindPlayerByID(player.player.puppetID) == null}");
                player.logger.LogWarning($"FindPlayerByID.Player {FindPlayerByID(player.player.puppetID).player == null}");
            }
        }
        public static void HandleClientCommand(ServerPlayer player, string s)
        {
            Command command = JsonConvert.DeserializeObject<Command>(s);
            if (command.command == "Disconnect") Disconnect(player);
            if (command.command == "PONG") GetPing(player, command);
        }
        #endregion
        public static void SendPing()
        {
            Command c = new Command();
            c.command = "PING";
            c.arguments = new[] { DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss.fff"), "await" };
            foreach (ServerPlayer player in Server.players)
            {
                NetworkStream stream = player.tcpClient.GetStream();
                PacketHandler.WriteStream(stream, c);
            }
        }
        public static void SendPingLoop()
        {
            while (true)
            {
                try
                {
                    Command c = new Command();
                    c.command = "PING";
                    c.arguments = new[] { DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss.fff"), "await" };
                    foreach (ServerPlayer player in Server.players)
                    {
                        if (player.loggedIn == true)
                        {
                            NetworkStream stream = player.tcpClient.GetStream();
                            PacketHandler.WriteStream(stream, c);
                        }
                    }
                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {

                }
            }
        }
        private static void GetPing(ServerPlayer player, Command c)
        {
            DateTime t1 = DateTime.Parse(c.arguments[0]);
            DateTime t2 = DateTime.UtcNow;
            TimeSpan ping = t2 - t1;
            player.logger.Log($"Ping: {ping.TotalMilliseconds} ms");
            player.ping = ping.TotalMilliseconds;
            if (player.player != null) player.player.ping = player.ping;
            PlayerListMonitor.WritePlayerList();
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
                if (player.userType != UserClient.Player) continue;
                if (player.player.puppetID == playerID) { return player; }
            }
            return null;
        }

        public static void Disconnect(ServerPlayer player)
        {
            players.Remove(player);
            player.logger.Log($"{player.tcpClient.Client.RemoteEndPoint} Disconnected");
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
