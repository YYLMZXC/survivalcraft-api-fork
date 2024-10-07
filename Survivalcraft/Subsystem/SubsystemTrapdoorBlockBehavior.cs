using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemTrapdoorBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemElectricity m_subsystemElectricity;

		public static Random m_random = new();

		public override int[] HandledBlocks => new int[2]
		{
			83,
			84
		};

		public bool IsTrapdoorElectricallyConnected(int x, int y, int z)
		{
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
			int num = Terrain.ExtractContents(cellValue);
			int data = Terrain.ExtractData(cellValue);
			if (BlocksManager.Blocks[num] is TrapdoorBlock)
			{
				ElectricElement electricElement = m_subsystemElectricity.GetElectricElement(x, y, z, TrapdoorBlock.GetMountingFace(data));
				if (electricElement != null && electricElement.Connections.Count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public bool OpenCloseTrapdoor(int x, int y, int z, bool open)
		{
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
			int num = Terrain.ExtractContents(cellValue);
			if (BlocksManager.Blocks[num] is TrapdoorBlock)
			{
				int data = TrapdoorBlock.SetOpen(Terrain.ExtractData(cellValue), open);
				int value = Terrain.ReplaceData(cellValue, data);
				SubsystemTerrain.ChangeCell(x, y, z, value);
				string name = open ? "Audio/Doors/DoorOpen" : "Audio/Doors/DoorClose";
				SubsystemTerrain.Project.FindSubsystem<SubsystemAudio>(throwOnError: true).PlaySound(name, 0.7f, m_random.Float(-0.1f, 0.1f), new Vector3(x, y, z), 4f, autoDelay: true);
				return true;
			}
			return false;
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			CellFace cellFace = raycastResult.CellFace;
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			int num = Terrain.ExtractContents(cellValue);
			int data = Terrain.ExtractData(cellValue);
			if (num == 83 || !IsTrapdoorElectricallyConnected(cellFace.X, cellFace.Y, cellFace.Z))
			{
				bool open = TrapdoorBlock.GetOpen(data);
				return OpenCloseTrapdoor(cellFace.X, cellFace.Y, cellFace.Z, !open);
			}
			return true;
		}

		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
			int num = Terrain.ExtractContents(cellValue);
			Block obj = BlocksManager.Blocks[num];
			int data = Terrain.ExtractData(cellValue);
			if (obj is TrapdoorBlock)
			{
				int rotation = TrapdoorBlock.GetRotation(data);
				bool upsideDown = TrapdoorBlock.GetUpsideDown(data);
				bool flag = false;
				Point3 point = CellFace.FaceToPoint3(rotation);
				int cellValue2 = SubsystemTerrain.Terrain.GetCellValue(x - point.X, y - point.Y, z - point.Z);
				flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)].IsNonAttachable(cellValue2);
				if (upsideDown)
				{
					int cellValue3 = SubsystemTerrain.Terrain.GetCellValue(x, y + 1, z);
					flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue3)].IsNonAttachable(cellValue3);
					int cellValue4 = SubsystemTerrain.Terrain.GetCellValue(x - point.X, y - point.Y + 1, z - point.Z);
					flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue4)].IsNonAttachable(cellValue4);
				}
				else
				{
					int cellValue5 = SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
					flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue5)].IsNonAttachable(cellValue5);
					int cellValue6 = SubsystemTerrain.Terrain.GetCellValue(x - point.X, y - point.Y - 1, z - point.Z);
					flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue6)].IsNonAttachable(cellValue6);
				}
				if (!flag)
				{
					SubsystemTerrain.DestroyCell(0, x, y, z, 0, noDrop: false, noParticleSystem: false);
				}
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemElectricity = Project.FindSubsystem<SubsystemElectricity>(throwOnError: true);
		}
	}
}
