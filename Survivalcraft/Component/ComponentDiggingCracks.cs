using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;
using System.Collections.Generic;
namespace Game
{
	public class ComponentDiggingCracks : Component, IDrawable
	{
		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemSky m_subsystemSky;

		public ComponentMiner m_componentMiner;

		public Texture2D[] m_textures;

		private Texture2D defaultTexture;

		private int defaultSlice = 0;

		public Shader m_shader;

		public Dictionary<Texture2D, TerrainGeometry[]> DrawItem;

		public TerrainGeometry m_geometry;

		public Point3 m_point;

		public int m_value;

		public static int[] m_drawOrders = new int[1]
		{
			200
		};

		public int[] DrawOrders => m_drawOrders;

		public DynamicArray<TerrainChunkGeometry.Buffer> Buffers = new DynamicArray<TerrainChunkGeometry.Buffer>();

		public void DisposeBuffers()
		{
			foreach (var b in Buffers) { b.Dispose(); }
			Buffers.Clear();
		}
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
			if (cellValue != m_value || point != m_point)
			{
				foreach (var item in DrawItem)
				{
					var subsets = item.Value[defaultSlice].Subsets;
					for (int p = 0; p < subsets.Length; p++) { subsets[p].Indices.Clear(); subsets[p].Vertices.Clear(); }
				}
				DisposeBuffers();
				block.GenerateTerrainVertices(m_subsystemTerrain.BlockGeometryGenerator, DrawItem[defaultTexture][defaultSlice], cellValue, point.X, point.Y, point.Z);
				m_point = point;
				m_value = cellValue;
				TerrainRenderer.CompileDrawSubsets(DrawItem, Buffers, block.SetDiggingCrackingTextureTransform);
			}
			Vector3 viewPosition = camera.ViewPosition;
			Vector3 v = new Vector3(MathUtils.Floor(viewPosition.X), 0f, MathUtils.Floor(viewPosition.Z));
			Matrix value = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
			float x = m_subsystemSky.ViewFogRange.X;
			float y = m_subsystemSky.ViewFogRange.Y;
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
				m_shader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
				m_shader.GetParameter("u_fogStartInvLength").SetValue(new Vector2(x, 1f / (y - x)));
				for (int i = 0; i < Buffers.Count; i++)
				{
					m_shader.GetParameter("u_texture").SetValue(block.GetDiggingCrackingTexture(m_componentMiner, m_componentMiner.m_digProgress, cellValue, m_textures));
					Display.DrawIndexed(PrimitiveType.TriangleList, m_shader, Buffers[i].VertexBuffer, Buffers[i].IndexBuffer, 0, Buffers[i].IndexBuffer.IndicesCount);
				}
			}
			catch
            {
            }
		}
		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			defaultTexture = Project.FindSubsystem<SubsystemAnimatedTextures>().AnimatedBlocksTexture;
			DrawItem = new Dictionary<Texture2D, TerrainGeometry[]>();
			var list = new TerrainGeometry[16];
			for (int i = 0; i < 16; i++) { var t = new TerrainGeometry(DrawItem, i); list[i] = t; }
			DrawItem.Add(defaultTexture, list);
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
	}
}
