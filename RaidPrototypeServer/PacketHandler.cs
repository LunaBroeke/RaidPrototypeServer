using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RaidPrototypeServer
{
    public static class PacketHandler
    {
        private static Logger logger = new Logger() { name = "packet" };
        public static string ReadStream(NetworkStream stream,int bytes)
        {
            byte[] buff = new byte[bytes];
            StringBuilder stringBuilder = new StringBuilder();
            int bytesRead = 0;
            do
            {
                bytesRead = stream.Read(buff, 0, buff.Length);
                stringBuilder.Append(Encoding.UTF8.GetString(buff, 0, bytesRead));
            } while (stream.DataAvailable);
            string a = stringBuilder.ToString();
#if DEBUG
            logger.Log($"Received: {a}");
#endif
            return a;
        }
        public static void WriteStream(NetworkStream stream, string s)
        {
            byte[] data = Encoding.UTF8.GetBytes(s);
            stream.Write(data, 0, data.Length);
#if DEBUG
            logger.Log($"Cent: {s}");
#endif
        }
        public static void WriteStream(NetworkStream stream, PlayerInfo player)
        {
            string s = JsonConvert.SerializeObject(player);
            byte[] data = Encoding.UTF8.GetBytes(s);
            stream.Write(data,0, data.Length);
#if DEBUG
            logger.Log($"Cent: {s}");
#endif
        }
        public static void WriteStream(NetworkStream stream, Command c)
        {
            string s = JsonConvert.SerializeObject(c);
            byte[] data = Encoding.UTF8.GetBytes(s);
            stream.Write(data, 0, data.Length);
#if DEBUG
            logger.Log($"Cent: {s}");
#endif
        }
    }
}
