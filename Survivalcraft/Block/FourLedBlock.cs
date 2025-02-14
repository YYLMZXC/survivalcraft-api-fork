using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Game
{
	public class FourLedBlock : MountedElectricElementBlock
	{
		public static int Index = 182;

		public BlockMesh m_standaloneBlockMesh;

		public BlockMesh[] m_blockMeshesByFace = new BlockMesh[6];

		public BoundingBox[][] m_collisionBoxesByFace = new BoundingBox[6][];

		public override void Initialize()
		{
			ModelMesh modelMesh = ContentManager.Get<Model>("Models/Leds").FindMesh("FourLed");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(modelMesh.ParentBone);
			for (int i = 0; i < 6; i++)
			{
				Matrix m = (i >= 4) ? ((i != 4) ? (Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f)) : Matrix.CreateTranslation(0.5f, 0f, 0.5f)) : (Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY(i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f));
				m_blockMeshesByFace[i] = new BlockMesh();
				m_blockMeshesByFace[i].AppendModelMeshPart(modelMesh.MeshParts[0], boneAbsoluteTransform * m, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
				m_collisionBoxesByFace[i] = new BoundingBox[1]
				{
					m_blockMeshesByFace[i].CalculateBoundingBox()
				};
			}
			Matrix m2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
			m_standaloneBlockMesh = new BlockMesh();
			m_standaloneBlockMesh.AppendModelMeshPart(modelMesh.MeshParts[0], boneAbsoluteTransform * m2, makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
		}

		public override IEnumerable<CraftingRecipe> GetProceduralCraftingRecipes()
		{
			int color = 0;
			while (color < 8)
			{
				var craftingRecipe = new CraftingRecipe
				{
					ResultCount = 4,
					ResultValue = Terrain.MakeBlockValue(182, 0, SetColor(0, color)),
					RemainsCount = 1,
					RemainsValue = Terrain.MakeBlockValue(90),
					RequiredHeatLevel = 0f,
					Description = LanguageControl.Get(GetType().Name, 1)
				};
				craftingRecipe.Ingredients[0] = "glass";
				craftingRecipe.Ingredients[1] = "glass";
				craftingRecipe.Ingredients[2] = "glass";
				craftingRecipe.Ingredients[4] = "paintbucket:" + color.ToString(CultureInfo.InvariantCulture);
				craftingRecipe.Ingredients[6] = "copperingot";
				craftingRecipe.Ingredients[7] = "copperingot";
				craftingRecipe.Ingredients[8] = "copperingot";
				yield return craftingRecipe;
				int num = color + 1;
				color = num;
			}
		}

		public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
		{
			int mountingFace = GetMountingFace(Terrain.ExtractData(value));
			return face != CellFace.OppositeFace(mountingFace);
		}

		public override int GetFace(int value)
		{
			return GetMountingFace(Terrain.ExtractData(value));
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			int data = Terrain.ExtractData(value);
			int color = GetColor(data);
            return string.Format(LanguageControl.Get("LedBlock", "Format"), LanguageControl.Get("LedBlock", color), LanguageControl.GetBlock(string.Format("{0}:{1}", GetType().Name, data.ToString()), "DisplayName"));
		}

		public override IEnumerable<int> GetCreativeValues()
		{
			int i = 0;
			while (i < 8)
			{
				yield return Terrain.MakeBlockValue(182, 0, SetColor(0, i));
				int num = i + 1;
				i = num;
			}
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			int data = SetMountingFace(Terrain.ExtractData(value), raycastResult.CellFace.Face);
			int value2 = Terrain.ReplaceData(value, data);
			BlockPlacementData result = default;
			result.Value = value2;
			result.CellFace = raycastResult.CellFace;
			return result;
		}

		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			int color = GetColor(Terrain.ExtractData(oldValue));
			dropValues.Add(new BlockDropValue
			{
				Value = Terrain.MakeBlockValue(182, 0, SetColor(0, color)),
				Count = 1
			});
			showDebris = true;
		}

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			int mountingFace = GetMountingFace(Terrain.ExtractData(value));
			if (mountingFace >= m_collisionBoxesByFace.Length)
			{
				return null;
			}
			return m_collisionBoxesByFace[mountingFace];
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
			int mountingFace = GetMountingFace(Terrain.ExtractData(value));
			if (mountingFace < m_blockMeshesByFace.Length)
			{
				generator.GenerateMeshVertices(this, x, y, z, m_blockMeshesByFace[mountingFace], Color.White, null, geometry.SubsetOpaque);
				generator.GenerateWireVertices(value, x, y, z, mountingFace, 1f, Vector2.Zero, geometry.SubsetOpaque);
			}
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
		}

		public override ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			return new FourLedElectricElement(subsystemElectricity, new CellFace(x, y, z, GetFace(value)));
		}

		public override ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			int face2 = GetFace(value);
			if (face == face2 && SubsystemElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue)
			{
				return ElectricConnectorType.Input;
			}
			return null;
		}

		public static int GetColor(int data)
		{
			return (data >> 3) & 7;
		}

		public static int SetColor(int data, int color)
		{
			return (data & -57) | ((color & 7) << 3);
		}

		public static int GetMountingFace(int data)
		{
			return data & 7;
		}

		public static int SetMountingFace(int data, int face)
		{
			return (data & -8) | (face & 7);
		}
	}
}
