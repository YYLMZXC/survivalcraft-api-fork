using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
namespace Game
{

	public class TerrainRenderer : IDisposable
	{
		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemSky m_subsystemSky;

		public SubsystemAnimatedTextures m_subsystemAnimatedTextures;

		public static Shader OpaqueShader;

		public static Shader AlphatestedShader;

		public static Shader TransparentShader;

		public SamplerState m_samplerState = new()
		{
			AddressModeU = TextureAddressMode.Clamp,
			AddressModeV = TextureAddressMode.Clamp,
			FilterMode = TextureFilterMode.Point,
			MaxLod = 0f
		};

		public SamplerState m_samplerStateMips = new()
		{
			AddressModeU = TextureAddressMode.Clamp,
			AddressModeV = TextureAddressMode.Clamp,
			FilterMode = TextureFilterMode.PointMipLinear,
			MaxLod = 4f
		};

		public DynamicArray<TerrainChunk> m_chunksToDraw = [];

		public static DynamicArray<int> m_tmpIndices = [];

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
							num += buffer.VertexBuffer?.GetGpuMemoryUsage() ?? 0;
							num += buffer.IndexBuffer?.GetGpuMemoryUsage() ?? 0;
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
			if (OpaqueShader == null) OpaqueShader = new Shader(ShaderCodeManager.GetFast("Shaders/Opaque.vsh"), ShaderCodeManager.GetFast("Shaders/Opaque.psh"), new ShaderMacro[] { new("Opaque") });
			if (AlphatestedShader == null) AlphatestedShader = new Shader(ShaderCodeManager.GetFast("Shaders/AlphaTested.vsh"), ShaderCodeManager.GetFast("Shaders/AlphaTested.psh"), new ShaderMacro[] { new("ALPHATESTED") });
			if (TransparentShader == null) TransparentShader = new Shader(ShaderCodeManager.GetFast("Shaders/Transparent.vsh"), ShaderCodeManager.GetFast("Shaders/Transparent.psh"), new ShaderMacro[] { new("Transparent") });
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
					lock (terrainChunk.Geometry)
					{
						if (terrainChunk.NewGeometryData)
						{
							terrainChunk.NewGeometryData = false;
							SetupTerrainChunkGeometryVertexIndexBuffers(terrainChunk);
						}
					}
				}
				if (terrainChunk.Geometry.Buffers.Count > 0 && (double)Vector2.DistanceSquared(xZ, terrainChunk.Center) <= (double)num)
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
			Vector3 viewPosition = camera.InvertedViewMatrix.Translation;
			Vector3 v = new(MathF.Floor(viewPosition.X), 0f, MathF.Floor(viewPosition.Z));
			Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
			Display.BlendState = BlendState.Opaque;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
			OpaqueShader.GetParameter("u_origin", true).SetValue(v.XZ);
			OpaqueShader.GetParameter("u_viewProjectionMatrix", true).SetValue(value);
			OpaqueShader.GetParameter("u_viewPosition", true).SetValue(viewPosition);
            OpaqueShader.GetParameter("u_texture").SetValue(this.m_subsystemAnimatedTextures.AnimatedBlocksTexture);
            OpaqueShader.GetParameter("u_samplerState", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
			OpaqueShader.GetParameter("u_fogYMultiplier", true).SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
			OpaqueShader.GetParameter("u_fogColor", true).SetValue(new Vector3(m_subsystemSky.ViewFogColor));
			ShaderParameter parameter = OpaqueShader.GetParameter("u_fogStartInvLength", true);
			ModsManager.HookAction("SetShaderParameter", (modLoader) => { modLoader.SetShaderParameter(OpaqueShader, camera); return true; });
			Point2 point = Terrain.ToChunk(camera.ViewPosition.XZ);
			var chunk = m_subsystemTerrain.Terrain.GetChunkAtCoords(point.X, point.Y);
			for (int i = 0; i < m_chunksToDraw.Count; i++)
			{
				TerrainChunk terrainChunk = m_chunksToDraw[i];
				float num = MathF.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
				float num2 = MathF.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
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
				DrawTerrainChunkGeometrySubsets(OpaqueShader, terrainChunk.Geometry, num3);
				ChunksDrawn++;
			}
		}

