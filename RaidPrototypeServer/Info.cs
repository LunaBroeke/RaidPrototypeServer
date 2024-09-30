using System.Net.Sockets;

namespace RaidPrototypeServer
{
    public class ServerInfo
    {
        public string type { get; private set; } = "ServerInfo";
    }
    public class PlayerInfo
    {
        public string type { get; private set; } = "PlayerInfo";
        public int puppetID { get; set; }
        public string name { get; set; }
    }
    public class ServerPlayer
    {
        public string type { get; private set; } = "ServerPlayer";
        public PlayerInfo player { get; set; }
        public TcpClient tcpClient { get; set; }
        public Logger logger { get; set; }
        public Thread thread { get; set; }
    }
    public class Command
    {
        public string type { get; private set; } = "Command";
        public string command { get; set; }
        public string[] arguments { get; set; }
    }
    public class ConsoleCommand
    {
        public string type { get; private set; } = "ConsoleCommand";
        public string command { get; set; }
        public string[] arguments { get; set; }
    }
    public class Event
    {
        public string type { get; private set; } = "Event";
        public string eventType { get; set; }
        public string[] arguments { get; set; }
    }
    public class TypeCheck
    {
        public string type { get; set; }
    }
}
