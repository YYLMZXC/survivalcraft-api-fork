using System.IO;
using System.Collections.Generic;
using Engine.Graphics;
using Engine;
using System.Text;
using SimpleJson;
using System.Reflection;
using System;
using System.Xml.Linq;
using System.Linq;
using GameEntitySystem;

namespace Game {
    public class ModEntity
    {
        public ModInfo modInfo;
        public Texture2D Icon;
        public ZipArchive ModArchive;
        public Dictionary<string, ZipArchiveEntry> ModFiles = new Dictionary<string, ZipArchiveEntry>();
        public List<Block> Blocks = new List<Block>();
        public bool IsChecked;
        public ModLoader ModLoader_;
        public ModEntity() { }
        public ModEntity(ZipArchive zipArchive)
        {
            ModArchive = zipArchive;
            InitResources();
        }
        public virtual void LoadIcon(Stream stream) {
            Icon = Texture2D.Load(stream);
            stream.Close();
        }
        /// <summary>
        /// 获取指定后缀文件列表，带.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public virtual List<Stream> GetFiles(string extension)
        {
            var files = new List<Stream>();
            //将每个zip里面的文件读进内存中
            foreach (ZipArchiveEntry zipArchiveEntry in ModArchive.ReadCentralDir())
            {
                if (Storage.GetExtension(zipArchiveEntry.FilenameInZip) == extension)
                {
                    var stream = new MemoryStream();
                    ModArchive.ExtractFile(zipArchiveEntry, stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    files.Add(stream);
                }
            }
            return files;
        }
        /// <summary>
        /// 获取指定文件，将ZipArchive解压到内存中
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public virtual bool GetFile(string filename, out Stream stream)
        {            
            if (ModFiles.TryGetValue(filename, out ZipArchiveEntry entry))
            {
                stream = new MemoryStream();
                ModArchive.ExtractFile(entry, stream);
                stream.Seek(0, SeekOrigin.Begin);
                return true;
            }
            stream = null;
            return false;
        }
        public virtual bool GetAssetsFile(string filename, out Stream stream)
        {
            return GetFile("Assets/" + filename, out stream);
        }
        /// <summary>
        /// 初始化语言包
        /// </summary>
        public virtual void LoadLauguage()
        {
            LoadingScreen.Info("Load Language:" + modInfo?.PackageName);
            if (GetAssetsFile($"Lang/{ModsManager.modSettings.languageType}.json", out Stream stream))
            {
                LanguageControl.loadJson(stream);
            }
        }
        /// <summary>
        /// Mod初始化
        /// </summary>
        public virtual void ModInitialize() {
            LoadingScreen.Info("Invoke ModInitialize:"+modInfo?.PackageName);
            ModLoader_?.__ModInitialize();
        }
        /// <summary>
        /// 初始化Pak资源
        /// </summary>
        public virtual void InitResources()
        {
            ModFiles.Clear();
            if (ModArchive == null) return;
            List<ZipArchiveEntry> entries = ModArchive.ReadCentralDir();
            LoadingScreen.Info("Loading Resources:" + modInfo?.PackageName);
            foreach (ZipArchiveEntry zipArchiveEntry in entries) {
                if (zipArchiveEntry.FileSize > 0) {
                    ModFiles.Add(zipArchiveEntry.FilenameInZip, zipArchiveEntry);
                    if (zipArchiveEntry.FilenameInZip.StartsWith("Assets/"))
                    {
                        ContentManager.Add(this, zipArchiveEntry.FilenameInZip.Substring(7));
                    }
                }
            }
            if (GetFile("modinfo.json", out Stream stream))
            {
                modInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(stream));
                stream.Close();
            }
            if (GetFile("icon.png", out Stream stream2))
            {
                LoadIcon(stream2);
                stream2.Close();
            }

        }
        /// <summary>
        /// 初始化BlocksData资源
        /// </summary>
        public virtual void LoadBlocksData()
        {
            LoadingScreen.Info("Loading Resources:" + modInfo?.PackageName);
            foreach (Stream stream in GetFiles(".csv"))
            {
                BlocksManager.LoadBlocksData(ModsManager.StreamToString(stream));
                stream.Dispose();
            }
        }
        /// <summary>
        /// 初始化Database数据
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadXdb(ref XElement xElement)
        {
            foreach (Stream stream in GetFiles(".xdb"))
            {
                ModsManager.CombineDataBase(xElement, stream);
                stream.Dispose();
            }
            if (ModLoader_ != null)
            {
                ModLoader_.OnXdbLoad(xElement);
            }
        }
        /// <summary>
        /// 初始化Clothing数据
        /// </summary>
        /// <param name="block"></param>
        /// <param name="xElement"></param>
        public virtual void LoadClo(ClothingBlock block, ref XElement xElement)
        {
            foreach (Stream stream in GetFiles(".clo"))
            {
                ModsManager.CombineClo(xElement, stream);
                stream.Dispose();
            }
        }
        /// <summary>
        /// 初始化CraftingRecipe
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadCr(ref XElement xElement)
        {
            foreach (Stream stream in GetFiles(".cr"))
            {
                ModsManager.CombineCr(xElement, stream);
                stream.Dispose();
            }
        }
        /// <summary>
        /// 加载mod程序集
        /// </summary>
        public virtual void LoadDll()
        {
            LoadingScreen.Info("Loading Assembly:" + modInfo?.PackageName);
            foreach (Stream stream in GetFiles(".dll"))
            {
                LoadDllLogic(stream);
                stream.Dispose();
            }
        }
        public void LoadDllLogic(Stream stream)
        {
            var assembly = Assembly.Load(ModsManager.StreamToBytes(stream));
            var BlockTypes = new List<Type>();
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type.IsSubclassOf(typeof(ModLoader)) && !type.IsAbstract)
                {
                    var modLoader = Activator.CreateInstance(types[i]) as ModLoader;
                    modLoader.Entity = this;
                    modLoader.__ModInitialize();
                    ModLoader_ = modLoader;
                    ModsManager.ModLoaders.Add(modLoader);
                }
                if (type.IsSubclassOf(typeof(Block)) && !type.IsAbstract)
                {
                    BlockTypes.Add(type);
                }
            }
            for (int i = 0; i < BlockTypes.Count; i++)
            {
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
        /// <summary>
        /// 检查依赖项
        /// </summary>
        public virtual void CheckDependencies()
        {
            LoadingScreen.Info("CheckDependencies:" + modInfo?.PackageName);
            for (int j = 0; j < modInfo.Dependencies.Count; j++)
            {
                int k = j;
                string name = modInfo.Dependencies[k];
                string dn = "";
                var dnversion = new Version();
                if (name.Contains(":"))
                {
                    string[] tmpa = name.Split(new char[] { ':' });
                    if (tmpa.Length == 2)
                    {
                        dn = tmpa[0];
                        dnversion = new Version(tmpa[1]);
                    }
                }
                else
                {
                    dn = name;
                }
                ModEntity entity = ModsManager.ModList.Find(px => px.modInfo.PackageName == dn && new Version(px.modInfo.Version) == dnversion);
                if (entity != null)
                {
                    entity.CheckDependencies();//依赖项最先被加载
                    IsChecked = true;
                }
                else
                {
                    ModsManager.AddException(new Exception($"[{modInfo.Name}]缺少依赖项{name}"), false);
                    return;
                }
            }
        }
        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void SaveSettings(XElement xElement)
        {
            if (ModLoader_ != null) ModLoader_.SaveSettings(xElement);

        }
        /// <summary>
        /// 加载设置
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadSettings(XElement xElement)
        {
            if (ModLoader_ != null) ModLoader_.LoadSettings(xElement);
        }
        /// <summary>
        /// BlocksManager初始化完毕
        /// </summary>
        /// <param name="categories"></param>
        public virtual void OnBlocksInitalized()
        {
            if (ModLoader_ != null) ModLoader_.OnBlocksManagerInitalized();
        }
        /// <summary>
        /// ScreensManager初始化完毕
        /// </summary>
        /// <param name="loading"></param>
        public virtual void InitScreens(LoadingScreen loading)
        {
            if (ModLoader_ != null) ModLoader_.OnScreensManagerInitalized(loading);

        }
        //释放资源
        public virtual void Dispose() {
            try { if (ModLoader_ != null) ModLoader_.Dispose(); } catch { }
            ModArchive?.ZipFileStream.Close();
        }
    }

}

