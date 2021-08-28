using Engine;
using Engine.Graphics;
using System;

namespace Game
{
    public class TerrainRenderer : IDisposable
    {
        public PrimitivesRenderer3D PrimitivesRenderer = new PrimitivesRenderer3D();
        public SubsystemTerrain m_subsystemTerrain;
        public static SubsystemSky m_subsystemSky;

        public SubsystemAnimatedTextures m_subsystemAnimatedTextures;

        public static Shader OpaqueShader;

        public static Shader AlphatestedShader;

        public static Shader TransparentShader;

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
                    if (terrainChunk.Geometry.DrawBuffers.Count > 0)
                    {
                        foreach (DrawBuffer buffer in terrainChunk.DrawBuffers)
                        {
                            if (buffer == null) continue;
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
            OpaqueShader = new Shader(ModsManager.GetInPakOrStorageFile<string>("Shaders/Opaque", ".vsh"), ModsManager.GetInPakOrStorageFile<string>("Shaders/Opaque", ".psh"), new ShaderMacro[] { new ShaderMacro("Opaque") });
            AlphatestedShader = new Shader(ModsManager.GetInPakOrStorageFile<string>("Shaders/AlphaTested", ".vsh"), ModsManager.GetInPakOrStorageFile<string>("Shaders/AlphaTested", ".psh"), new ShaderMacro[] { new ShaderMacro("ALPHATESTED") });
            TransparentShader = new Shader(ModsManager.GetInPakOrStorageFile<string>("Shaders/Transparent", ".vsh"), ModsManager.GetInPakOrStorageFile<string>("Shaders/Transparent", ".psh"), new ShaderMacro[] { new ShaderMacro("Transparent") });
            Display.DeviceReset += Display_DeviceReset;
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
                    lock (terrainChunk.Geometry.DrawBuffers) {
                        SetupTerrainChunkGeometryVertexIndexBuffers(terrainChunk);
                        terrainChunk.NewGeometryData = false;
                    }
                }
                terrainChunk.DrawDistanceSquared = Vector2.DistanceSquared(xZ, terrainChunk.Center);
                if (viewFrustum.Intersection(terrainChunk.BoundingBox) && terrainChunk.DrawDistanceSquared <= num)
                {
                    m_chunksToDraw.Add(terrainChunk);
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
            Vector3 viewPosition = camera.ViewPosition;
            Vector3 v = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
            Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
            Display.BlendState = BlendState.Opaque;
            Display.DepthStencilState = DepthStencilState.Default;
            Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
            OpaqueShader.GetParameter("u_origin").SetValue(v.XZ);
            OpaqueShader.GetParameter("u_viewProjectionMatrix").SetValue(value);
            OpaqueShader.GetParameter("u_viewPosition").SetValue(viewPosition);
            OpaqueShader.GetParameter("u_samplerState").SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            OpaqueShader.GetParameter("u_fogYMultiplier").SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
            OpaqueShader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
            ModsManager.HookAction("SetShaderParameter", modLoader => {
                modLoader.SetShaderParameter(OpaqueShader);
                return false;
            });
            for (int i = 0; i < m_chunksToDraw.Count; i++)
            {
                TerrainChunk terrainChunk = m_chunksToDraw[i];
                DrawTerrainChunkGeometrySubsets(OpaqueShader, terrainChunk, CalculateSubsetsMask(terrainChunk, m_subsystemSky, camera, OpaqueShader));
                ChunksDrawn++;
            }
        }

        public static int CalculateSubsetsMask(TerrainChunk terrainChunk,SubsystemSky m_subsystemSky,Camera camera,Shader shader) {
            int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
            float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
            float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
            shader.GetParameter("u_fogStartInvLength").SetValue(new Vector2(num2, 1f / (num - num2)));
            int num3 = 16;
            if (camera.ViewPosition.Z > terrainChunk.BoundingBox.Min.Z)
            {
                num3 |= 1;
            }
            if (camera.ViewPosition.X > terrainChunk.BoundingBox.Min.X)
            {
                num3 |= 2;
            }
            if (camera.ViewPosition.Z < terrainChunk.BoundingBox.Max.Z)
            {
                num3 |= 4;
            }
            if (camera.ViewPosition.X < terrainChunk.BoundingBox.Max.X)
            {
                num3 |= 8;
            }
            return num3;
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
            AlphatestedShader.GetParameter("u_origin").SetValue(v.XZ);
            AlphatestedShader.GetParameter("u_viewProjectionMatrix").SetValue(value);
            AlphatestedShader.GetParameter("u_viewPosition").SetValue(viewPosition);
            AlphatestedShader.GetParameter("u_samplerState").SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            AlphatestedShader.GetParameter("u_fogYMultiplier").SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
            AlphatestedShader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
            ShaderParameter parameter = AlphatestedShader.GetParameter("u_fogStartInvLength");
            ModsManager.HookAction("SetShaderParameter", modLoader => {
                modLoader.SetShaderParameter(AlphatestedShader);
                return false;
            });

            for (int i = 0; i < m_chunksToDraw.Count; i++)
            {
                TerrainChunk terrainChunk = m_chunksToDraw[i];
                float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
                float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
                parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
                DrawTerrainChunkGeometrySubsets(AlphatestedShader, terrainChunk, 32);
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
            TransparentShader.GetParameter("u_origin").SetValue(v.XZ);
            TransparentShader.GetParameter("u_viewProjectionMatrix").SetValue(value);
            TransparentShader.GetParameter("u_viewPosition").SetValue(viewPosition);
            TransparentShader.GetParameter("u_samplerState").SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            TransparentShader.GetParameter("u_fogYMultiplier").SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
            TransparentShader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
            ModsManager.HookAction("SetShaderParameter", modLoader => {
                modLoader.SetShaderParameter(TransparentShader);
                return false;
            });
            ShaderParameter parameter = TransparentShader.GetParameter("u_fogStartInvLength");
            for (int i = 0; i < m_chunksToDraw.Count; i++)
            {
                TerrainChunk terrainChunk = m_chunksToDraw[i];
                float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
                float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
                parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
                DrawTerrainChunkGeometrySubsets(TransparentShader, terrainChunk, 64);
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
                terrainChunk.DisposeDrawBuffers();
            }
        }


        public void SetupTerrainChunkGeometryVertexIndexBuffers(TerrainChunk chunk)
        {
            /*
            for (int i = 0; i < chunk.Slices.Length; i++)
            {
                if (chunk.Change[i])
                {
                    //重新创建DrawBuffer
                    TerrainChunkSliceGeometry sliceGeometry = chunk.Slices[i];
                    int VerticesCount = 0;
                    int IndicesCount = 0;
                    int Count = 0;
                    for (int j = 0; j < sliceGeometry.Subsets.Length; j++)
                    {
                        Count += sliceGeometry.Subsets[j].Indices.Count;
                    }
                    if (Count == 0) continue;
                    DrawBuffer buffer = new DrawBuffer(VerticesCount, IndicesCount);
                    for (int j = 0; j < sliceGeometry.Subsets.Length; j++)
                    {
                        buffer.VertexBuffer.SetData(sliceGeometry.Subsets[j].Vertices.Array, 0, sliceGeometry.Subsets[j].Vertices.Count, VerticesCount);
                        buffer.IndexBuffer.SetData(sliceGeometry.Subsets[j].Indices.Array, 0, sliceGeometry.Subsets[j].Indices.Count, IndicesCount);
                        buffer.SubsetIndexBufferStarts[j] = IndicesCount;
                        VerticesCount += sliceGeometry.Subsets[j].Vertices.Count;
                        IndicesCount += sliceGeometry.Subsets[j].Indices.Count;
                        buffer.SubsetIndexBufferEnds[j] = IndicesCount;
                    }
                }
                else if (chunk.DrawBuffers[i] != null)
                {
                    chunk.DrawBuffers[i].Dispose();
                    chunk.DrawBuffers[i] = null;
                }
            }
            */
        }

        public static void DrawTerrainChunkGeometrySubsets(Shader shader,TerrainChunk chunk, int subsetsMask)
        {
            for (int i = 0; i < chunk.DrawBuffers.Length; i++)
            {
                if (chunk.DrawBuffers[i] != null) DrawTerrainChunkGeometrySubset(shader, chunk.DrawBuffers[i], subsetsMask);
            }
        }

        public static void DrawTerrainChunkGeometrySubset(Shader shader, DrawBuffer buffer, int subsetsMask) {
            shader.GetParameter("u_texture").SetValue(buffer.Texture);
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
                        Display.DrawIndexed(PrimitiveType.TriangleList, shader, buffer.VertexBuffer, buffer.IndexBuffer, num, num2 - num);
                        ChunkTrianglesDrawn += (num2 - num) / 3;
                        ChunkDrawCalls++;
                    }
                    num = 2147483647;
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

