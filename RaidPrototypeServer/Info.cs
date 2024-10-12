using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;

namespace RaidPrototypeServer
{
    public class ServerInfo
    {
        public string type { get; private set; } = "ServerInfo";
        public List<ServerPlayer> players { get; set; }
        public Settings settings { get; set; }
    }
    public class PlayerInfo
    {
        public string type { get; private set; } = "PlayerInfo";
        public int puppetID { get; set; }
        public string name { get; set; }
        public int health { get; set; }
        public Position position { get; set; } = new();
        public Rotation rotation { get; set; } = new();
        public Stats playerStats { get; set; } = new();
        public double ping { get; set; }
    }
    public class PlayerInfoList
    {
        public string type { get; set; } = "PlayerInfoList";
        public List<PlayerInfo> players { get; set; } = new List<PlayerInfo>();
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
        [JsonConverter(typeof(TcpClientJsonConverter))]
        public TcpClient tcpClient { get; set; }
        [JsonIgnore]
        public Logger logger { get; set; }
        [JsonIgnore]
        public Thread thread { get; set; }
        public UserClient userType { get; set; }
        public bool loggedIn { get; set; }
        public double ping { get; set; }
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
        public int health { get; set; }
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
    public class TcpClientJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var tcpClient = value as TcpClient;
            if (tcpClient != null && tcpClient.Client.RemoteEndPoint is IPEndPoint remoteEndPoint)
            {
                var ipAddress = remoteEndPoint.Address.ToString();
                var port = remoteEndPoint.Port;

                writer.WriteStartObject();
                writer.WritePropertyName("IPAddress");
                writer.WriteValue(ipAddress);
                writer.WritePropertyName("Port");
                writer.WriteValue(port);
                writer.WriteEndObject();
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            // Read IPAddress and Port from the JSON object
            string ipAddress = (string)obj["IPAddress"];
            int port = (int)obj["Port"];

            // Create a TcpClient without connecting
            TcpClient tcpClient = new TcpClient();

            // Optionally, you can also store the IP and Port in the TcpClient
            // for later use when you decide to connect manually.
            // This is just a placeholder for how you might implement that.
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Return the TcpClient without connecting it
            return new { TcpClient = tcpClient, IPAddress = ipAddress, Port = port };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TcpClient);
        }
    }
}
