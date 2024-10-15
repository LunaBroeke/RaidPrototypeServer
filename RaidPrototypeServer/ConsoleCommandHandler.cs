using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RaidPrototypeServer.GameScripts;

namespace RaidPrototypeServer
{
    public static class ConsoleCommandHandler
    {
        public static void ProcessCommand(string m)
        {
            Logger logger = Server.logger;
            string[] data = m.Split(new char[] { ' ' });
            foreach (string s in data)
            {
                logger.Log(s);
            }
            switch (data[0])
            {
                case "debug":
                    CommandDebug(data);
                    break;
                case "ping":
                    Server.SendPing();
                    break;
                case "ban":
                    CommandBan(data);
                    break;
                case "start":
                    Thread serverThread = new Thread(MainWindow.server.StartServer) { IsBackground = true };
                    serverThread.Start();
                    break;
                case "startgame":
                    Thread gameThread = new Thread(() => GameManager.StartGame(GameManager.GetFloorByNum(10))) { IsBackground = true };
                    gameThread.Start();
                    break;
                case "debugmap":
                    Thread debugmap = new Thread(() =>
                    {
                        Application.Run(new MapScreen());
                    });
                    debugmap.Start();
                    break;
                case "debuglog":
                    break;
                case "stop":
                    MainWindow.server.StopServer();
                    break;
                case "q":
                    Application.Exit();
                    break;
                default:
                    logger.Log($"Command {data[0]} not found");
                    break;
            }
        }

        private static void CommandDebug(string[] data)
        {
            Logger logger = Server.logger;
            try
            {

                switch (data[1])
                {
                    case "ping":
                        Server.SendPing();
                        break;
                    case "move":
                        DebugRandomPos();
                        break;
                    default:
                        logger.LogWarning("Incorrect arguments");
                        break;
                }
            }
            catch (IndexOutOfRangeException) { logger.LogWarning("Missing arguments"); }
        }
        private static void CommandBan(string[] data)
        {
            Logger logger = Server.logger;
            Settings settings = Server.settings;
            string name = data[1];
            string time = string.Join(" ", data.Skip(2));
            try
            {
                DateTime t = DateTime.Parse(time).ToUniversalTime();
                Account acc = AccountManager.FindAccountByName(name);
                acc.banExpire = t;
                AccountManager.WriteAccountDatabase();
            }
            catch (InvalidAccountException e) {logger.LogWarning(e.Message); return; }
            catch (Exception e) { logger.LogWarning(e.ToString()); return; }
        }

        private static void DebugRandomPos()
        {
            Random r = new Random();
            foreach (ServerPlayer player in Server.players)
            {
                if (player.player != null)
                {
                    int x = r.Next(-20, 20);
                    int y = r.Next(-20, 20);
                    int z = r.Next(-20, 20);
                    Command c = new Command() { command = "TeleportPlayer", arguments = new string[] { $"{x}", $"{y}", $"{z}" } };
                    PacketHandler.WriteStream(player.tcpClient.GetStream(), c);
                }
            }
        }
    }
}
