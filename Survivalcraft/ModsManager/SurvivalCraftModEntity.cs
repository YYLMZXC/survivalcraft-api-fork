using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
namespace Game
{
    public class SurvivalCrafModEntity : ModEntity
    {

        public SurvivalCrafModEntity(){
            modInfo = new ModInfo();
            modInfo.ApiVersion = "1.34";
            modInfo.ScVersion = "2.2.10.4";
            modInfo.Name = "SurvivalCraft";
            modInfo.Version = "2.2.10.4";
            modInfo.PackageName = "com.survivalcraf";
            using (Stream stream = Storage.OpenFile("app:Content.zip", OpenFileMode.Read))
            {
                ModArchive = ZipArchive.Open(stream, true);
            }
        }
        public override void CheckDependencies()
        {
        }
        public override bool GetFile(string filename, out Stream stream)
        {
            stream = null;
            return false;
        }
        public override void InitPak()
        {
            //ContentManager.Add("app:/Content.pak");
        }
        public override void LoadBlocksData()
        {
            BlocksManager.LoadBlocksData(ContentManager.Get<string>("BlocksData"));
            ContentManager.Dispose("BlocksData");

        }
        public override void LoadDll()
        {
            List<Type> BlockTypes = new List<Type>();
            Type[] types = typeof(BlocksManager).Assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type.IsSubclassOf(typeof(ModLoader)) && !type.IsAbstract)
                {
                    var modLoader = Activator.CreateInstance(types[i]) as ModLoader;
                    modLoader.Entity = this;
                    modLoader.__ModInitialize();
                    ModsManager.ModLoaders.Add(modLoader);
                }
                if (type.IsSubclassOf(typeof(Block)) && !type.IsAbstract)
                {
                    BlockTypes.Add(type);
                }
            }
            for (int i=0;i<BlockTypes.Count;i++) {
                Type type = BlockTypes[i];
                FieldInfo fieldInfo = type.GetRuntimeFields().FirstOrDefault(p => p.Name == "Index" && p.IsPublic && p.IsStatic);
                if (fieldInfo == null || fieldInfo.FieldType != typeof(int))
                {
                    ModsManager.AddException(new InvalidOperationException($"Block type \"{type.FullName}\" does not have static field Index of type int."));
                }
                else
                {
                    int staticIndex = (int)fieldInfo.GetValue(null);
                    var block = (Block)Activator.CreateInstance(type.GetTypeInfo().AsType());
                    block.BlockIndex = staticIndex;
                    Blocks.Add(block);
                }

            }
        }
        public override void LoadXdb(ref XElement xElement)
        {
            xElement = ContentManager.Get<XElement>("Database");
            ContentManager.Dispose("Database");
        }
        public override void LoadCr(ref XElement xElement)
        {
            xElement = ContentManager.Get<XElement>("CraftingRecipes");
            ContentManager.Dispose("CraftingRecipes");
        }
        public override void LoadClo(ClothingBlock block, ref XElement xElement)
        {
            xElement = ContentManager.Get<XElement>("Clothes");
            ContentManager.Dispose("Clothes");
        }
        public override void LoadLauguage()
        {
            string name = "app:lang/" + ModsManager.modSettings.languageType.ToString() + ".json";
            LanguageControl.loadJson(Storage.OpenFile(name, OpenFileMode.Read));

        }
        public override void SaveSettings(XElement xElement)
        {


        }
        public override void LoadSettings(XElement xElement)
        {



        }
        public override void OnBlocksInitalized()
        {
            BlocksManager.AddCategory("Terrain");
            BlocksManager.AddCategory("Plants");
            BlocksManager.AddCategory("Construction");
            BlocksManager.AddCategory("Items");
            BlocksManager.AddCategory("Tools");
            BlocksManager.AddCategory("Weapons");
            BlocksManager.AddCategory("Clothes");
            BlocksManager.AddCategory("Electrics");
            BlocksManager.AddCategory("Food");
            BlocksManager.AddCategory("Spawner Eggs");
            BlocksManager.AddCategory("Painted");
            BlocksManager.AddCategory("Dyed");
            BlocksManager.AddCategory("Fireworks");
        }
       
    }
}
