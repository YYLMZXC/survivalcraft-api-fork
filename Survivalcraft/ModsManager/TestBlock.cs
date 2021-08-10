using Engine.Graphics;
using Engine;
using System;
using System.IO;
namespace Game
{
    public class TestBlock:Block
    {
        public const int Index = 999;
        public BlockMesh BlockMesh = new BlockMesh();
        public override void Initialize()
        {
            using (Stream stream = Storage.OpenFile("app:demo.obj", OpenFileMode.Read))
            {
                try {
                    Model model = ObjModelReader.Load(stream);
                    foreach (ModelMesh mesh in model.Meshes) {
                        BlockMesh.AppendModelMeshPart(mesh.MeshParts[0],Matrix.Identity,false,false,false,false,Color.White);                    
                    }
                }
                catch (Exception e) {
                
                }
            }
        }
        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
        }
        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
        {
            return "2326565121313456";
        }
        public override string GetCategory(int value)
        {
            return "Terrain";
        }
        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
        {
            BlocksManager.DrawMeshBlock(primitivesRenderer,BlockMesh,1f,ref matrix,environmentData);
        }
    }
}
