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

    public class FireInjury : Injury
    {
        public FireInjury(float amount, ComponentCreature attacker) : base(amount, attacker, false, LanguageControl.Get(typeof(ComponentHealth).Name, 5))
        {

        }
    }
}
