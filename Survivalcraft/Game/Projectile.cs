using Engine;
using System;

namespace Game
{
	public class Projectile : WorldItem
	{
		public Vector3 Rotation;

		public Vector3 AngularVelocity;

		public bool IsInWater;

		public double LastNoiseTime;

		public ComponentCreature Owner;

		public GameEntitySystem.Entity OwnerEntity;

		public ProjectileStoppedAction ProjectileStoppedAction;

		public ITrailParticleSystem TrailParticleSystem;

		public Vector3 TrailOffset;

		public bool NoChunk;

		public bool IsIncendiary;

		public Action OnRemove;
	}
}
