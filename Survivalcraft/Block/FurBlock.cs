using Engine;
using Engine.Graphics;

namespace Game
{
    public class FurBlock : Block
    {
        public const int Index = 207;

        public BlockMesh m_standaloneBlockMesh = new BlockMesh();

        public override void Initialize()
        {
            Model model = ContentManager.Get<Model>("Models/Fur");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Fur").ParentBone);
            m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Fur").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, 0f, 0f), makeEmissive: false, flipWindingOrder: false, doubleSided: false, flipNormals: false, Color.White);
            m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Fur").MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, 0f, 0f), makeEmissive: false, flipWindingOrder: true, doubleSided: false, flipNormals: false, new Color(128, 128, 160));
            base.Initialize();
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
        {
            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
        }
    }
}
