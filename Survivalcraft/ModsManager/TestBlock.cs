using Engine.Graphics;
using Engine;
using System.Collections.Generic;
using System.IO;
namespace Game
{
    public class TestBlock:Block
    {
        public const int Index = 999;
        public Dictionary<Texture2D, BlockMesh> Meshes = new Dictionary<Texture2D, BlockMesh>();
        public Dictionary<string, Texture2D> TextureMaps = new Dictionary<string, Texture2D>();
        public override void Initialize()
        {
            using (Stream stream = Storage.OpenFile("app:12.json", OpenFileMode.Read))
            {
                JsonModel jsonModel = JsonModelReader.Load(stream);
                foreach (ModelMesh mesh in jsonModel.Meshes)
                {
                    ModelMeshPart modelMeshPart = mesh.MeshParts[0];
                    if (TextureMaps.TryGetValue(modelMeshPart.TexturePath, out Texture2D texture) == false)
                    {
                        texture = ContentManager.Get<Texture2D>(modelMeshPart.TexturePath);
                        TextureMaps.Add(modelMeshPart.TexturePath, texture);
                    }
                    if (Meshes.TryGetValue(texture, out BlockMesh blockMesh))
                    {
                        blockMesh.AppendModelMeshPart(modelMeshPart, Matrix.Identity, false, false, false, false, Color.White);
                    }
                    else
                    {
                        blockMesh = new BlockMesh();
                        blockMesh.AppendModelMeshPart(modelMeshPart, Matrix.Identity, false, false, false, false, Color.White);
                        Meshes.Add(texture, blockMesh);
                    }
                }
            }

            /*
            using (Stream stream = Storage.OpenFile("app:chemical_oxidizer.obj", OpenFileMode.Read))
            {
                ObjModel jsonModel = ObjModelReader.Load(stream);
                foreach (ModelMesh mesh in jsonModel.Meshes)
                {
                    ModelMeshPart modelMeshPart = mesh.MeshParts[0];
                    if (TextureMaps.TryGetValue(modelMeshPart.TexturePath, out Texture2D texture) == false)
                    {
                        texture = ContentManager.Get<Texture2D>(modelMeshPart.TexturePath);
                        TextureMaps.Add(modelMeshPart.TexturePath, texture);
                    }
                    if (Meshes.TryGetValue(texture, out BlockMesh blockMesh))
                    {
                        blockMesh.AppendModelMeshPart(modelMeshPart, Matrix.Identity, false, false, false, false, Color.White);
                    }
                    else
                    {
                        blockMesh = new BlockMesh();
                        blockMesh.AppendModelMeshPart(modelMeshPart, Matrix.Identity, false, false, false, false, Color.White);
                        Meshes.Add(texture, blockMesh);
                    }
                }
            }
            */

        }
        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
        {
            return true;
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            foreach (var c in Meshes)
            {
                generator.GenerateMeshVertices(this, x, y, z, c.Value, Color.White, null, geometry.GetGeometry(c.Key).SubsetAlphaTest);
            }
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
            foreach (var c in Meshes)
            {
                BlocksManager.DrawMeshBlock(primitivesRenderer, c.Value, c.Key, Color.White, 1f, ref matrix, environmentData);
            }
        }
    }
}
