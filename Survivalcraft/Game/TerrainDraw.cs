using Engine.Graphics;
using Engine;
using System.Collections.Generic;
namespace Game
{
    public class TerrainDraw
    {
        public Texture2D Texture;
        public TerrainGeometry Geometry = new TerrainGeometry();
        public Dictionary<Point3, List<TerrainGeometry>> Caches = new Dictionary<Point3, List<TerrainGeometry>>();
        public Dictionary<Texture2D, TerrainGeometry> Draws = new Dictionary<Texture2D, TerrainGeometry>();


        public TerrainDraw() {
            Geometry = new TerrainGeometry();
            TerrainGeometrySubset terrainGeometrySubset = new TerrainGeometrySubset(new DynamicArray<TerrainVertex>(), new DynamicArray<ushort>());
            Geometry.AlphaTestSubsetsByFace = new TerrainGeometrySubset[6] { terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset };
            Geometry.OpaqueSubsetsByFace = new TerrainGeometrySubset[6] { terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset };
            Geometry.TransparentSubsetsByFace = new TerrainGeometrySubset[6] { terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset };
            Geometry.SubsetAlphaTest = terrainGeometrySubset;
            Geometry.SubsetOpaque = terrainGeometrySubset;
            Geometry.SubsetTransparent = terrainGeometrySubset;
        }


        public void Combile() {
            Draws.Clear();
            foreach (var item in Caches) {
                List<TerrainGeometry> geometries = item.Value;
                foreach (TerrainGeometry terrainGeometry in geometries) {
                    if (Draws.TryGetValue(terrainGeometry.Texture, out TerrainGeometry geometry))
                    {
                        geometry.SubsetOpaque.Indices.AddRange(terrainGeometry.SubsetOpaque.Indices);
                        geometry.SubsetOpaque.Vertices.AddRange(terrainGeometry.SubsetOpaque.Vertices);
                    }
                    else {
                        TerrainGeometry geometry1 = new TerrainGeometry();
                        Draws.Add(terrainGeometry.Texture,terrainGeometry);
                    }
                
                }            
            }
        
        }

    }
}
