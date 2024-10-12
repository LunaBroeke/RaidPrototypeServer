using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RaidPrototypeServer.PlayerScripts
{
    public class Attack
    {
        public string attackName { get; set; }
        public float attackTime { get; set; }
    }
    public class Player
    {
        public PlayerInfo playerInfo { get; set; }
        protected EnemyInfo? target { get; set; }
        protected virtual async Task BroadcastAttack(Attack attack)
        {
            Command c = new Command() { command = "PlayerAttack", arguments = new string[] { $"{playerInfo.puppetID}", $"{attack.attackName}", $"{target.puppetID}" } };
            foreach (ServerPlayer player in Server.players)
            {
                NetworkStream stream = player.tcpClient.GetStream();
                PacketHandler.WriteStream(stream, c);
            }
            await Task.Delay(TimeSpan.FromSeconds(attack.attackTime));
        }
    }
}
