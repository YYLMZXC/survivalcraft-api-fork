using GameEntitySystem;
using Engine;
using TemplatesDatabase;
using System.Reflection;
using static Game.ComponentLevel;

namespace Game
{
    public class ComponentFactors : Component, IUpdateable
    {
        public Random m_random = new();

        public SubsystemGameInfo m_subsystemGameInfo;

        public SubsystemAudio m_subsystemAudio;

        public SubsystemTime m_subsystemTime;

        public List<Factor> m_factors = [];
        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public static string fName = "ComponentFactors";

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

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
        {
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
            StrengthFactor = 1f;
            SpeedFactor = 1f;
            HungerFactor = 1f;
            ResilienceFactor = 1f;
        }
        public virtual float CalculateStrengthFactor(ICollection<Factor> factors) {
            return 1f;
        }
        public virtual float CalculateResilienceFactor(ICollection<Factor> factors)
        {
            return 1f;
        }
        public virtual float CalculateSpeedFactor(ICollection<Factor> factors)
        {
            return 1f;
        }
        public virtual float CalculateHungerFactor(ICollection<Factor> factors)
        {
            return 1f;
        }
        public virtual void Update(float dt)
        {
            StrengthFactor = CalculateStrengthFactor(null);
            SpeedFactor = CalculateSpeedFactor(null);
            HungerFactor = CalculateHungerFactor(null);
            ResilienceFactor = CalculateResilienceFactor(null);
            ModsManager.HookAction("OnFactorsUpdate", Loader =>
            {
                Loader.OnFactorsUpdate(this, dt);
                return false;
            });
        }
    }
}
