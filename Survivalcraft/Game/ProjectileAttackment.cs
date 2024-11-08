﻿using Engine;
using GameEntitySystem;

namespace Game
{
    public class ProjectileAttackment : Attackment
    {
        Projectile Projectile;
        public ProjectileAttackment(Entity target, Entity attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower, Projectile projectile)
            : base(target, attacker, hitPoint, hitDirection, attackPower)
		{
			Projectile = projectile;
			float injuryAmount = CalculateInjuryAmount();
			if(injuryAmount == 0f && AttackPower > 0f)
			{
				ImpulseFactor = 0f;
				StunTimeAdd = 0f;
			}
			else
			{
				ImpulseFactor = 2f;
				StunTimeAdd = 0.2f;
			}
        }

        public ProjectileAttackment(ComponentBody target, Entity attacker, Vector3 hitPoint, Vector3 hitDirection, float attackPower, Projectile projectile)
            : this(target.Entity, attacker, hitPoint, hitDirection, attackPower, projectile)
        {
        }
    }
}