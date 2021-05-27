// Game.ModsManager
using Engine;
using Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using XmlUtilities;

public static class ModsManager
{
    public static Dictionary<string, ZipArchive> Archives;

    public static Action Initialized;

    public static List<ModInfo> LoadedMods=new List<ModInfo>();

    public static Func<XElement, IEnumerable<FileEntry>, string, string, string, XElement> CombineXml1;

#if desktop
    public static string ModsPath = "app:/Mods";
    public static string userDataPath = "app:/UserId.dat";
    public static string CharacterSkinsDirectoryName = "app:/CharacterSkins";
    public static string FurniturePacksDirectoryName = "app:/FurniturePacks";
    public static string BlockTexturesDirectoryName = "app:/TexturePacks";
    public static string WorldsDirectoryName = "app:/Worlds";
    public static string communityContentCachePath = "app:CommunityContentCache.xml";
    public static string ModsSetPath = "app:/ModSettings.xml";
    public static string settingPath = "app:/Settings.xml";
    public static string logPath = "app:/Logs";
    public const string APIVersion="1.34";
    public const string SCVersion = "2.2.10.4";

#endif
#if android
    public static string baseDir = EngineActivity.basePath;
    public static string screenCapturePath = Storage.CombinePaths(baseDir , "ScreenCapture");
    public static string ModsPath = baseDir + "/Mods";
    public static string userDataPath = "config:/UserId.dat";
    public static string FurniturePacksDirectoryName => "config:/FurniturePacks";
    public static string CharacterSkinsDirectoryName => "config:/CharacterSkins";
    public static string BlockTexturesDirectoryName => "config:/TexturePacks";
    public static string WorldsDirectoryName = "config:/Worlds";
    public static string communityContentCachePath = "config:/CommunityContentCache.xml";
    public static string ModsSetPath = "config:/ModSettings.xml";
    public static string settingPath = "config:/Settings.xml";
    public static string logPath = "config:/Logs";

#endif

    public static string path;//移动端mods数据文件夹

