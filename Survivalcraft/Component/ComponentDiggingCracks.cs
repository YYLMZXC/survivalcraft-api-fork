using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System.Collections.Generic;
using TemplatesDatabase;
namespace Game
{
	public class ComponentDiggingCracks : Component, IDrawable
	{
		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemSky m_subsystemSky;

		public ComponentMiner m_componentMiner;

		public Texture2D[] m_textures;

		public Shader m_shader;

		public ComponentDiggingCracks.Geometry m_geometry;

        private DynamicArray<ComponentDiggingCracks.CracksVertex> m_vertices = new DynamicArray<ComponentDiggingCracks.CracksVertex>();

        public Point3 m_point;

		public int m_value;

		public static int[] m_drawOrders = new int[1]
		{
			200//原版是1
		};

		public int[] DrawOrders => m_drawOrders;
		public void Draw(Camera camera, int drawOrder)
		{
			if (!m_componentMiner.DigCellFace.HasValue || !(m_componentMiner.DigProgress > 0f) || !(m_componentMiner.DigTime > 0.2f))
			{
				return;
			}
			Point3 point = m_componentMiner.DigCellFace.Value.Point;
			int cellValue = m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z);
			Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
			if (m_geometry == null || cellValue != m_value || point != m_point)
            {
                m_geometry = new ComponentDiggingCracks.Geometry();
				block.GenerateTerrainVertices(m_subsystemTerrain.BlockGeometryGenerator, m_geometry, cellValue, point.X, point.Y, point.Z);
				m_point = point;
				m_value = cellValue;
                m_vertices.Count = 0;
                foreach(TerrainVertex terrainVertex in m_geometry.SubsetOpaque.Vertices)
				{
                    byte num = (byte)(((int)terrainVertex.Color.R + (int)terrainVertex.Color.G + (int)terrainVertex.Color.B) / 3);
                    ComponentDiggingCracks.CracksVertex cracksVertex;
                    cracksVertex.X = terrainVertex.X;
                    cracksVertex.Y = terrainVertex.Y;
                    cracksVertex.Z = terrainVertex.Z;
                    cracksVertex.Tx = (float)((double)terrainVertex.Tx / (double)short.MaxValue * 16.0);
                    cracksVertex.Ty = (float)((double)terrainVertex.Ty / (double)short.MaxValue * 16.0);
                    cracksVertex.Color = new Color(num, num, num, (byte)128);
                    this.m_vertices.Add(cracksVertex);
                }
			}
			Vector3 viewPosition = camera.InvertedViewMatrix.Translation;
			Vector3 v = new(MathF.Floor(viewPosition.X), 0f, MathF.Floor(viewPosition.Z));
			Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
            DynamicArray<int> indices = m_geometry.SubsetOpaque.Indices;
			//根据外置材质生成破坏纹理
			try
			{
				Display.BlendState = BlendState.NonPremultiplied;
				Display.DepthStencilState = DepthStencilState.Default;
				Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
				m_shader.GetParameter("u_origin").SetValue(v.XZ);
				m_shader.GetParameter("u_viewProjectionMatrix").SetValue(value);
				m_shader.GetParameter("u_viewPosition").SetValue(camera.ViewPosition);
				m_shader.GetParameter("u_samplerState").SetValue(SamplerState.PointWrap);
                m_shader.GetParameter("u_fogYMultiplier").SetValue(this.m_subsystemSky.VisibilityRangeYMultiplier);
                m_shader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
				m_shader.GetParameter("u_fogBottomTopDensity").SetValue(new Vector3(m_subsystemSky.ViewFogBottom, m_subsystemSky.ViewFogTop, m_subsystemSky.ViewFogDensity));
				m_shader.GetParameter("u_hazeStartDensity").SetValue(new Vector2(m_subsystemSky.ViewHazeStart, m_subsystemSky.ViewHazeDensity));
				m_shader.GetParameter("u_alphaThreshold").SetValue(0.5f);
				m_shader.GetParameter("u_texture").SetValue(block.GetDiggingCrackingTexture(m_componentMiner, m_componentMiner.m_digProgress, cellValue, m_textures));
                Display.DrawUserIndexed<ComponentDiggingCracks.CracksVertex>(PrimitiveType.TriangleList, m_shader, ComponentDiggingCracks.CracksVertex.VertexDeclaration, m_vertices.Array, 0, m_vertices.Count, indices.Array, 0, indices.Count);
            }
			catch
			{
			}
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
		public class Geometry : TerrainGeometry
		{
			public Geometry()
			{
				TerrainGeometrySubset terrainGeometrySubset = new TerrainGeometrySubset();
				this.SubsetOpaque = terrainGeometrySubset;
				this.SubsetAlphaTest = terrainGeometrySubset;
				this.SubsetTransparent = terrainGeometrySubset;
				this.OpaqueSubsetsByFace = new TerrainGeometrySubset[6]
				{
                    terrainGeometrySubset,
                    terrainGeometrySubset,
                    terrainGeometrySubset,
                    terrainGeometrySubset,
                    terrainGeometrySubset,
					terrainGeometrySubset
                };
				this.AlphaTestSubsetsByFace = new TerrainGeometrySubset[6]
				{
                    terrainGeometrySubset,
                    terrainGeometrySubset,
                    terrainGeometrySubset,
                    terrainGeometrySubset,
                    terrainGeometrySubset,
                    terrainGeometrySubset
                };
				this.TransparentSubsetsByFace = new TerrainGeometrySubset[6]
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
        private struct CracksVertex
        {
            public float X;
            public float Y;
            public float Z;
            public float Tx;
            public float Ty;
            public Color Color;
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement[3]
            {
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementSemantic.Position),
				new VertexElement(12, VertexElementFormat.Vector2, VertexElementSemantic.TextureCoordinate),
				new VertexElement(20, VertexElementFormat.NormalizedByte4, VertexElementSemantic.Color)
            });
        }
    }
}
