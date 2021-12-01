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

		public Dictionary<Texture2D, TerrainGeometry> Draws = new Dictionary<Texture2D, TerrainGeometry>();

		public TerrainGeometry GetGeometry(Texture2D texture)
		{
			if (texture == null) throw new System.Exception("Texture can not be null");
			if (!Draws.TryGetValue(texture, out var geometry))
			{
				geometry = new TerrainGeometry();
				geometry.Subsets = new TerrainGeometrySubset[7];
				for (int i = 0; i < 7; i++) { geometry.Subsets[i] = new TerrainGeometrySubset(); }
				geometry.SubsetOpaque = geometry.Subsets[4];
				geometry.SubsetAlphaTest = geometry.Subsets[5];
				geometry.SubsetTransparent = geometry.Subsets[6];
				geometry.OpaqueSubsetsByFace = new TerrainGeometrySubset[6]
				{
					geometry.Subsets[0],
					geometry.Subsets[1],
					geometry.Subsets[2],
					geometry.Subsets[3],
					geometry.Subsets[4],
					geometry.Subsets[4]
				};
				geometry.AlphaTestSubsetsByFace = new TerrainGeometrySubset[6]
				{
					geometry.Subsets[5],
					geometry.Subsets[5],
					geometry.Subsets[5],
					geometry.Subsets[5],
					geometry.Subsets[5],
					geometry.Subsets[5]
				};
				geometry.TransparentSubsetsByFace = new TerrainGeometrySubset[6]
				{
					geometry.Subsets[6],
					geometry.Subsets[6],
					geometry.Subsets[6],
					geometry.Subsets[6],
					geometry.Subsets[6],
					geometry.Subsets[6]
				};
				Draws.Add(texture, geometry);
			}
			return geometry;
		}

		public void ClearSubsets(SubsystemAnimatedTextures animatedTextures)
		{
			Draws.Clear();
			TerrainGeometry geometry = GetGeometry(animatedTextures.AnimatedBlocksTexture);
			SubsetOpaque = geometry.SubsetOpaque;
			SubsetAlphaTest = geometry.SubsetAlphaTest;
			SubsetTransparent = geometry.SubsetOpaque;
			OpaqueSubsetsByFace = geometry.OpaqueSubsetsByFace;
			AlphaTestSubsetsByFace = geometry.AlphaTestSubsetsByFace;
			TransparentSubsetsByFace = geometry.TransparentSubsetsByFace;
			Subsets = geometry.Subsets;
		}

	}
}