    public class ModSettings
    {
        public LanguageControl.LanguageType languageType;
    }
    public static ModSettings modSettings=new ModSettings();
    public static Dictionary<string, Type> TypeCaches = new Dictionary<string, Type>();
    public static List<Assembly> LoadQueque = new List<Assembly>();
    public static Dictionary<string, ZipArchive> zip_filelist;
    public static List<FileEntry> quickAddModsFileList = new List<FileEntry>();
    public static XElement CombineXml(XElement node, IEnumerable<FileEntry> files, string attr1 = null, string attr2 = null, string type = null)
    {
        Func<XElement, IEnumerable<FileEntry>, string, string, string, XElement> combineXml = CombineXml1;
        if (combineXml != null)
        {
            return combineXml(node, files, attr1, attr2, type);
        }
        IEnumerator<FileEntry> enumerator = files.GetEnumerator();
        while (enumerator.MoveNext())
        {
            try
            {
                XElement src = XmlUtils.LoadXmlFromStream(enumerator.Current.Stream, null, throwOnError: true);
                Modify(node, src, attr1, attr2, type);
            }
            catch (Exception arg)
            {
                throw new InvalidDataException(arg.Message);
            }
        }
        return node;
    }
    public static void SaveSettings(XElement xElement)
    {
        XElement la = XmlUtils.AddElement(xElement, "Set");
        la.SetAttributeValue("Name", "Language");
        la.SetAttributeValue("Value", (int)modSettings.languageType);
    }
    public static void LoadSettings(XElement xElement)
    {
        try
        {
            foreach (XElement item in xElement.Elements())
            {
                if (item.Attribute("Name").Value == "Language")
                {
                    modSettings.languageType = (LanguageControl.LanguageType)int.Parse(item.Attribute("Value").Value);
                }
            }

        }
        catch { 
        
        }
    }
    public static void Modify(XElement dst, XElement src, string attr1 = null, string attr2 = null, XName type = null)
    {
        List<XElement> list = new List<XElement>();
        IEnumerator<XElement> enumerator = src.Elements().GetEnumerator();
        while (enumerator.MoveNext())
        {
            XElement current = enumerator.Current;
            string localName = current.Name.LocalName;
            string text = current.Attribute(attr1)?.Value;
            string text2 = current.Attribute(attr2)?.Value;
            int num = (localName.Length >= 2 && localName[0] == 'r' && localName[1] == '-') ? (current.IsEmpty ? 2 : (-2)) : 0;
            IEnumerator<XElement> enumerator2 = dst.DescendantsAndSelf((localName.Length == 2 && num != 0) ? type : ((XName)current.Name.LocalName.Substring(Math.Abs(num)))).GetEnumerator();
            while (enumerator2.MoveNext())
            {
                XElement current2 = enumerator2.Current;
                IEnumerator<XAttribute> enumerator3 = current2.Attributes().GetEnumerator();
                while (true)
                {
                    if (enumerator3.MoveNext())
                    {
                        localName = enumerator3.Current.Name.LocalName;
                        string value = enumerator3.Current.Value;
                        XAttribute xAttribute;
                        if (text != null && string.Equals(localName, attr1))
                        {
                            if (!string.Equals(value, text))
                            {
                                break;
                            }
                        }
                        else if (text2 != null && string.Equals(localName, attr2))
                        {
                            if (!string.Equals(value, text2))
                            {
                                break;
                            }
                        }
                        else if ((xAttribute = current.Attribute(XName.Get("new-" + localName))) != null)
                        {
                            current2.SetAttributeValue(XName.Get(localName), xAttribute.Value);
                        }
                        continue;
                    }
                    if (num < 0)
                    {
                        current2.RemoveNodes();
                        current2.Add(current.Elements());
                    }
                    else if (num > 0)
                    {
                        list.Add(current2);
                    }
                    else if (!current.IsEmpty)
                    {
                        current2.Add(current.Elements());
                    }
                    break;
                }
            }
        }
        List<XElement>.Enumerator enumerator4 = list.GetEnumerator();
        while (enumerator4.MoveNext())
        {
            enumerator4.Current.Remove();
        }
    }
    public static string ImportMod(string name,Stream stream) {
        string path = Storage.CombinePaths(ModsPath,name);
        Stream fileStream = Storage.OpenFile(path,OpenFileMode.CreateOrOpen);
        stream.CopyTo(fileStream);
        fileStream.Close();

        return "下载成功,重启游戏生效";

    }
    public static void DisableMod() {
    
    }
    public static void Initialize()
    {
        zip_filelist = new Dictionary<string, ZipArchive>();
        if (!Storage.DirectoryExists(ModsPath)) Storage.CreateDirectory(ModsPath);
        GetAllFiles(ModsPath);//获取zip列表        
        List<FileEntry> dlls = GetEntries(".dll");
        int cnt = 0;
        foreach (FileEntry item in dlls)
        {
            try
            {
                LoadMod(item.SourceFile, Assembly.Load(StreamToBytes(item.Stream)));
            }
            catch
            {
                Log.Error("未能成功加载[" + item.Filename + "]");
                cnt++;
            }
        }
    }
    public static List<Exception> exceptions = new List<Exception>();

