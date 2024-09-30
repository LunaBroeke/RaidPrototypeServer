using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RaidPrototypeServer
{
    public class Server
    {
        public int maxPlayers = 4;
        public int port = 2051;
        public static TcpListener server;
        public static List<ServerPlayer> players = new List<ServerPlayer>();
        public static ServerInfo serverInfo = new ServerInfo();
        public static Logger logger = new Logger() { name = "Server" };
        public static IPEndPoint localEP = null;
        public bool serverActive;

        public void StartServer()
        {
            if (serverActive == false)
            {
                localEP = new IPEndPoint(IPAddress.Any, port);
                try
                {
                    server = new TcpListener(localEP);
                    server.Start();
                    serverActive = true;
                    logger.Log($"Listening on {localEP}");
                    logger.Log("Server Started. waiting for connections");
                    while (true)
                    {
                        TcpClient client = server.AcceptTcpClient();
                        IPEndPoint endPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        ServerPlayer player = new ServerPlayer() { tcpClient = client, logger = new Logger() { name = $"{endPoint}" } };
                        player.logger.Log($"Client connected!");
                        Thread clientThread = new Thread(() => HandleClient(player)) { IsBackground = true };
                        clientThread.Start();
                        Thread.Sleep(100);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"General Error: {e.Message}");
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
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) { throw new Exception("No bytes read"); }
                s = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                player.logger.Log($"{s}");
                pi = JsonConvert.DeserializeObject<PlayerInfo>(s);
                if (!ValidatePacket(pi)) throw new Exception("PlayerInfo is missing");

                player.player = JsonConvert.DeserializeObject<PlayerInfo>(s);
                player.player.puppetID = AssignPuppetID();
                player.logger.name = $"{player.player.name}({player.player.puppetID})";
                player.logger.Log($"Obtained name {player.player.name} from {endPoint} and assigned ID {player.player.puppetID}");
                players.Add(player);
                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(player.player));
                stream.Write(data, 0, data.Length);
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
                    PlayerListMonitor.WritePlayerList();
                    s = null;
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) { throw new Exception("No bytes read"); }
                    s = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    player.logger.Log(s);
                    string check = ValidatePacket(s);
                    switch (check)
                    {
                        case "PlayerInfo":
                            break;
                        case "Command":
                            HandleClientCommand(player, s);
                            break;
                        default:
                            continue;
                    }
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
        #region Packet Validating
        public bool ValidatePacket(PlayerInfo pi)
        {
            if (pi.type != "PlayerInfo") return false;
            if (pi == null) return false;
            if (pi.name == null) return false;
            return true;
        }

        public bool ValidatePacket(Command c)
        {
            if (c.type != "Command") return false;
            if (c.command == null) return false;
            return true;
        }

        public string ValidatePacket(string s)
        {
            if (ValidatePacket(JsonConvert.DeserializeObject<PlayerInfo>(s))) return "PlayerInfo";
            if (ValidatePacket(JsonConvert.DeserializeObject<Command>(s))) return "Command";
            return "invalid";
        }
        #endregion
        #region Packet Handling
        public void HandleClientCommand(ServerPlayer player , string s)
        {
            Command command = JsonConvert.DeserializeObject<Command>(s);
            if (command.command == "Disconnect") Disconnect(player);
        }
        #endregion
        public static int AssignPuppetID()
        {
            int id;
            Random random = new Random();
            do { id = random.Next(1, 99); } while (players.Contains(FindPlayerByID(id)));
            return id;
        }
        public static ServerPlayer FindPlayerByID(int playerID)
        {
            foreach (ServerPlayer player in players) { if (player.player.puppetID == playerID) { return player; } }
            return null;
        }

        public void Disconnect(ServerPlayer player)
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
