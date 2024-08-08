using Engine;
using Engine.Graphics;
using System;

namespace Game
{
	public class TerrainChunkGeometry
    {
        public TerrainChunkGeometry()
        {
            for (int i = 0; i < this.Slices.Length; i++)
            {
                this.Slices[i] = new TerrainChunkSliceGeometry();
            }
        }

        public void Dispose()
        {
            this.DisposeVertexIndexBuffers();
            TerrainChunkSliceGeometry[] slices = this.Slices;
            for (int i = 0; i < slices.Length; i++)
            {
                slices[i].Dispose();
            }
        }

        public void InvalidateSlicesGeometryHashes()
        {
            TerrainChunkSliceGeometry[] slices = this.Slices;
            for (int i = 0; i < slices.Length; i++)
            {
                slices[i].GeometryHash = 0;
            }
        }

        public void DisposeVertexIndexBuffers()
        {
            foreach (TerrainChunkGeometry.Buffer buffer in this.Buffers)
            {
                buffer.Dispose();
            }
            this.Buffers.Clear();
        }

        // Token: 0x0400148D RID: 5261
        public const int SubsetsCount = 7;

        // Token: 0x0400148E RID: 5262
        public TerrainChunkSliceGeometry[] Slices = new TerrainChunkSliceGeometry[16];

        // Token: 0x0400148F RID: 5263
        public DynamicArray<TerrainChunkGeometry.Buffer> Buffers = new DynamicArray<TerrainChunkGeometry.Buffer>();

        public class Buffer : IDisposable
		{
			public VertexBuffer VertexBuffer;

			public IndexBuffer IndexBuffer;

			public int[] SubsetIndexBufferStarts = new int[7];

			public int[] SubsetIndexBufferEnds = new int[7];

			public void Dispose()
			{
				Utilities.Dispose(ref VertexBuffer);
				Utilities.Dispose(ref IndexBuffer);
			}
		}
	}
}