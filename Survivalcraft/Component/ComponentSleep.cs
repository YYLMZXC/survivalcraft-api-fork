using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentSleep : Component, IUpdateable
	{
		public SubsystemPlayers m_subsystemPlayers;

		public SubsystemTime m_subsystemTime;

		public SubsystemUpdate m_subsystemUpdate;

		public SubsystemGameInfo m_subsystemGameInfo;

		public SubsystemTimeOfDay m_subsystemTimeOfDay;

		public SubsystemTerrain m_subsystemTerrain;

		public ComponentPlayer m_componentPlayer;

		public double? m_sleepStartTime;

		public float m_sleepFactor;

		public bool m_allowManualWakeUp;
		public static string fName = "ComponentSleep";
		public float m_minWetness;

		public float m_messageFactor;

		public bool IsSleeping => m_sleepStartTime.HasValue;

		public float SleepFactor => m_sleepFactor;

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public virtual bool CanSleep(out string reason)
		{
			Block block = m_componentPlayer.ComponentBody.StandingOnValue.HasValue ? BlocksManager.Blocks[Terrain.ExtractContents(m_componentPlayer.ComponentBody.StandingOnValue.Value)] : null;
			if (block == null || m_componentPlayer.ComponentBody.ImmersionDepth > 0f)
			{
				reason = LanguageControl.Get(fName, 1);
				return false;
			}
			if (block != null && block.SleepSuitability == 0f)
			{
				reason = LanguageControl.Get(fName, 2);
				return false;
			}
			if (m_componentPlayer.ComponentVitalStats.Sleep > 0.99f)
			{
				reason = LanguageControl.Get(fName, 3);
				return false;
			}
			if (m_componentPlayer.ComponentVitalStats.Wetness > 0.95f)
			{
				reason = LanguageControl.Get(fName, 4);
				return false;
			}
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					Vector3 start = m_componentPlayer.ComponentBody.Position + new Vector3(i, 1f, j);
					var end = new Vector3(start.X, 255f, start.Z);
					if (!m_subsystemTerrain.Raycast(start, end, useInteractionBoxes: false, skipAirBlocks: true, (int value, float distance) => Terrain.ExtractContents(value) != 0).HasValue)
					{
						reason = LanguageControl.Get(fName, 5);
						return false;
					}
				}
			}
			reason = string.Empty;
			return true;
		}

		public virtual void Sleep(bool allowManualWakeup)
		{
			if (!IsSleeping)
			{
				m_sleepStartTime = m_subsystemGameInfo.TotalElapsedGameTime;
				m_allowManualWakeUp = allowManualWakeup;
				m_minWetness = float.MaxValue;
				m_messageFactor = 0f;
				if (m_componentPlayer.PlayerStats != null)
				{
					m_componentPlayer.PlayerStats.TimesWentToSleep++;
				}
			}
		}

		public virtual void WakeUp()
		{
			if (m_sleepStartTime.HasValue)
			{
				m_sleepStartTime = null;
				m_componentPlayer.PlayerData.SpawnPosition = m_componentPlayer.ComponentBody.Position + new Vector3(0f, 0.1f, 0f);
			}
		}

		public void Update(float dt)
		{
			if (IsSleeping && m_componentPlayer.ComponentHealth.Health > 0f)
			{
				m_sleepFactor = MathUtils.Min(m_sleepFactor + (0.33f * Time.FrameDuration), 1f);
				m_minWetness = MathUtils.Min(m_minWetness, m_componentPlayer.ComponentVitalStats.Wetness);
				m_componentPlayer.PlayerStats.TimeSlept += m_subsystemGameInfo.TotalElapsedGameTimeDelta;
				if ((m_componentPlayer.ComponentVitalStats.Sleep >= 1f || m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative) && m_subsystemTimeOfDay.TimeOfDay > 0.3f && m_subsystemTimeOfDay.TimeOfDay < 0.599999964f && m_sleepStartTime.HasValue && m_subsystemGameInfo.TotalElapsedGameTime > m_sleepStartTime + 180.0)
				{
					WakeUp();
				}
				if (m_componentPlayer.ComponentHealth.HealthChange < 0f && (m_componentPlayer.ComponentHealth.Health < 0.5f || m_componentPlayer.ComponentVitalStats.Sleep > 0.5f))
				{
					WakeUp();
				}
				if (m_componentPlayer.ComponentVitalStats.Wetness > m_minWetness + 0.05f && m_componentPlayer.ComponentVitalStats.Sleep > 0.2f)
				{
					WakeUp();
					m_subsystemTime.QueueGameTimeDelayedExecution(m_subsystemTime.GameTime + 1.0, delegate
					{
						m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(fName, 6), Color.White, blinking: true, playNotificationSound: true);
					});
				}
				if (m_sleepStartTime.HasValue)
				{
					float num = (float)(m_subsystemGameInfo.TotalElapsedGameTime - m_sleepStartTime.Value);
					if (m_allowManualWakeUp && num > 10f)
					{
						if (m_componentPlayer.GameWidget.Input.Any && !DialogsManager.HasDialogs(m_componentPlayer.GameWidget))
						{
							m_componentPlayer.GameWidget.Input.Clear();
							WakeUp();
							m_subsystemTime.QueueGameTimeDelayedExecution(m_subsystemTime.GameTime + 2.0, delegate
							{
								m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(fName, 7), Color.White, blinking: true, playNotificationSound: false);
							});
						}
						m_messageFactor = MathUtils.Min(m_messageFactor + (0.5f * Time.FrameDuration), 1f);
						m_componentPlayer.ComponentScreenOverlays.Message = LanguageControl.Get(fName, 8);
						m_componentPlayer.ComponentScreenOverlays.MessageFactor = m_messageFactor;
					}
					if (!m_allowManualWakeUp && num > 5f)
					{
						m_messageFactor = MathUtils.Min(m_messageFactor + (1f * Time.FrameDuration), 1f);
						m_componentPlayer.ComponentScreenOverlays.Message = LanguageControl.Get(fName, 9);
						m_componentPlayer.ComponentScreenOverlays.MessageFactor = m_messageFactor;
					}
				}
			}
			else
			{
				m_sleepFactor = MathUtils.Max(m_sleepFactor - (1f * Time.FrameDuration), 0f);
			}
			m_componentPlayer.ComponentScreenOverlays.BlackoutFactor = MathUtils.Max(m_componentPlayer.ComponentScreenOverlays.BlackoutFactor, m_sleepFactor);
			if (m_sleepFactor > 0.01f)
			{
				m_componentPlayer.ComponentScreenOverlays.FloatingMessage = LanguageControl.Get(fName, 10);
				m_componentPlayer.ComponentScreenOverlays.FloatingMessageFactor = MathUtils.Saturate(10f * (m_sleepFactor - 0.9f));
			}
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(throwOnError: true);
			m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
			m_subsystemUpdate = Project.FindSubsystem<SubsystemUpdate>(throwOnError: true);
			m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
			m_subsystemTimeOfDay = Project.FindSubsystem<SubsystemTimeOfDay>(throwOnError: true);
			m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: true);
			m_sleepStartTime = valuesDictionary.GetValue<double>("SleepStartTime");
			m_allowManualWakeUp = valuesDictionary.GetValue<bool>("AllowManualWakeUp");
			m_componentPlayer.ComponentBody.Attacked += delegate (Attackment attackment)
			{
				if (IsSleeping && m_componentPlayer.ComponentVitalStats.Sleep > 0.25f)
				{
					WakeUp();
				}
			};
			if (m_sleepStartTime == 0.0)
			{
				m_sleepStartTime = null;
			}
			if (m_sleepStartTime.HasValue)
			{
				m_sleepFactor = 1f;
				m_minWetness = float.MaxValue;
			}
		}

		public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
			valuesDictionary.SetValue("SleepStartTime", m_sleepStartTime ?? 0.0);
			valuesDictionary.SetValue("AllowManualWakeUp", m_allowManualWakeUp);
		}
	}
}
