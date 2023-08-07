using Engine;
using Engine.Serialization;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemSpawn : Subsystem, IUpdateable
	{
		public SubsystemGameInfo m_subsystemGameInfo;

		public SubsystemPlayers m_subsystemPlayers;

		public SubsystemGameWidgets m_subsystemViews;

		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemTime m_subsystemTime;

		public Random m_random = new Random();

		public double m_nextDiscardOldChunksTime = 1.0;

		public double m_nextVisitedTime = 1.0;

		public double m_nextChunkSpawnTime = 1.0;

		public double m_nextDespawnTime = 1.0;

		public Dictionary<Point2, SpawnChunk> m_chunks = new Dictionary<Point2, SpawnChunk>();

		public Dictionary<ComponentSpawn, bool> m_spawns = new Dictionary<ComponentSpawn, bool>();

		public const float MaxChunkAge = 76800f;

		public const float VisitedRadius = 8f;

		public const float SpawnRadius = 40f;

		public const float DespawnRadius = 52f;

		public Dictionary<ComponentSpawn, bool>.KeyCollection Spawns => m_spawns.Keys;

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public virtual Action<SpawnChunk> SpawningChunk { get; set; }

		public virtual SpawnChunk GetSpawnChunk(Point2 point)
		{
			m_chunks.TryGetValue(point, out SpawnChunk value);
			return value;
		}

		public void Update(float dt)
		{
			if (m_subsystemTime.GameTime >= m_nextDiscardOldChunksTime)
			{
				m_nextDiscardOldChunksTime = m_subsystemTime.GameTime + 60.0;
				DiscardOldChunks();
			}
			if (m_subsystemTime.GameTime >= m_nextVisitedTime)
			{
				m_nextVisitedTime = m_subsystemTime.GameTime + 5.0;
				UpdateLastVisitedTime();
			}
			if (m_subsystemTime.GameTime >= m_nextChunkSpawnTime)
			{
				m_nextChunkSpawnTime = m_subsystemTime.GameTime + 4.0;
				SpawnChunks();
			}
			if (m_subsystemTime.GameTime >= m_nextDespawnTime)
			{
				m_nextDespawnTime = m_subsystemTime.GameTime + 2.0;
				DespawnChunks();
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
			m_subsystemViews = Project.FindSubsystem<SubsystemGameWidgets>(throwOnError: true);
			m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			foreach (KeyValuePair<string, object> item in valuesDictionary.GetValue<ValuesDictionary>("Chunks"))
			{
				var valuesDictionary2 = (ValuesDictionary)item.Value;
				var spawnChunk = new SpawnChunk();
				spawnChunk.Point = HumanReadableConverter.ConvertFromString<Point2>(item.Key);
				spawnChunk.IsSpawned = valuesDictionary2.GetValue<bool>("IsSpawned");
				spawnChunk.LastVisitedTime = valuesDictionary2.GetValue<double>("LastVisitedTime");
				ValuesDictionary data = new ValuesDictionary();
				object obj = valuesDictionary2.GetValue("SpawnsData", new object());
				if (obj is string)
				{
					string[] oldData = ((string)obj).Split(new char[] { ';' });
					int i = 0;
					foreach (var oldItem in oldData)
					{
						if (!string.IsNullOrEmpty(oldItem))
						{
							ValuesDictionary va = new ValuesDictionary();
							string[] parmas = oldItem.Split(new char[] { ',' });
							data.SetValue(i++.ToString(), va);
							va.SetValue("n", parmas[0]);
							va.SetValue("p", new Vector3(float.Parse(parmas[1]), float.Parse(parmas[2]), float.Parse(parmas[3])));
							va.SetValue("c", bool.Parse(parmas[4]));
							va.SetValue("d", new ValuesDictionary());
						}
					}
				}
				else if (obj is ValuesDictionary) data = (ValuesDictionary)obj;
				LoadSpawnsData(data, spawnChunk.SpawnsData);
				m_chunks[spawnChunk.Point] = spawnChunk;
			}
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			var valuesDictionary2 = new ValuesDictionary();
			valuesDictionary.SetValue("Chunks", valuesDictionary2);
			foreach (SpawnChunk value2 in m_chunks.Values)
			{
				if (value2.LastVisitedTime.HasValue)
				{
					var valuesDictionary3 = new ValuesDictionary();
					valuesDictionary2.SetValue(HumanReadableConverter.ConvertToString(value2.Point), valuesDictionary3);
					valuesDictionary3.SetValue("IsSpawned", value2.IsSpawned);
					valuesDictionary3.SetValue("LastVisitedTime", value2.LastVisitedTime.Value);
					ValuesDictionary v = new ValuesDictionary();
					SaveSpawnsData(v, value2.SpawnsData);
					valuesDictionary3.SetValue("SpawnsData", v);
				}
			}
		}

		public override void OnEntityAdded(Entity entity)
		{
			foreach (ComponentSpawn item in entity.FindComponents<ComponentSpawn>())
			{
				m_spawns.Add(item, value: true);
			}
		}

		public override void OnEntityRemoved(Entity entity)
		{
			foreach (ComponentSpawn item in entity.FindComponents<ComponentSpawn>())
			{
				m_spawns.Remove(item);
			}
		}

		public virtual SpawnChunk GetOrCreateSpawnChunk(Point2 point)
		{
			SpawnChunk spawnChunk = GetSpawnChunk(point);
			if (spawnChunk == null)
			{
				spawnChunk = new SpawnChunk
				{
					Point = point
				};
				m_chunks.Add(point, spawnChunk);
			}
			return spawnChunk;
		}

		public virtual void DiscardOldChunks()
		{
			var list = new List<Point2>();
			foreach (SpawnChunk value in m_chunks.Values)
			{
				if (!value.LastVisitedTime.HasValue || m_subsystemGameInfo.TotalElapsedGameTime - value.LastVisitedTime.Value > 76800.0)
				{
					list.Add(value.Point);
				}
			}
			foreach (Point2 item in list)
			{
				m_chunks.Remove(item);
			}
		}

		public virtual void UpdateLastVisitedTime()
		{
			foreach (ComponentPlayer componentPlayer in m_subsystemPlayers.ComponentPlayers)
			{
				var v = new Vector2(componentPlayer.ComponentBody.Position.X, componentPlayer.ComponentBody.Position.Z);
				Vector2 p = v - new Vector2(8f);
				Vector2 p2 = v + new Vector2(8f);
				Point2 point = Terrain.ToChunk(p);
				Point2 point2 = Terrain.ToChunk(p2);
				for (int i = point.X; i <= point2.X; i++)
				{
					for (int j = point.Y; j <= point2.Y; j++)
					{
						SpawnChunk spawnChunk = GetSpawnChunk(new Point2(i, j));
						if (spawnChunk != null)
						{
							spawnChunk.LastVisitedTime = m_subsystemGameInfo.TotalElapsedGameTime;
						}
					}
				}
			}
		}

		public virtual void SpawnChunks()
		{
			var list = new List<SpawnChunk>();
			foreach (GameWidget gameWidget in m_subsystemViews.GameWidgets)
			{
				var v = new Vector2(gameWidget.ActiveCamera.ViewPosition.X, gameWidget.ActiveCamera.ViewPosition.Z);
				Vector2 p = v - new Vector2(40f);
				Vector2 p2 = v + new Vector2(40f);
				Point2 point = Terrain.ToChunk(p);
				Point2 point2 = Terrain.ToChunk(p2);
				for (int i = point.X; i <= point2.X; i++)
				{
					for (int j = point.Y; j <= point2.Y; j++)
					{
						var v2 = new Vector2((i + 0.5f) * 16f, (j + 0.5f) * 16f);
						if (Vector2.DistanceSquared(v, v2) < 1600f)
						{
							TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(v2.X), Terrain.ToCell(v2.Y));
							if (chunkAtCell != null && chunkAtCell.State > TerrainChunkState.InvalidPropagatedLight)
							{
								var point3 = new Point2(i, j);
								SpawnChunk orCreateSpawnChunk = GetOrCreateSpawnChunk(point3);
								foreach (SpawnEntityData spawnsDatum in orCreateSpawnChunk.SpawnsData)
								{
									SpawnEntity(spawnsDatum);
								}
								orCreateSpawnChunk.SpawnsData.Clear();
								SpawningChunk?.Invoke(orCreateSpawnChunk);
								orCreateSpawnChunk.IsSpawned = true;
							}
						}
					}
				}
			}
			foreach (SpawnChunk item in list)
			{
				foreach (SpawnEntityData spawnsDatum2 in item.SpawnsData)
				{
					SpawnEntity(spawnsDatum2);
				}
				item.SpawnsData.Clear();
			}
		}

		public virtual void DespawnChunks()
		{
			var list = new List<ComponentSpawn>(0);
			foreach (ComponentSpawn key in m_spawns.Keys)
			{
				if (key.AutoDespawn && !key.IsDespawning)
				{
					bool flag = true;
					Vector3 position = key.ComponentFrame.Position;
					var v = new Vector2(position.X, position.Z);
					foreach (GameWidget gameWidget in m_subsystemViews.GameWidgets)
					{
						Vector3 viewPosition = gameWidget.ActiveCamera.ViewPosition;
						var v2 = new Vector2(viewPosition.X, viewPosition.Z);
						if (Vector2.DistanceSquared(v, v2) <= 2704f)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						list.Add(key);
					}
				}
			}
			foreach (ComponentSpawn item in list)
			{
				Point2 point = Terrain.ToChunk(item.ComponentFrame.Position.XZ);
				var data = new SpawnEntityData
				{
					TemplateName = item.Entity.ValuesDictionary.DatabaseObject.Name,
					Position = item.ComponentFrame.Position,
					ConstantSpawn = (item.ComponentCreature?.ConstantSpawn ?? false),
					Data = new ValuesDictionary()
				};
				ModsManager.HookAction("OnSaveSpawnData", (ModLoader loader) => { loader.OnSaveSpawnData(item, data); return true; });
				GetOrCreateSpawnChunk(point).SpawnsData.Add(data);
				item.Despawn();
			}
		}

		public virtual Entity SpawnEntity(SpawnEntityData data)
		{
			try
			{
				Entity entity = DatabaseManager.CreateEntity(Project, data.TemplateName, throwIfNotFound: true);
				ModsManager.HookAction("OnReadSpawnData", (ModLoader loader) => { loader.OnReadSpawnData(entity, data); return true; });
				entity.FindComponent<ComponentBody>(throwOnError: true).Position = data.Position;
				entity.FindComponent<ComponentBody>(throwOnError: true).Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, m_random.Float(0f, (float)Math.PI * 2f));
				ComponentCreature componentCreature = entity.FindComponent<ComponentCreature>();
				if (componentCreature != null)
				{
					componentCreature.ConstantSpawn = data.ConstantSpawn;
				}
				Project.AddEntity(entity);
				return entity;
			}
			catch (Exception ex)
			{
				Log.Error($"Unable to spawn entity with template \"{data.TemplateName}\". Reason: {ex.Message}");
				return null;
			}
		}

		public virtual void LoadSpawnsData(ValuesDictionary loadData, List<SpawnEntityData> creaturesData)
		{
			foreach (ValuesDictionary item in loadData.Values)
			{
				var data = new SpawnEntityData();
				data.ConstantSpawn = item.GetValue<bool>("c");
				data.Position = item.GetValue<Vector3>("p");
				data.TemplateName = item.GetValue<string>("n");
				data.Data = item.GetValue<ValuesDictionary>("d");
				creaturesData.Add(data);
			}
		}

		public virtual void SaveSpawnsData(ValuesDictionary saveData, List<SpawnEntityData> spawnsData)
		{
			int i = 0;
			foreach (var d in spawnsData)
			{
				ValuesDictionary v2 = new ValuesDictionary();
				v2.SetValue("c", d.ConstantSpawn);
				v2.SetValue("p", d.Position);
				v2.SetValue("n", d.TemplateName);
				v2.SetValue("d", d.Data);
				saveData.SetValue($"{i++}", v2);
			}
		}
	}
}
