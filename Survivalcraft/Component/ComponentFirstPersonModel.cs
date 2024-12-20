using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System;
using TemplatesDatabase;

namespace Game
{
	public class ComponentFirstPersonModel : Component, IDrawable, IUpdateable
	{
		public SubsystemTerrain m_subsystemTerrain;

		public ComponentMiner m_componentMiner;

		public ComponentPlayer m_componentPlayer;

		public ComponentRider m_componentRider;

		public int m_value;

		public Model m_handModel;

		public Vector3? m_lastYpr;

		public Vector2 m_lagAngles;

		public float m_swapAnimationTime;

		public float m_pokeAnimationTime;

		public Vector3 m_itemOffset;

		public Vector3 m_itemRotation;

		public double m_nextHandLightTime;

		public float m_handLight;

		public int m_itemLight;

		public DrawBlockEnvironmentData m_drawBlockEnvironmentData = new();

		public PrimitivesRenderer3D m_primitivesRenderer = new();

		public static LitShader LitShader = new(ShaderCodeManager.GetFast("Shaders/Lit.vsh"), ShaderCodeManager.GetFast("Shaders/Lit.psh"), 2, useEmissionColor: false, useVertexColor: false, useTexture: true, useFog: false, useAlphaThreshold: false);

		public static int[] m_drawOrders = new int[1]
		{
			1
		};

		public Vector3 ItemOffsetOrder
		{
			get;
			set;
		}

		public Vector3 ItemRotationOrder
		{
			get;
			set;
		}

		public int[] DrawOrders => m_drawOrders;

		public UpdateOrder UpdateOrder => UpdateOrder.FirstPersonModels;

		public void Draw(Camera camera, int drawOrder)
		{
			if (m_componentPlayer.ComponentHealth.Health > 0f && camera.GameWidget.IsEntityFirstPersonTarget(Entity) && !m_componentPlayer.ComponentInput.IsControlledByVr)
			{
				Viewport viewport = Display.Viewport;
				Viewport viewport2 = viewport;
				viewport2.MaxDepth *= 0.1f;
				Display.Viewport = viewport2;
				try
				{
					Matrix identity = Matrix.Identity;
					if (m_swapAnimationTime > 0f)
					{
						float num = MathF.Pow(MathF.Sin(m_swapAnimationTime * (float)Math.PI), 3f);
						identity *= Matrix.CreateTranslation(0f, -0.8f * num, 0.2f * num);
					}
					if (m_pokeAnimationTime > 0f)
					{
						float num2 = MathF.Sin(MathF.Sqrt(m_pokeAnimationTime) * (float)Math.PI);
						if (m_value != 0)
						{
							identity *= Matrix.CreateRotationX((0f - MathUtils.DegToRad(90f)) * num2);
							identity *= Matrix.CreateTranslation(-0.5f * num2, 0.1f * num2, 0f * num2);
						}
						else
						{
							identity *= Matrix.CreateRotationX((0f - MathUtils.DegToRad(45f)) * num2);
							identity *= Matrix.CreateTranslation(-0.1f * num2, 0.2f * num2, -0.05f * num2);
						}
					}
					if (m_componentRider.Mount != null)
					{
						ComponentCreatureModel componentCreatureModel = m_componentRider.Mount.Entity.FindComponent<ComponentCreatureModel>();
						if (componentCreatureModel != null)
						{
							float num3 = (componentCreatureModel.MovementAnimationPhase * (float)Math.PI * 2f) + 0.5f;
							Vector3 position = default;
							position.Y = 0.02f * MathF.Sin(num3);
							position.Z = 0.02f * MathF.Sin(num3);
							identity *= Matrix.CreateRotationX(0.05f * MathF.Sin(num3 * 1f)) * Matrix.CreateTranslation(position);
						}
					}
					else
					{
						float num4 = m_componentPlayer.ComponentCreatureModel.MovementAnimationPhase * (float)Math.PI * 2f;
						Vector3 position2 = default;
						position2.X = 0.03f * MathF.Sin(num4 * 1f);
						position2.Y = 0.02f * MathF.Sin(num4 * 2f);
						position2.Z = 0.02f * MathF.Sin(num4 * 1f);
						identity *= Matrix.CreateRotationZ(1f * position2.X) * Matrix.CreateTranslation(position2);
					}
					Vector3 eyePosition = m_componentPlayer.ComponentCreatureModel.EyePosition;
					int x = Terrain.ToCell(eyePosition.X);
					int num5 = Terrain.ToCell(eyePosition.Y);
					int z = Terrain.ToCell(eyePosition.Z);
					var m = Matrix.CreateFromQuaternion(m_componentPlayer.ComponentCreatureModel.EyeRotation);
					m.Translation = m_componentPlayer.ComponentCreatureModel.EyePosition;
					if (m_value != 0)
					{
						if (num5 >= 0 && num5 <= 255)
						{
							TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(x, z);
							if (chunkAtCell != null && chunkAtCell.State >= TerrainChunkState.InvalidVertices1)
							{
								m_itemLight = m_subsystemTerrain.Terrain.GetCellLightFast(x, num5, z);
							}
						}
						int num6 = Terrain.ExtractContents(m_value);
						Block block = BlocksManager.Blocks[num6];
						Vector3 vector = (block.GetFirstPersonRotation(m_value) * ((float)Math.PI / 180f)) + m_itemRotation;
						Vector3 position3 = block.GetFirstPersonOffset(m_value) + m_itemOffset;
						Matrix matrix = Matrix.CreateFromYawPitchRoll(vector.Y, vector.X, vector.Z) * identity * Matrix.CreateTranslation(position3) * Matrix.CreateFromYawPitchRoll(m_lagAngles.X, m_lagAngles.Y, 0f) * m;
						Matrix matrix2 = matrix * camera.ViewMatrix;
						m_drawBlockEnvironmentData.DrawBlockMode = DrawBlockMode.FirstPerson;
						m_drawBlockEnvironmentData.SubsystemTerrain = m_subsystemTerrain;
						m_drawBlockEnvironmentData.InWorldMatrix = matrix;
						m_drawBlockEnvironmentData.Light = m_itemLight;
						m_drawBlockEnvironmentData.Humidity = m_subsystemTerrain.Terrain.GetSeasonalHumidity(x, z);
						m_drawBlockEnvironmentData.Temperature = m_subsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(num5);
						m_drawBlockEnvironmentData.EnvironmentTemperature = m_componentPlayer.ComponentVitalStats.EnvironmentTemperature;
						m_drawBlockEnvironmentData.Owner = m_entity;
						block.DrawBlock(m_primitivesRenderer, m_value, Color.White, block.GetFirstPersonScale(m_value), ref matrix2, m_drawBlockEnvironmentData);
						m_primitivesRenderer.Flush(camera.ProjectionMatrix);
					}
					else
					{
						if (Time.FrameStartTime >= m_nextHandLightTime)
						{
							float? num7 = LightingManager.CalculateSmoothLight(m_subsystemTerrain, eyePosition);
							if (num7.HasValue)
							{
								m_nextHandLightTime = Time.FrameStartTime + 0.1;
								m_handLight = num7.Value;
							}
						}
						var position4 = new Vector3(0.25f, -0.3f, -0.05f);
						Matrix matrix2 = Matrix.CreateScale(0.01f) * Matrix.CreateRotationX(0.8f) * Matrix.CreateRotationY(0.4f) * identity * Matrix.CreateTranslation(position4) * Matrix.CreateFromYawPitchRoll(m_lagAngles.X, m_lagAngles.Y, 0f) * m * camera.ViewMatrix;
						Display.DepthStencilState = DepthStencilState.Default;
						Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
						LitShader.Texture = m_componentPlayer.ComponentCreatureModel.TextureOverride;
						LitShader.SamplerState = SamplerState.PointClamp;
						LitShader.MaterialColor = Vector4.One;
						LitShader.AmbientLightColor = new Vector3(m_handLight * LightingManager.LightAmbient);
						LitShader.DiffuseLightColor1 = new Vector3(m_handLight);
						LitShader.DiffuseLightColor2 = new Vector3(m_handLight);
						LitShader.LightDirection1 = Vector3.TransformNormal(LightingManager.DirectionToLight1, camera.ViewMatrix);
						LitShader.LightDirection2 = Vector3.TransformNormal(LightingManager.DirectionToLight2, camera.ViewMatrix);
						LitShader.Transforms.World[0] = matrix2;
						LitShader.Transforms.View = Matrix.Identity;
						LitShader.Transforms.Projection = camera.ProjectionMatrix;
						foreach (ModelMesh mesh in m_handModel.Meshes)
						{
							foreach (ModelMeshPart meshPart in mesh.MeshParts)
							{
								Display.DrawIndexed(PrimitiveType.TriangleList, LitShader, meshPart.VertexBuffer, meshPart.IndexBuffer, meshPart.StartIndex, meshPart.IndicesCount);
							}
						}
					}
				}
				finally
				{
					Display.Viewport = viewport;
				}
			}
		}

