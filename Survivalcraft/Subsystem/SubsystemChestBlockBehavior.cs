using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemChestBlockBehavior : SubsystemEntityBlockBehavior
	{
		public override int[] HandledBlocks => new int[1]
		{
			45
		};

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_databaseObject = Project.GameDatabase.Database.FindDatabaseObject("Chest",Project.GameDatabase.EntityTemplateType,throwIfNotFound: true);
		}

		public override bool InteractBlockEntity(ComponentBlockEntity blockEntity,ComponentMiner componentMiner)
		{
			if(blockEntity != null && componentMiner.ComponentPlayer != null)
			{
				ComponentChest componentChest = blockEntity.Entity.FindComponent<ComponentChest>(throwOnError: true);
				componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new ChestWidget(componentMiner.Inventory,componentChest);
				AudioManager.PlaySound("Audio/UI/ButtonClick",1f,0f,0f);
				return true;
			}
			return false;
		}
	}
}
