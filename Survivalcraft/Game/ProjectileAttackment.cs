using Engine;
using GameEntitySystem;

namespace Game
{
    public class ProjectileAttackment : Attackment
    {
        public ProjectileAttackment(Entity target, Entity attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower)
            : base(target, attacker, hitPoint, hitDirection, attackPower)
        {
            ImpulseFactor = 2f;
            StunTimeAdd = 0.2f;
        }

        public ProjectileAttackment(ComponentBody target, Entity attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower)
            : this(target.Entity, attacker, hitPoint, hitDirection, attackPower)
        {
        }
    }
}