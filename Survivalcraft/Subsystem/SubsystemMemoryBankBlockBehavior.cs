using Engine;

namespace Game
{
    public class SubsystemMemoryBankBlockBehavior : SubsystemEditableItemBehavior<MemoryBankData>
    {
        public override int[] HandledBlocks => new int[1]
        {
            186
        };
        public static string fName = "MemoryBankBlockBehavior";

        public SubsystemMemoryBankBlockBehavior()
            : base(186)
        {
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
        {
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = Terrain.ExtractData(value);
            MemoryBankData memoryBankData = GetItemData(id);
            memoryBankData = memoryBankData != null ? (MemoryBankData)memoryBankData.Copy() : new MemoryBankData();
            var listSelectionDialog = new ListSelectionDialog(LanguageControl.Get(fName,1), new int[] { 0, 1 }, 60f, (a) => {
                string[] l = new string[] { LanguageControl.Get(fName, 2), LanguageControl.Get(fName, 3) };
                return l[(int)a];
            }, (obj) => {
                if ((int)obj == 1)
                {
                    DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditMemeryDialogB(memoryBankData, delegate ()
                    {
                        int data = StoreItemDataAtUniqueId(memoryBankData);
                        int value2 = Terrain.ReplaceData(value, data);
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, value2, 1);
                    }));
                }
                else
                {
                    DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditMemoryBankDialog(memoryBankData, () => {
                        int data = StoreItemDataAtUniqueId(memoryBankData);
                        int value2 = Terrain.ReplaceData(value, data);
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, value2, 1);
                    }));
                }
            });
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, listSelectionDialog);
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
        {
            MemoryBankData memoryBankData = GetBlockData(new Point3(x, y, z)) ?? new MemoryBankData();
            var listSelectionDialog = new ListSelectionDialog(LanguageControl.Get(fName, 1), new int[] {0,1 }, 60f, (a)=> {
                string[] l = new string[] { LanguageControl.Get(fName, 2), LanguageControl.Get(fName, 3) };
                return l[(int)a]; 
            },(obj)=> {
                if ((int)obj == 1)
                {
                    DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditMemeryDialogB(memoryBankData, delegate ()
                    {
                        SetBlockData(new Point3(x, y, z), memoryBankData);
                        int face = ((MemoryBankBlock)BlocksManager.Blocks[186]).GetFace(value);
                        SubsystemElectricity subsystemElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemElectricity>(throwOnError: true);
                        ElectricElement electricElement = subsystemElectricity.GetElectricElement(x, y, z, face);
                        if (electricElement != null)
                        {
                            subsystemElectricity.QueueElectricElementForSimulation(electricElement, subsystemElectricity.CircuitStep + 1);
                        }
                    }));
                }else {
                    DialogsManager.ShowDialog(componentPlayer.GuiWidget,new EditMemoryBankDialog(memoryBankData,()=> {
                        SetBlockData(new Point3(x, y, z), memoryBankData);
                        int face = ((MemoryBankBlock)BlocksManager.Blocks[186]).GetFace(value);
                        SubsystemElectricity subsystemElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemElectricity>(throwOnError: true);
                        ElectricElement electricElement = subsystemElectricity.GetElectricElement(x, y, z, face);
                        if (electricElement != null)
                        {
                            subsystemElectricity.QueueElectricElementForSimulation(electricElement, subsystemElectricity.CircuitStep + 1);
                        }
                    }));
                }
            });
            DialogsManager.ShowDialog(componentPlayer.GuiWidget,listSelectionDialog);
            return true;
        }
    }
}
