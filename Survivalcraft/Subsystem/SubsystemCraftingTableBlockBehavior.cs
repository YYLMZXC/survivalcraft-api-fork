using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
    public class SubsystemCraftingTableBlockBehavior : SubsystemBlockBehavior
    {
        public override int[] HandledBlocks => new int[1]
        {
            27
        };

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
        {
            DatabaseObject databaseObject = SubsystemTerrain.Project.GameDatabase.Database.FindDatabaseObject("CraftingTable", SubsystemTerrain.Project.GameDatabase.EntityTemplateType, throwIfNotFound: true);
            var valuesDictionary = new ValuesDictionary();
            valuesDictionary.PopulateFromDatabaseObject(databaseObject);
            valuesDictionary.GetValue<ValuesDictionary>("BlockEntity").SetValue("Coordinates", new Point3(x, y, z));
            Entity entity = SubsystemTerrain.Project.CreateEntity(valuesDictionary);
            SubsystemTerrain.Project.AddEntity(entity);
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
        {
            ComponentBlockEntity blockEntity = SubsystemTerrain.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true).GetBlockEntity(x, y, z);
            if (blockEntity != null)
            {
                Vector3 position = new Vector3(x, y, z) + new Vector3(0.5f);
                foreach (IInventory item in blockEntity.Entity.FindComponents<IInventory>())
                {
                    item.DropAllItems(position);
                }
                SubsystemTerrain.Project.RemoveEntity(blockEntity.Entity, disposeEntity: true);
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
        {
            ComponentBlockEntity blockEntity = SubsystemTerrain.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true).GetBlockEntity(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
            if (blockEntity != null && componentMiner.ComponentPlayer != null)
            {
                ComponentCraftingTable componentCraftingTable = blockEntity.Entity.FindComponent<ComponentCraftingTable>(throwOnError: true);
                componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new CraftingTableWidget(componentMiner.Inventory, componentCraftingTable);
                AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                return true;
            }
            return false;
        }
    }
}
