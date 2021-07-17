using Engine;
using System.Collections.Generic;
using Engine.Graphics;
namespace Game
{
    public enum GeometryType
    {
        Opaque, Alphatest, Transparent
    }

    public class TerrainGeometry
    {
        public TerrainGeometrySubset SubsetOpaque;

        public TerrainGeometrySubset SubsetAlphaTest;

        public TerrainGeometrySubset SubsetTransparent;

        public TerrainGeometrySubset[] OpaqueSubsetsByFace;

        public TerrainGeometrySubset[] AlphaTestSubsetsByFace;

        public TerrainGeometrySubset[] TransparentSubsetsByFace;

        public Dictionary<Texture2D, TerrainChunkSliceGeometry> GeometrySubsets = new Dictionary<Texture2D, TerrainChunkSliceGeometry>();

        public TerrainGeometry() { 
        
        }

        public TerrainChunkSliceGeometry GetGeometry(Texture2D texture,GeometryType geometryType=GeometryType.Opaque)
        {
            if (GeometrySubsets.TryGetValue(texture, out TerrainChunkSliceGeometry subset)==false)
            {
                subset = new TerrainChunkSliceGeometry();
                GeometrySubsets.Add(texture, subset);
            }
            return subset;
        }
    }
}
