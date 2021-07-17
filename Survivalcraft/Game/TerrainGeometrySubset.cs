using Engine;

namespace Game
{
    public class TerrainGeometrySubset
    {
        public DynamicArray<TerrainVertex> Vertices = new DynamicArray<TerrainVertex>();

        public DynamicArray<ushort> Indices = new DynamicArray<ushort>();

        public GeometryType GeometryType = default;

        public TerrainGeometrySubset()
        {
        }

        public void CopyFrom(TerrainGeometrySubset subset) {
            for (int i = 0; i < subset.Vertices.Count; i++)
            {
                Vertices.Add(subset.Vertices[i]);
            }
            for (int i = 0; i < subset.Indices.Count; i++)
            {
                ushort index = (ushort)(subset.Indices[i] + subset.Vertices.Count);
                Indices.Add(index);
            }

        }

        public TerrainGeometrySubset(DynamicArray<TerrainVertex> vertices, DynamicArray<ushort> indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}
