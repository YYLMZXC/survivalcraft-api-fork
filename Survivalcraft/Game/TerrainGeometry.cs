using Engine;
using System.Collections.Generic;
using Engine.Graphics;
using System;

namespace Game
{
    public class TerrainGeometry
    {
        public class DrawBuffer : IDisposable
        {
            public VertexBuffer VertexBuffer;

            public IndexBuffer IndexBuffer;

            public Texture2D Texture;

            public int[] SubsetIndexBufferStarts = new int[7];

            public int[] SubsetIndexBufferEnds = new int[7];

            public void Dispose()
            {
                Utilities.Dispose(ref VertexBuffer);
                Utilities.Dispose(ref IndexBuffer);
            }
        }

        public TerrainGeometrySubset SubsetOpaque
        {
            get
            {
                return DefaultGeometry.SubsetOpaque;
            }
            set
            {
                DefaultGeometry.SubsetOpaque = value;
            }
        }

        public TerrainGeometrySubset SubsetAlphaTest
        {
            get
            {
                return DefaultGeometry.SubsetAlphaTest;
            }
            set
            {
                DefaultGeometry.SubsetAlphaTest = value;
            }
        }

        public TerrainGeometrySubset SubsetTransparent
        {
            get
            {
                return DefaultGeometry.SubsetTransparent;
            }

            set
            {
                DefaultGeometry.SubsetTransparent = value;
            }
        }

        public TerrainGeometrySubset[] OpaqueSubsetsByFace
        {
            get
            {
                return DefaultGeometry.OpaqueSubsetsByFace;
            }
            set
            {
                DefaultGeometry.OpaqueSubsetsByFace = value;
            }
        }

        public TerrainGeometrySubset[] AlphaTestSubsetsByFace
        {
            get
            {
                return DefaultGeometry.AlphaTestSubsetsByFace;
            }
            set
            {
                DefaultGeometry.AlphaTestSubsetsByFace = value;
            }
        }

        public TerrainGeometrySubset[] TransparentSubsetsByFace
        {
            get
            {
                return DefaultGeometry.TransparentSubsetsByFace;
            }
            set
            {
                DefaultGeometry.TransparentSubsetsByFace = value;
            }
        }

        public Dictionary<Texture2D, TerrainChunkSliceGeometry> GeometrySubsets = new Dictionary<Texture2D, TerrainChunkSliceGeometry>();

        public TerrainChunkSliceGeometry DefaultGeometry;

        public bool AllInOne = false;

        public TerrainGeometry(bool AllInOne=false)
        {
            this.AllInOne = AllInOne;
        }

        public DynamicArray<DrawBuffer> DrawBuffers = new DynamicArray<DrawBuffer>();

        public bool CompileVertexAndIndex(TerrainGeometrySubset[] geometry, Texture2D texture, out DrawBuffer buffer)
        {
            int IndicesPosition = 0;
            int VerticesPosition = 0;
            int IndicesCount = 0;
            int VerticesCount = 0;
            buffer = new DrawBuffer();
            buffer.Texture = texture;
            for (int i = 0; i < geometry.Length; i++)
            {
                VerticesCount += geometry[i].Vertices.Count;
                IndicesCount += geometry[i].Indices.Count;
                if (AllInOne) break;
            }
            if (IndicesCount == 0) return false;
            buffer.VertexBuffer = new VertexBuffer(TerrainVertex.VertexDeclaration, VerticesCount);
            buffer.IndexBuffer = new IndexBuffer(IndexFormat.SixteenBits, IndicesCount);
            for (int i = 0; i < geometry.Length; i++)
            {
                if (IndicesPosition > 0)
                {
                    for (int j = 0; j < geometry[i].Indices.Count; j+=3)
                    {
                        TerrainGeometrySubset subset = geometry[i];
                        /*
                        Vector3 v1 = new Vector3(subset.Vertices[subset.Indices[j]].X, subset.Vertices[subset.Indices[j]].Y, subset.Vertices[subset.Indices[j]].Z);
                        Vector3 v2 = new Vector3(subset.Vertices[subset.Indices[j+1]].X, subset.Vertices[subset.Indices[j+1]].Y, subset.Vertices[subset.Indices[j+1]].Z);
                        Vector3 v3 = new Vector3(subset.Vertices[subset.Indices[j+2]].X, subset.Vertices[subset.Indices[j+2]].Y, subset.Vertices[subset.Indices[j+2]].Z);
                        //������ķ���
                        Vector3 normal = ModsManager.Cal_Normal_3D(v1, v2, v3);
                        TerrainVertex vertex1 = subset.Vertices[subset.Indices[j]];
                        TerrainVertex vertex2 = subset.Vertices[subset.Indices[j+1]];
                        TerrainVertex vertex3 = subset.Vertices[subset.Indices[j+2]];
                        vertex1.Normal = normal;
                        vertex2.Normal = normal;
                        vertex3.Normal = normal;
                        subset.Vertices[subset.Indices[j]] = vertex1;
                        subset.Vertices[subset.Indices[j + 1]] = vertex2;
                        subset.Vertices[subset.Indices[j + 2]] = vertex3;
                        */
                        //�����޸�Indices
                        subset.Indices[j] += (ushort)(VerticesPosition);
                        subset.Indices[j + 1] += (ushort)(VerticesPosition);
                        subset.Indices[j + 2] += (ushort)(VerticesPosition);

                    }

                }
                buffer.VertexBuffer.SetData(geometry[i].Vertices.Array, 0, geometry[i].Vertices.Count, VerticesPosition);
                buffer.IndexBuffer.SetData(geometry[i].Indices.Array, 0, geometry[i].Indices.Count, IndicesPosition);
                buffer.SubsetIndexBufferStarts[i] = IndicesPosition;
                buffer.SubsetIndexBufferEnds[i] = IndicesPosition + geometry[i].Indices.Count;
                VerticesPosition += geometry[i].Vertices.Count;
                IndicesPosition += geometry[i].Indices.Count;
                if (AllInOne) break;
            }
            buffer.Texture = texture;
            return true;
        }

        public void Compile() {
            Dispose();
            foreach (var item in GeometrySubsets)
            {
                if (CompileVertexAndIndex(item.Value.Subsets, item.Key, out DrawBuffer buffer))
                {
                    DrawBuffers.Add(buffer);
                }
            }
        }

        public void ClearGeometry() {
            foreach (var item in GeometrySubsets) {
                for (int i=0;i<item.Value.Subsets.Length;i++) {
                    item.Value.Subsets[i].Vertices.Clear();
                    item.Value.Subsets[i].Indices.Clear();
                }
            }
        }

        public void CreateDefalutGeometry(Texture2D texture)
        {
            DefaultGeometry = GetGeometry(texture);
        }

        public TerrainChunkSliceGeometry GetGeometry(Texture2D texture=null)
        {
            if (GeometrySubsets.TryGetValue(texture, out TerrainChunkSliceGeometry subset) == false)
            {
                subset = new TerrainChunkSliceGeometry(AllInOne);
                GeometrySubsets.Add(texture, subset);
            }
            return subset;
        }

        public void Dispose()
        {
            for (int i = 0; i < DrawBuffers.Count; i++)
            {
                DrawBuffers[i].Dispose();
            }
            DrawBuffers.Clear();
        }
    }
}
