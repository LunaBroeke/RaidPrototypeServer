using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RaidPrototypeServer.EnemyScripts
{
    public class Attack
    {
        public string attackName { get; set; }
        public float attackTime { get; set; }
    }
    public class Enemy
    {
        public EnemyInfo enemyInfo { get; set; }
        protected PlayerInfo target { get; set; }
        protected virtual async Task BroadcastAttack(Attack attack)
        {
            Command c = new Command() { command = "EnemyAttack", arguments = new string[] { $"{enemyInfo.puppetID}", $"{attack.attackName}", $"{target.puppetID}" } };
            foreach (ServerPlayer player in Server.players)
            {
                NetworkStream stream = player.tcpClient.GetStream();
                PacketHandler.WriteStream(stream, c);
            }
            await Task.Delay(TimeSpan.FromSeconds(attack.attackTime));
        }

        protected virtual async Task AttackSequence(List<Attack> attacks, bool loop)
        {
            do
            {
                foreach (Attack attack in attacks)
                {
                    await BroadcastAttack(attack);
                }
            }
            while (loop);
        }
    }
}
