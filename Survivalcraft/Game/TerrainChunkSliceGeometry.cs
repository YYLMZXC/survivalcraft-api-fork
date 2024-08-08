using System;

namespace Game
{
    // Token: 0x02000498 RID: 1176
    public class TerrainChunkSliceGeometry : TerrainGeometry
    {
        // Token: 0x06001B7A RID: 7034 RVA: 0x000B9170 File Offset: 0x000B7370
        public TerrainChunkSliceGeometry()
        {
            this.Subsets = new TerrainGeometrySubset[7];
            for (int i = 0; i < this.Subsets.Length; i++)
            {
                this.Subsets[i] = new TerrainGeometrySubset();
            }
            this.SubsetOpaque = this.Subsets[4];
            this.SubsetAlphaTest = this.Subsets[5];
            this.SubsetTransparent = this.Subsets[6];
            this.OpaqueSubsetsByFace = new TerrainGeometrySubset[]
            {
                this.Subsets[0],
                this.Subsets[1],
                this.Subsets[2],
                this.Subsets[3],
                this.Subsets[4],
                this.Subsets[4]
            };
            this.AlphaTestSubsetsByFace = new TerrainGeometrySubset[]
            {
                this.Subsets[5],
                this.Subsets[5],
                this.Subsets[5],
                this.Subsets[5],
                this.Subsets[5],
                this.Subsets[5]
            };
            this.TransparentSubsetsByFace = new TerrainGeometrySubset[]
            {
                this.Subsets[6],
                this.Subsets[6],
                this.Subsets[6],
                this.Subsets[6],
                this.Subsets[6],
                this.Subsets[6]
            };
        }

        // Token: 0x04001483 RID: 5251
        public const int OpaqueFace0Index = 0;

        // Token: 0x04001484 RID: 5252
        public const int OpaqueFace1Index = 1;

        // Token: 0x04001485 RID: 5253
        public const int OpaqueFace2Index = 2;

        // Token: 0x04001486 RID: 5254
        public const int OpaqueFace3Index = 3;

        // Token: 0x04001487 RID: 5255
        public const int OpaqueIndex = 4;

        // Token: 0x04001488 RID: 5256
        public const int AlphaTestIndex = 5;

        // Token: 0x04001489 RID: 5257
        public const int TransparentIndex = 6;

        // Token: 0x0400148A RID: 5258
        public TerrainGeometrySubset[] Subsets = new TerrainGeometrySubset[7];

        // Token: 0x0400148B RID: 5259
        public int ContentsHash;

        // Token: 0x0400148C RID: 5260
        public int GeometryHash;
    }
}