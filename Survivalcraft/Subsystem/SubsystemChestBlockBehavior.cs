using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemChestBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemBlockEntities m_subsystemBlockEntities;

		public SubsystemAudio m_subsystemAudio;

		public override int[] HandledBlocks => new int[1]
		{
			45
		};

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemBlockEntities = Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
			m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
		}

		public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
			Log.Information("Chest Block Added.");
			DatabaseObject databaseObject = Project.GameDatabase.Database.FindDatabaseObject("Chest", Project.GameDatabase.EntityTemplateType, throwIfNotFound: true);
			var valuesDictionary = new ValuesDictionary();
			valuesDictionary.PopulateFromDatabaseObject(databaseObject);
			valuesDictionary.GetValue<ValuesDictionary>("BlockEntity").SetValue("Coordinates", new Point3(x, y, z));
			Entity entity = Project.CreateEntity(valuesDictionary);
			Project.AddEntity(entity);
		}

		public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
		{
			Log.Information("Chest Block Removed.");
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(x, y, z);
			if (blockEntity != null)
			{
				Vector3 position = new Vector3(x, y, z) + new Vector3(0.5f);
				foreach (IInventory item in blockEntity.Entity.FindComponents<IInventory>())
				{
					item.DropAllItems(position);
				}
				Project.RemoveEntity(blockEntity.Entity, disposeEntity: true);
			}
		}

		public override void OnBlockStartMoving(int value,int newValue,int x,int y,int z,MovingBlock movingBlock)
		{
			Log.Information("Chest Block Start Moving");
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(x,y,z);
			if(blockEntity != null)
			{
				m_subsystemBlockEntities.m_blockEntities.Remove(blockEntity.Coordinates);
				m_subsystemBlockEntities.m_movingBlockEntities[movingBlock] = blockEntity;
				blockEntity.MovingBlock = movingBlock;
				blockEntity.Coordinates = new Point3(0,-99999,0);
			}
		}

		public override void OnBlockStopMoving(int value,int oldValue,int x,int y,int z,MovingBlock movingBlock)
		{
			Log.Information("Chest Block Stop Moving");
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(movingBlock);
			if(blockEntity != null)
			{
				m_subsystemBlockEntities.m_movingBlockEntities.Remove(movingBlock);
				m_subsystemBlockEntities.m_blockEntities[new Point3(x, y, z)] = blockEntity;
				blockEntity.MovingBlock = null;
				blockEntity.Coordinates = new Point3(x,y,z);
			}
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
			if (blockEntity != null && componentMiner.ComponentPlayer != null)
			{
				ComponentChest componentChest = blockEntity.Entity.FindComponent<ComponentChest>(throwOnError: true);
				componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new ChestWidget(componentMiner.Inventory, componentChest);
				AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
				return true;
			}
			return false;
		}

		public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
		{
			if (worldItem.ToRemove)
			{
				return;
			}
			ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(cellFace.X, cellFace.Y, cellFace.Z);
			if (blockEntity != null)
			{
				ComponentChest inventory = blockEntity.Entity.FindComponent<ComponentChest>(throwOnError: true);
				var pickable = worldItem as Pickable;
				int num = pickable?.Count ?? 1;
				int num2 = ComponentInventoryBase.AcquireItems(inventory, worldItem.Value, num);
				if (num2 < num)
				{
					m_subsystemAudio.PlaySound("Audio/PickableCollected", 1f, 0f, worldItem.Position, 3f, autoDelay: true);
				}
				if (num2 <= 0)
				{
					worldItem.ToRemove = true;
				}
				else if (pickable != null)
				{
					pickable.Count = num2;
				}
			}
		}
	}
}
