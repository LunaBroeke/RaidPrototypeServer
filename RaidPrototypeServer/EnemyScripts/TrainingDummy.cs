using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaidPrototypeServer.EnemyScripts
{
    public class TrainingDummy : Enemy
    {
        private void Test()
        {
            List<Attack> attackList = new List<Attack>() { new Attack() { attackName = "slash", attackTime = 3f } };
            AttackSequence(attackList,true);
        }
    }
}
