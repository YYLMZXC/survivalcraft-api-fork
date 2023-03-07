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
        /// <param name="action">参数1文件名参数，2打开的文件流</param>
        public virtual void GetFiles(string extension,Action<string,Stream> action)
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
                    try
                    {
                        action.Invoke(zipArchiveEntry.FilenameInZip, stream);
                    }
                    catch (Exception e)
                    {
                        Log.Error(string.Format("GetFile {0} Error:{1}", zipArchiveEntry.FilenameInZip, e.Message));
                    }
                    finally
                    {
                        stream.Dispose();
                    }
                }
            }
        }
        /// <summary>
        /// 获取指定文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="stream">参数1打开的文件流</param>
        /// <returns></returns>
        public virtual bool GetFile(string filename, Action<Stream> stream)
        {            
            if (ModFiles.TryGetValue(filename, out ZipArchiveEntry entry))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ModArchive.ExtractFile(entry, ms);
                    ms.Position = 0L;
                    try
                    {
                        stream?.Invoke(ms);
                    }
                    catch (Exception e)
                    {
                        LoadingScreen.Error("GetFile " + filename + " Error:" + e.Message);
                    }
                }
            }
            return false;
        }
        public virtual bool GetAssetsFile(string filename, Action<Stream> stream)
        {
            return GetFile("Assets/" + filename, stream);
        }
        /// <summary>
        /// 初始化语言包
        /// </summary>
        public virtual void LoadLauguage()
        {
            LoadingScreen.Info("加载语言:" + modInfo?.Name);
            GetAssetsFile($"Lang/{ModsManager.Configs["Language"]}.json", (stream) => { LanguageControl.loadJson(stream); });
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
            GetFile("modinfo.json", (stream) => {
                modInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(stream));
            });
            if (modInfo == null) return;
            GetFile("icon.png", (stream) => {
                LoadIcon(stream);
            });
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
            GetFiles(".csv", (filename, stream) => {
                BlocksManager.LoadBlocksData(ModsManager.StreamToString(stream));
            });
        }
        /// <summary>
        /// 初始化Database数据
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadXdb(ref XElement xElement)
        {
            XElement element = xElement;
            LoadingScreen.Info("加载数据库:" + modInfo?.Name);
            GetFiles(".xdb", (filename, stream) => {
                ModsManager.CombineDataBase(element, stream);
            });
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
            XElement element = xElement;
            LoadingScreen.Info("加载衣物数据:" + modInfo?.Name);
            GetFiles(".clo", (filename, stream) => {
                ModsManager.CombineClo(element, stream);
            });
        }
        /// <summary>
        /// 初始化CraftingRecipe
        /// </summary>
        /// <param name="xElement"></param>
        public virtual void LoadCr(ref XElement xElement)
        {
            XElement element = xElement;
            LoadingScreen.Info("加载合成谱:" + modInfo?.Name);
            GetFiles(".cr", (filename, stream) => { ModsManager.CombineCr(element, stream); });
        }
        /// <summary>
        /// 加载mod程序集
        /// </summary>
        public virtual void LoadDll()
        {
            LoadingScreen.Info("加载程序集:" + modInfo?.PackageName);
            GetFiles(".dll",(filename,stream)=> {
                LoadDllLogic(stream);
            });
        }
        public void LoadDllLogic(Stream stream)
        {
            var assembly = Assembly.Load(ModsManager.StreamToBytes(stream));
            ModsManager.Dlls.Add(assembly.FullName, assembly);
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
        public virtual void LoadJs()
        {
            LoadingScreen.Info("加载Javascript脚本:" + modInfo?.PackageName);
            GetFiles(".js", (filename, stream) => {
                JsInterface.Execute(new StreamReader(stream).ReadToEnd());
            });
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
        //释放资源
        public virtual void Dispose() {
            try { if (Loader != null) Loader.ModDispose(); } catch { }
            ModArchive?.ZipFileStream.Close();
        }
    }
}

