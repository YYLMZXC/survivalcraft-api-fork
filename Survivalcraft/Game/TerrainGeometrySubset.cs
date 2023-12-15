using Engine;

namespace Game
{
	public class TerrainGeometrySubset
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
	}
}
