using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class AttackInjury : Injury
    {
        public AttackInjury(float amount, Attackment attackment)
            : base(amount, attackment?.Attacker?.FindComponent<ComponentCreature>(), false, attackment.CauseOfDeath)
        {
            Attackment = attackment;
        }
    }
}
