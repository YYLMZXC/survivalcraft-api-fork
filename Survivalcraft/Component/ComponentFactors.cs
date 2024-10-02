using GameEntitySystem;
using Engine;
using TemplatesDatabase;
using System.Reflection;

namespace Game
{
    public enum FactorCalculateMethod
    {
        Add,
        Multiply,
    }
    public class Factor
    {
        public string Name;
        public string Description
        {
            get { return GetDescription(); }
            set { }
        }
        public FactorCalculateMethod FactorCalculateMethod = FactorCalculateMethod.Multiply;
        public int CalculateOrder = 100;
        public float Value { get { return GetValue(); } set { } }
        public Func<float> GetValue = delegate { return 1; };
        public Func<string> GetDescription = delegate { return String.Empty; };
        public Factor(string name)
        {
            Name = name;
        }
    }

    public class ClothingFactor : Factor
    {
        public ClothingFactor(string name) : base(name) { }
    }
    public class FactorComparer : IComparer<Factor>
    {
        public static FactorComparer Instance = new();
        public int Compare(Factor f1, Factor f2)
        {
            int CalculateOrderMinus = f1.CalculateOrder - f2.CalculateOrder;
            if (CalculateOrderMinus != 0) return CalculateOrderMinus;
            return -1;
        }
    }

    public class FactorSet
    {
        public string Name = String.Empty;

        public List<Factor> Factors = new List<Factor>();

        public FactorSet(string name)
        {
            Name = name;
        }
        public float Answer => GetFactorAnswer();
        public virtual float GetFactorAnswer()
        {
            float answer = 1f;
            for (int i = 0; i < Factors.Count; i++)
            {
                Factor factor = Factors[i];
                switch (factor.FactorCalculateMethod)
                {
                    case FactorCalculateMethod.Multiply:
                        {
                            answer *= factor.Value;
                            break;
                        }
                    case FactorCalculateMethod.Add:
                        {
                            answer += factor.Value;
                            break;
                        }
                }
            }
            return answer;
        }
        public Factor GetFactor(Type type, string name)
        {
            for (int i = 0; i < Factors.Count; i++)
            {
                Factor factor = Factors[i];
                if (type.GetTypeInfo().IsAssignableFrom(factor.GetType().GetTypeInfo()) && (name == null || name == factor.Name))
                {
                    return factor;
                }
            }
            return null;
        }
        public T GetFactor<T>(string name = null) where T : Factor
        {
            return GetFactor(typeof(T), name) as T;
        }
        public bool RemoveFactor(Factor factor)
        {
            lock (Factors)
            {
                return Factors.Remove(factor);
            }
        }
        public void AddFactor(Factor factor)
        {
            lock (Factors)
            {
                Factors.Add(factor);
                Factors.Sort(FactorComparer.Instance);
            }
        }
    }
    public class ComponentFactors : Component, IUpdateable
    {
        public Random m_random = new();

        public SubsystemGameInfo m_subsystemGameInfo;

        public SubsystemAudio m_subsystemAudio;

        public SubsystemTime m_subsystemTime;
        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public static string fName = "ComponentFactors";

        public List<FactorSet> FactorSets = new List<FactorSet>() { };
        public FactorSet GetFactorSet(Type type, string name)
        {
            for (int i = 0; i < FactorSets.Count; i++)
            {
                FactorSet factor = FactorSets[i];
                if (type.GetTypeInfo().IsAssignableFrom(factor.GetType().GetTypeInfo()) && (name == null || name == factor.Name))
                {
                    return factor;
                }
            }
            return null;
        }
        public T GetFactorSet<T>(string name = null) where T : FactorSet
        {
            return GetFactorSet(typeof(T), name) as T;
        }

        public float GetFactorSetAnswer<T>(string name = null, bool throwOnError = false) where T : FactorSet
        {
            FactorSet factorSet = GetFactorSet<T>(name);
            if (factorSet != null) return factorSet.Answer;
            if (throwOnError) throw new NullReferenceException("The Factor set is not found.");
            return 1;
        }

        public FactorSet m_strengthFactorSet = new FactorSet("StrengthFactorSet");
        public FactorSet m_resilienceFactorSet = new FactorSet("ResilienceFactorSet");
        public FactorSet m_speedFactorSet = new FactorSet("SpeedFactorSet");
        public FactorSet m_hungerFactorSet = new FactorSet("HungerFactorSet");

        [Obsolete("Use ComponentFactors.GetFactorSetAnswer instead.")]
        public float StrengthFactor {
            get { return m_strengthFactorSet.Answer; }
            set { }
        }
        [Obsolete("Use ComponentFactors.GetFactorSetAnswer instead.")]
        public float SpeedFactor
        {
            get { return m_speedFactorSet.Answer; }
            set { }
        }
        [Obsolete("Use ComponentFactors.GetFactorSetAnswer instead.")]
        public float ResilienceFactor
        {
            get { return m_resilienceFactorSet.Answer; }
            set { }
        }
        [Obsolete("Use ComponentFactors.GetFactorSetAnswer instead.")]
        public float HungerFactor
        {
            get { return m_hungerFactorSet.Answer; }
            set { }
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
        {
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(throwOnError: true);
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
            if (FactorSets.Count == 0) LoadFactorSets();
            
        }

        public virtual void LoadFactorSets()
        {
            FactorSets.Add(m_strengthFactorSet);
            FactorSets.Add(m_resilienceFactorSet);
            FactorSets.Add(m_speedFactorSet);
            FactorSets.Add(m_hungerFactorSet);
        }

        public virtual void Update(float dt)
        {
            for (int i = 0; i < FactorSets.Count; i++)
            {
                FactorSet factorSet = FactorSets[i];
                //factorSet.Update(dt);
            }
        }
    }
}
