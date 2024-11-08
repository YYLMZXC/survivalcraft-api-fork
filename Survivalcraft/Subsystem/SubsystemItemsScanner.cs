using Engine;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemItemsScanner : Subsystem, IUpdateable
	{
		public const float m_automaticScanPeriod = 60f;

		public double m_nextAutomaticScanTime;

		public List<ScannedItemData> m_items = [];

		public UpdateOrder UpdateOrder => UpdateOrder.Default;

		public virtual Action<ReadOnlyList<ScannedItemData>> ItemsScanned { get; set; }

		public ReadOnlyList<ScannedItemData> ScanItems()
		{
			m_items.Clear();
			foreach (Subsystem subsystem in Project.Subsystems)
			{
				var inventory = subsystem as IInventory;
				if (inventory != null)
				{
					ScanInventory(inventory, m_items);
				}
			}
			foreach (Entity entity in Project.Entities)
			{
				foreach (Component component in entity.Components)
				{
					var inventory2 = component as IInventory;
					if (inventory2 != null)
					{
						ScanInventory(inventory2, m_items);
					}
				}
			}
			ScannedItemData item;
			foreach (Pickable pickable in Project.FindSubsystem<SubsystemPickables>(throwOnError: true).Pickables)
			{
				if (pickable.Count > 0 && pickable.Value != 0)
				{
					List<ScannedItemData> items = m_items;
					item = new ScannedItemData
					{
						Container = pickable,
						Value = pickable.Value,
						Count = pickable.Count
					};
					items.Add(item);
				}
			}
			foreach (Projectile projectile in Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true).Projectiles)
			{
				if (projectile.Value != 0)
				{
					List<ScannedItemData> items2 = m_items;
					item = new ScannedItemData
					{
						Container = projectile,
						Value = projectile.Value,
						Count = 1
					};
					items2.Add(item);
				}
			}
			foreach (IMovingBlockSet movingBlockSet in Project.FindSubsystem<SubsystemMovingBlocks>(throwOnError: true).MovingBlockSets)
			{
				for (int i = 0; i < movingBlockSet.Blocks.Count; i++)
				{
					List<ScannedItemData> items3 = m_items;
					item = new ScannedItemData
					{
						Container = movingBlockSet,
						Value = movingBlockSet.Blocks[i].Value,
						Count = 1,
						IndexInContainer = i
					};
					items3.Add(item);
				}
			}
			return new ReadOnlyList<ScannedItemData>(m_items);
		}

		public bool TryModifyItem(ScannedItemData itemData, int newValue)
		{
			if (itemData.Container is IInventory)
			{
				var obj = (IInventory)itemData.Container;
				int slotCapacity = obj.GetSlotCapacity(itemData.IndexInContainer,newValue);
				if(slotCapacity < itemData.Count) return false;
				obj.RemoveSlotItems(itemData.IndexInContainer, itemData.Count);
				obj.AddSlotItems(itemData.IndexInContainer, newValue, itemData.Count);
				return true;
			}
			if (itemData.Container is WorldItem)
			{
				((WorldItem)itemData.Container).Value = newValue;
				return true;
			}
			if (itemData.Container is IMovingBlockSet)
			{
				var obj2 = (IMovingBlockSet)itemData.Container;
				MovingBlock movingBlock = obj2.Blocks.ElementAt(itemData.IndexInContainer);
				obj2.SetBlock(movingBlock.Offset, newValue);
				return true;
			}
			return false;
		}

		public void Update(float dt)
		{
			if (Time.FrameStartTime >= m_nextAutomaticScanTime)
			{
				m_nextAutomaticScanTime = Time.FrameStartTime + 60.0;
				ItemsScanned?.Invoke(ScanItems());
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_nextAutomaticScanTime = Time.FrameStartTime + 60.0;
		}

		public void ScanInventory(IInventory inventory, List<ScannedItemData> items)
		{
			for (int i = 0; i < inventory.SlotsCount; i++)
			{
				int slotCount = inventory.GetSlotCount(i);
				if (slotCount > 0)
				{
					int slotValue = inventory.GetSlotValue(i);
					if (slotValue != 0)
					{
						items.Add(new ScannedItemData
						{
							Container = inventory,
							IndexInContainer = i,
							Value = slotValue,
							Count = slotCount
						});
					}
				}
			}
		}
	}
}
