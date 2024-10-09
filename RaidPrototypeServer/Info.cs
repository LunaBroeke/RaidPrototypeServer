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
        public Position position { get; set; } = new();
        public Rotation rotation { get; set; } = new();
        public Stats playerStats { get; set; } = new();
    }
    public class Position
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

    }
    public class Rotation
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }

    }
    public class Stats
    {
        public int maxHealth { get; set; }
        public int attack { get; set; }
        public int defense { get; set; }
    }
    public class ServerPlayer
    {
        public string type { get; private set; } = "ServerPlayer";
        public string name { get; set; }
        public PlayerInfo player { get; set; }
        public TcpClient tcpClient { get; set; }
        public Logger logger { get; set; }
        public Thread thread { get; set; }
        public UserClient userType { get; set; }
    }
    public enum UserClient
    {
        None = 0,
        Player,
        Spectator,
        AdminPanel,
    }
    public class EnemyInfo
    {
        public string type { get; set; } = "EnemyInfo";
        public int puppetID { get; set; }
        public string name { get; set; }
        public Position position { get; set; } = new();
        public Rotation rotation { get; set; } = new();
        public Stats enemyStats { get; set; } = new();
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
