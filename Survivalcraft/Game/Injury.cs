using Engine;
using GameEntitySystem;

namespace Game
{
    public class Injury
    {
        public float Amount;
        Attackment Attackment;
        bool IgnoreInvulnerability;
        string Cause;
        ComponentCreature Attacker
        {
            get
            {
                return Attackment?.Attacker?.FindComponent<ComponentCreature>();
            }
        }
    }
}
