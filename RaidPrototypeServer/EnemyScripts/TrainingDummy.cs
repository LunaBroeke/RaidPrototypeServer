using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidPrototypeServer.EnemyScripts
{
    public class TrainingDummy : Enemy
    {
        public override void Start()
        {
            base.Start();
            AttackSequence(attackList, true);
        }
        protected override List<Attack> AttackList()
        {
            List<Attack> attackList = new List<Attack>() { new Attack() { attackName = "slash", attackTime = 3f } };
            return attackList;
        }
        public static TrainingDummy InitEnemy(EnemyInfo enemyInfo)
        {
            TrainingDummy dummy = new TrainingDummy();
            dummy.enemyInfo = enemyInfo;
            return dummy;
        }
        public static EnemyInfo EnemyInfo(int id)
        {
            EnemyInfo e = new EnemyInfo();
            e.name = "Training Dummy";
            e.puppetID = 800+id;
            e.health = 10000000;
            e.enemyStats.maxHealth = 10000000;
            e.enemyStats.attack = 1;
            e.enemyStats.defense = 1;
            return e;
        }
    }
}
