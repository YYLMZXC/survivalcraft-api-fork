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

		public virtual void GatherPickable(ComponentBlockEntity blockEntity, WorldItem worldItem)
		{
			if(blockEntity != null)
			{
				ComponentChest inventory = blockEntity.Entity.FindComponent<ComponentChest>(throwOnError: true);
				var pickable = worldItem as Pickable;
				int num = pickable?.Count ?? 1;
				int num2 = ComponentInventoryBase.AcquireItems(inventory,worldItem.Value,num);
				if(num2 < num)
				{
					m_subsystemAudio.PlaySound("Audio/PickableCollected",1f,0f,worldItem.Position,3f,autoDelay: true);
				}
				if(num2 <= 0)
				{
					worldItem.ToRemove = true;
				}
				else if(pickable != null)
				{
					pickable.Count = num2;
				}
			}
		}
		public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
		{
			if (worldItem.ToRemove)
			{
				return;
			}
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(cellFace.X, cellFace.Y, cellFace.Z);
			GatherPickable(blockEntity, worldItem);
		}

		public override void OnHitByProjectile(MovingBlock movingBlock,WorldItem worldItem)
		{
			if(worldItem.ToRemove)
			{
				return;
			}
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(movingBlock);
			GatherPickable(blockEntity,worldItem);
		}
	}
}
