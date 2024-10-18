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
        public string displayName { get; set; }
    }
    public class EnemyTarget
    {
        public PlayerInfo player { get; set; }
        public int aggro { get; set; }
    }
    public class Enemy
    {
        public EnemyInfo enemyInfo { get; set; }
        protected List<EnemyTarget> targets { get; set; }
        protected List<Attack> attackList { get; set; }
        public static Enemy InitEnemy(EnemyInfo enemyInfo)
        {
            Enemy e = new Enemy();
            e.enemyInfo = enemyInfo;
            return e;
        }
        public virtual void Start()
        {
            targets = AllAvailableTargets();
            attackList = AttackList();
        }
        protected virtual async Task StartEnemy(List<Attack> attacks, bool loop)
        {
            await AttackSequence(attacks, loop);
        }
        protected virtual async Task AttackSequence(List<Attack> attacks, bool loop)
        {
            do
            {
                foreach (Attack attack in attacks)
                {
                    Server.logger.Log($"{attack.attackName}");
                    await BroadcastAttack(attack);
                }
            }
            while (loop);
        }
        protected virtual async Task BroadcastAttack(Attack attack)
        {
            try
            {
                Command c = new Command() { command = "EnemyAttack", arguments = new string[] { $"{enemyInfo.puppetID}", $"{attack.attackName}", $"{CheckTarget().player.puppetID}", attack.displayName } };
                Server.logger.Log(attack.attackName);
                foreach (ServerPlayer player in Server.players)
                {
                    player.logger.Log("cent");
                    NetworkStream stream = player.tcpClient.GetStream();
                    PacketHandler.WriteStream(stream, c);

                }
                await Task.Delay(TimeSpan.FromSeconds(attack.attackTime));
            }
            catch (Exception ex)
            {
                Server.logger.LogError($"Error in BroadcastAttack {ex}");
                await Task.Delay(TimeSpan.FromSeconds(attack.attackTime));
            }
        }

        protected virtual List<Attack> AttackList()
        {
            List<Attack> list = new List<Attack>();
            return list;
        }

        protected virtual List<EnemyTarget> AllAvailableTargets()
        {
            List<EnemyTarget> target = new List<EnemyTarget>();
            foreach (ServerPlayer player in Server.players)
            {
                if (player.player != null)
                {
                    EnemyTarget t = new EnemyTarget
                    {
                        player = player.player,
                        aggro = 0
                    };
                    target.Add(t);
                }
            }
            return target;
        }
        protected virtual EnemyTarget CheckTarget()
        {
            return targets.OrderByDescending(t => t.aggro).FirstOrDefault();
        }
    }
}



