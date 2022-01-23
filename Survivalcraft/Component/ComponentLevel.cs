using Engine;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;
using Engine.Graphics;

namespace Game
{
    public class ComponentLevel : Component, IUpdateable
    {
        public struct Factor
        {
            public string Description;

            public Texture2D IconTexture;

            public double endTime;

            public float Value;

            public double startTime;

        }

        public Random m_random = new Random();

        public static string fName = "ComponentLevel";

        public List<Factor> m_factors = new List<Factor>();

        public float? m_lastLevelTextValue;

        public SubsystemGameInfo m_subsystemGameInfo;

        public SubsystemAudio m_subsystemAudio;

        public SubsystemTime m_subsystemTime;

        public ComponentPlayer m_componentPlayer;

        public const float FemaleStrengthFactor = 0.8f;

        public const float FemaleResilienceFactor = 0.8f;

        public const float FemaleSpeedFactor = 1.03f;

        public const float FemaleHungerFactor = 0.7f;

        public Factor CreateFactor(float existsSeconds, string Description, float value, Texture2D texture)
        {
            Factor factor = new Factor();
            factor.Description = Description;
            factor.Value = value;
            factor.IconTexture = texture;
            factor.startTime = m_subsystemTime.GameTime;
            factor.endTime = m_subsystemTime.GameTime + existsSeconds;
            return factor;
        }

        public float StrengthFactor
        {
            get;
            set;
        }

        public float ResilienceFactor
        {
            get;
            set;
        }

        public float SpeedFactor
        {
            get;
            set;
        }

        public float HungerFactor
        {
            get;
            set;
        }

