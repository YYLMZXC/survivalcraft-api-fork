using Engine;
using System.Collections.Generic;
using Engine.Graphics;

namespace Game
{
    public class TerrainGeometry
    {
		public virtual TerrainGeometrySubset SubsetOpaque { get; set; }

		public virtual TerrainGeometrySubset SubsetAlphaTest { get; set; }

		public virtual TerrainGeometrySubset SubsetTransparent { get; set; }

		public virtual TerrainGeometrySubset[] OpaqueSubsetsByFace { get; set; }

		public virtual TerrainGeometrySubset[] AlphaTestSubsetsByFace { get; set; }

		public virtual TerrainGeometrySubset[] TransparentSubsetsByFace { get; set; }

		public TerrainGeometrySubset[] Subsets = new TerrainGeometrySubset[7];

		public TerrainGeometry(bool SubsetsIsSame = false)
		{
			InitSubsets(SubsetsIsSame);
		}
		public virtual void InitSubsets(bool SubsetsIsSame) {
			Subsets = new TerrainGeometrySubset[7];
			if (SubsetsIsSame)
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
		public virtual TerrainGeometry GetGeomtry(Texture2D texture)
		{
			return this;
		}
		public virtual void ClearSubsets()
		{
			for (int i = 0; i < Subsets.Length; i++)
			{
				Subsets[i].Indices.Clear();
				Subsets[i].Vertices.Clear();
			}

		}
	}
}
