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
    private bool IsLoaded = false;
    public bool IsChecked;
    public Action ModInit;
    public ModLoader ModLoader_;
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
    public bool GetFile(string filename,out Stream stream)
    {
        filename = filename.ToLower();
        List<Stream> files = new List<Stream>();
        //将每个zip里面的文件读进内存中
        foreach (ZipArchiveEntry zipArchiveEntry in ModArchive.ReadCentralDir())
        {
            if (zipArchiveEntry.FilenameInZip.ToLower() == filename)
            {
                stream = new MemoryStream();
                ModArchive.ExtractFile(zipArchiveEntry, stream);
                return true;
            }
        }
        stream = null;
        return false;
    }

    public void InitLauguage() {
        if (GetFile($"{ModsManager.modSettings.languageType}.json", out Stream stream)) {
            LanguageControl.loadJson(stream);
        }
    }
    public void InitPak() {
        foreach (Stream stream in GetFiles(".pak")) {
            ContentManager.Add(stream);
            stream.Dispose();
        }
    }
    public void LoadXdb(XElement xElement) {
        foreach (Stream stream in GetFiles(".xdb"))
        {
            string xml = ModsManager.StreamToString(stream);

            stream.Dispose();
        }

    }
    public void LoadDll() {
        foreach (Stream stream in GetFiles(".dll"))
        {
            Assembly assembly = Assembly.Load(ModsManager.StreamToBytes(stream));
            Type[] types = assembly.GetTypes();
            for (int i=0;i<types.Length;i++) {
                if (types[i].IsSubclassOf(typeof(ModLoader))) {
                    ModLoader_ = Activator.CreateInstance(types[i]) as ModLoader;
                    break;
                }
            }
            stream.Dispose();
        }
    }

    public void CheckDependencies() {
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
                ModsManager.exceptions.Add(new System.Exception($"[{modInfo.Name}]缺少依赖项{name}"));
                return;
            }
        }
        ModsManager.CacheToLoadMods.Add(this);
    }

    public void InitDataBase() {
        foreach (Stream stream in GetFiles(".xdb"))
        {
            ContentManager.Add(stream);
            stream.Dispose();
        }
    }
    public void Unload() { 
    
    }

}
