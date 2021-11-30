using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;
using System.Collections.Generic;
namespace Game
{
	public class ComponentDiggingCracks : Component, IDrawable
	{
		public class Geometry : TerrainGeometry
		{
			public Geometry()
			{
				TerrainGeometrySubset terrainGeometrySubset = SubsetTransparent = (SubsetAlphaTest = (SubsetOpaque = new TerrainGeometrySubset()));
				OpaqueSubsetsByFace = new TerrainGeometrySubset[6]
				{
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset
				};
				AlphaTestSubsetsByFace = new TerrainGeometrySubset[6]
				{
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset
				};
				TransparentSubsetsByFace = new TerrainGeometrySubset[6]
				{
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset,
					terrainGeometrySubset
				};
			}
		}

		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemSky m_subsystemSky;

		public ComponentMiner m_componentMiner;

		public Texture2D[] m_textures;

		public Shader m_shader;

		public Geometry m_geometry;

		public Point3 m_point;

		public int m_value;

		public static int[] m_drawOrders = new int[1]
		{
			1
		};

		public int[] DrawOrders => m_drawOrders;

		public TerrainChunkGeometry.Buffer Buffer = null;

		public int textureSlotCount;

		public int textureSlotSize;

		public Dictionary<int, RenderTarget2D[]> CrackTextures = new Dictionary<int, RenderTarget2D[]>();

		public void Draw(Camera camera, int drawOrder)
		{
			if (!m_componentMiner.DigCellFace.HasValue || !(m_componentMiner.DigProgress > 0f) || !(m_componentMiner.DigTime > 0.2f))
			{
				return;
			}
			Point3 point = m_componentMiner.DigCellFace.Value.Point;
			int cellValue = m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z);
			int num = Terrain.ExtractContents(cellValue);
			Block block = BlocksManager.Blocks[num];
			if (m_geometry == null || cellValue != m_value || point != m_point)
			{
				m_geometry = new Geometry();
				m_geometry.ClearSubsets(Project.FindSubsystem<SubsystemAnimatedTextures>());
				block.GenerateTerrainVertices(m_subsystemTerrain.BlockGeometryGenerator, m_geometry, cellValue, point.X, point.Y, point.Z);
				textureSlotCount = block.GetTextureSlotCount(cellValue);
				textureSlotSize = 32 * textureSlotCount;
				m_point = point;
				m_value = cellValue;
				DynamicArray<TerrainVertex> vertices = new DynamicArray<TerrainVertex>();
				DynamicArray<ushort> indices = new DynamicArray<ushort>();
				foreach (var c in m_geometry.Draws)
				{
					for (int i = 0; i < c.Value.Subsets.Length; i++)
					{
						TerrainGeometrySubset subset = c.Value.Subsets[i];
						if (subset.Indices.Count > 0)
						{
							for (int j = 0; j < subset.Indices.Count; j++)
							{
								indices.Add((ushort)(subset.Indices[j] + vertices.Count));
							}
							for (int j = 0; j < subset.Vertices.Count; j++)
							{
								TerrainVertex vertex = subset.Vertices[j];
								byte b = (byte)((vertex.Color.R + vertex.Color.G + vertex.Color.B) / 3);
								vertex.Color = new Color(b, b, b, (byte)128);
								vertices.Add(vertex);
							}
						}
					}
				}
				Buffer?.Dispose();
				Buffer = new TerrainChunkGeometry.Buffer();
				Buffer.IndexBuffer = new IndexBuffer(IndexFormat.SixteenBits, indices.Count);
				Buffer.VertexBuffer = new VertexBuffer(TerrainVertex.VertexDeclaration, vertices.Count);
				Buffer.IndexBuffer.SetData(indices.Array, 0, indices.Count);
				Buffer.VertexBuffer.SetData(vertices.Array, 0, vertices.Count);
			}
			Vector3 viewPosition = camera.ViewPosition;
			Vector3 v = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
			Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
			float x = m_subsystemSky.ViewFogRange.X;
			float y = m_subsystemSky.ViewFogRange.Y;
			int num2 = MathUtils.Clamp((int)(m_componentMiner.DigProgress * 8f), 0, 7);

			if (!CrackTextures.TryGetValue(textureSlotSize, out var list))
			{
				list = new RenderTarget2D[8];
				CrackTextures.Add(textureSlotSize,list);
			}
			if (list[num2] == null)
			{
				RenderTarget2D render = Display.RenderTarget;
				RenderTarget2D target = new RenderTarget2D(textureSlotSize, textureSlotSize, 1, ColorFormat.Rgba8888, DepthFormat.None);
				Display.RenderTarget = target;
				PrimitivesRenderer2D primitives = new PrimitivesRenderer2D();
				TexturedBatch2D texturedBatch = primitives.TexturedBatch(m_textures[num2], true);
				for (int i = 0; i < textureSlotCount; i++)
				{
					for (int j = 0; j < textureSlotCount; j++)
					{
						Vector2 s = new Vector2(i * 32, j * 32);
						Vector2 e = new Vector2((i + 1) * 32, (j + 1) * 32);
						texturedBatch.QueueQuad(s, e, 1f, Vector2.Zero, Vector2.One, Color.White);
					}
				}
				primitives.Flush();
				Display.RenderTarget = render;
				list[num2] = target;
			}
			Display.BlendState = BlendState.NonPremultiplied;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
			m_shader.GetParameter("u_origin").SetValue(v.XZ);
			m_shader.GetParameter("u_texture").SetValue(list[num2]);
			m_shader.GetParameter("u_viewProjectionMatrix").SetValue(value);
			m_shader.GetParameter("u_viewPosition").SetValue(camera.ViewPosition);
			m_shader.GetParameter("u_samplerState").SetValue(SamplerState.PointWrap);
			m_shader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
			m_shader.GetParameter("u_fogStartInvLength").SetValue(new Vector2(x, 1f / (y - x)));
			Display.DrawIndexed(PrimitiveType.TriangleList, m_shader, Buffer.VertexBuffer, Buffer.IndexBuffer, 0, Buffer.IndexBuffer.IndicesCount);
		}
		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemSky = base.Project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_componentMiner = base.Entity.FindComponent<ComponentMiner>(throwOnError: true);
			m_shader = ContentManager.Get<Shader>("Shaders/AlphaTested");
			m_textures = new Texture2D[8];
			for (int i = 0; i < 8; i++)
			{
				m_textures[i] = ContentManager.Get<Texture2D>($"Textures/Cracks{i + 1}");
			}
		}
		public override void Dispose()
		{
			foreach (var c in CrackTextures)
			{
				foreach (var r in c.Value)
				{
					if(r != null) r.Dispose();
				}
			}
		}
	}
}
