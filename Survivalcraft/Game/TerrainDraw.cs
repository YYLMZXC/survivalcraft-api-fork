using Engine.Graphics;
using Engine;
using System.Collections.Generic;
namespace Game
{
    public class TerrainDraw
    {
        public Texture2D Texture;
        public Dictionary<Point3, List<TerrainGeometry>> Caches = new Dictionary<Point3, List<TerrainGeometry>>();
        public Dictionary<Texture2D, TerrainGeometry> Draws = new Dictionary<Texture2D, TerrainGeometry>();
        public TerrainDraw() {}
        public void AppendTerrainGeometrySubset(TerrainGeometrySubset source,TerrainGeometrySubset append)
        {
            int count = source.Vertices.Count;
            for (int i = 0; i < append.Vertices.Count; i++)
            {
                source.Vertices.Add(append.Vertices.Array[i]);
            }
            for (int j = 0; j < append.Indices.Count; j++)
            {
                source.Indices.Add((ushort)(append.Indices.Array[j] + count));
            }
        }

        public void Combile() {
            Draws.Clear();
            foreach (var item in Caches) {
                List<TerrainGeometry> geometries = item.Value;
                foreach (TerrainGeometry terrainGeometry in geometries) {
                    if (Draws.TryGetValue(terrainGeometry.Texture, out TerrainGeometry geometry))
                    {
                        AppendTerrainGeometrySubset(geometry.SubsetOpaque,terrainGeometry.SubsetOpaque);
                    }
                    else {
                        Draws.Add(terrainGeometry.Texture,terrainGeometry);
                    }                
                }
            }
            Caches.Clear();
        }
    }
}