		public void Update(float dt)
		{
			Vector3 vector = m_componentPlayer.ComponentCreatureModel.EyeRotation.ToYawPitchRoll();
			m_lagAngles *= MathF.Pow(0.2f, dt);
			if (m_lastYpr.HasValue)
			{
				Vector3 vector2 = vector - m_lastYpr.Value;
				m_lagAngles.X = Math.Clamp(m_lagAngles.X - (0.08f * MathUtils.NormalizeAngle(vector2.X)), -0.1f, 0.1f);
				m_lagAngles.Y = Math.Clamp(m_lagAngles.Y - (0.08f * MathUtils.NormalizeAngle(vector2.Y)), -0.1f, 0.1f);
			}
			m_lastYpr = vector;
			int activeBlockValue = m_componentMiner.ActiveBlockValue;
			if (m_swapAnimationTime == 0f && activeBlockValue != m_value)
			{
				if (BlocksManager.Blocks[Terrain.ExtractContents(activeBlockValue)].IsSwapAnimationNeeded(m_value, activeBlockValue))
				{
					m_swapAnimationTime = 0.0001f;
				}
				else
				{
					m_value = activeBlockValue;
				}
			}
			if (m_swapAnimationTime > 0f)
			{
				float swapAnimationTime = m_swapAnimationTime;
				m_swapAnimationTime += 2f * dt;
				if (swapAnimationTime < 0.5f && m_swapAnimationTime >= 0.5f)
				{
					m_value = activeBlockValue;
				}
				if (m_swapAnimationTime > 1f)
				{
					m_swapAnimationTime = 0f;
				}
			}
			m_pokeAnimationTime = m_componentMiner.PokingPhase;
			m_itemOffset = Vector3.Lerp(m_itemOffset, ItemOffsetOrder, MathUtils.Saturate(10f * dt));
			m_itemRotation = Vector3.Lerp(m_itemRotation, ItemRotationOrder, MathUtils.Saturate(10f * dt));
			ItemOffsetOrder = Vector3.Zero;
			ItemRotationOrder = Vector3.Zero;
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: true);
			m_componentRider = Entity.FindComponent<ComponentRider>(throwOnError: true);
			m_componentMiner = Entity.FindComponent<ComponentMiner>(throwOnError: true);
			m_handModel = ContentManager.Get<Model>(valuesDictionary.GetValue<string>("HandModelName"));
		}
	}
}
