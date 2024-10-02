using Engine;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class ComponentLevel : ComponentFactors, IUpdateable
	{
        public static string fName = "ComponentLevel";

        public ComponentPlayer m_componentPlayer;

		public const float FemaleStrengthFactor = 0.8f;

		public const float FemaleResilienceFactor = 0.8f;

		public const float FemaleSpeedFactor = 1.03f;

		public const float FemaleHungerFactor = 0.7f;

        public float? m_lastLevelTextValue;

		public int MaxLevel = 21;
		public virtual void AddExperience(int count, bool playSound)
		{
			if (playSound)
			{
				m_subsystemAudio.PlaySound("Audio/ExperienceCollected", 0.2f, m_random.Float(-0.1f, 0.4f), 0f, 0f);
			}
			for (int i = 0; i < count; i++)
			{
				float num = 0.012f / MathF.Pow(1.08f, MathF.Floor(m_componentPlayer.PlayerData.Level - 1f));
				if (MathF.Floor(m_componentPlayer.PlayerData.Level + num) > MathF.Floor(m_componentPlayer.PlayerData.Level))
				{
					Time.QueueTimeDelayedExecution(Time.FrameStartTime + 0.5 + 0.0, delegate
					{
						m_componentPlayer.ComponentGui.DisplaySmallMessage(LanguageControl.Get(fName, 1), Color.White, blinking: true, playNotificationSound: false);
					});
					Time.QueueTimeDelayedExecution(Time.FrameStartTime + 0.5 + 0.0, delegate
					{
						m_subsystemAudio.PlaySound("Audio/ExperienceCollected", 1f, -0.2f, 0f, 0f);
					});
					Time.QueueTimeDelayedExecution(Time.FrameStartTime + 0.5 + 0.15000000596046448, delegate
					{
						m_subsystemAudio.PlaySound("Audio/ExperienceCollected", 1f, -0.03333333f, 0f, 0f);
					});
					Time.QueueTimeDelayedExecution(Time.FrameStartTime + 0.5 + 0.30000001192092896, delegate
					{
						m_subsystemAudio.PlaySound("Audio/ExperienceCollected", 1f, 142f / (339f * (float)Math.PI), 0f, 0f);
					});
					Time.QueueTimeDelayedExecution(Time.FrameStartTime + 0.5 + 0.45000001788139343, delegate
					{
						m_subsystemAudio.PlaySound("Audio/ExperienceCollected", 1f, 23f / 60f, 0f, 0f);
					});
					Time.QueueTimeDelayedExecution(Time.FrameStartTime + 0.5 + 0.75, delegate
					{
						m_subsystemAudio.PlaySound("Audio/ExperienceCollected", 1f, -0.03333333f, 0f, 0f);
					});
					Time.QueueTimeDelayedExecution(Time.FrameStartTime + 0.5 + 0.90000003576278687, delegate
					{
						m_subsystemAudio.PlaySound("Audio/ExperienceCollected", 1f, 23f / 60f, 0f, 0f);
					});
				}
				m_componentPlayer.PlayerData.Level += num;
			}
		}
		private void LoadStrengthFactor()
		{
			m_strengthFactorSet.AddFactor(new Factor("PlayerClass")
            {
                GetDescription = delegate {
                    return m_componentPlayer.PlayerData.PlayerClass.ToString();
                },
                GetValue = delegate
                {
                    return (m_componentPlayer.PlayerData.PlayerClass == PlayerClass.Female) ? 0.8f : 1f;
                }
            });
			m_strengthFactorSet.AddFactor(new Factor("Level")
			{
				GetDescription = delegate {
					return string.Format(LanguageControl.Get(fName, 2), MathF.Floor(m_componentPlayer.PlayerData.Level).ToString());
				},
				GetValue = delegate
				{
					return 1f + (0.05f * MathF.Floor(Math.Clamp(m_componentPlayer.PlayerData.Level, 1f, MaxLevel) - 1f));
				}
			});
            m_strengthFactorSet.AddFactor(new Factor("Stamina")
            {
				GetDescription = delegate
				{
                    float stamina = m_componentPlayer.ComponentVitalStats.Stamina;
                    return string.Format(LanguageControl.Get(fName, 3), $"{stamina * 100f:0}");
                },
                GetValue = delegate
                {
                    float stamina = m_componentPlayer.ComponentVitalStats.Stamina;
                    return MathUtils.Lerp(0.5f, 1f, MathUtils.Saturate(4f * stamina)) * MathUtils.Lerp(0.9f, 1f, MathUtils.Saturate(stamina));
                }
            });
			m_strengthFactorSet.AddFactor(new Factor("Sickness")
			{
				GetDescription = delegate
				{
					return m_componentPlayer.ComponentSickness.IsSick ? LanguageControl.Get(fName, 4) : LanguageControl.Get(fName, 5);
                },
				GetValue = delegate
				{
					return m_componentPlayer.ComponentSickness.IsSick ? 0.75f : 1f;
				}
            });
            m_strengthFactorSet.AddFactor(new Factor("Puking")
            {
                GetDescription = delegate
                {
					return m_componentPlayer.ComponentSickness.IsPuking ? LanguageControl.Get(fName, 6) : LanguageControl.Get(fName, 7);
                },
                GetValue = delegate
                {
                    return (!m_componentPlayer.ComponentSickness.IsPuking) ? 1 : 0;
                }
            });
            m_strengthFactorSet.AddFactor(new Factor("Flu")
            {
                GetDescription = delegate
                {
					return m_componentPlayer.ComponentFlu.HasFlu ? LanguageControl.Get(fName, 8) : LanguageControl.Get(fName, 9);
                },
                GetValue = delegate
                {
                    return m_componentPlayer.ComponentFlu.HasFlu ? 0.75f : 1f;
                }
            });
            m_strengthFactorSet.AddFactor(new Factor("Coughing")
            {
                GetDescription = delegate
                {
					return m_componentPlayer.ComponentFlu.IsCoughing ? LanguageControl.Get(fName, 10) : LanguageControl.Get(fName, 11);
                },
                GetValue = delegate
                {
                    return (!m_componentPlayer.ComponentFlu.IsCoughing) ? 1 : 0;
                }
            });
            m_strengthFactorSet.AddFactor(new Factor("GameMode")
            {
                GetDescription = delegate
                {
					return string.Format(LanguageControl.Get(fName, 12), m_subsystemGameInfo.WorldSettings.GameMode.ToString());
                },
                GetValue = delegate
                {
                    return (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Harmless) ? 1.25f : 1f;
                }
            });
        }//自己写一个，或者用factorSet.Remove()，别老是覆盖
		private void LoadResilienceFactor()
		{
            m_resilienceFactorSet.AddFactor(new Factor("PlayerClass")
            {
                GetDescription = delegate {
                    return m_componentPlayer.PlayerData.PlayerClass.ToString();
                },
                GetValue = delegate
                {
                    return (m_componentPlayer.PlayerData.PlayerClass == PlayerClass.Female) ? 0.8f : 1f;
                }
            });
            m_resilienceFactorSet.AddFactor(new Factor("Level")
            {
                GetDescription = delegate {
                    return string.Format(LanguageControl.Get(fName, 2), MathF.Floor(m_componentPlayer.PlayerData.Level).ToString());
                },
                GetValue = delegate
                {
                    float level = m_componentPlayer.PlayerData.Level;
                    return 1f + (0.05f * MathF.Floor(Math.Clamp(level, 1f, MaxLevel) - 1f));
                }
            });
            m_resilienceFactorSet.AddFactor(new Factor("Sickness")
            {
                GetDescription = delegate {
					return m_componentPlayer.ComponentSickness.IsSick ? LanguageControl.Get(fName, 4) : LanguageControl.Get(fName, 5);
                },
                GetValue = delegate
                {
                    return m_componentPlayer.ComponentSickness.IsSick ? 0.75f : 1f;
                }
            });
            m_resilienceFactorSet.AddFactor(new Factor("Flu")
            {
                GetDescription = delegate {
					return m_componentPlayer.ComponentFlu.HasFlu ? LanguageControl.Get(fName, 8) : LanguageControl.Get(fName, 9);
                },
                GetValue = delegate
                {
                    return m_componentPlayer.ComponentFlu.HasFlu ? 0.75f : 1f;
                }
            });
            m_resilienceFactorSet.AddFactor(new Factor("GameMode")
            {
                GetDescription = delegate {
                    return string.Format(LanguageControl.Get(fName, 12), m_subsystemGameInfo.WorldSettings.GameMode.ToString());
                },
                GetValue = delegate
                {
					switch (m_subsystemGameInfo.WorldSettings.GameMode)
					{
						case GameMode.Creative: return float.PositiveInfinity;
						case GameMode.Harmless: return 1.5f;
						case GameMode.Survival: return 1.25f;
						default: return 1f;
                    }
                }
            });
            
		}
		private void LoadSpeedFactor()
		{
            m_speedFactorSet.AddFactor(new Factor("PlayerClass")
            {
                GetDescription = delegate {
                    return m_componentPlayer.PlayerData.PlayerClass.ToString();
                },
                GetValue = delegate
                {
                    return (m_componentPlayer.PlayerData.PlayerClass == PlayerClass.Female) ? 1.03f : 1f;
                }
            });
            m_speedFactorSet.AddFactor(new Factor("Level")
            {
                GetDescription = delegate {
					return string.Format(LanguageControl.Get(fName, 2), MathF.Floor(m_componentPlayer.PlayerData.Level).ToString());
                },
                GetValue = delegate
                {
                    return 1f + (0.02f * MathF.Floor(Math.Clamp(m_componentPlayer.PlayerData.Level, 1f, MaxLevel) - 1f));
                }
            });
            m_speedFactorSet.AddFactor(new Factor("Stamina")
            {
                GetDescription = delegate {
                    float stamina = m_componentPlayer.ComponentVitalStats.Stamina;
					return string.Format(LanguageControl.Get(fName, 3), $"{stamina * 100f:0}");
                },
                GetValue = delegate
                {
					float stamina = m_componentPlayer.ComponentVitalStats.Stamina;
                    return MathUtils.Lerp(0.5f, 1f, MathUtils.Saturate(4f * stamina)) * MathUtils.Lerp(0.9f, 1f, MathUtils.Saturate(stamina));
                }
            });
            m_speedFactorSet.AddFactor(new Factor("Sickness")
            {
                GetDescription = delegate {
                    return m_componentPlayer.ComponentSickness.IsSick ? LanguageControl.Get(fName, 4) : LanguageControl.Get(fName, 5);
                },
                GetValue = delegate
                {
                    return m_componentPlayer.ComponentSickness.IsSick ? 0.75f : 1f;
                }
            });
            m_speedFactorSet.AddFactor(new Factor("Puking")
            {
                GetDescription = delegate
                {
                    return m_componentPlayer.ComponentSickness.IsPuking ? LanguageControl.Get(fName, 6) : LanguageControl.Get(fName, 7);
                },
                GetValue = delegate
                {
                    return (!m_componentPlayer.ComponentSickness.IsPuking) ? 1 : 0;
                }
            });
            m_speedFactorSet.AddFactor(new Factor("Flu")
            {
                GetDescription = delegate
                {
                    return m_componentPlayer.ComponentFlu.HasFlu ? LanguageControl.Get(fName, 8) : LanguageControl.Get(fName, 9);
                },
                GetValue = delegate
                {
                    return m_componentPlayer.ComponentFlu.HasFlu ? 0.75f : 1f;
                }
            });
            m_speedFactorSet.AddFactor(new Factor("Coughing")
            {
                GetDescription = delegate
                {
                    return m_componentPlayer.ComponentFlu.IsCoughing ? LanguageControl.Get(fName, 10) : LanguageControl.Get(fName, 11);
                },
                GetValue = delegate
                {
                    return (!m_componentPlayer.ComponentFlu.IsCoughing) ? 1 : 0;
                }
            });
		}
		private void LoadHungerFactor()
		{
            m_hungerFactorSet.AddFactor(new Factor("PlayerClass")
            {
                GetDescription = delegate {
                    return m_componentPlayer.PlayerData.PlayerClass.ToString();
                },
                GetValue = delegate
                {
                    return (m_componentPlayer.PlayerData.PlayerClass == PlayerClass.Female) ? 0.7f : 1f;
                }
            });
            m_hungerFactorSet.AddFactor(new Factor("Level")
            {
                GetDescription = delegate {
                    return string.Format(LanguageControl.Get(fName, 2), MathF.Floor(m_componentPlayer.PlayerData.Level).ToString());
                },
                GetValue = delegate
                {
                    return 1f - (0.01f * MathF.Floor(Math.Clamp(m_componentPlayer.PlayerData.Level, 1f, MaxLevel) - 1f));
                }
            });
            m_hungerFactorSet.AddFactor(new Factor("GameMode")
            {
                GetDescription = delegate {
                    return string.Format(LanguageControl.Get(fName, 12), m_subsystemGameInfo.WorldSettings.GameMode.ToString());
                },
                GetValue = delegate
                {
                    switch (m_subsystemGameInfo.WorldSettings.GameMode)
                    {
                        case GameMode.Creative: return 0f;
                        case GameMode.Harmless: return 0.66f;
                        case GameMode.Survival: return 0.75f;
                        default: return 1f;
                    }
                }
            });
        }
        

        public virtual void LoadClothingFactor()
        {
            ClothingBlock clothingBlock = BlocksManager.GetBlock<ClothingBlock>();
            if (clothingBlock == null) return;
        }

        /*public static void OnClothingMounted(int value, ComponentClothing componentClothing)
        {
            ComponentFactors componentFactors = componentClothing.Entity.FindComponent<ComponentFactors>();
            ClothingBlock clothingBlock = BlocksManager.GetBlock<ClothingBlock>();
            if(componentFactors == null || clothingBlock == null) return;
            ClothingData clothingData = clothingBlock.GetClothingData(value);
            if (clothingData == null) return;
            ClothingFactor clothingFactor = componentFactors.m_speedFactorSet.GetFactor<ClothingFactor>("Clothing-" + clothingData.DisplayName);
            if (clothingFactor != null) clothingFactor = new ClothingFactor("Clothing-" + clothingData.DisplayName)
            {
                GetDescription = delegate {
                    return clothingData.DisplayName;
                },
                GetValue = delegate
                {
                    return clothingData.MovementSpeedFactor;
                }
            };
            componentFactors.m_speedFactorSet.AddFactor(clothingFactor);
        }*/

        
        public override void Update(float dt)
		{
			if (m_subsystemTime.PeriodicGameTimeEvent(180.0, 179.0))
			{
				AddExperience(1, playSound: false);
			}
			if (!m_lastLevelTextValue.HasValue || m_lastLevelTextValue.Value != MathF.Floor(m_componentPlayer.PlayerData.Level))
			{
				m_componentPlayer.ComponentGui.LevelLabelWidget.Text = string.Format(LanguageControl.Get(fName, 2), MathF.Floor(m_componentPlayer.PlayerData.Level).ToString());
				m_lastLevelTextValue = MathF.Floor(m_componentPlayer.PlayerData.Level);
			}
			m_componentPlayer.PlayerStats.HighestLevel = MathUtils.Max(m_componentPlayer.PlayerStats.HighestLevel, m_componentPlayer.PlayerData.Level);
            //UpdateClothingFactor();
			ModsManager.HookAction("OnLevelUpdate", modLoader =>
			{
				modLoader.OnLevelUpdate(this);
				return false;
			});
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			base.Load(valuesDictionary, idToEntityMap);
			m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: true);
            LoadStrengthFactor();
            LoadHungerFactor();
            LoadSpeedFactor();
            LoadResilienceFactor();
		}
	}
}
