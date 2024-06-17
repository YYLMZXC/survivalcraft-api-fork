using System;

namespace Engine.Graphics
{
	public class ModelMeshPart : IDisposable
	{
        public BoundingBox m_boundingBox;

		public string TexturePath;

		public VertexBuffer VertexBuffer
		{
			get;
			set;
		}

		public IndexBuffer IndexBuffer
		{
			get;
			set;
		}

		public int StartIndex
		{
			get;
			set;
		}

		public int IndicesCount
		{
			get;
			set;
		}

		public BoundingBox BoundingBox
		{
			get
			{
				return m_boundingBox;
			}
			set
			{
				m_boundingBox = value;
			}
		}

        public ModelMeshPart()
		{
		}

		public void Dispose()
		{
			if (VertexBuffer != null)
			{
				VertexBuffer.Dispose();
				VertexBuffer = null;
			}
			if (IndexBuffer != null)
			{
				IndexBuffer.Dispose();
				IndexBuffer = null;
			}
		}
	}
}