    public static void GetAllFiles(string path)
    {//获取zip包列表，变成ZipArchive
        foreach (string item in Storage.ListFileNames(path))
        {
            string ms = Storage.GetExtension(item);
            string ks = Storage.CombinePaths(path, item);
            Stream stream = Storage.OpenFile(ks, OpenFileMode.Read);
            quickAddModsFileList.Add(new FileEntry() { Stream = stream,storageType=FileEntry.StorageType.InStorage,Filename = item,SourceFile=item });
            try
            {
                if (ms == ".zip" || ms == ".scmod")
                {
                    ZipArchive zipArchive = ZipArchive.Open(stream, true);
                    zip_filelist.Add(item, zipArchive);
                }
            }
            catch (Exception e)
            {
                Log.Error("load file [" + ks + "] error." + e.ToString());
            }
        }
        foreach (string dir in Storage.ListDirectoryNames(path))
        {
            GetAllFiles(Storage.CombinePaths(path, dir));
        }
    }
    public static List<FileEntry> GetEntries(string ext)
    {//获取制定后缀的文件集
        List<FileEntry> fileEntries = new List<FileEntry>();
        foreach (FileEntry fileEntry1 in quickAddModsFileList)
        {
            if (Storage.GetExtension(fileEntry1.Filename) == ext)
            {
                FileEntry fileEntry = new FileEntry() { storageType=FileEntry.StorageType.InStorage, Filename=fileEntry1.Filename};
                byte[] tmp = new byte[fileEntry1.Stream.Length];
                fileEntry1.Stream.Position = 0L;
                fileEntry1.Stream.Read(tmp, 0, (int)fileEntry1.Stream.Length);
                MemoryStream memoryStream = new MemoryStream(tmp);
                fileEntry.Stream = memoryStream;
                fileEntries.Add(fileEntry);
            }
        }
        foreach (var zipArchive in zip_filelist)
        {
            foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Value.ReadCentralDir())
            {
                string fn = zipArchiveEntry.FilenameInZip;
                if (Storage.GetExtension(fn) == ext)
                {
                    MemoryStream stream = new MemoryStream();
                    zipArchive.Value.ExtractFile(zipArchiveEntry, stream);
                    FileEntry fileEntry = new FileEntry();
                    fileEntry.SourceFile = zipArchive.Key;
                    fileEntry.storageType = FileEntry.StorageType.InZip;
                    fileEntry.Filename = fn;
                    stream.Position = 0L;
                    fileEntry.Stream = stream;
                    fileEntries.Add(fileEntry);
                }
            }
        }
        return fileEntries;
    }
    public static void LogException(FileEntry file, Exception ex)
    {
        Log.Warning("Loading \"" + file.Filename.Substring(path.Length + 1) + "\" failed: " + ex.ToString());
        file.Stream.Close();
    }
    /// <summary> 
    /// 将 Stream 转成 byte[] 
    /// </summary> 
    public static byte[] StreamToBytes(Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        // 设置当前流的位置为流的开始 
        stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }

    /// <summary> 
    /// 将 byte[] 转成 Stream 
    /// </summary> 
    public static Stream BytesToStream(byte[] bytes)
    {
        Stream stream = new MemoryStream(bytes);
        return stream;
    }

    /// <summary> 
    /// 将 Stream 写入文件 
    /// </summary> 
    public static void StreamToFile(Stream stream, string fileName)
    {
        // 把 Stream 转换成 byte[] 
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        // 设置当前流的位置为流的开始 
        stream.Seek(0, SeekOrigin.Begin);

        // 把 byte[] 写入文件 
        FileStream fs = new FileStream(fileName, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(bytes);
        bw.Close();
        fs.Close();
    }

    /// <summary> 
    /// 从文件读取 Stream 
    /// </summary> 
    public static Stream FileToStream(string fileName)
    {
        // 打开文件 
        FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        // 读取文件的 byte[] 
        byte[] bytes = new byte[fileStream.Length];
        fileStream.Read(bytes, 0, bytes.Length);
        fileStream.Close();
        // 把 byte[] 转换成 Stream 
        Stream stream = new MemoryStream(bytes);
        return stream;
    }

    public static void LoadMod(string name,Assembly asm)
    {
        if (asm == null) return;
        Type typeFromHandle = typeof(PluginLoaderAttribute);
        Type[] types = asm.GetTypes();
        for (int i = 0; i < types.Length; i++)
        {
            PluginLoaderAttribute pluginLoaderAttribute = (PluginLoaderAttribute)Attribute.GetCustomAttribute(types[i], typeFromHandle);
            if (pluginLoaderAttribute != null)
            {
                ModInfo modInfo = pluginLoaderAttribute.ModInfo;
                if (modInfo.APIVersion == null)
                {
                    exceptions.Add(new InvalidOperationException($"[{modInfo.Name}]加载失败\n缺少APIVersion声明"));
                }
                else {
                    Version modapiver = new Version(modInfo.APIVersion);
                    Version apiver = new Version(APIVersion);
                    if (modapiver < apiver)
                    {
                        exceptions.Add(new InvalidOperationException($"[{modInfo.Name}]加载失败\nMod要求API版本为:{modapiver}\n当前API版本:{apiver}"));
                    }
                    else
                    {
                        modInfo.FileName = name;
                        MethodInfo method;
                        if ((method = types[i].GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) != null)
                        {
                            method.Invoke(Activator.CreateInstance(types[i]), null);
                        }
                        LoadedMods.Add(modInfo);
                        Log.Information("loaded mod [" + pluginLoaderAttribute.ModInfo.Name + "]");
                    }
                }
            }
        }
    }

    public static string GetMd5(string input)
    {
        MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
}
