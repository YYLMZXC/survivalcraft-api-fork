using Engine;
using Engine.Graphics;
using System;

namespace Game
{
    public class TerrainRenderer : IDisposable
    {
        public PrimitivesRenderer3D PrimitivesRenderer = new PrimitivesRenderer3D();
        public SubsystemTerrain m_subsystemTerrain;
        public RenderTarget2D RenderTarget;

        public static SubsystemSky m_subsystemSky;

        public SubsystemAnimatedTextures m_subsystemAnimatedTextures;

        public static Shader OpaqueShader;

        public static Shader ShadowShader;

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
                        foreach (TerrainGeometry.DrawBuffer buffer in terrainChunk.Geometry.DrawBuffers)
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
            OpaqueShader=new OpaqueShader();
            ShadowShader = new Shader(ModsManager.GetInPakOrStorageFile("Shaders/shadow",".vsh"), ModsManager.GetInPakOrStorageFile("Shaders/shadow",".psh"), new ShaderMacro[] { new ShaderMacro("ShadowShader") });
            AlphatestedShader = new AlphaTestedShader();
            TransparentShader = new TransparentShader();
            Display.DeviceReset += Display_DeviceReset;
            RenderTarget = new RenderTarget2D(Display.Viewport.Width,Display.Viewport.Height,1,ColorFormat.Rgba8888,DepthFormat.Depth16);
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
                    lock (terrainChunk.Geometry.DrawBuffers)
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
            Display.BlendState = BlendState.Opaque;
            Display.DepthStencilState = DepthStencilState.Default;
            Matrix LightViewMatrix = Matrix.CreateLookAt(m_subsystemSky.LightPosition, m_subsystemSky.LightPosition + m_subsystemSky.LightDirection, Vector3.UnitY);
            Matrix LightMatrix = LightViewMatrix * FppCamera.CalculateBaseProjectionMatrix(camera.GameWidget.ViewWidget);
            #region 开始绘制参照纹理            
            ShadowShader.GetParameter("u_texture").SetValue(RenderTarget);
            //ShadowShader.GetParameter("viewsize").SetValue(new Vector2(Display.Viewport.Width,Display.Viewport.Height));
            ShadowShader.GetParameter("LightPosition").SetValue(m_subsystemSky.LightPosition);
            ShadowShader.GetParameter("ViewProjectionMatrix").SetValue(LightMatrix);
            ShadowShader.GetParameter("u_samplerState").SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            RenderTarget2D target2D = Display.RenderTarget;
            Display.RenderTarget = RenderTarget;
            Display.Clear(Color.White,1f);
            for (int i = 0; i < m_chunksToDraw.Count; i++)
            {
                TerrainChunk terrainChunk = m_chunksToDraw[i];
                DrawTerrainChunkGeometrySubsets(ShadowShader, terrainChunk.Geometry.DrawBuffers, CalculateSubsetsMask(terrainChunk, m_subsystemSky, camera, OpaqueShader));
                DrawTerrainChunkGeometrySubsets(ShadowShader, terrainChunk.Geometry.DrawBuffers, 32);//绘制Alphatest
                DrawTerrainChunkGeometrySubsets(ShadowShader, terrainChunk.Geometry.DrawBuffers, 64);//绘制Transparent
            }
            Display.RenderTarget = target2D;
            if (Time.PeriodicEvent(5.0, 4.0)) ModsManager.SaveToImage("shadow", RenderTarget);
            #endregion
            #region 开始正常绘制
            //OpaqueShader.GetParameter("viewsize").SetValue(new Vector2(Display.Viewport.Width,Display.Viewport.Height));
            OpaqueShader.GetParameter("LightPosition").SetValue(m_subsystemSky.LightPosition);
            OpaqueShader.GetParameter("LightMatrix").SetValue(LightMatrix);
            OpaqueShader.GetParameter("ViewProjectionMatrix").SetValue(camera.ViewProjectionMatrix);
            OpaqueShader.GetParameter("s_samplerState").SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            OpaqueShader.GetParameter("u_samplerState").SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            OpaqueShader.GetParameter("ShadowTexture").SetValue(RenderTarget);
            for (int i = 0; i < m_chunksToDraw.Count; i++)
            {
                TerrainChunk terrainChunk = m_chunksToDraw[i];
                DrawTerrainChunkGeometrySubsets(OpaqueShader, terrainChunk.Geometry.DrawBuffers, CalculateSubsetsMask(terrainChunk, m_subsystemSky, camera, OpaqueShader));
                DrawTerrainChunkGeometrySubsets(OpaqueShader, terrainChunk.Geometry.DrawBuffers, 32);//绘制Alphatest
                DrawTerrainChunkGeometrySubsets(OpaqueShader, terrainChunk.Geometry.DrawBuffers, 64);//绘制Transparent
                ChunksDrawn++;
            }
            #endregion
            ModsManager.HookAction("SetShaderParameter", modLoader => {
                modLoader.SetShaderParameter(OpaqueShader);
                return false; 
            });
        }

        public static int CalculateSubsetsMask(TerrainChunk terrainChunk,SubsystemSky m_subsystemSky,Camera camera,Shader shader) {
            int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
            float num = MathUtils.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
            float num2 = MathUtils.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
            //shader.GetParameter("u_fogStartInvLength").SetValue(new Vector2(num2, 1f / (num - num2)));
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
                DrawTerrainChunkGeometrySubsets(AlphatestedShader, terrainChunk.Geometry.DrawBuffers, 32);
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
            TransparentShader.GetParameter("u_texture").SetValue(m_subsystemAnimatedTextures.AnimatedBlocksTexture);
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
                DrawTerrainChunkGeometrySubsets(TransparentShader, terrainChunk.Geometry.DrawBuffers, 64);
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
                terrainChunk.Geometry.Dispose();
            }
        }


        public void SetupTerrainChunkGeometryVertexIndexBuffers(TerrainChunk chunk)
        {
            chunk.Geometry.Compile();

        }

        public static void DrawTerrainChunkGeometrySubsets(Shader shader, DynamicArray<TerrainGeometry.DrawBuffer> geometries, int subsetsMask)
        {
            foreach (TerrainGeometry.DrawBuffer buffer in geometries)
            {
                DrawTerrainChunkGeometrySubset(shader,buffer,subsetsMask);
            }
        }

        public static void DrawTerrainChunkGeometrySubset(Shader shader, TerrainGeometry.DrawBuffer buffer, int subsetsMask) {
            if(shader!=ShadowShader)shader.GetParameter("u_texture").SetValue(buffer.Texture);
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

