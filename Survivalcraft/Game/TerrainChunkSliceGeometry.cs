using Engine.Graphics;
using System.Collections.Generic;
using Engine;
namespace Game
{
	public enum TerrainGeometryType
	{
		OpaqueFace0,
		OpaqueFace1,
		OpaqueFace2,
		OpaqueFace3,
		Opaque,
		AlphaTest,
		Transparent
	}
	public class TerrainChunkSliceGeometry : TerrainGeometry
	{
		public Dictionary<Texture2D, TerrainGeometry> Geometries = new Dictionary<Texture2D, TerrainGeometry>();
		public bool CompileVertexAndIndex(TerrainGeometrySubset[] geometry, Texture2D texture, out DrawBuffer buffer)
		{
			int IndicesPosition = 0;
			int VerticesPosition = 0;
			int IndicesCount = 0;
			int VerticesCount = 0;
			buffer = null;
			for (int i = 0; i < geometry.Length; i++)
			{
				VerticesCount += geometry[i].Vertices.Count;
				IndicesCount += geometry[i].Indices.Count;
				if (m_subsetsIsSame) break;
			}
			if (IndicesCount == 0) return false;
			buffer = new DrawBuffer(VerticesCount,IndicesCount,texture);
			for (int i = 0; i < geometry.Length; i++)
			{
				if (IndicesPosition > 0)
				{
					for (int j = 0; j < geometry[i].Indices.Count; j++)
					{
						geometry[i].Indices[j] += (ushort)(VerticesPosition);
					}
				}
				buffer.VertexBuffer.SetData(geometry[i].Vertices.Array, 0, geometry[i].Vertices.Count, VerticesPosition);
				buffer.IndexBuffer.SetData(geometry[i].Indices.Array, 0, geometry[i].Indices.Count, IndicesPosition);
				buffer.SubsetIndexBufferStarts[i] = IndicesPosition;
				buffer.SubsetIndexBufferEnds[i] = IndicesPosition + geometry[i].Indices.Count;
				VerticesPosition += geometry[i].Vertices.Count;
				IndicesPosition += geometry[i].Indices.Count;
				if (m_subsetsIsSame) break;
			}
			return true;
		}
		public bool m_subsetsIsSame;
		public int ContentsHash;
		public TerrainGeometry Default;
		public override TerrainGeometrySubset SubsetOpaque { get => Default.SubsetOpaque; }
		public override TerrainGeometrySubset SubsetAlphaTest { get => Default.SubsetAlphaTest; }
		public override TerrainGeometrySubset SubsetTransparent { get => Default.SubsetTransparent; }
		public override TerrainGeometrySubset[] AlphaTestSubsetsByFace { get => Default.AlphaTestSubsetsByFace; }
		public override TerrainGeometrySubset[] OpaqueSubsetsByFace { get => Default.OpaqueSubsetsByFace; }
		public override TerrainGeometrySubset[] TransparentSubsetsByFace { get => Default.TransparentSubsetsByFace; }
		public DynamicArray<DrawBuffer> DrawBuffers = new DynamicArray<DrawBuffer>();
		public bool ShouldDisposeDrawBuffer = false;
		public TerrainChunkSliceGeometry(bool SubsetsIsSame=false)
		{
			m_subsetsIsSame = SubsetsIsSame;
		}
		public void DisposeDrawBuffer()
		{
			for (int i = 0; i < DrawBuffers.Count; i++)
			{
				DrawBuffers[i].Dispose();
			}
			DrawBuffers.Clear();
		}
		public void GetDrawBuffers(DynamicArray<DrawBuffer> drawBuffers)
		{
			if (ShouldDisposeDrawBuffer)
			{
				DisposeDrawBuffer();
				foreach (var c in Geometries)
				{
					if (CompileVertexAndIndex(c.Value.Subsets, c.Key, out DrawBuffer drawBuffer2))
					{
						drawBuffers.Add(drawBuffer2);
					}
				}
				ShouldDisposeDrawBuffer = false;
			}
		}

		public void CreateDefaultGeometry() {
			Texture2D texture = GameManager.Project.FindSubsystem<SubsystemAnimatedTextures>().AnimatedBlocksTexture;
			Default = GetGeomtry(texture);
		}

		public override TerrainGeometry GetGeomtry(Texture2D texture = null)
		{
			if (!Geometries.TryGetValue(texture, out TerrainGeometry geometry))
			{
				geometry = new TerrainGeometry();
				Geometries.Add(texture, geometry);
			}
			return geometry;
		}
        public override void ClearSubsets()
        {
			ShouldDisposeDrawBuffer = true;
			Geometries.Clear();
			CreateDefaultGeometry();
        }
    }
}
