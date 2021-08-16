using Engine;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
    public class SubsystemBlockBehaviors : Subsystem
    {
        public SubsystemBlockBehavior[][] m_blockBehaviorsByContents;

        public List<SubsystemBlockBehavior> m_blockBehaviors = new List<SubsystemBlockBehavior>();

        public ReadOnlyList<SubsystemBlockBehavior> BlockBehaviors => new ReadOnlyList<SubsystemBlockBehavior>(m_blockBehaviors);

        public SubsystemBlockBehavior[] GetBlockBehaviors(int value)
        {
            return BlocksManager.Blocks[Terrain.ExtractContents(value)].GetBehaviors(Project,value).ToArray();
        }
        public override void Load(ValuesDictionary valuesDictionary)
        {
            foreach (var subsystem in Project.Subsystems) {
                if (subsystem is SubsystemBlockBehavior) m_blockBehaviors.Add(subsystem as SubsystemBlockBehavior);
            }
        }
    }
}
