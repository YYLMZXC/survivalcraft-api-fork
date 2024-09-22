using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemProjectiles : Subsystem, IUpdateable, IDrawable
	{
		public SubsystemAudio m_subsystemAudio;

		public SubsystemSoundMaterials m_subsystemSoundMaterials;

		public SubsystemParticles m_subsystemParticles;

		public SubsystemPickables m_subsystemPickables;

		public SubsystemBodies m_subsystemBodies;

		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemSky m_subsystemSky;

		public SubsystemTime m_subsystemTime;

		public SubsystemNoise m_subsystemNoise;

		public SubsystemExplosions m_subsystemExplosions;

		public SubsystemGameInfo m_subsystemGameInfo;

		public SubsystemBlockBehaviors m_subsystemBlockBehaviors;

		public SubsystemFluidBlockBehavior m_subsystemFluidBlockBehavior;

		public SubsystemFireBlockBehavior m_subsystemFireBlockBehavior;

		public List<Projectile> m_projectiles = [];

		public List<Projectile> m_projectilesToRemove = [];

		public PrimitivesRenderer3D m_primitivesRenderer = new();

		public Random m_random = new();

		public DrawBlockEnvironmentData m_drawBlockEnvironmentData = new();

		public const float BodyInflateAmount = 0.2f;

		public static int[] m_drawOrders = new int[1]
		{
			10
		};

		public ReadOnlyList<Projectile> Projectiles => new(m_projectiles);

		public int[] DrawOrders => m_drawOrders;

		public virtual Action<Projectile> ProjectileAdded { get; set; }

		public virtual Action<Projectile> ProjectileRemoved { get; set; }

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public virtual Projectile AddProjectile(int value, Vector3 position, Vector3 velocity, Vector3 angularVelocity, ComponentCreature owner)
		{
			var projectile = new Projectile();
			projectile.Value = value;
			projectile.Position = position;
			projectile.Velocity = velocity;
			projectile.Rotation = Vector3.Zero;
			projectile.AngularVelocity = angularVelocity;
			projectile.CreationTime = m_subsystemGameInfo.TotalElapsedGameTime;
			projectile.IsInWater = IsWater(position);
			projectile.Owner = owner;
			projectile.OwnerEntity = owner?.Entity;
			projectile.ProjectileStoppedAction = ProjectileStoppedAction.TurnIntoPickable;

            ModsManager.HookAction("OnProjectileAdded", loader =>
            {
                loader.OnProjectileAdded(this, ref projectile, null);
                return false;
            });

            m_projectiles.Add(projectile);
			ProjectileAdded?.Invoke(projectile);
			if (owner != null && owner.PlayerStats != null)
			{
				owner.PlayerStats.RangedAttacks++;
			}
			return projectile;
		}

		public virtual Projectile FireProjectile(int value, Vector3 position, Vector3 velocity, Vector3 angularVelocity, ComponentCreature owner)
		{
			int num = Terrain.ExtractContents(value);
			Block block = BlocksManager.Blocks[num];
			var v = Vector3.Normalize(velocity);
			Vector3 vector = position;
			if (owner != null)
			{
				var ray = new Ray3(position + (v * 5f), -v);
				BoundingBox boundingBox = owner.ComponentBody.BoundingBox;
				boundingBox.Min -= new Vector3(0.4f);
				boundingBox.Max += new Vector3(0.4f);
				float? num2 = ray.Intersection(boundingBox);
				if (num2.HasValue)
				{
					if (num2.Value == 0f)
					{
						return null;
					}
					vector = position + (v * (5f - num2.Value + 0.1f));
				}
			}
			Vector3 end = vector + (v * block.ProjectileTipOffset);
			if (!m_subsystemTerrain.Raycast(position, end, useInteractionBoxes: false, skipAirBlocks: true, (int testValue, float distance) => BlocksManager.Blocks[Terrain.ExtractContents(testValue)].IsCollidable_(testValue)).HasValue)
			{
				Projectile projectile = AddProjectile(value, vector, velocity, angularVelocity, owner);
				SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(value));
				for (int i = 0; i < blockBehaviors.Length; i++)
				{
					blockBehaviors[i].OnFiredAsProjectile(projectile);
				}
				return projectile;
			}
			return null;
		}

		public virtual void AddTrail(Projectile projectile, Vector3 offset, ITrailParticleSystem particleSystem)
		{
			RemoveTrail(projectile);
			projectile.TrailParticleSystem = particleSystem;
			projectile.TrailOffset = offset;
		}

		public virtual void RemoveTrail(Projectile projectile)
		{
			if (projectile.TrailParticleSystem != null)
			{
				if (m_subsystemParticles.ContainsParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem))
				{
					m_subsystemParticles.RemoveParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem);
				}
				projectile.TrailParticleSystem = null;
			}
		}

		public void Draw(Camera camera, int drawOrder)
		{
			m_drawBlockEnvironmentData.SubsystemTerrain = m_subsystemTerrain;
			m_drawBlockEnvironmentData.InWorldMatrix = Matrix.Identity;
			float num = MathUtils.Sqr(m_subsystemSky.VisibilityRange);
			foreach (Projectile projectile in m_projectiles)
			{
				Vector3 position = projectile.Position;
				if (!projectile.NoChunk && Vector3.DistanceSquared(camera.ViewPosition, position) < num && camera.ViewFrustum.Intersection(position))
				{
					int x = Terrain.ToCell(position.X);
					int num2 = Terrain.ToCell(position.Y);
					int z = Terrain.ToCell(position.Z);
					int num3 = Terrain.ExtractContents(projectile.Value);
					Block block = BlocksManager.Blocks[num3];
					TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(x, z);
					if (chunkAtCell != null && chunkAtCell.State >= TerrainChunkState.InvalidVertices1 && num2 >= 0 && num2 < 255)
					{
						m_drawBlockEnvironmentData.Humidity = m_subsystemTerrain.Terrain.GetSeasonalHumidity(x, z);
						m_drawBlockEnvironmentData.Temperature = m_subsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(num2);
						projectile.Light = m_subsystemTerrain.Terrain.GetCellLightFast(x, num2, z);
					}
					m_drawBlockEnvironmentData.Light = projectile.Light;
					m_drawBlockEnvironmentData.BillboardDirection = block.GetAlignToVelocity(projectile.Value) ? null : new Vector3?(camera.ViewDirection);
					m_drawBlockEnvironmentData.InWorldMatrix.Translation = position;
					Matrix matrix;
					if (block.GetAlignToVelocity(projectile.Value))
					{
						CalculateVelocityAlignMatrix(block, position, projectile.Velocity, out matrix);
					}
					else if (projectile.Rotation != Vector3.Zero)
					{
						matrix = Matrix.CreateFromAxisAngle(Vector3.Normalize(projectile.Rotation), projectile.Rotation.Length());
						matrix.Translation = projectile.Position;
					}
					else
					{
						matrix = Matrix.CreateTranslation(projectile.Position);
					}
					bool shouldDrawBlock = true;
					float drawBlockSize = 0.3f;
					Color drawBlockColor = Color.White;
                    ModsManager.HookAction("OnProjectileDraw", loader =>
                    {
                        loader.OnProjectileDraw(projectile, this, camera, drawOrder, ref shouldDrawBlock, ref drawBlockSize, ref drawBlockColor);
                        return false;
                    });
                    if(shouldDrawBlock)
						block.DrawBlock(m_primitivesRenderer, projectile.Value, drawBlockColor, drawBlockSize, ref matrix, m_drawBlockEnvironmentData);
				}
			}
			m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
		}

		public void Update(float dt)
		{
			double totalElapsedGameTime = m_subsystemGameInfo.TotalElapsedGameTime;
			foreach (Projectile projectile in m_projectiles)
			{
				bool skipVanilla_ = false;
                ModsManager.HookAction("OnProjectileUpdate", loader =>
                {
					loader.OnProjectileUpdate(projectile, this, dt, skipVanilla_, out bool skipVanilla);
                    skipVanilla_ |= skipVanilla;
                    return false;
                });
                if (skipVanilla_) continue;
                if (projectile.ToRemove)
				{
					m_projectilesToRemove.Add(projectile);
				}
				else
				{
					Block block = BlocksManager.Blocks[Terrain.ExtractContents(projectile.Value)];
					if (totalElapsedGameTime - projectile.CreationTime > 40.0)
					{
						projectile.ToRemove = true;
					}
					TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Z));
					if (chunkAtCell == null || chunkAtCell.State <= TerrainChunkState.InvalidContents4)
					{
						projectile.NoChunk = true;
						if (projectile.TrailParticleSystem != null)
						{
							projectile.TrailParticleSystem.IsStopped = true;
						}
					}
					else
					{
						projectile.NoChunk = false;
						Vector3 position = projectile.Position;
						Vector3 vector = position + (projectile.Velocity * dt);
						Vector3 v = block.ProjectileTipOffset * Vector3.Normalize(projectile.Velocity);
						BodyRaycastResult? bodyRaycastResult = m_subsystemBodies.Raycast(position + v, vector + v, 0.2f, (ComponentBody body, float distance) => true);
						TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(position + v, vector + v, useInteractionBoxes: false, skipAirBlocks: true, (int value, float distance) => BlocksManager.Blocks[Terrain.ExtractContents(value)].IsCollidable_(value));
						bool flag = block.DisintegratesOnHit;
						if (terrainRaycastResult.HasValue || bodyRaycastResult.HasValue)
						{
							CellFace? cellFace = terrainRaycastResult.HasValue ? new CellFace?(terrainRaycastResult.Value.CellFace) : null;
							ComponentBody componentBody = bodyRaycastResult.HasValue ? bodyRaycastResult.Value.ComponentBody : null;
							SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(projectile.Value));
							for (int i = 0; i < blockBehaviors.Length; i++)
							{
								flag |= blockBehaviors[i].OnHitAsProjectile(cellFace, componentBody, projectile);
							}
							projectile.ToRemove |= flag;
						}
						Vector3? vector2 = null;
						if (bodyRaycastResult.HasValue && (!terrainRaycastResult.HasValue || bodyRaycastResult.Value.Distance < terrainRaycastResult.Value.Distance))
						{
							if (projectile.Velocity.Length() > 10f)
							{
								ComponentMiner.AttackBody(bodyRaycastResult.Value.ComponentBody, projectile.OwnerEntity, bodyRaycastResult.Value.HitPoint(), Vector3.Normalize(projectile.Velocity), block.GetProjectilePower(projectile.Value), isMeleeAttack: false);
								if (projectile.Owner != null && projectile.Owner.PlayerStats != null)
								{
									projectile.Owner.PlayerStats.RangedHits++;
								}
							}
							if (projectile.IsIncendiary)
							{
								bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentOnFire>()?.SetOnFire(projectile?.Owner, m_random.Float(6f, 8f));
							}
							vector = position;
							projectile.Velocity *= -0.05f;
							projectile.Velocity += m_random.Vector3(0.33f * projectile.Velocity.Length());
							projectile.AngularVelocity *= -0.05f;
						}
						else if (terrainRaycastResult.HasValue)
						{
							CellFace cellFace2 = terrainRaycastResult.Value.CellFace;
							int cellValue = m_subsystemTerrain.Terrain.GetCellValue(cellFace2.X, cellFace2.Y, cellFace2.Z);
							int num = Terrain.ExtractContents(cellValue);
							Block block2 = BlocksManager.Blocks[num];
							float num2 = projectile.Velocity.Length();
							SubsystemBlockBehavior[] blockBehaviors2 = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(cellValue));
							for (int j = 0; j < blockBehaviors2.Length; j++)
							{
								blockBehaviors2[j].OnHitByProjectile(cellFace2, projectile);
							}
							if (num2 > 10f && m_random.Float(0f, 1f) > block2.GetProjectileResilience(cellValue))
							{
								m_subsystemTerrain.DestroyCell(0, cellFace2.X, cellFace2.Y, cellFace2.Z, 0, noDrop: true, noParticleSystem: false);
								m_subsystemSoundMaterials.PlayImpactSound(cellValue, position, 1f);
							}
							if (projectile.IsIncendiary)
							{
								m_subsystemFireBlockBehavior.SetCellOnFire(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z, 1f);
								Vector3 vector3 = projectile.Position - (0.75f * Vector3.Normalize(projectile.Velocity));
								for (int k = 0; k < 8; k++)
								{
									Vector3 v2 = (k == 0) ? Vector3.Normalize(projectile.Velocity) : m_random.Vector3(1.5f);
									TerrainRaycastResult? terrainRaycastResult2 = m_subsystemTerrain.Raycast(vector3, vector3 + v2, useInteractionBoxes: false, skipAirBlocks: true, (int value, float distance) => true);
									if (terrainRaycastResult2.HasValue)
									{
										m_subsystemFireBlockBehavior.SetCellOnFire(terrainRaycastResult2.Value.CellFace.X, terrainRaycastResult2.Value.CellFace.Y, terrainRaycastResult2.Value.CellFace.Z, 1f);
									}
								}
							}
							if (num2 > 5f)
							{
								m_subsystemSoundMaterials.PlayImpactSound(cellValue, position, 1f);
							}
							if (block.IsStickable_(projectile.Value) && num2 > 10f && m_random.Bool(block2.GetProjectileStickProbability(projectile.Value)))
							{
								var v3 = Vector3.Normalize(projectile.Velocity);
								float s = MathUtils.Lerp(0.1f, 0.2f, MathUtils.Saturate((num2 - 15f) / 20f));
								vector2 = position + (terrainRaycastResult.Value.Distance * Vector3.Normalize(projectile.Velocity)) + (v3 * s);
							}
							else
							{
								Plane plane = cellFace2.CalculatePlane();
								vector = position;
								if (plane.Normal.X != 0f)
								{
									projectile.Velocity *= new Vector3(-0.3f, 0.3f, 0.3f);
								}
								if (plane.Normal.Y != 0f)
								{
									projectile.Velocity *= new Vector3(0.3f, -0.3f, 0.3f);
								}
								if (plane.Normal.Z != 0f)
								{
									projectile.Velocity *= new Vector3(0.3f, 0.3f, -0.3f);
								}
								float num3 = projectile.Velocity.Length();
								projectile.Velocity = num3 * Vector3.Normalize(projectile.Velocity + m_random.Vector3(num3 / 6f, num3 / 3f));
								projectile.AngularVelocity *= -0.3f;
							}
							MakeProjectileNoise(projectile);
						}
						if (terrainRaycastResult.HasValue || bodyRaycastResult.HasValue)
						{
							if (flag)
							{
								m_subsystemParticles.AddParticleSystem(block.CreateDebrisParticleSystem(m_subsystemTerrain, projectile.Position, projectile.Value, 1f));
							}
							else if (!projectile.ToRemove && (vector2.HasValue || projectile.Velocity.Length() < 1f))
							{
								if (projectile.ProjectileStoppedAction == ProjectileStoppedAction.TurnIntoPickable)
								{
									int num4 = BlocksManager.DamageItem(projectile.Value, 1, projectile.OwnerEntity);
									if (num4 != 0)
									{
										if (vector2.HasValue)
										{
											CalculateVelocityAlignMatrix(block, vector2.Value, projectile.Velocity, out Matrix matrix);
											m_subsystemPickables.AddPickable(num4, 1, projectile.Position, Vector3.Zero, matrix);
										}
										else
										{
											m_subsystemPickables.AddPickable(num4, 1, position, Vector3.Zero, null);
										}
									}
									projectile.ToRemove = true;
								}
								else if (projectile.ProjectileStoppedAction == ProjectileStoppedAction.Disappear)
								{
									projectile.ToRemove = true;
								}
							}
						}
						float num5 = projectile.IsInWater ? MathF.Pow(0.001f, dt) : MathF.Pow(block.GetProjectileDamping(projectile.Value), dt);
						projectile.Velocity.Y += -10f * dt;
						projectile.Velocity *= num5;
						projectile.AngularVelocity *= num5;
						projectile.Position = vector;
						projectile.Rotation += projectile.AngularVelocity * dt;
						if (projectile.TrailParticleSystem != null)
						{
							if (!m_subsystemParticles.ContainsParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem))
							{
								m_subsystemParticles.AddParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem);
							}
							Vector3 v4 = (projectile.TrailOffset != Vector3.Zero) ? Vector3.TransformNormal(projectile.TrailOffset, Matrix.CreateFromAxisAngle(Vector3.Normalize(projectile.Rotation), projectile.Rotation.Length())) : Vector3.Zero;
							projectile.TrailParticleSystem.Position = projectile.Position + v4;
							if (projectile.IsInWater)
							{
								projectile.TrailParticleSystem.IsStopped = true;
							}
						}
						bool flag2 = IsWater(projectile.Position);
						if (projectile.IsInWater != flag2)
						{
							if (flag2)
							{
								float num6 = new Vector2(projectile.Velocity.X + projectile.Velocity.Z).Length();
								if (num6 > 6f && num6 > 4f * MathF.Abs(projectile.Velocity.Y))
								{
									projectile.Velocity *= 0.5f;
									projectile.Velocity.Y *= -1f;
									flag2 = false;
								}
								else
								{
									projectile.Velocity *= 0.2f;
								}
								float? surfaceHeight = m_subsystemFluidBlockBehavior.GetSurfaceHeight(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y), Terrain.ToCell(projectile.Position.Z));
								if (surfaceHeight.HasValue)
								{
									m_subsystemParticles.AddParticleSystem(new WaterSplashParticleSystem(m_subsystemTerrain, new Vector3(projectile.Position.X, surfaceHeight.Value, projectile.Position.Z), large: false));
									m_subsystemAudio.PlayRandomSound("Audio/Splashes", 1f, m_random.Float(-0.2f, 0.2f), projectile.Position, 6f, autoDelay: true);
									MakeProjectileNoise(projectile);
								}
							}
							projectile.IsInWater = flag2;
						}
						if (IsMagma(projectile.Position))
						{
							m_subsystemParticles.AddParticleSystem(new MagmaSplashParticleSystem(m_subsystemTerrain, projectile.Position, large: false));
							m_subsystemAudio.PlayRandomSound("Audio/Sizzles", 1f, m_random.Float(-0.2f, 0.2f), projectile.Position, 3f, autoDelay: true);
							projectile.ToRemove = true;
							m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y), Terrain.ToCell(projectile.Position.Z), projectile.Value);
						}
						if (m_subsystemTime.PeriodicGameTimeEvent(1.0, projectile.GetHashCode() % 100 / 100.0) && (m_subsystemFireBlockBehavior.IsCellOnFire(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y + 0.1f), Terrain.ToCell(projectile.Position.Z)) || m_subsystemFireBlockBehavior.IsCellOnFire(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y + 0.1f) - 1, Terrain.ToCell(projectile.Position.Z))))
						{
							m_subsystemAudio.PlayRandomSound("Audio/Sizzles", 1f, m_random.Float(-0.2f, 0.2f), projectile.Position, 3f, autoDelay: true);
							projectile.ToRemove = true;
							m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y), Terrain.ToCell(projectile.Position.Z), projectile.Value);
						}
					}
				}
			}
			foreach (Projectile item in m_projectilesToRemove)
			{
				if (item.TrailParticleSystem != null)
				{
					item.TrailParticleSystem.IsStopped = true;
				}
				item.OnRemove?.Invoke();
				m_projectiles.Remove(item);
				ProjectileRemoved?.Invoke(item);
			}
			m_projectilesToRemove.Clear();
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
			m_subsystemSoundMaterials = Project.FindSubsystem<SubsystemSoundMaterials>(throwOnError: true);
			m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(throwOnError: true);
			m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
			m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(throwOnError: true);
			m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemSky = Project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemNoise = Project.FindSubsystem<SubsystemNoise>(throwOnError: true);
			m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
			m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemBlockBehaviors = Project.FindSubsystem<SubsystemBlockBehaviors>(throwOnError: true);
			m_subsystemFluidBlockBehavior = Project.FindSubsystem<SubsystemFluidBlockBehavior>(throwOnError: true);
			m_subsystemFireBlockBehavior = Project.FindSubsystem<SubsystemFireBlockBehavior>(throwOnError: true);
			foreach (ValuesDictionary item in valuesDictionary.GetValue<ValuesDictionary>("Projectiles").Values.Where((object v) => v is ValuesDictionary))
			{
				var projectile = new Projectile();
				projectile.Value = item.GetValue<int>("Value");
				projectile.Position = item.GetValue<Vector3>("Position");
				projectile.Velocity = item.GetValue<Vector3>("Velocity");
				projectile.CreationTime = item.GetValue<double>("CreationTime");
				projectile.ProjectileStoppedAction = item.GetValue("ProjectileStoppedAction", projectile.ProjectileStoppedAction);
                ModsManager.HookAction("OnProjectileAdded", loader =>
                {
                    loader.OnProjectileAdded(this, ref projectile, item);
                    return false;
                });
                m_projectiles.Add(projectile);
			}
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			var valuesDictionary2 = new ValuesDictionary();
			valuesDictionary.SetValue("Projectiles", valuesDictionary2);
			int num = 0;
			foreach (Projectile projectile in m_projectiles)
			{
				var valuesDictionary3 = new ValuesDictionary();
				valuesDictionary2.SetValue(num.ToString(CultureInfo.InvariantCulture), valuesDictionary3);
				valuesDictionary3.SetValue("Value", projectile.Value);
				valuesDictionary3.SetValue("Position", projectile.Position);
				valuesDictionary3.SetValue("Velocity", projectile.Velocity);
				valuesDictionary3.SetValue("CreationTime", projectile.CreationTime);
				valuesDictionary3.SetValue("ProjectileStoppedAction", projectile.ProjectileStoppedAction);
				num++;
			}
		}

		public virtual bool IsWater(Vector3 position)
		{
			int cellContents = m_subsystemTerrain.Terrain.GetCellContents(Terrain.ToCell(position.X), Terrain.ToCell(position.Y), Terrain.ToCell(position.Z));
			return BlocksManager.Blocks[cellContents] is WaterBlock;
		}

		public virtual bool IsMagma(Vector3 position)
		{
			int cellContents = m_subsystemTerrain.Terrain.GetCellContents(Terrain.ToCell(position.X), Terrain.ToCell(position.Y), Terrain.ToCell(position.Z));
			return BlocksManager.Blocks[cellContents] is MagmaBlock;
		}

		public virtual void MakeProjectileNoise(Projectile projectile)
		{
			if (m_subsystemTime.GameTime - projectile.LastNoiseTime > 0.5)
			{
				m_subsystemNoise.MakeNoise(projectile.Position, 0.25f, 6f);
				projectile.LastNoiseTime = m_subsystemTime.GameTime;
			}
		}

		public static void CalculateVelocityAlignMatrix(Block projectileBlock, Vector3 position, Vector3 velocity, out Matrix matrix)
		{
			matrix = Matrix.Identity;
			matrix.Up = Vector3.Normalize(velocity);
			matrix.Right = Vector3.Normalize(Vector3.Cross(matrix.Up, Vector3.UnitY));
			matrix.Forward = Vector3.Normalize(Vector3.Cross(matrix.Up, matrix.Right));
			matrix.Translation = position;
		}
	}
}
