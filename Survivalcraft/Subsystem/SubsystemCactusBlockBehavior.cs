using Engine;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemCactusBlockBehavior : SubsystemPollableBlockBehavior, IUpdateable
	{
		public SubsystemTime m_subsystemTime;

		public SubsystemGameInfo m_subsystemGameInfo;

		public Dictionary<Point3, int> m_toUpdate = [];

		public Random m_random = new();

		public int m_sandBlockIndex;

		public int m_cactusBlockIndex;
		public override int[] HandledBlocks => new int[1]
		{
			BlocksManager.GetBlockIndex<CactusBlock>()
		};

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int cellContents = SubsystemTerrain.Terrain.GetCellContents(x, y - 1, z);
			if (cellContents != m_sandBlockIndex && cellContents != m_cactusBlockIndex)
			{
				SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
			}
		}

		public override void OnPoll(int value, int x, int y, int z, int pollPass)
		{
			if (m_subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode != 0)
			{
				return;
			}
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y + 1, z);
			if (Terrain.ExtractContents(cellValue) == 0 && Terrain.ExtractLight(cellValue) >= 12)
			{
				int cellContents = SubsystemTerrain.Terrain.GetCellContents(x, y - 1, z);
				int cellContents2 = SubsystemTerrain.Terrain.GetCellContents(x, y - 2, z);
				if ((cellContents != m_cactusBlockIndex || cellContents2 != m_cactusBlockIndex) && m_random.Float(0f, 1f) < 0.25f)
				{
					m_toUpdate[new Point3(x, y + 1, z)] = Terrain.MakeBlockValue(m_cactusBlockIndex, 0, 0);
				}
			}
		}

		public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
		{
            ComponentHealth componentHealth = componentBody.Entity.FindComponent<ComponentHealth>();
            if (componentHealth != null)
            {
				componentHealth.OnSpiked(this, 0.01f * MathF.Abs(velocity), cellFace, velocity, componentBody, "Spiked by cactus");
            }
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_sandBlockIndex = BlocksManager.GetBlockIndex<SandBlock>();
			m_cactusBlockIndex = BlocksManager.GetBlockIndex<CactusBlock>();
			base.Load(valuesDictionary);
		}

		public void Update(float dt)
		{
			if (m_subsystemTime.PeriodicGameTimeEvent(60.0, 0.0))
			{
				foreach (KeyValuePair<Point3, int> item in m_toUpdate)
				{
					if (SubsystemTerrain.Terrain.GetCellContents(item.Key.X, item.Key.Y, item.Key.Z) == 0)
					{
						SubsystemTerrain.ChangeCell(item.Key.X, item.Key.Y, item.Key.Z, item.Value);
					}
				}
				m_toUpdate.Clear();
			}
		}
	}
}
