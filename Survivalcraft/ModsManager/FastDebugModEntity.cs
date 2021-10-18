using Engine;
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
            if (GetFile("modinfo.json", out Stream stream))
            {
                modInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(stream));
                modInfo.Name = $"[Debug]{modInfo.Name}";
                stream.Close();
            }
            else
            {
                modInfo = new ModInfo() { Name = "FastDebug", Version = "1.0.0", ApiVersion = ModsManager.APIVersion, Author = "Mod", Description = "调试Mod插件", ScVersion = "2.2.10.4", PackageName = "com.fastdebug" };
            }
            if (GetFile("icon.png", out Stream stream2))
            {
                LoadIcon(stream2);
                stream2.Close();
            }
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
                    FModFiles.Add(name,new FileInfo(Storage.GetSystemPath(abpath)));
                    ContentInfo contentInfo = new ContentInfo(modInfo.PackageName, name);
                    MemoryStream memoryStream = new MemoryStream();
                    using (Stream stream = Storage.OpenFile(abpath, OpenFileMode.Read))
                    {
                        stream.CopyTo(memoryStream);
                        contentInfo.SetContentStream(memoryStream);
                        ContentManager.Add(contentInfo);
                    }
                }

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
        public override List<Stream> GetFiles(string extension)
        {
            var files = new List<Stream>();
            foreach (string name in Storage.ListFileNames(ModsManager.ModsPath))
            {
                if (name.EndsWith(extension)) files.Add(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, name), OpenFileMode.Read));
            }
            return files;
        }

        /// <summary>
        /// 获取指定文件，这里申请的流需要Mod自行释放
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public override bool GetFile(string filename, out Stream stream)
        {
            stream = null;
            if (FModFiles.TryGetValue(filename, out FileInfo fileInfo))
            {
                stream = fileInfo.OpenRead();
                return true;
            }
            return false;
        }

        public override bool GetAssetsFile(string filename, out Stream stream)
        {
            return GetFile("Assets/" + filename, out stream);
        }
    }
}
