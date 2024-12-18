using Engine;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemPlantBlockBehavior : SubsystemPollableBlockBehavior
	{

		public SubsystemTime m_subsystemTime;

		public SubsystemCellChangeQueue m_subsystemCellChangeQueue;

		public SubsystemGameInfo m_subsystemGameInfo;

		public SubsystemSeasons m_subsystemSeasons;

		public Random m_random = new();

		public override int[] HandledBlocks => new int[11]
		{
			19,
			20,
			24,
			25,
			28,
			99,
			131,
			244,
			132,
			174,
			204
		};

		public sealed override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int plantValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            int plantContents = Terrain.ExtractContents(plantValue);
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
			int soilContents = Terrain.ExtractContents(cellValue);
			Block soilBlock = BlocksManager.Blocks[soilContents];
			if(!soilBlock.IsSuitableForPlants(cellValue, plantValue))
			{
                SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
            }
		}

		public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
		{
			Lifecycle(value, x, y, z, applyImmediately: true);
		}

		public override void OnPoll(int value, int x, int y, int z, int pollPass)
		{
			if (m_subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode == EnvironmentBehaviorMode.Living)
			{
				Grow(value, x, y, z, pollPass);
				Lifecycle(value, x, y, z, applyImmediately: false);
			}
		}
		public virtual void Grow(int value, int x, int y, int z, int pollPass)
		{
			if (y <= 0 || y >= 255)
			{
				return;
			}
			int num = Terrain.ExtractContents(value);
			Block block = BlocksManager.Blocks[num];
			if (num == 19)
			{
				GrowTallGrass(value, x, y, z, pollPass);
				return;
			}
			if (block is FlowerBlock)
			{
				GrowFlower(value, x, y, z, pollPass);
				return;
			}
			switch (num)
			{
				case 174:
					GrowRye(value, x, y, z, pollPass);
					break;
				case 204:
					GrowCotton(value, x, y, z, pollPass);
					break;
				case 131:
					GrowPumpkin(value, x, y, z, pollPass);
					break;
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemTime = base.Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemCellChangeQueue = base.Project.FindSubsystem<SubsystemCellChangeQueue>(throwOnError: true);
			m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemSeasons = base.Project.FindSubsystem<SubsystemSeasons>(throwOnError: true);
		}

		public void GrowTallGrass(int value, int x, int y, int z, int pollPass)
		{
			int data = Terrain.ExtractData(value);
			if (TallGrassBlock.GetIsSmall(data) && Terrain.ExtractLight(SubsystemTerrain.Terrain.GetCellValueFast(x, y + 1, z)) >= 9)
			{
				int data2 = TallGrassBlock.SetIsSmall(data, isSmall: false);
				int value2 = Terrain.ReplaceData(value, data2);
				m_subsystemCellChangeQueue.QueueCellChange(x, y, z, value2);
			}
		}

		public void GrowFlower(int value, int x, int y, int z, int pollPass)
		{
			int data = Terrain.ExtractData(value);
			if (FlowerBlock.GetIsSmall(data) && Terrain.ExtractLight(SubsystemTerrain.Terrain.GetCellValueFast(x, y + 1, z)) >= 9)
			{
				int data2 = FlowerBlock.SetIsSmall(data, isSmall: false);
				int value2 = Terrain.ReplaceData(value, data2);
				m_subsystemCellChangeQueue.QueueCellChange(x, y, z, value2);
			}
		}

		public void GrowRye(int value, int x, int y, int z, int pollPass)
		{
			if (Terrain.ExtractLight(SubsystemTerrain.Terrain.GetCellValueFast(x, y + 1, z)) < 9)
			{
				return;
			}
			int data = Terrain.ExtractData(value);
			int size = RyeBlock.GetSize(data);
			if (size == 7)
			{
				return;
			}
			if (RyeBlock.GetIsWild(data))
			{
				if (size < 7)
				{
					int data2 = RyeBlock.SetSize(RyeBlock.SetIsWild(data, isWild: true), size + 1);
					int value2 = Terrain.ReplaceData(value, data2);
					m_subsystemCellChangeQueue.QueueCellChange(x, y, z, value2);
				}
				return;
			}
			int cellValueFast = SubsystemTerrain.Terrain.GetCellValueFast(x, y - 1, z);
			if (Terrain.ExtractContents(cellValueFast) == 168)
			{
				int data3 = Terrain.ExtractData(cellValueFast);
				bool hydration = SoilBlock.GetHydration(data3);
				int nitrogen = SoilBlock.GetNitrogen(data3);
				int num = SubsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(y);
				int num2 = 4;
				float num3 = 0.8f;
				if (nitrogen > 0)
				{
					num2--;
					num3 -= 0.4f;
				}
				if (hydration)
				{
					num2--;
					num3 -= 0.4f;
				}
				if (num <= 4)
				{
					num2 += 4;
				}
				if (pollPass % MathUtils.Max(num2, 1) == 0 || !(num3 < 1f))
				{
					int data4 = RyeBlock.SetSize(data, MathUtils.Min(size + 1, 7));
					if (m_random.Float(0f, 1f) < num3 && size == 3)
					{
						data4 = RyeBlock.SetIsWild(data4, isWild: true);
					}
					int value3 = Terrain.ReplaceData(value, data4);
					m_subsystemCellChangeQueue.QueueCellChange(x, y, z, value3);
					if (size + 1 == 7)
					{
						int data5 = SoilBlock.SetNitrogen(data3, MathUtils.Max(nitrogen - 1, 0));
						int value4 = Terrain.ReplaceData(cellValueFast, data5);
						m_subsystemCellChangeQueue.QueueCellChange(x, y - 1, z, value4);
					}
				}
			}
			else
			{
				int value5 = Terrain.ReplaceData(value, RyeBlock.SetIsWild(data, isWild: true));
				m_subsystemCellChangeQueue.QueueCellChange(x, y, z, value5);
			}
		}

		public void GrowCotton(int value, int x, int y, int z, int pollPass)
		{
			if (Terrain.ExtractLight(SubsystemTerrain.Terrain.GetCellValueFast(x, y + 1, z)) < 9)
			{
				return;
			}
			int data = Terrain.ExtractData(value);
			int size = CottonBlock.GetSize(data);
			if (size >= 2)
			{
				return;
			}
			if (CottonBlock.GetIsWild(data))
			{
				if (size < 2)
				{
					int data2 = CottonBlock.SetSize(CottonBlock.SetIsWild(data, isWild: true), size + 1);
					int value2 = Terrain.ReplaceData(value, data2);
					m_subsystemCellChangeQueue.QueueCellChange(x, y, z, value2);
				}
				return;
			}
			int cellValueFast = SubsystemTerrain.Terrain.GetCellValueFast(x, y - 1, z);
			if (Terrain.ExtractContents(cellValueFast) == 168)
			{
				int data3 = Terrain.ExtractData(cellValueFast);
				bool hydration = SoilBlock.GetHydration(data3);
				int nitrogen = SoilBlock.GetNitrogen(data3);
				int num = SubsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(y);
				int num2 = 8;
				float num3 = 0.8f;
				if (nitrogen > 0)
				{
					num2 -= 2;
					num3 -= 0.4f;
				}
				if (hydration)
				{
					num2 -= 2;
					num3 -= 0.4f;
				}
				if (num <= 4)
				{
					num2 += 8;
				}
				if (pollPass % MathUtils.Max(num2, 1) == 0 || !(num3 < 1f))
				{
					int data4 = CottonBlock.SetSize(data, MathUtils.Min(size + 1, 2));
					if (m_random.Float(0f, 1f) < num3 && size == 1)
					{
						data4 = CottonBlock.SetIsWild(data4, isWild: true);
					}
					int value3 = Terrain.ReplaceData(value, data4);
					m_subsystemCellChangeQueue.QueueCellChange(x, y, z, value3);
					if (size + 1 == 2)
					{
						int data5 = SoilBlock.SetNitrogen(data3, MathUtils.Max(nitrogen - 1, 0));
						int value4 = Terrain.ReplaceData(cellValueFast, data5);
						m_subsystemCellChangeQueue.QueueCellChange(x, y - 1, z, value4);
					}
				}
			}
			else
			{
				int value5 = Terrain.ReplaceData(value, CottonBlock.SetIsWild(data, isWild: true));
				m_subsystemCellChangeQueue.QueueCellChange(x, y, z, value5);
			}
		}

		public void GrowPumpkin(int value, int x, int y, int z, int pollPass)
		{
			if (Terrain.ExtractLight(SubsystemTerrain.Terrain.GetCellValueFast(x, y + 1, z)) < 9)
			{
				return;
			}
			int data = Terrain.ExtractData(value);
			int size = BasePumpkinBlock.GetSize(data);
			if (BasePumpkinBlock.GetIsDead(data) || size >= 7)
			{
				return;
			}
			int cellValueFast = SubsystemTerrain.Terrain.GetCellValueFast(x, y - 1, z);
			int num = Terrain.ExtractContents(cellValueFast);
			int data2 = Terrain.ExtractData(cellValueFast);
			bool flag = num == 168 && SoilBlock.GetHydration(data2);
			int num2 = (num == 168) ? SoilBlock.GetNitrogen(data2) : 0;
			int num3 = SubsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(y);
			int num4 = 4;
			float num5 = 0.15f;
			if (num == 168)
			{
				num4--;
				num5 -= 0.05f;
			}
			if (num2 > 0)
			{
				num4--;
				num5 -= 0.05f;
			}
			if (flag)
			{
				num4--;
				num5 -= 0.05f;
			}
			if (num3 <= 8)
			{
				num4 += 5;
			}
			if (pollPass % MathUtils.Max(num4, 1) == 0 || !(num5 < 1f))
			{
				int data3 = BasePumpkinBlock.SetSize(data, MathUtils.Min(size + 1, 7));
				if (m_random.Float(0f, 1f) < num5)
				{
					data3 = BasePumpkinBlock.SetIsDead(data3, isDead: true);
				}
				int value2 = Terrain.ReplaceData(value, data3);
				m_subsystemCellChangeQueue.QueueCellChange(x, y, z, value2);
				if (num == 168 && size + 1 == 7)
				{
					int data4 = SoilBlock.SetNitrogen(data2, MathUtils.Max(num2 - 3, 0));
					int value3 = Terrain.ReplaceData(cellValueFast, data4);
					m_subsystemCellChangeQueue.QueueCellChange(x, y - 1, z, value3);
				}
			}
		}

		public virtual void Lifecycle(int value, int x, int y, int z, bool applyImmediately)
		{
		}
	}
}
