using Engine;
using GameEntitySystem;
using TemplatesDatabase;
using static Game.SubsystemMovingBlocks;

namespace Game
{
	public class ComponentBlockEntity : Component
	{
		SubsystemMovingBlocks m_subsystemMovingBlocks;
		public MovingBlock MovingBlock { get; set; }
		public Point3 Coordinates
		{
			get;
			set;
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			m_subsystemMovingBlocks = Project.FindSubsystem<SubsystemMovingBlocks>(true);
			Coordinates = valuesDictionary.GetValue<Point3>("Coordinates");
			object movingBlocksTag = valuesDictionary.GetValue<object>("MovingBlocksTag", null);
			if(movingBlocksTag != null)
			{
				IMovingBlockSet movingBlockSet = m_subsystemMovingBlocks.MovingBlockSets.FirstOrDefault(set => set.Tag.ToString() == movingBlocksTag.ToString());
				if(movingBlockSet != null)
				{
					Point3 point = valuesDictionary.GetValue<Point3>("MovingBlockOffset");
					MovingBlock movingBlock = movingBlockSet.Blocks.FirstOrDefault(block => block.Offset == point);
					if(movingBlock != null)
					{
						MovingBlock = movingBlock;
					}
					else Log.Error("Required moving block offset " + point.ToString() + " is not found in MovingBlockSet " + movingBlocksTag.ToString() + "fot BlockEntity " + Entity.Id + ": " + Entity.ValuesDictionary.DatabaseObject.Name);
				}
				else Log.Error("Required moving block set " + movingBlocksTag.ToString() + " is not found in BlockEntity " + Entity.Id + ": " + Entity.ValuesDictionary.DatabaseObject.Name);
			}
		}

		public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap)
		{
			valuesDictionary.SetValue("Coordinates", Coordinates);
			if(MovingBlock != null) {
				valuesDictionary.SetValue("MovingBlocksTag",MovingBlock.MovingBlockSet.Tag);
				valuesDictionary.SetValue("MovingBlockOffset", MovingBlock.Offset);
			}
		}
	}
}
