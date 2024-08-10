using Engine;

namespace Game
{
	public class TerrainGeometrySubset : IDisposable
    {
		public DynamicArray<TerrainVertex> Vertices = [];

		public DynamicArray<int> Indices = [];

		public TerrainGeometrySubset()
		{
		}

		public TerrainGeometrySubset(DynamicArray<TerrainVertex> vertices, DynamicArray<int> indices)
		{
			Vertices = vertices;
			Indices = indices;
		}
        public void Dispose()
        {
	        Vertices.Clear();
	        Indices.Clear();
        }
    }
}
