using Engine;
using Engine.Graphics;
using System;

namespace Game
{
    public class DrawBuffer : IDisposable
    {
        public VertexBuffer VertexBuffer;

        public IndexBuffer IndexBuffer;

        public TerrainChunk Chunk;

        public Texture2D Texture;

        public int[] SubsetIndexBufferStarts = new int[7];

        public int[] SubsetIndexBufferEnds = new int[7];

        public DrawBuffer(int VertexCount,int IndicesCount) {
            VertexBuffer = new VertexBuffer(TerrainVertex.VertexDeclaration,VertexCount);
            IndexBuffer = new IndexBuffer(IndexFormat.SixteenBits,IndicesCount);        
        }
        public DrawBuffer(int VertexCount, int IndicesCount,Texture2D texture2D)
        {
            VertexBuffer = new VertexBuffer(TerrainVertex.VertexDeclaration, VertexCount);
            IndexBuffer = new IndexBuffer(IndexFormat.SixteenBits, IndicesCount);
            Texture = texture2D;
        }

        public void Dispose()
        {
            Utilities.Dispose(ref VertexBuffer);
            Utilities.Dispose(ref IndexBuffer);
        }
    }

}
