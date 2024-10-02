using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
                    SendPing();
                    break;
                case "start":
                    Thread serverThread = new Thread(MainWindow.server.StartServer) { IsBackground = true };
                    serverThread.Start();
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
                        SendPing();
                        break;
                    default:
                        logger.LogWarning("Incorrect arguments");
                        break;
                }
            }
            catch (IndexOutOfRangeException) { logger.LogWarning("Missing arguments"); }
        }

        private static void SendPing()
        {
            Command c = new Command();
            c.command = "PING";
            c.arguments = new[] { DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), "await" };
            string m = JsonConvert.SerializeObject(c);
            foreach (ServerPlayer player in Server.players)
            {
                NetworkStream stream = player.tcpClient.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(m);
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
