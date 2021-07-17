using Engine;
using Engine.Graphics;
using System;

namespace Game
{
    public class TerrainRenderer : IDisposable
    {
        public SubsystemTerrain m_subsystemTerrain;

        public SubsystemSky m_subsystemSky;

        public SubsystemAnimatedTextures m_subsystemAnimatedTextures;

        public Shader m_opaqueShader;

        public Shader m_alphaTestedShader;

        public Shader m_transparentShader;

        public SamplerState m_samplerState = new SamplerState
        {
            AddressModeU = TextureAddressMode.Clamp,
            AddressModeV = TextureAddressMode.Clamp,
            FilterMode = TextureFilterMode.Point,
            MaxLod = 0f
        };

        public SamplerState m_samplerStateMips = new SamplerState
        {
            AddressModeU = TextureAddressMode.Clamp,
            AddressModeV = TextureAddressMode.Clamp,
            FilterMode = TextureFilterMode.PointMipLinear,
            MaxLod = 4f
        };

        public DynamicArray<TerrainChunk> m_chunksToDraw = new DynamicArray<TerrainChunk>();

        public static DynamicArray<ushort> m_tmpIndices = new DynamicArray<ushort>();

        public static bool DrawChunksMap;

        public static int ChunksDrawn;

        public static int ChunkDrawCalls;

        public static int ChunkTrianglesDrawn;

        public string ChunksGpuMemoryUsage
        {
            get
            {
                long num = 0L;
                TerrainChunk[] allocatedChunks = m_subsystemTerrain.Terrain.AllocatedChunks;
                foreach (TerrainChunk terrainChunk in allocatedChunks)
                {
                    if (terrainChunk.Geometry != null)
                    {
                        foreach (TerrainChunkGeometry.Buffer buffer in terrainChunk.Geometry.Buffers)
                        {
                            num += (buffer.VertexBuffer?.GetGpuMemoryUsage() ?? 0);
                            num += (buffer.IndexBuffer?.GetGpuMemoryUsage() ?? 0);
                        }
                    }
                }
                return $"{num / 1024 / 1024:0.0}MB";
            }
        }

        public TerrainRenderer(SubsystemTerrain subsystemTerrain)
        {
            m_subsystemTerrain = subsystemTerrain;
            m_subsystemSky = subsystemTerrain.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
            m_subsystemAnimatedTextures = subsystemTerrain.SubsystemAnimatedTextures;
            m_opaqueShader = new OpaqueShader();
            m_alphaTestedShader = new AlphaTestedShader();
            m_transparentShader = new TransparentShader();
            Display.DeviceReset += Display_DeviceReset;
        }

        public void DisposeTerrainChunkGeometryVertexIndexBuffers(TerrainChunkGeometry geometry)
        {
            foreach (TerrainChunkGeometry.Buffer buffer in geometry.Buffers)
            {
                buffer.Dispose();
            }
            geometry.Buffers.Clear();
            geometry.InvalidateSliceContentsHashes();
        }

        public void PrepareForDrawing(Camera camera)
        {
            Vector2 xZ = camera.ViewPosition.XZ;
            float num = MathUtils.Sqr(m_subsystemSky.VisibilityRange);
            BoundingFrustum viewFrustum = camera.ViewFrustum;
            int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
            m_chunksToDraw.Clear();
            TerrainChunk[] allocatedChunks = m_subsystemTerrain.Terrain.AllocatedChunks;
            foreach (TerrainChunk terrainChunk in allocatedChunks)
            {
                if (terrainChunk.NewGeometryData)
                {
                    lock (terrainChunk.Geometry)
                    {
                        if (terrainChunk.NewGeometryData)
                        {
                            terrainChunk.NewGeometryData = false;
                            SetupTerrainChunkGeometryVertexIndexBuffers(terrainChunk);
                        }
                    }
                }
                terrainChunk.DrawDistanceSquared = Vector2.DistanceSquared(xZ, terrainChunk.Center);
                if (terrainChunk.DrawDistanceSquared <= num)
                {
                    if (viewFrustum.Intersection(terrainChunk.BoundingBox))
                    {
                        m_chunksToDraw.Add(terrainChunk);
                    }
                    if (terrainChunk.State != TerrainChunkState.Valid)
                    {
                        continue;
                    }
                    float num2 = terrainChunk.FogEnds[gameWidgetIndex];
                    if (num2 != 3.40282347E+38f)
                    {
                        if (num2 == 0f)
                        {
                            StartChunkFadeIn(camera, terrainChunk);
                        }
                        else
                        {
                            RunChunkFadeIn(camera, terrainChunk);
                        }
                    }
                }
                else
                {
                    terrainChunk.FogEnds[gameWidgetIndex] = 0f;
                }
            }
            ChunksDrawn = 0;
            ChunkDrawCalls = 0;
            ChunkTrianglesDrawn = 0;
        }

