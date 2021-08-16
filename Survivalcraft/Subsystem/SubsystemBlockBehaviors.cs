using Engine;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
    public class SubsystemBlockBehaviors : Subsystem
    {
        public List<SubsystemBlockBehavior> m_blockBehaviors = new List<SubsystemBlockBehavior>();

        public Dictionary<int, List<SubsystemBlockBehavior>> m_blockBehaviorsbyid = new Dictionary<int, List<SubsystemBlockBehavior>>();

        public ReadOnlyList<SubsystemBlockBehavior> BlockBehaviors => new ReadOnlyList<SubsystemBlockBehavior>(m_blockBehaviors);

        public SubsystemBlockBehavior[] GetBlockBehaviors(int value)
        {
            int id = Terrain.ExtractContents(value);
            List<SubsystemBlockBehavior> behaviors = BlocksManager.Blocks[id].GetBehaviors(Project, value);
            if (m_blockBehaviorsbyid.TryGetValue(id, out List<SubsystemBlockBehavior> bs))
            {
                foreach (var c in bs)
                {
                    if (!behaviors.Contains(c)) behaviors.Add(c);
                }

            }
            return behaviors.ToArray();
        }
        public override void Load(ValuesDictionary valuesDictionary)
        {
            foreach (var subsystem in Project.Subsystems) {
                if (subsystem is SubsystemBlockBehavior)
                {
                    SubsystemBlockBehavior behavior = subsystem as SubsystemBlockBehavior;
                    foreach (int id in behavior.HandledBlocks)
                    {
                        if (m_blockBehaviorsbyid.TryGetValue(id, out List<SubsystemBlockBehavior> bs))
                        {
                            if (!bs.Contains(behavior)) bs.Add(behavior);
                        }
                        else
                        {
                            bs = new List<SubsystemBlockBehavior>();
                            bs.Add(behavior);
                            m_blockBehaviorsbyid.Add(id, bs);
                        }
                    }
                    m_blockBehaviors.Add(behavior);
                }
            }
        }
    }
}
