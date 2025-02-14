using Engine;
using System.Xml.Linq;

namespace Game
{
	public class DispenserWidget : CanvasWidget
	{
		public SubsystemTerrain m_subsystemTerrain;

		public ComponentDispenser m_componentDispenser;

		public ComponentBlockEntity m_componentBlockEntity;

		public GridPanelWidget m_inventoryGrid;

		public GridPanelWidget m_dispenserGrid;

		public ButtonWidget m_dispenseButton;

		public ButtonWidget m_shootButton;

		public CheckboxWidget m_acceptsDropsBox;

		public DispenserWidget(IInventory inventory, ComponentDispenser componentDispenser)
		{
			m_componentDispenser = componentDispenser;
			m_componentBlockEntity = componentDispenser.Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
			m_subsystemTerrain = componentDispenser.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			XElement node = ContentManager.Get<XElement>("Widgets/DispenserWidget");
			LoadContents(this, node);
			m_inventoryGrid = Children.Find<GridPanelWidget>("InventoryGrid");
			m_dispenserGrid = Children.Find<GridPanelWidget>("DispenserGrid");
			m_dispenseButton = Children.Find<ButtonWidget>("DispenseButton");
			m_shootButton = Children.Find<ButtonWidget>("ShootButton");
			m_acceptsDropsBox = Children.Find<CheckboxWidget>("AcceptsDropsBox");
			int num = 0;
			for (int i = 0; i < m_dispenserGrid.RowsCount; i++)
			{
				for (int j = 0; j < m_dispenserGrid.ColumnsCount; j++)
				{
					var inventorySlotWidget = new InventorySlotWidget();
					inventorySlotWidget.AssignInventorySlot(componentDispenser, num++);
					m_dispenserGrid.Children.Add(inventorySlotWidget);
					m_dispenserGrid.SetWidgetCell(inventorySlotWidget, new Point2(j, i));
				}
			}
			num = 10;
			for (int k = 0; k < m_inventoryGrid.RowsCount; k++)
			{
				for (int l = 0; l < m_inventoryGrid.ColumnsCount; l++)
				{
					var inventorySlotWidget2 = new InventorySlotWidget();
					inventorySlotWidget2.AssignInventorySlot(inventory, num++);
					m_inventoryGrid.Children.Add(inventorySlotWidget2);
					m_inventoryGrid.SetWidgetCell(inventorySlotWidget2, new Point2(l, k));
				}
			}
		}

		public override void Update()
		{
			int value = m_componentBlockEntity.BlockValue;
			int data = Terrain.ExtractData(value);
			if (m_dispenseButton.IsClicked)
			{
				data = DispenserBlock.SetMode(data, DispenserBlock.Mode.Dispense);
				value = Terrain.ReplaceData(value, data);
				m_componentBlockEntity.BlockValue = value;
			}
			if (m_shootButton.IsClicked)
			{
				data = DispenserBlock.SetMode(data, DispenserBlock.Mode.Shoot);
				value = Terrain.ReplaceData(value, data);
				m_componentBlockEntity.BlockValue = value;
			}
			if (m_acceptsDropsBox.IsClicked)
			{
				data = DispenserBlock.SetAcceptsDrops(data, !DispenserBlock.GetAcceptsDrops(data));
				value = Terrain.ReplaceData(value, data);
				m_componentBlockEntity.BlockValue = value;
			}
			DispenserBlock.Mode mode = DispenserBlock.GetMode(data);
			m_dispenseButton.IsChecked = mode == DispenserBlock.Mode.Dispense;
			m_shootButton.IsChecked = mode == DispenserBlock.Mode.Shoot;
			m_acceptsDropsBox.IsChecked = DispenserBlock.GetAcceptsDrops(data);
			if (!m_componentDispenser.IsAddedToProject)
			{
				ParentWidget.Children.Remove(this);
			}
		}
	}
}