        public void DrawOpaque(Camera camera)
        {
            int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
            Vector3 viewPosition = camera.ViewPosition;
            Vector3 v = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
            Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
            Display.BlendState = BlendState.Opaque;
            Display.DepthStencilState = DepthStencilState.Default;
            Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
            m_opaqueShader.GetParameter("u_origin").SetValue(v.XZ);
            m_opaqueShader.GetParameter("u_viewProjectionMatrix").SetValue(value);
            m_opaqueShader.GetParameter("u_viewPosition").SetValue(viewPosition);
            m_opaqueShader.GetParameter("u_samplerState").SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            m_opaqueShader.GetParameter("u_fogYMultiplier").SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
            m_opaqueShader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
            ShaderParameter parameter = m_opaqueShader.GetParameter("u_fogStartInvLength");
            for (int i = 0; i < m_chunksToDraw.Count; i++)
            {
                TerrainChunk terrainChunk = m_chunksToDraw[i];
                float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
                float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
                parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
                int num3 = 16;
                if (viewPosition.Z > terrainChunk.BoundingBox.Min.Z)
                {
                    num3 |= 1;
                }
                if (viewPosition.X > terrainChunk.BoundingBox.Min.X)
                {
                    num3 |= 2;
                }
                if (viewPosition.Z < terrainChunk.BoundingBox.Max.Z)
                {
                    num3 |= 4;
                }
                if (viewPosition.X < terrainChunk.BoundingBox.Max.X)
                {
                    num3 |= 8;
                }
                DrawTerrainChunkGeometrySubsets(m_opaqueShader, terrainChunk.Geometry, num3);
                ChunksDrawn++;
            }
        }

        public void DrawAlphaTested(Camera camera)
        {
            int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
            Vector3 viewPosition = camera.ViewPosition;
            Vector3 v = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
            Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
            Display.BlendState = BlendState.Opaque;
            Display.DepthStencilState = DepthStencilState.Default;
            Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
            m_alphaTestedShader.GetParameter("u_origin").SetValue(v.XZ);
            m_alphaTestedShader.GetParameter("u_viewProjectionMatrix").SetValue(value);
            m_alphaTestedShader.GetParameter("u_viewPosition").SetValue(viewPosition);
            m_alphaTestedShader.GetParameter("u_samplerState").SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            m_alphaTestedShader.GetParameter("u_fogYMultiplier").SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
            m_alphaTestedShader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
            ShaderParameter parameter = m_alphaTestedShader.GetParameter("u_fogStartInvLength");
            for (int i = 0; i < m_chunksToDraw.Count; i++)
            {
                TerrainChunk terrainChunk = m_chunksToDraw[i];
                float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
                float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
                parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
                int subsetsMask = 32;
                DrawTerrainChunkGeometrySubsets(m_alphaTestedShader, terrainChunk.Geometry, subsetsMask);
            }
        }

        public void DrawTransparent(Camera camera)
        {
            int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
            Vector3 viewPosition = camera.ViewPosition;
            Vector3 v = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
            Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
            Display.BlendState = BlendState.AlphaBlend;
            Display.DepthStencilState = DepthStencilState.Default;
            Display.RasterizerState = ((m_subsystemSky.ViewUnderWaterDepth > 0f) ? RasterizerState.CullClockwiseScissor : RasterizerState.CullCounterClockwiseScissor);
            m_transparentShader.GetParameter("u_origin").SetValue(v.XZ);
            m_transparentShader.GetParameter("u_viewProjectionMatrix").SetValue(value);
            m_transparentShader.GetParameter("u_viewPosition").SetValue(viewPosition);
            m_transparentShader.GetParameter("u_samplerState").SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            m_transparentShader.GetParameter("u_fogYMultiplier").SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
            m_transparentShader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
            ShaderParameter parameter = m_transparentShader.GetParameter("u_fogStartInvLength");
            for (int i = 0; i < m_chunksToDraw.Count; i++)
            {
                TerrainChunk terrainChunk = m_chunksToDraw[i];
                float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
                float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
                parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
                int subsetsMask = 64;
                DrawTerrainChunkGeometrySubsets(m_transparentShader, terrainChunk.Geometry, subsetsMask);
            }
        }

        public void Dispose()
        {
            Display.DeviceReset -= Display_DeviceReset;
        }

        public void Display_DeviceReset()
        {
            m_subsystemTerrain.TerrainUpdater.DowngradeAllChunksState(TerrainChunkState.InvalidVertices1, forceGeometryRegeneration: false);
            TerrainChunk[] allocatedChunks = m_subsystemTerrain.Terrain.AllocatedChunks;
            foreach (TerrainChunk terrainChunk in allocatedChunks)
            {
                DisposeTerrainChunkGeometryVertexIndexBuffers(terrainChunk.Geometry);
            }
        }

