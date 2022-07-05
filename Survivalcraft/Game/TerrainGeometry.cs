using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
	public class TerrainGeometry
	{
		public TerrainGeometrySubset SubsetOpaque;

		public TerrainGeometrySubset SubsetAlphaTest;

		public TerrainGeometrySubset SubsetTransparent;

		public TerrainGeometrySubset[] OpaqueSubsetsByFace;

		public TerrainGeometrySubset[] AlphaTestSubsetsByFace;

		public TerrainGeometrySubset[] TransparentSubsetsByFace;

		public TerrainGeometrySubset[] Subsets;

		public TerrainChunk terrainChunk;

		public int slice;

		public TerrainGeometry()
		{
			Subsets = new TerrainGeometrySubset[7];
			for (int i = 0; i < 7; i++) { Subsets[i] = new TerrainGeometrySubset(); }
			SubsetOpaque = Subsets[4];
			SubsetAlphaTest = Subsets[5];
			SubsetTransparent = Subsets[6];
			OpaqueSubsetsByFace = new TerrainGeometrySubset[6]
			{
					Subsets[0],
					Subsets[1],
					Subsets[2],
					Subsets[3],
					Subsets[4],
					Subsets[4]
			};
			AlphaTestSubsetsByFace = new TerrainGeometrySubset[6]
			{
					Subsets[5],
					Subsets[5],
					Subsets[5],
					Subsets[5],
					Subsets[5],
					Subsets[5]
			};
			TransparentSubsetsByFace = new TerrainGeometrySubset[6]
			{
					Subsets[6],
					Subsets[6],
					Subsets[6],
					Subsets[6],
					Subsets[6],
					Subsets[6]
			};
		}

		public TerrainGeometry GetGeometry(Texture2D texture)
		{
			if (terrainChunk.Draws.TryGetValue(texture, out var geometries)) return geometries[slice];
			else
			{
				var list = new TerrainGeometry[16];
				for (int i = 0; i < 16; i++) { var t = new TerrainGeometry(); t.slice = i; t.terrainChunk = terrainChunk; list[i] = t; }
				terrainChunk.Draws.Add(texture, list);
				return list[slice];
			}
		}
	}
}