        public List<Factor> StrengthFactors = new List<Factor>();
        public List<Factor> ResilienceFactors = new List<Factor>();
        public List<Factor> SpeedFactors = new List<Factor>();
        public List<Factor> HungerFactors = new List<Factor>();

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public virtual void AddExperience(int count, bool playSound)
        {
            if (playSound)
            {
                m_subsystemAudio.PlaySound("Audio/ExperienceCollected", 0.2f, m_random.Float(-0.1f, 0.4f), 0f, 0f);
            }
            for (int i = 0; i < count; i++)
            {
                float num = 0.012f / MathUtils.Pow(1.08f, MathUtils.Floor(m_componentPlayer.PlayerData.Level - 1f));
                if (MathUtils.Floor(m_componentPlayer.PlayerData.Level + num) > MathUtils.Floor(m_componentPlayer.PlayerData.Level))
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

        public virtual float CalculateStrengthFactor()
        {
            StrengthFactors.Clear();
            ModsManager.HookAction("CalculateStrengthFactor", modLoader => {
                modLoader.CalculateStrengthFactor(this, StrengthFactors);
                return true;
            });
            StrengthFactors.Add(new Factor
            {
                Value = (m_componentPlayer.PlayerData.PlayerClass == PlayerClass.Female) ? 0.8f : 1f,
                Description = m_componentPlayer.PlayerData.PlayerClass.ToString()
            });
            StrengthFactors.Add(new Factor
            {
                Value = 1f + 0.05f * MathUtils.Floor(MathUtils.Clamp(m_componentPlayer.PlayerData.Level, 1f, 21f) - 1f),
                Description = string.Format(LanguageControl.Get(fName, 2), MathUtils.Floor(m_componentPlayer.PlayerData.Level).ToString())
            });
            StrengthFactors.Add(new Factor
            {
                Value = MathUtils.Lerp(0.5f, 1f, MathUtils.Saturate(4f * m_componentPlayer.ComponentVitalStats.Stamina)) * MathUtils.Lerp(0.9f, 1f, MathUtils.Saturate(m_componentPlayer.ComponentVitalStats.Stamina)),
                Description = string.Format(LanguageControl.Get(fName, 3), $"{m_componentPlayer.ComponentVitalStats.Stamina * 100f:0}")
            });
            StrengthFactors.Add(new Factor
            {
                Value = m_componentPlayer.ComponentSickness.IsSick ? 0.75f : 1f,
                Description = (m_componentPlayer.ComponentSickness.IsSick ? LanguageControl.Get(fName, 4) : LanguageControl.Get(fName, 5))
            });
            StrengthFactors.Add(new Factor
            {
                Value = (!m_componentPlayer.ComponentSickness.IsPuking) ? 1 : 0,
                Description = (m_componentPlayer.ComponentSickness.IsPuking ? LanguageControl.Get(fName, 6) : LanguageControl.Get(fName, 7))
            });
            StrengthFactors.Add(new Factor
            {
                Value = m_componentPlayer.ComponentFlu.HasFlu ? 0.75f : 1f,
                Description = (m_componentPlayer.ComponentFlu.HasFlu ? LanguageControl.Get(fName, 8) : LanguageControl.Get(fName, 9))
            });
            StrengthFactors.Add(new Factor
            {
                Value = (!m_componentPlayer.ComponentFlu.IsCoughing) ? 1 : 0,
                Description = (m_componentPlayer.ComponentFlu.IsCoughing ? LanguageControl.Get(fName, 10) : LanguageControl.Get(fName, 11))
            });
            StrengthFactors.Add(new Factor
            {
                Value = (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Harmless) ? 1.25f : 1f,
                Description = string.Format(LanguageControl.Get(fName, 12), m_subsystemGameInfo.WorldSettings.GameMode.ToString())
            });
            float result = 1f;
            foreach (var f in StrengthFactors)
            {
                result *= f.Value;
            }
            return result;
        }

        public virtual float CalculateResilienceFactor()
        {
            ResilienceFactors.Clear();
            ModsManager.HookAction("CalculateResilienceFactor", modLoader => {
                modLoader.CalculateResilienceFactor(this, StrengthFactors);
                return true;
            });
            ResilienceFactors.Add(new Factor
            {
                Value = (m_componentPlayer.PlayerData.PlayerClass == PlayerClass.Female) ? 0.8f : 1f,
                Description = m_componentPlayer.PlayerData.PlayerClass.ToString()
            });
            ResilienceFactors.Add(new Factor
            {
                Value = 1f + 0.05f * MathUtils.Floor(MathUtils.Clamp(m_componentPlayer.PlayerData.Level, 1f, 21f) - 1f),
                Description = string.Format(LanguageControl.Get(fName, 2), MathUtils.Floor(m_componentPlayer.PlayerData.Level).ToString())
            });
            ResilienceFactors.Add(new Factor
            {
                Value = m_componentPlayer.ComponentSickness.IsSick ? 0.75f : 1f,
                Description = (m_componentPlayer.ComponentSickness.IsSick ? LanguageControl.Get(fName, 4) : LanguageControl.Get(fName, 5))
            });
            ResilienceFactors.Add(new Factor
            {
                Value = m_componentPlayer.ComponentFlu.HasFlu ? 0.75f : 1f,
                Description = (m_componentPlayer.ComponentFlu.HasFlu ? LanguageControl.Get(fName, 8) : LanguageControl.Get(fName, 9))
            });
            ResilienceFactors.Add(new Factor
            {
                Value = m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Harmless ? 1.5f : (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative ? float.PositiveInfinity : 1f),
                Description = string.Format(LanguageControl.Get(fName, 12), m_subsystemGameInfo.WorldSettings.GameMode.ToString())
            });
            float result = 1f;
            foreach (var f in ResilienceFactors)
            {
                result *= f.Value;
            }
            return result;
        }

        public virtual float CalculateSpeedFactor()
        {
            SpeedFactors.Clear();
            ModsManager.HookAction("CalculateSpeedFactor", modLoader => {
                modLoader.CalculateSpeedFactor(this, StrengthFactors);
                return true;
            });
            SpeedFactors.Add(new Factor
            {
                Value = (m_componentPlayer.PlayerData.PlayerClass == PlayerClass.Female) ? 1.03f : 1f,
                Description = m_componentPlayer.PlayerData.PlayerClass.ToString()
            });
            SpeedFactors.Add(new Factor
            {
                Value = 1f + 0.02f * MathUtils.Floor(MathUtils.Clamp(m_componentPlayer.PlayerData.Level, 1f, 21f) - 1f),
                Description = string.Format(LanguageControl.Get(fName, 2), MathUtils.Floor(m_componentPlayer.PlayerData.Level).ToString())
            });

            float clothingFactor = 1f;
            foreach (int clothe in m_componentPlayer.ComponentClothing.GetClothes(ClothingSlot.Head))
            {
                AddClothingFactor(clothe, ref clothingFactor, SpeedFactors);
            }
            foreach (int clothe2 in m_componentPlayer.ComponentClothing.GetClothes(ClothingSlot.Torso))
            {
                AddClothingFactor(clothe2, ref clothingFactor, SpeedFactors);
            }
            foreach (int clothe3 in m_componentPlayer.ComponentClothing.GetClothes(ClothingSlot.Legs))
            {
                AddClothingFactor(clothe3, ref clothingFactor, SpeedFactors);
            }
            foreach (int clothe4 in m_componentPlayer.ComponentClothing.GetClothes(ClothingSlot.Feet))
            {
                AddClothingFactor(clothe4, ref clothingFactor, SpeedFactors);
            }
            SpeedFactors.Add(new Factor
            {
                Value = MathUtils.Lerp(0.5f, 1f, MathUtils.Saturate(4f * m_componentPlayer.ComponentVitalStats.Stamina)) * MathUtils.Lerp(0.9f, 1f, MathUtils.Saturate(m_componentPlayer.ComponentVitalStats.Stamina)),
                Description = string.Format(LanguageControl.Get(fName, 3), $"{m_componentPlayer.ComponentVitalStats.Stamina * 100f:0}")
            });
            SpeedFactors.Add(new Factor
            {
                Value = m_componentPlayer.ComponentSickness.IsSick ? 0.75f : 1f,
                Description = (m_componentPlayer.ComponentSickness.IsSick ? LanguageControl.Get(fName, 4) : LanguageControl.Get(fName, 5))
            });
            SpeedFactors.Add(new Factor
            {
                Value = (!m_componentPlayer.ComponentSickness.IsPuking) ? 1 : 0,
                Description = (m_componentPlayer.ComponentSickness.IsPuking ? LanguageControl.Get(fName, 6) : LanguageControl.Get(fName, 7))
            });
            SpeedFactors.Add(new Factor
            {
                Value = m_componentPlayer.ComponentFlu.HasFlu ? 0.75f : 1f,
                Description = (m_componentPlayer.ComponentFlu.HasFlu ? LanguageControl.Get(fName, 8) : LanguageControl.Get(fName, 9))
            });
            SpeedFactors.Add(new Factor
            {
                Value = (!m_componentPlayer.ComponentFlu.IsCoughing) ? 1 : 0,
                Description = (m_componentPlayer.ComponentFlu.IsCoughing ? LanguageControl.Get(fName, 10) : LanguageControl.Get(fName, 11))
            });
            float result = clothingFactor;
            foreach (var f in SpeedFactors)
            {
                result *= f.Value;
            }
            return result;

        }

        public virtual float CalculateHungerFactor()
        {
            HungerFactors.Clear();
            ModsManager.HookAction("CalculateSpeedFactor", modLoader => {
                modLoader.CalculateSpeedFactor(this, StrengthFactors);
                return true;
            });
            HungerFactors.Add(new Factor
            {
                Value = (m_componentPlayer.PlayerData.PlayerClass == PlayerClass.Female) ? 0.7f : 1f,
                Description = m_componentPlayer.PlayerData.PlayerClass.ToString()
            });
            HungerFactors.Add(new Factor
            {
                Value = 1f - 0.01f * MathUtils.Floor(MathUtils.Clamp(m_componentPlayer.PlayerData.Level, 1f, 21f) - 1f),
                Description = string.Format(LanguageControl.Get(fName, 2), MathUtils.Floor(m_componentPlayer.PlayerData.Level).ToString())
            });
            HungerFactors.Add(new Factor
            {
                Value = (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Harmless) ? 1.25f : 1f,
                Description = string.Format(LanguageControl.Get(fName, 12), m_subsystemGameInfo.WorldSettings.GameMode.ToString())
            });
            float result = 1f;
            foreach (var f in HungerFactors)
            {
                result *= f.Value;
            }
            return result;
        }

        public virtual void Update(float dt)
        {
            if (m_subsystemTime.PeriodicGameTimeEvent(180.0, 179.0))
            {
                AddExperience(1, playSound: false);
            }
            if (!m_lastLevelTextValue.HasValue || m_lastLevelTextValue.Value != MathUtils.Floor(m_componentPlayer.PlayerData.Level))
            {
                m_componentPlayer.ComponentGui.LevelLabelWidget.Text = string.Format(LanguageControl.Get(fName, 2), MathUtils.Floor(m_componentPlayer.PlayerData.Level).ToString());
                m_lastLevelTextValue = MathUtils.Floor(m_componentPlayer.PlayerData.Level);
            }
            m_componentPlayer.PlayerStats.HighestLevel = MathUtils.Max(m_componentPlayer.PlayerStats.HighestLevel, m_componentPlayer.PlayerData.Level);
            StrengthFactor = CalculateStrengthFactor();
            SpeedFactor = CalculateSpeedFactor();
            HungerFactor = CalculateHungerFactor();
            ResilienceFactor = CalculateResilienceFactor();
            ModsManager.HookAction("OnLevelUpdate", modLoader => {
                modLoader.OnLevelUpdate(this);
                return false;
            });
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
        {
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
            m_componentPlayer = Entity.FindComponent<ComponentPlayer>(throwOnError: true);
            StrengthFactor = 1f;
            SpeedFactor = 1f;
            HungerFactor = 1f;
            ResilienceFactor = 1f;
        }

        public static void AddClothingFactor(int clothingValue, ref float clothingFactor, ICollection<Factor> factors)
        {
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(clothingValue)];
            ClothingData clothingData = block.GetClothingData(clothingValue);
            if (clothingData.MovementSpeedFactor != 1f)
            {
                clothingFactor *= clothingData.MovementSpeedFactor;
                factors?.Add(new Factor
                {
                    Value = clothingData.MovementSpeedFactor,
                    Description = clothingData.DisplayName
                });
            }
        }
    }
}
