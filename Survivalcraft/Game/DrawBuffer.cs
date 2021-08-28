using Engine;
using Engine.Graphics;
using System;

namespace Game
{
    public class DrawBuffer : IDisposable
    {
        public VertexBuffer VertexBuffer;

        public IndexBuffer IndexBuffer;

        public Texture2D Texture;

        public int[] SubsetIndexBufferStarts = new int[7];

        public int[] SubsetIndexBufferEnds = new int[7];

        public DrawBuffer(int VertexCount,int IndicesCount) {
            VertexBuffer = new VertexBuffer(TerrainVertex.VertexDeclaration,VertexCount);
            IndexBuffer = new IndexBuffer(IndexFormat.SixteenBits,IndicesCount);
        
        }

        public void Dispose()
        {
            Utilities.Dispose(ref VertexBuffer);
            Utilities.Dispose(ref IndexBuffer);
        }
    }

}
