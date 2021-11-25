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
        public ModLoader Loader { get { return ModLoader_; } set { ModLoader_ = value; } }
        private ModLoader ModLoader_;
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
                    stream.Position = 0L;
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
                stream.Position = 0L;
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
            LoadingScreen.Info("加载语言:" + modInfo?.Name);
            if (GetAssetsFile($"Lang/{ModsManager.Configs["Language"]}.json", out Stream stream))
            {
                LanguageControl.loadJson(stream);
            }
        }
        /// <summary>
        /// Mod初始化
        /// </summary>
        public virtual void ModInitialize() {
            LoadingScreen.Info("初始化Mod方法:"+modInfo?.Name);
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
            foreach (ZipArchiveEntry zipArchiveEntry in entries) {
                if (zipArchiveEntry.FileSize > 0)
                {
                    ModFiles.Add(zipArchiveEntry.FilenameInZip, zipArchiveEntry);
                }
            }
            if (GetFile("modinfo.json", out Stream stream))
            {
                modInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(stream));
                stream.Close();
            }
            if (modInfo == null) return;
            if (GetFile("icon.png", out Stream stream2))
            {
                LoadIcon(stream2);
                stream2.Close();
            }
            foreach (var c in ModFiles) {
                ZipArchiveEntry zipArchiveEntry = c.Value;
                string filename = zipArchiveEntry.FilenameInZip;
                if (!zipArchiveEntry.IsFilenameUtf8)
                {
                    ModsManager.AddException(new Exception("文件名["+zipArchiveEntry.FilenameInZip+"]编码不是Utf8，请进行修正，相关Mod[" + modInfo.Name + "]"));
                }
                if (filename.StartsWith("Assets/"))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    ContentInfo contentInfo = new ContentInfo(filename.Substring(7));
                    ModArchive.ExtractFile(zipArchiveEntry, memoryStream);
                    contentInfo.SetContentStream(memoryStream);
                    ContentManager.Add(contentInfo);
                }
            }
            LoadingScreen.Info("加载资源:" + modInfo?.Name+" 共"+ModFiles.Count+"文件");
        }
        /// <summary>
        /// 初始化BlocksData资源
        /// </summary>
        public virtual void LoadBlocksData()
        {
            LoadingScreen.Info("加载方块数据:" + modInfo?.Name);
            foreach (Stream stream in GetFiles(".csv"))
            {
                try
                {
                    BlocksManager.LoadBlocksData(ModsManager.StreamToString(stream));
                }
                catch (Exception e)
                {
                    LoadingScreen.Warning("<" + modInfo?.PackageName + ">" + e.Message);
                }
                stream.Dispose();
            }
        }
        /// <summary>
        /// 初始化Database数据
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadXdb(ref XElement xElement)
        {
            LoadingScreen.Info("加载数据库:" + modInfo?.Name);
            foreach (Stream stream in GetFiles(".xdb"))
            {
                ModsManager.CombineDataBase(xElement, stream);
                stream.Dispose();
            }
            if (Loader != null)
            {
                Loader.OnXdbLoad(xElement);
            }
        }
        /// <summary>
        /// 初始化Clothing数据
        /// </summary>
        /// <param name="block"></param>
        /// <param name="xElement"></param>
        public virtual void LoadClo(ClothingBlock block, ref XElement xElement)
        {
            LoadingScreen.Info("加载衣物数据:" + modInfo?.Name);
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
            LoadingScreen.Info("加载合成谱:" + modInfo?.Name);
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
            LoadingScreen.Info("加载程序集:" + modInfo?.PackageName);
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
                    Loader = modLoader;
                    modLoader.__ModInitialize();
                    ModsManager.ModLoaders.Add(modLoader);
                }
                if (type.IsSubclassOf(typeof(IContentReader.IContentReader)) && !type.IsAbstract)
                {
                    IContentReader.IContentReader reader = Activator.CreateInstance(type) as IContentReader.IContentReader;
                    if(!ContentManager.ReaderList.ContainsKey(reader.Type))ContentManager.ReaderList.Add(reader.Type, reader);
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
                    LoadingScreen.Warning($"Block type \"{type.FullName}\" does not have static field Index of type int.");
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
            LoadingScreen.Info("检查依赖项:" + modInfo?.PackageName);
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
            if (Loader != null) Loader.SaveSettings(xElement);

        }
        /// <summary>
        /// 加载设置
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadSettings(XElement xElement)
        {
            if (Loader != null) Loader.LoadSettings(xElement);
        }
        /// <summary>
        /// BlocksManager初始化完毕
        /// </summary>
        /// <param name="categories"></param>
        public virtual void OnBlocksInitalized()
        {
            if (Loader != null) Loader.BlocksInitalized();
        }
        /// <summary>
        /// LoadingScreen任务执行完毕
        /// </summary>
        /// <param name="loading"></param>
        public virtual void OnLoadingFinished(List<Action> LoadingActions)
        {
            if (Loader != null) Loader.OnLoadingFinished(LoadingActions);

        }
        //释放资源
        public virtual void Dispose() {
            try { if (Loader != null) Loader.ModDispose(); } catch { }
            ModArchive?.ZipFileStream.Close();
        }
    }

}

