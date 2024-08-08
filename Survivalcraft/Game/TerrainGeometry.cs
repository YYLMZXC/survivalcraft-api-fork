using Engine;
using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
	public class TerrainGeometry : IDisposable
    {
		public TerrainGeometrySubset SubsetOpaque;

		public TerrainGeometrySubset SubsetAlphaTest;

		public TerrainGeometrySubset SubsetTransparent;

		public TerrainGeometrySubset[] OpaqueSubsetsByFace;

		public TerrainGeometrySubset[] AlphaTestSubsetsByFace;

		public TerrainGeometrySubset[] TransparentSubsetsByFace;

        public void Dispose()
        {
            Utilities.Dispose<TerrainGeometrySubset>(ref this.SubsetOpaque);
            Utilities.Dispose<TerrainGeometrySubset>(ref this.SubsetAlphaTest);
            Utilities.Dispose<TerrainGeometrySubset>(ref this.SubsetTransparent);
            for (int i = 0; i < this.OpaqueSubsetsByFace.Length; i++)
            {
                Utilities.Dispose<TerrainGeometrySubset>(ref this.OpaqueSubsetsByFace[i]);
            }
            for (int j = 0; j < this.AlphaTestSubsetsByFace.Length; j++)
            {
                Utilities.Dispose<TerrainGeometrySubset>(ref this.AlphaTestSubsetsByFace[j]);
            }
            for (int k = 0; k < this.TransparentSubsetsByFace.Length; k++)
            {
                Utilities.Dispose<TerrainGeometrySubset>(ref this.TransparentSubsetsByFace[k]);
            }
        }
    }
}