		public void DrawAlphaTested(Camera camera)
		{
			int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
			Vector3 viewPosition = camera.InvertedViewMatrix.Translation;
			Vector3 v = new(MathF.Floor(viewPosition.X), 0f, MathF.Floor(viewPosition.Z));
			Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
			Display.BlendState = BlendState.Opaque;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
			AlphatestedShader.GetParameter("u_origin", true).SetValue(v.XZ);
			AlphatestedShader.GetParameter("u_viewProjectionMatrix", true).SetValue(value);
			AlphatestedShader.GetParameter("u_viewPosition", true).SetValue(viewPosition);
            AlphatestedShader.GetParameter("u_texture").SetValue(this.m_subsystemAnimatedTextures.AnimatedBlocksTexture);
			AlphatestedShader.GetParameter("u_samplerState", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
			AlphatestedShader.GetParameter("u_fogYMultiplier", true).SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
			AlphatestedShader.GetParameter("u_fogColor", true).SetValue(new Vector3(m_subsystemSky.ViewFogColor));
			ShaderParameter parameter = AlphatestedShader.GetParameter("u_fogStartInvLength", true);
			ModsManager.HookAction("SetShaderParameter", (modLoader) => { modLoader.SetShaderParameter(AlphatestedShader, camera); return true; });
			for (int i = 0; i < m_chunksToDraw.Count; i++)
			{
				TerrainChunk terrainChunk = m_chunksToDraw[i];
				float num = MathF.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
				float num2 = MathF.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
				parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
				int subsetsMask = 32;
				DrawTerrainChunkGeometrySubsets(AlphatestedShader, terrainChunk.Geometry, subsetsMask);
			}
		}

		public void DrawTransparent(Camera camera)
		{
			int gameWidgetIndex = camera.GameWidget.GameWidgetIndex;
			Vector3 viewPosition = camera.InvertedViewMatrix.Translation;
			Vector3 v = new(MathF.Floor(viewPosition.X), 0f, MathF.Floor(viewPosition.Z));
			Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
			Display.BlendState = BlendState.AlphaBlend;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = (m_subsystemSky.ViewUnderWaterDepth > 0f) ? RasterizerState.CullClockwiseScissor : RasterizerState.CullCounterClockwiseScissor;
			TransparentShader.GetParameter("u_origin", true).SetValue(v.XZ);
			TransparentShader.GetParameter("u_viewProjectionMatrix", true).SetValue(value);
			TransparentShader.GetParameter("u_viewPosition", true).SetValue(viewPosition);
            TransparentShader.GetParameter("u_texture").SetValue(this.m_subsystemAnimatedTextures.AnimatedBlocksTexture);
            TransparentShader.GetParameter("u_samplerState", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
			TransparentShader.GetParameter("u_fogYMultiplier", true).SetValue(m_subsystemSky.VisibilityRangeYMultiplier);
			TransparentShader.GetParameter("u_fogColor", true).SetValue(new Vector3(m_subsystemSky.ViewFogColor));
			ShaderParameter parameter = TransparentShader.GetParameter("u_fogStartInvLength", true);
			ModsManager.HookAction("SetShaderParameter", (modLoader) => { modLoader.SetShaderParameter(TransparentShader, camera); return true; });
			for (int i = 0; i < m_chunksToDraw.Count; i++)
			{
				TerrainChunk terrainChunk = m_chunksToDraw[i];
				float num = MathF.Min(terrainChunk.FogEnds[gameWidgetIndex], m_subsystemSky.ViewFogRange.Y);
				float num2 = MathF.Min(m_subsystemSky.ViewFogRange.X, num - 1f);
				parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
				int subsetsMask = 64;
				DrawTerrainChunkGeometrySubsets(TransparentShader, terrainChunk.Geometry, subsetsMask);
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
                terrainChunk.Geometry.DisposeVertexIndexBuffers();
            }
		}

		public void SetupTerrainChunkGeometryVertexIndexBuffers(TerrainChunk chunk)
		{
            TerrainChunkGeometry geometry = chunk.Geometry;
            geometry.DisposeVertexIndexBuffers();
            int num1;
            for (int index1 = 0; index1 < 112; index1 = num1)
            {
                int verticesCount = 0;
                int indicesCount = 0;
                for (num1 = index1; num1 < 112; ++num1)
                {
                    int index2 = num1 / 16;
                    int index3 = num1 % 16;
                    TerrainGeometrySubset subset = geometry.Slices[index3].Subsets[index2];
                    if (num1 <= index1)
                    {
                        verticesCount += subset.Vertices.Count;
                        indicesCount += subset.Indices.Count;
                    }
                    else
                        break;
                }
                //if (verticesCount > (int)ushort.MaxValue)
                //    Log.Warning("Max vertices count exceeded around ({0},{1},{2}), geometry will be corrupted ({3}/{4} vertices).", (object)chunk.Origin.X, (object)(num1 % 16 * 16), (object)chunk.Origin.Y, (object)verticesCount, (object)(int)ushort.MaxValue);
                if (verticesCount > 0 && indicesCount > 0)
                {
                    TerrainChunkGeometry.Buffer buffer = new TerrainChunkGeometry.Buffer();
                    geometry.Buffers.Add(buffer);
                    buffer.VertexBuffer = new VertexBuffer(TerrainVertex.VertexDeclaration, verticesCount);
                    buffer.IndexBuffer = new IndexBuffer(IndexFormat.ThirtyTwoBits, indicesCount);
                    int num2 = 0;
                    int targetStartIndex = 0;
                    for (int index4 = index1; index4 < num1; ++index4)
                    {
                        int index5 = index4 / 16;
                        int index6 = index4 % 16;
                        TerrainGeometrySubset subset = geometry.Slices[index6].Subsets[index5];
                        if (index6 == 0 || index4 == index1)
                            buffer.SubsetIndexBufferStarts[index5] = targetStartIndex;
                        if (subset.Indices.Count > 0)
                        {
                            TerrainRenderer.m_tmpIndices.Count = subset.Indices.Count;
                            TerrainRenderer.ShiftIndices(subset.Indices.Array, TerrainRenderer.m_tmpIndices.Array, num2, subset.Indices.Count);
                            buffer.IndexBuffer.SetData<int>(TerrainRenderer.m_tmpIndices.Array, 0, TerrainRenderer.m_tmpIndices.Count, targetStartIndex);
                            targetStartIndex += TerrainRenderer.m_tmpIndices.Count;
                        }
                        if (subset.Vertices.Count > 0)
                        {
                            buffer.VertexBuffer.SetData<TerrainVertex>(subset.Vertices.Array, 0, subset.Vertices.Count, num2);
                            num2 += subset.Vertices.Count;
                        }
                        if (index6 == 15 || index4 == num1 - 1)
                            buffer.SubsetIndexBufferEnds[index5] = targetStartIndex;
                    }
                }
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
							Display.DrawIndexed(PrimitiveType.TriangleList, shader, buffer.VertexBuffer, buffer.IndexBuffer, num, num2 - num);
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
			Vector2 v = new(chunk.Origin.X, chunk.Origin.Y);
			Vector2 v2 = new(chunk.Origin.X + 16, chunk.Origin.Y);
			Vector2 v3 = new(chunk.Origin.X, chunk.Origin.Y + 16);
			Vector2 v4 = new(chunk.Origin.X + 16, chunk.Origin.Y + 16);
			float x = Vector2.Distance(viewPosition.XZ, v);
			float x2 = Vector2.Distance(viewPosition.XZ, v2);
			float x3 = Vector2.Distance(viewPosition.XZ, v3);
			float x4 = Vector2.Distance(viewPosition.XZ, v4);
			chunk.FogEnds[camera.GameWidget.GameWidgetIndex] = MathF.Max(Math.Min(Math.Min(Math.Min(x, x2), x3), x4), 0.001f);
		}

		public void RunChunkFadeIn(Camera camera, TerrainChunk chunk)
		{
			chunk.FogEnds[camera.GameWidget.GameWidgetIndex] += 32f * Time.FrameDuration;
			if (chunk.FogEnds[camera.GameWidget.GameWidgetIndex] >= m_subsystemSky.ViewFogRange.Y)
			{
				chunk.FogEnds[camera.GameWidget.GameWidgetIndex] = 3.40282347E+38f;
			}
		}

		public static void ShiftIndices(int[] source, int[] destination, int shift, int count)
		{
			for (int i = 0; i < count; i++)
			{
				destination[i] = source[i] + shift;
			}
		}
	}
}
