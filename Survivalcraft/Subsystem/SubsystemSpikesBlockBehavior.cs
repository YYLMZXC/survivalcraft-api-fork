using Engine;
using GameEntitySystem;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemSpikesBlockBehavior : SubsystemBlockBehavior, IUpdateable
	{
		public static Random m_random = new();

		public SubsystemAudio m_subsystemAudio;

		public SubsystemTime m_subsystemTime;

		public Vector3? m_closestSoundToPlay;

		public Dictionary<ComponentCreature, double> m_lastInjuryTimes = [];

		public override int[] HandledBlocks => new int[1]
		{
			BlocksManager.GetBlockIndex<SpikedPlankBlock>()
		};

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public void Update(float dt)
		{
			if (m_closestSoundToPlay.HasValue)
			{
				m_subsystemAudio.PlaySound("Audio/Spikes", 0.7f, m_random.Float(-0.1f, 0.1f), m_closestSoundToPlay.Value, 4f, autoDelay: true);
				m_closestSoundToPlay = null;
			}
		}

		public bool RetractExtendSpikes(int x, int y, int z, bool extend)
		{
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
			int num = Terrain.ExtractContents(cellValue);
			if (BlocksManager.Blocks[num] is SpikedPlankBlock)
			{
				int data = SpikedPlankBlock.SetSpikesState(Terrain.ExtractData(cellValue), extend);
				int value = Terrain.ReplaceData(cellValue, data);
				SubsystemTerrain.ChangeCell(x, y, z, value);
				var vector = new Vector3(x, y, z);
				float num2 = m_subsystemAudio.CalculateListenerDistance(vector);
				if (!m_closestSoundToPlay.HasValue || num2 < m_subsystemAudio.CalculateListenerDistance(m_closestSoundToPlay.Value))
				{
					m_closestSoundToPlay = vector;
				}
				return true;
			}
			return false;
		}

		public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
		{
			int data = Terrain.ExtractData(SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
			if (!SpikedPlankBlock.GetSpikesState(data))
			{
				return;
			}
			int mountingFace = SpikedPlankBlock.GetMountingFace(data);
			if (cellFace.Face != mountingFace)
			{
				return;
			}
			ComponentCreature componentCreature = componentBody.Entity.FindComponent<ComponentCreature>();
			if (componentCreature != null)
			{
				m_lastInjuryTimes.TryGetValue(componentCreature, out double value);
				if (m_subsystemTime.GameTime - value > 1.0)
				{
					m_lastInjuryTimes[componentCreature] = m_subsystemTime.GameTime;
					componentCreature.ComponentHealth.OnSpiked(this, 1f / componentCreature.ComponentHealth.SpikeResilience, cellFace, velocity, componentBody, "Spiked by a trap");
				}
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
			m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
		}

		public override void OnEntityRemoved(Entity entity)
		{
			ComponentCreature componentCreature = entity.FindComponent<ComponentCreature>();
			if (componentCreature != null)
			{
				m_lastInjuryTimes.Remove(componentCreature);
			}
		}
	}
}
