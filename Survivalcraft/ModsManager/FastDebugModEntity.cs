using Engine;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System;
namespace Game
{
    public class FastDebugModEntity :ModEntity
    {
        public FastDebugModEntity() {
            if (GetFile("modinfo.json", out Stream stream))
            {
                modInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(stream));
                modInfo.Name = $"[Debug]{modInfo.Name}";
                stream.Close();
            }
            else {
                modInfo = new ModInfo() { Name = "FastDebug", Version = "1.0.0", ApiVersion = "1.34", Author = "Mod", Description = "调试Mod插件", ScVersion = "2.2.10.4", PackageName = "com.fastdebug" };
            }
            if (GetFile("icon.png", out Stream stream2)) {
                LoadIcon(stream2);
                stream2.Close();
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
                   // ContentManager.Add(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
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
            ModLoader_?.OnXdbLoad(xElement);
        }
        /// <summary>
        /// 获取指定后缀文件列表，带.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public override List<Stream> GetFiles(string extension)
        {
            var files = new List<Stream>();
            foreach (string name in Storage.ListFileNames(ModsManager.ModsPath)) {
                if(name.EndsWith(extension))files.Add(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath,name),OpenFileMode.Read));            
            }
            return files;
        }
        /// <summary>
        /// 获取指定文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public override bool GetFile(string filename, out Stream stream)
        {
            foreach (string name in Storage.ListFileNames(ModsManager.ModsPath))
            {
                if (name == filename) {
                    stream = Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, name), OpenFileMode.Read);
                    return true;
                }
            }
            stream=null;
            return false;
        }
    }
}
