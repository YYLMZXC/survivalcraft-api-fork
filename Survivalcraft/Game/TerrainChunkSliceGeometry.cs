namespace Game
{
    public class TerrainChunkSliceGeometry : TerrainGeometry
    {
        public const int OpaqueIndex = 0;

        public const int AlphaTestIndex = 1;

        public const int TransparentIndex = 2;

        public TerrainGeometrySubset[] Subsets = new TerrainGeometrySubset[3];

        public int ContentsHash;

        public TerrainChunkSliceGeometry()
        {
            Subsets = new TerrainGeometrySubset[3];
            for (int i = 0; i < Subsets.Length; i++)
            {
                Subsets[i] = new TerrainGeometrySubset();
            }
            SubsetOpaque = Subsets[OpaqueIndex];
            SubsetAlphaTest = Subsets[AlphaTestIndex];
            SubsetTransparent = Subsets[TransparentIndex];
            OpaqueSubsetsByFace = new TerrainGeometrySubset[6]
            {
                Subsets[OpaqueIndex],
                Subsets[OpaqueIndex],
                Subsets[OpaqueIndex],
                Subsets[OpaqueIndex],
                Subsets[OpaqueIndex],
                Subsets[OpaqueIndex]
            };
            AlphaTestSubsetsByFace = new TerrainGeometrySubset[6]
            {
                Subsets[AlphaTestIndex],
                Subsets[AlphaTestIndex],
                Subsets[AlphaTestIndex],
                Subsets[AlphaTestIndex],
                Subsets[AlphaTestIndex],
                Subsets[AlphaTestIndex]
            };
            TransparentSubsetsByFace = new TerrainGeometrySubset[6]
            {
                Subsets[TransparentIndex],
                Subsets[TransparentIndex],
                Subsets[TransparentIndex],
                Subsets[TransparentIndex],
                Subsets[TransparentIndex],
                Subsets[TransparentIndex]
            };
        }
    }
}