        public bool CombileVertexAndIndex(TerrainGeometrySubset geometry, Texture2D texture, out TerrainChunkGeometry.Buffer buffer) {
            if (geometry.Indices.Count > 0) {
                buffer = new TerrainChunkGeometry.Buffer();
                buffer.Texture = texture;
                buffer.VertexBuffer = new VertexBuffer(TerrainVertex.VertexDeclaration, geometry.Vertices.Count);
                buffer.IndexBuffer = new IndexBuffer(IndexFormat.SixteenBits, geometry.Indices.Count);
                buffer.VertexBuffer.SetData(geometry.Vertices.Array, 0, geometry.Vertices.Count);
                buffer.IndexBuffer.SetData(geometry.Indices.Array, 0, geometry.Indices.Count);
                return true;
            }
            buffer = null;
            return false;
        }


        public void SetupChunkGeometryVertexIndexBuffers(TerrainChunk chunk,Texture2D texture, TerrainChunkSliceGeometry item) {
            //构建原版的Geometry
            for (int x = 0; x < item.Subsets.Length; x++)
            {
                if (CombileVertexAndIndex(item.Subsets[x], texture, out TerrainChunkGeometry.Buffer buffer))
                {
                    buffer.SubsetIndexBufferStarts[x] = 0;
                    buffer.SubsetIndexBufferEnds[x] = item.Subsets[x].Indices.Count;
                    chunk.Geometry.Buffers.Add(buffer);
                }
            }
            foreach (var itxn in item.GeometrySubsets)
            {
                SetupChunkGeometryVertexIndexBuffers(chunk, itxn.Key, itxn.Value);
            }
        }

        public void SetupTerrainChunkGeometryVertexIndexBuffers(TerrainChunk chunk)
        {
            DisposeTerrainChunkGeometryVertexIndexBuffers(chunk.Geometry);
            for (int i = 0; i < chunk.Geometry.Slices.Length; i++)
            {
                SetupChunkGeometryVertexIndexBuffers(chunk, m_subsystemAnimatedTextures.AnimatedBlocksTexture, chunk.Geometry.Slices[i]);
            }
        }

        public void DrawTerrainChunkGeometrySubsets(Shader shader, TerrainChunkGeometry geometry, int subsetsMask)
        {
            foreach (TerrainChunkGeometry.Buffer buffer in geometry.Buffers)
            {
                int num = 2147483647;
                int num2 = 0;
                for (int i = 0; i < 8; i++)
                {
                    if (i < 7 && (subsetsMask & (1 << i)) != 0)
                    {
                        if (buffer.SubsetIndexBufferEnds[i] > 0)
                        {
                            if (num == 2147483647)
                            {
                                num = buffer.SubsetIndexBufferStarts[i];
                            }
                            num2 = buffer.SubsetIndexBufferEnds[i];
                        }
                    }
                    else
                    {
                        if (num2 > num)
                        {
                            shader.GetParameter("u_texture").SetValue(buffer.Texture);
                            Display.DrawIndexed(PrimitiveType.TriangleList, shader, buffer.VertexBuffer, buffer.IndexBuffer, 0, buffer.IndexBuffer.IndicesCount);
                            ChunkTrianglesDrawn += (num2 - num) / 3;
                            ChunkDrawCalls++;
                        }
                        num = 2147483647;
                    }
                }

            }
        }
        public void StartChunkFadeIn(Camera camera, TerrainChunk chunk)
        {
            Vector3 viewPosition = camera.ViewPosition;
            Vector2 v = new Vector2(chunk.Origin.X, chunk.Origin.Y);
            Vector2 v2 = new Vector2(chunk.Origin.X + 16, chunk.Origin.Y);
            Vector2 v3 = new Vector2(chunk.Origin.X, chunk.Origin.Y + 16);
            Vector2 v4 = new Vector2(chunk.Origin.X + 16, chunk.Origin.Y + 16);
            float x = Vector2.Distance(viewPosition.XZ, v);
            float x2 = Vector2.Distance(viewPosition.XZ, v2);
            float x3 = Vector2.Distance(viewPosition.XZ, v3);
            float x4 = Vector2.Distance(viewPosition.XZ, v4);
            chunk.FogEnds[camera.GameWidget.GameWidgetIndex] = MathUtils.Max(MathUtils.Min(x, x2, x3, x4), 0.001f);
        }

        public void RunChunkFadeIn(Camera camera, TerrainChunk chunk)
        {
            chunk.FogEnds[camera.GameWidget.GameWidgetIndex] += 32f * Time.FrameDuration;
            if (chunk.FogEnds[camera.GameWidget.GameWidgetIndex] >= m_subsystemSky.ViewFogRange.Y)
            {
                chunk.FogEnds[camera.GameWidget.GameWidgetIndex] = 3.40282347E+38f;
            }
        }

        public static void ShiftIndices(ushort[] source, ushort[] destination, int shift, int count)
        {
            for (int i = 0; i < count; i++)
            {
                destination[i] = (ushort)(source[i] + shift);
            }
        }
    }
}
