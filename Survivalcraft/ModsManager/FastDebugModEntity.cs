using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Game
{
    public class FastDebugModEntity : ModEntity
    {
        public Dictionary<string, FileInfo> FModFiles = new Dictionary<string, FileInfo>();

        public FastDebugModEntity()
        {
            modInfo = new ModInfo() { Name="[Debug]",PackageName="debug"};
            InitResources();
        }

        public override void InitResources()
        {
            ReadDirResouces(ModsManager.ModsPath, "");
            if (!GetFile("modinfo.json", (stream) =>
            {
                modInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(stream));
                modInfo.Name = $"[Debug]{modInfo.Name}";
            }))
            {
                modInfo = new ModInfo() { Name = "FastDebug", Version = "1.0.0", ApiVersion = ModsManager.APIVersion, Author = "Mod", Description = "调试Mod插件", ScVersion = "2.2.10.4", PackageName = "com.fastdebug" };
            }
            GetFile("icon.png", (stream) => { LoadIcon(stream); });
        }

        public void ReadDirResouces(string basepath, string path)
        {
            if (string.IsNullOrEmpty(path)) path = basepath;
            foreach (string d in Storage.ListDirectoryNames(path)) ReadDirResouces(basepath, path + "/" + d);
            foreach (string f in Storage.ListFileNames(path))
            {
                string abpath = path + "/" + f;
                string FilenameInZip = abpath.Substring(basepath.Length + 1);
                if (FilenameInZip.StartsWith("Assets/"))
                {
                    string name = FilenameInZip.Substring(7);
                    ContentInfo contentInfo = new ContentInfo(name);
                    MemoryStream memoryStream = new MemoryStream();
                    using (Stream stream = Storage.OpenFile(abpath, OpenFileMode.Read))
                    {
                        stream.CopyTo(memoryStream);
                        contentInfo.SetContentStream(memoryStream);
                        ContentManager.Add(contentInfo);
                    }
                }
                FModFiles.Add(FilenameInZip, new FileInfo(Storage.GetSystemPath(abpath)));
            }
        }

        public override void LoadDll()
        {
            foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
            {
                if (c.EndsWith(".dll") && !(c.StartsWith("EntitySystem") || c.StartsWith("Engine") || c.StartsWith("Survivalcraft") || c.StartsWith("OpenTK")))
                {
                    LoadDllLogic(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
                    break;
                }
            }
        }

        public override void LoadClo(ClothingBlock block, ref XElement xElement)
        {
            foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
            {
                if (c.EndsWith(".clo")) ModsManager.CombineClo(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
            }
        }

        public override void LoadCr(ref XElement xElement)
        {
            foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
			{
				if (c.EndsWith(".cr"))
					ModsManager.CombineCr(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
			}
		}

        public override void LoadLauguage()
        {
            foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
            {
                if (c == ModsManager.Configs["Language"] + ".json") LanguageControl.loadJson(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
            }
        }

        public override void LoadBlocksData()
        {
            foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
			{
				if (c.EndsWith(".csv"))
					BlocksManager.LoadBlocksData(ModsManager.StreamToString(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read)));
			}
		}

        public override void LoadXdb(ref XElement xElement)
        {
            foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
            {
                if (c.EndsWith(".xdb"))
                {
                    ModsManager.CombineDataBase(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
                }
            }
            Loader?.OnXdbLoad(xElement);
        }

        /// <summary>
        /// 获取指定后缀文件列表，带.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public override void GetFiles(string extension, Action<string, Stream> action)
        {
            foreach (string name in Storage.ListFileNames(ModsManager.ModsPath))
            {
                using (Stream stream = Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, name), OpenFileMode.Read))
                {
                    try { action.Invoke(name, stream); } catch (Exception e) { LoadingScreen.Error(string.Format("GetFile {0} Error:{1}", name, e.Message)); }
                }
            }
        }
        public override bool GetFile(string filename, Action<Stream> stream)
        {
            if (FModFiles.TryGetValue(filename, out FileInfo fileInfo))
            {
                using (Stream fs = fileInfo.OpenRead())
                {
                    try
                    {
                        stream?.Invoke(fs);
                    }
                    catch (Exception e)
                    {
                        Log.Error(string.Format("GetFile {0} Error:{1}", filename, e.Message));
                    }
                }
                return true;
            }
            return false;
        }
        public override bool GetAssetsFile(string filename, Action<Stream> stream)
        {
            return GetFile("Assets/" + filename, stream);
        }
    }
}
