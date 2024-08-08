using Engine;

namespace Game
{
	public class TerrainGeometrySubset : IDisposable
    {
		public TerrainGeometryDynamicArray<TerrainVertex> Vertices = [];

		public TerrainGeometryDynamicArray<int> Indices = [];

		public TerrainGeometrySubset()
		{
		}

		public TerrainGeometrySubset(TerrainGeometryDynamicArray<TerrainVertex> vertices, TerrainGeometryDynamicArray<int> indices)
		{
			Vertices = vertices;
			Indices = indices;
		}
        public void Dispose()
        {
            Utilities.Dispose<TerrainGeometryDynamicArray<TerrainVertex>>(ref this.Vertices);
            Utilities.Dispose<TerrainGeometryDynamicArray<int>>(ref this.Indices);
        }
    }
}
