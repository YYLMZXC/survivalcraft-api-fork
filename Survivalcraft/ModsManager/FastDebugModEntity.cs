using Engine;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
    public class FastDebugModEntity :ModEntity
    {
        public FastDebugModEntity() {
            modInfo = new ModInfo() { Name = "FastDebug", Version = "1.0.0", ApiVersion = "1.34", Author = "Mod", Description = "调试Mod插件", ScVersion = "2.2.10.4", PackageName = "com.fastdebug" };
            IEnumerable<string> dlls = Storage.ListFileNames(ModsManager.ModsPath);
            foreach (string c in dlls)
            {
                if (c == "icon.png")
                    LoadIcon(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
            }
        }
        public override void LoadDll()
        {
            IEnumerable<string> dlls = Storage.ListFileNames(ModsManager.ModsPath);
            foreach (string c in dlls)
            {
                if (c.EndsWith(".dll")) {
                    LoadDllLogic(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
                    break;
                }
            }
        }
        public override void InitPak()
        {
            IEnumerable<string> dlls = Storage.ListFileNames(ModsManager.ModsPath);
            foreach (string c in dlls)
            {
                if (c.EndsWith(".pak"))
                {
                    ContentManager.Add(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
                }

            }
        }
        public override void LoadClo(ClothingBlock block, ref XElement xElement)
        {
            IEnumerable<string> dlls = Storage.ListFileNames(ModsManager.ModsPath);
            foreach (string c in dlls)
            {
                if (c.EndsWith(".clo")) ModsManager.CombineClo(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));

            }
        }
        public override void LoadCr(ref XElement xElement)
        {
            IEnumerable<string> dlls = Storage.ListFileNames(ModsManager.ModsPath);
            foreach (string c in dlls)
            {
                if (c.EndsWith(".cr")) ModsManager.CombineCr(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));

            }
        }
        public override void LoadLauguage()
        {
            IEnumerable<string> dlls = Storage.ListFileNames(ModsManager.ModsPath);
            foreach (string c in dlls)
            {
                if (c == ModsManager.modSettings.languageType + ".json") LanguageControl.loadJson(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
            }

        }
        public override void LoadBlocksData()
        {
            IEnumerable<string> dlls = Storage.ListFileNames(ModsManager.ModsPath);
            foreach (string c in dlls)
            {
                if (c.EndsWith(".csv")) BlocksManager.LoadBlocksData(ModsManager.StreamToString(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read)));

            }
        }
        public override void LoadXdb(ref XElement xElement)
        {
            IEnumerable<string> dlls = Storage.ListFileNames(ModsManager.ModsPath);
            foreach (string c in dlls)
            {
                if (c.EndsWith(".xdb"))
                {
                    ModsManager.CombineDataBase(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
                }

            }
        }

        public override void OnBlocksInitalized(List<string> categories)
        {
        }

    }
}
