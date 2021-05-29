using System.IO;
using Game;
using System.Collections.Generic;
using Engine.Graphics;
using Engine;
using System.Text;
using SimpleJson;
using System.Reflection;
using System;
using System.Xml.Linq;
using System.Linq;
using Game;
using GameEntitySystem;
public class ModEntity
{

    public enum StorageType
    {
        InZip,
        InStorage
    }
    public ModInfo modInfo;
    public string SourceFile;
    public string Filename;
    public Texture2D Icon;
    public StorageType storageType;
    public ZipArchive ModArchive;
    public Dictionary<string, Stream> ModFiles = new Dictionary<string, Stream>();
    public List<Block> Blocks = new List<Block>();
    public bool HasException = false;
    public bool IsChecked;
    public Action ModInit;
    public ModEntity() { }
    public ModEntity(ZipArchive zipArchive) {
        ModArchive = zipArchive;
        if (GetFile("modinfo.json", out Stream stream)) {
            modInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(stream));
            stream.Dispose();
        }
    }
    /// <summary>
    /// 获取指定后缀文件列表，带.
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    public List<Stream> GetFiles(string extension) {
        List<Stream> files = new List<Stream>();
        //将每个zip里面的文件读进内存中
        foreach (ZipArchiveEntry zipArchiveEntry in ModArchive.ReadCentralDir())
        {
            if (Storage.GetExtension(zipArchiveEntry.FilenameInZip) == extension) {
                MemoryStream stream = new MemoryStream();
                ModArchive.ExtractFile(zipArchiveEntry, stream);
                stream.Seek(0, SeekOrigin.Begin);
                files.Add(stream);
            }
        }
        return files;
    }
    /// <summary>
    /// 获取指定文件
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public virtual bool GetFile(string filename, out Stream stream)
    {
        filename = filename.ToLower();
        //将每个zip里面的文件读进内存中
        foreach (ZipArchiveEntry zipArchiveEntry in ModArchive.ReadCentralDir())
        {
            if (zipArchiveEntry.FilenameInZip.ToLower() == filename)
            {
                stream = new MemoryStream();
                ModArchive.ExtractFile(zipArchiveEntry, stream);
                stream.Seek(0, SeekOrigin.Begin);
                return true;
            }
        }
        stream = null;
        return false;
    }
    public virtual void LoadLauguage() {
        if (GetFile($"{ModsManager.modSettings.languageType}.json", out Stream stream)) {
            LanguageControl.loadJson(stream);
        }
    }
    public virtual void InitPak() {
        foreach (Stream stream in GetFiles(".pak")) {
            ContentManager.Add(stream);
        }
    }
    public virtual void LoadBlocksData() {
        foreach (Stream stream in GetFiles(".csv"))
        {
            BlocksManager.LoadBlocksData(ModsManager.StreamToString(stream));
            stream.Dispose();
        }
    }
    public virtual void LoadXdb(ref XElement xElement) {
        foreach (Stream stream in GetFiles(".xdb"))
        {
            ModsManager.CombineDataBase(xElement, stream);
            stream.Dispose();
        }
    }
    public virtual void LoadClo(ClothingBlock block, ref XElement xElement) {
        foreach (Stream stream in GetFiles(".clo"))
        {
            ModsManager.CombineClo(xElement, stream);
            stream.Dispose();
        }
    }
    public virtual void LoadCr(ref XElement xElement)
    {
        foreach (Stream stream in GetFiles(".cr"))
        {
            ModsManager.CombineCr(xElement, stream);
            stream.Dispose();
        }
    }
    public virtual void LoadDll() {
        foreach (Stream stream in GetFiles(".dll"))
        {
            Assembly assembly = Assembly.Load(ModsManager.StreamToBytes(stream));
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++) {
                Type type = types[i];
                if (type.IsSubclassOf(typeof(ModLoader))) {
                    ModLoader modLoader = Activator.CreateInstance(types[i]) as ModLoader;
                    ModsManager.ModLoaders.Add(modLoader);
                }
                if (type.IsSubclassOf(typeof(Block)) && !type.IsAbstract) {
                    FieldInfo fieldInfo = type.GetRuntimeFields().FirstOrDefault(p => p.Name == "Index" && p.IsPublic && p.IsStatic);
                    if (fieldInfo == null || fieldInfo.FieldType != typeof(int))
                    {
                        ModsManager.AddException(new InvalidOperationException($"Block type \"{type.FullName}\" does not have static field Index of type int."));
                    }
                    else {
                        int staticIndex = (int)fieldInfo.GetValue(null);
                        Block block = (Block)Activator.CreateInstance(type.GetTypeInfo().AsType());
                        block.BlockIndex = staticIndex;
                        Blocks.Add(block);
                    }
                }
            }
            stream.Dispose();
        }
    }
    public virtual void CheckDependencies() {
        for (int j = 0; j < modInfo.Dependencies.Count; j++)
        {
            int k = j;
            string name = modInfo.Dependencies[k];
            string dn = "";
            Version dnversion = new Version();
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
            ModEntity entity = ModsManager.WaitToLoadMods.Find(px => px.modInfo.Name == dn && new Version(px.modInfo.Version) == dnversion);
            if (entity != null)
            {
                entity.CheckDependencies();//依赖项最先被加载
                ModsManager.CacheToLoadMods.Add(entity);
                IsChecked = true;
            }
            else {
                ModsManager.AddException(new System.Exception($"[{modInfo.Name}]缺少依赖项{name}"));
                return;
            }
        }
        ModsManager.CacheToLoadMods.Add(this);
    }
    public virtual void SaveSettings(XElement xElement)
    {
    }
    public virtual void LoadSettings(XElement xElement)
    {
    }
    public virtual void OnBlocksInitalized(List<string> categories)
    {

    }
    public virtual void InitScreens(LoadingScreen loading)
    {


    }
}
