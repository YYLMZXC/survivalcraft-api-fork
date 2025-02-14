using Engine;
using System.Collections.Generic;
using System.Linq;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemBombBlockBehavior : SubsystemBlockBehavior, IUpdateable
	{
		public SubsystemGameInfo m_subsystemGameInfo;

		public SubsystemTime m_subsystemTime;

		public SubsystemBlockBehaviors m_subsystemBlockBehaviors;

		public SubsystemExplosions m_subsystemExplosions;

		public SubsystemProjectiles m_subsystemProjectiles;

		public Dictionary<Projectile, bool> m_projectiles = [];

		public override int[] HandledBlocks => new int[0];

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
			m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
			m_subsystemBlockBehaviors = Project.FindSubsystem<SubsystemBlockBehaviors>(throwOnError: true);
			m_subsystemProjectiles.ProjectileAdded += (projectile) =>
			{
				ScanProjectile(projectile);
			};
			m_subsystemProjectiles.ProjectileRemoved += (projectile) =>
			{
				m_projectiles.Remove(projectile);
			};
		}

		public void ScanProjectile(Projectile projectile)
		{
			if (!m_projectiles.ContainsKey(projectile))
			{
				int num = Terrain.ExtractContents(projectile.Value);
				if (m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(projectile.Value)).Contains(this))
				{
					m_projectiles.Add(projectile, value: true);
					projectile.ProjectileStoppedAction = ProjectileStoppedAction.DoNothing;
					Color color = (num == 228) ? new Color(255, 140, 192) : Color.White;
					m_subsystemProjectiles.AddTrail(projectile, new Vector3(0f, 0.25f, 0.1f), new SmokeTrailParticleSystem(20, 0.33f, float.MaxValue, color));
				}
			}
		}

		public void Update(float dt)
		{
			if (m_subsystemTime.PeriodicGameTimeEvent(0.1, 0.0))
			{
				foreach (Projectile key in m_projectiles.Keys)
				{
					if (m_subsystemGameInfo.TotalElapsedGameTime - key.CreationTime > 5.0)
					{
						m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(key.Position.X), Terrain.ToCell(key.Position.Y), Terrain.ToCell(key.Position.Z), key.Value);
						key.ToRemove = true;
					}
				}
			}
		}
	}
}
