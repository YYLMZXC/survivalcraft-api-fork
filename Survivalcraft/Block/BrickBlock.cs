using Engine;
using Engine.Graphics;

namespace Game
{
	public class BrickBlock : Block
	{
		public static int Index = 74;

		public BlockMesh m_standaloneBlockMesh = new();

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/Brick");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Brick").ParentBone);
			m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Brick").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.075f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
			base.Initialize();
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
		{
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2.5f * size, ref matrix, environmentData);
		}
	}
}
