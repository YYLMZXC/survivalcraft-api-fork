using Engine;
using System.Collections.Generic;
using Engine.Graphics;

namespace Game
{
    public class TerrainGeometry
    {

		public const int OpaqueFace0Index = 0;

		public const int OpaqueFace1Index = 1;

		public const int OpaqueFace2Index = 2;

		public const int OpaqueFace3Index = 3;

		public const int OpaqueIndex = 4;

		public const int AlphaTestIndex = 5;

		public const int TransparentIndex = 6;

		public TerrainGeometrySubset SubsetOpaque;

		public TerrainGeometrySubset SubsetAlphaTest;

		public TerrainGeometrySubset SubsetTransparent;

		public TerrainGeometrySubset[] OpaqueSubsetsByFace;

		public TerrainGeometrySubset[] AlphaTestSubsetsByFace;

		public TerrainGeometrySubset[] TransparentSubsetsByFace;

		public TerrainGeometrySubset[] Subsets = new TerrainGeometrySubset[7];

		public int ContentsHash;

		public TerrainGeometry(bool AllSame = false)
		{
			Subsets = new TerrainGeometrySubset[7];

			if (AllSame)
			{
				TerrainGeometrySubset subset = new TerrainGeometrySubset();
				for (int i = 0; i < Subsets.Length; i++)
				{
					Subsets[i] = subset;
				}
			}
			else
			{

				for (int i = 0; i < Subsets.Length; i++)
				{
					Subsets[i] = new TerrainGeometrySubset();
				}

			}
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
	}
}
