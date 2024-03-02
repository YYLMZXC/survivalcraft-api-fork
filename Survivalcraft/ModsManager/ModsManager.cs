// Game.ModsManager

using Engine;
using SimpleJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using XmlUtilities;

namespace Game;
public static class ModsManager
{
    // ReSharper disable once StringLiteralTypo
    [Obsolete(error: true, message:"字段 ModsManager.Apiv 已过时，请使用 ModsManager.ApiCurrentVersion")]
    // ReSharper disable once IdentifierTypo
    public static readonly int Apiv = -1;
    
    [Obsolete(error: true, message:"字段 ModsManager.APIVersion 已过时，请使用 ModsManager.ApiCurrentVersionString")] 
    // ReSharper disable once InconsistentNaming
    public static readonly string APIVersion = null!;
    
    [Obsolete(error: true, message:"字段 ModsManager.APIVersion 已过时，请使用 ModsManager.SurvivalcraftCurrentVersion")] 
    // ReSharper disable once InconsistentNaming
    public static readonly string SCVersion  = null!;
    
    public static string ModFileSuffix => ".scmod";

    public static ApiVersion ApiCurrentVersion => ApiVersion.Api170;
    public static string ApiCurrentVersionString => "1.70";

    public static string SurvivalcraftCurrentVersion { get; } =
        typeof(SurvivalCraftModEntity).Assembly.GetName().Version?.ToString();

    public static bool IsAndroid { get; } = VersionsManager.Platform == Platform.Android;

    #region PathDefintions
    
    /// <summary>
    /// ExternalPath, 用于存储地图、模组、家具包等文档
    /// </summary>
    public static string ExtPath { get; } = VersionsManager.Platform switch
    {
        Platform.Android => "android:/SurvivalcraftApi1.6",
        _ => "app:"
    };

    public static string ScreenCapturePath { get; } = ExtPath + "/ScreenCapture";
    public static string DocPath { get; } = ExtPath + "/doc";
    public static string UserDataPath = ExtPath + DocPath + "/UserId.dat";
    public static string CharacterSkinsDirectoryName {get;} = DocPath + "/CharacterSkins";
    public static string FurniturePacksDirectoryName{get;} = DocPath + "/FurniturePacks";
    public static string BlockTexturesDirectoryName {get;} =  DocPath + "/TexturePacks";
    public static string WorldsDirectoryName { get; } = ExtPath + "/Worlds";
    public static string CommunityContentCachePath { get; } = DocPath + "/CommunityContentCache.xml";
    public static string ModSettingsPath { get; } = ExtPath + "/ModSettings.xml";
    public static string SettingPath { get; } = ExtPath + "/Settings.xml";
    public static string ModCachePath { get; } = ExtPath + "/Mods/Cache";
    public static string LogPath { get; } = ExtPath + "/Bugs";

    #endregion

    public static string ModsPath { get; } = ExtPath + "/Mods"; //移动端mods数据文件夹

    internal static ModEntity SurvivalCraftModEntity;
    internal static bool ConfigLoaded;

    [Obsolete(error:true, message:"Game.ModsManager.ModHook 已过时")]
    public class ModHook
    {
        public string HookName;
        public Dictionary<ModLoader, bool> Loaders = [];
        public Dictionary<ModLoader, string> DisableReason = [];

        public ModHook(string name)
        {
            throw new NotSupportedException();
        }

        public void AddModLoader(ModLoader modLoader)
        {
            throw new NotSupportedException();
        }

        public void RemoveModLoader(ModLoader modLoader)
        {
            throw new NotSupportedException();
        }

        public void Disable(ModLoader from, ModLoader toDisable, string reason)
        {
            throw new NotSupportedException();
        }
    }

    private static bool m_allowContinue = true;
    public static readonly Dictionary<string, string> Configs = [];
    public static readonly List<ModEntity> ModList = [];

    public static List<IModLoader> ModLoaders => ModList.SelectMany(entity => entity.Loaders).ToList();
    
    [Obsolete(error: true, message: "Game.ModsManager.DisabledMods 字段已弃用")]
    public static readonly List<ModInfo> DisabledMods = [];
    
    [Obsolete(error: true, message: "Game.ModsManager.ModHooks 字段已弃用")]
    public static readonly List<ModHook> ModHooks = [];
    
    public static readonly Dictionary<string, Assembly> Assemblies = [];

    public static bool GetModEntity(string packageName, out ModEntity? modEntity)
    {
        modEntity = ModList.FirstOrDefault(px => px.ModInfo.PackageName == packageName);
        return modEntity != null;
    }

    public static bool GetAllowContinue()
    {
        return m_allowContinue;
    }

    internal static void Reboot()
    {
        SettingsManager.SaveSettings();
        SettingsManager.LoadSettings();
        foreach (var mod in ModList)
        {
            mod.Dispose();
        }

        ScreensManager.SwitchScreen("Loading");
    }

    [Obsolete(error:true, message:"Game.ModsManager.HookAction 已过时，请使用静态方法 Game.ModInterface.InvokeHooks")]
    public static void HookAction(string hookName, Func<ModLoader, bool> action)
    {
        throw new NotSupportedException();
    }

    [Obsolete(error:true, message:"Game.ModsManager.RegisterHook 已过时，请使用实例方法 Game.ModInterface.RegisterHook")]
    public static void RegisterHook(string hookName, ModLoader modLoader)
    {
        throw new NotSupportedException();
    }

    [Obsolete(error: true, message:"Game.ModsManager.DisableHook 已弃用")]
    public static void DisableHook(ModLoader from, string hookName, string packageName, string reason)
    {
        throw new NotSupportedException();
    }

#if DEBUG
    public static void StreamCompress(Stream input, MemoryStream data)
    {
        byte[] dat = data.ToArray();
        using (var stream = new GZipStream(input, CompressionMode.Compress))
        {
            stream.Write(dat, 0, dat.Length);
        }
    }

    public static Stream StreamDecompress(Stream input)
    {
        var outStream = new MemoryStream();
        using (var zipStream = new GZipStream(input, CompressionMode.Decompress))
        {
            zipStream.CopyTo(outStream);
            zipStream.Close();
            outStream.Seek(0, SeekOrigin.Begin);
            return outStream;
        }
    }
#endif
    public static T GetInPakOrStorageFile<T>(string filepath, string suffix = null) where T : class
    {
        try
        {
            return ContentManager.Get<T>(filepath, suffix);
        }
        catch
        {
            //ignore
        }
        if (!ContentManager.ReaderList.TryGetValue(typeof(T).FullName!, out var reader))
        {
            throw new ArgumentOutOfRangeException(
                $"Cannot find reader for {'"'}{typeof(T).FullName}{'"'} correctly");
        }

        string storagePath = Storage.CombinePaths(ExtPath, filepath);
        
        if (suffix is null)
        {
            foreach (var possibleSuffix in reader.DefaultSuffix)
            {
                if (Storage.FileExists(storagePath + possibleSuffix))
                {
                    storagePath += possibleSuffix;
                }
            }
        }

        if (!Storage.FileExists(storagePath)) throw new FileNotFoundException();
        
        byte[] content = Storage.ReadAllBytes(storagePath);
        return (T)reader.Get(new[] { new ContentInfo(storagePath) { ContentStream = new MemoryStream(content) } });

    }

    public static T DeserializeJson<T>(string text) where T : class
    {
        var obj = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(text, typeof(JsonObject));
        var outObj = Activator.CreateInstance(typeof(T)) as T;
        if (outObj is null) throw new InvalidOperationException($"Cannot deserialize input json into type {typeof(T).FullName}");
        Type outType = outObj.GetType();
        foreach (KeyValuePair<string, object> c in obj)
        {
            FieldInfo field = outType.GetField(c.Key, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) continue;
            if (c.Value is JsonArray array)
            {
                Type[] types = field.FieldType.GetGenericArguments();
                object list1 = Activator.CreateInstance(typeof(List<>).MakeGenericType(types));
                foreach (object item in array)
                {
                    Type type = list1?.GetType();
                    if(type is null) continue;
                    MethodInfo methodInfo = type.GetMethod("Add");
                    if (types.Length == 1 && methodInfo is not null)
                    {
                        string typeName = types[0].Name.ToLower();
                        switch (typeName)
                        {
                            case "int32":
                                int.TryParse(item.ToString(), out int r);
                                methodInfo.Invoke(list1, new object[] { r });
                                break;
                            case "int64":
                                long.TryParse(item.ToString(), out long r1);
                                methodInfo.Invoke(list1, new object[] { r1 });
                                break;
                            case "single":
                                float.TryParse(item.ToString(), out float r2);
                                methodInfo.Invoke(list1, new object[] { r2 });
                                break;
                            case "double":
                                double.TryParse(item.ToString(), out double r3);
                                methodInfo.Invoke(list1, new object[] { r3 });
                                break;
                            case "bool":
                                bool.TryParse(item.ToString(), out bool r4);
                                methodInfo.Invoke(list1, new object[] { r4 });
                                break;
                            default:
                                methodInfo.Invoke(list1, new object[] { item });
                                break;
                        }
                    }
                }

                if (list1 != null)
                {
                    field.SetValue(outObj, list1);
                }
            }
            else
            {
                field.SetValue(outObj, c.Value);
            }
        }

        return outObj;
    }

    public static void SaveModSettings(XElement xElement)
    {
        foreach (ModEntity modEntity in ModList)
        {
            modEntity.SaveSettings(xElement);
        }
    }

    public static void SaveSettings(XElement xElement)
    {
        XElement element = new("Configs");
        foreach (var c in Configs)
        {
            element.SetAttributeValue(c.Key, c.Value);
        }

        xElement.Add(element);
    }

    public static void LoadSettings(XElement xElement)
    {
        foreach (var c in xElement.Element("Configs").Attributes())
        {
            if (!Configs.ContainsKey(c.Name.LocalName)) SetConfig(c.Name.LocalName, c.Value);
        }

        ConfigLoaded = true;
    }

    public static void LoadModSettings(XElement xElement)
    {
        foreach (ModEntity modEntity in ModList)
        {
            modEntity.LoadSettings(xElement);
        }
    }

    public static void SetConfig(string key, string value)
    {
        if (!Configs.TryGetValue(key, out string mm))
        {
            Configs.Add(key, value);
        }

        Configs[key] = value;
    }

    public static string ImportMod(string name, Stream stream)
    {
        if (!Storage.DirectoryExists(ModCachePath)) Storage.CreateDirectory(ModCachePath);
        string realName = name + ModFileSuffix;
        string path = Storage.CombinePaths(ModCachePath, realName);
        int num = 1;
        while (Storage.FileExists(path))
        {
            realName = name + "(" + num + ")" + ModFileSuffix;
            path = Storage.CombinePaths(ModCachePath, realName);
            num++;
        }

        using (Stream fileStream = Storage.OpenFile(path, OpenFileMode.CreateOrOpen))
        {
            stream.CopyTo(fileStream);
        }

        var importModList = ScreensManager.FindScreen<ModsManageContentScreen>("ModsManageContent").m_latestScanModList;
        if (!importModList.Contains(realName)) importModList.Add(realName);
        DialogsManager.ShowDialog(null, new MessageDialog("Mod下载成功", "请到Mod管理器中进行手动安装，是否跳转", "前往", "返回",
            delegate(MessageDialogButton result)
            {
                if (result == MessageDialogButton.Button1)
                {
                    ScreensManager.SwitchScreen("ModsManageContent");
                }
            }));
        return "Mod下载成功";
    }

    public static void ModListAllDo(Action<ModEntity> action)
    {
        foreach (ModEntity entity in ModList)
        {
            action?.Invoke(entity);
        }
    }

    public static void Initialize()
    {
        if (!Storage.DirectoryExists(ModsPath)) Storage.CreateDirectory(ModsPath);
        ModLoaders.Clear();
        SurvivalCraftModEntity = new SurvivalCraftModEntity();
        ModEntity fastDebug = new FastDebugModEntity();
        ModList.Add(SurvivalCraftModEntity);
        ModList.Add(fastDebug);
        LoadMods(ModsPath);
        //读取SCMOD文件到ModList列表
        
        foreach (ModEntity modEntity1 in ModList)
        {
            ModInfo modInfo = modEntity1.ModInfo;
            if (modInfo == null)
            {
                LoadingScreen.Info($"[{modEntity1.ModFilePath}]缺少ModInfo文件，模组将不会被加载");
                continue;
            }
            //float.TryParse(modInfo.ApiVersion, out float curr);
            //if (curr < api)
            //{//api版本检测
            //    ToDisable.Add(modInfo);
            //    ToRemove.Add(modEntity1);
            //    AddException(new Exception($"[{modEntity1.modInfo.PackageName}]Target version {modInfo.Version} is less than api version {APIVersion}."), true);
            //}

            List<ModEntity> modEntities = ModList.FindAll(px => px.ModInfo.PackageName == modInfo.PackageName);
            if (modEntities.Count > 1) AddException(new Exception($"Multiple installed [{modInfo.PackageName}]"));
        }

        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            Assembly? asm = null;
            try
            {
                 asm = Assemblies.GetValueOrDefault(args.Name);
                 asm ??= AppDomain.CurrentDomain.GetAssemblies()
                     .FirstOrDefault(asm => asm.GetName().FullName == args.Name);
                 return asm;
            }
            catch (Exception e)
            {
                Log.Information($"无法定位程序集 {args.Name} 异常:{e.Message}");
                throw;
            }
        };
    }

    public static void AddException(Exception e, bool allowContinue = false)
    {
        LoadingScreen.Error(e.Message);
        m_allowContinue = !SettingsManager.DisplayLog || allowContinue;
    }

    /// <summary>
    /// 加载所有 Mod
    /// </summary>
    /// <param name="path"></param>
    public static void LoadMods(string path)
    {
        foreach (string item in Storage.ListFileNames(path))
        {
            string fileExtension = Storage.GetExtension(item);
            string itemPath = Storage.CombinePaths(path, item);
            Stream stream = null!;
            try
            {
                if (fileExtension.Equals(ModFileSuffix, StringComparison.CurrentCultureIgnoreCase))
                {
                    stream = Storage.OpenFile(itemPath, OpenFileMode.Read);
                    Stream keepOpenStream = ModsManageContentScreen.GetDecipherStream(stream);
                    var modEntity = new ModEntity(itemPath, Game.ZipArchive.Open(keepOpenStream, true));
                    if (modEntity.ModInfo == null) continue;
                    if (string.IsNullOrEmpty(modEntity.ModInfo.PackageName)) continue;
                    ModList.Add(modEntity);
                }

                if (string.Equals(fileExtension, ".ScNext", StringComparison.CurrentCultureIgnoreCase))
                {
                    stream = Storage.OpenFile(itemPath, OpenFileMode.Read);
                    Stream keepOpenStream = ModsManageContentScreen.GetDecipherStream(stream);
                    var modEntity = new ModEntity(itemPath, Game.ZipArchive.Open(keepOpenStream, true));
                    if (modEntity.ModInfo == null) continue;
                    if (string.IsNullOrEmpty(modEntity.ModInfo.PackageName)) continue;
                    ModList.Add(modEntity);
                }
            }
            catch (Exception e)
            {
                AddException(e);
                stream?.Close();
            }
        }

        foreach (string dir in Storage.ListDirectoryNames(path))
        {
            LoadMods(Storage.CombinePaths(path, dir));
        }
    }

    public static string StreamToString(Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        return new StreamReader(stream).ReadToEnd();
    }

    /// <summary>
    /// 将 Stream 转成 byte[]
    /// </summary>
    public static byte[] StreamToBytes(Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(bytes, 0, bytes.Length);
        // 设置当前流的位置为流的开始
        return bytes;
    }

#if DEBUG
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
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(bytes, 0, bytes.Length);
        // 设置当前流的位置为流的开始
        // 把 byte[] 写入文件
        var fs = new FileStream(fileName, FileMode.Create);
        var bw = new BinaryWriter(fs);
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
        var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        // 读取文件的 byte[]
        byte[] bytes = new byte[fileStream.Length];
        fileStream.Read(bytes, 0, bytes.Length);
        fileStream.Close();
        // 把 byte[] 转换成 Stream
        Stream stream = new MemoryStream(bytes);
        return stream;
    }
#endif

    public static string GetMd5(string input)
    {
        var md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        var sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        return sBuilder.ToString();
    }

    public static bool FindElement(XElement xElement, Func<XElement, bool> func, out XElement elementout)
    {
        foreach (XElement element in xElement.Elements())
        {
            if (func(element))
            {
                elementout = element;
                return true;
            }

            if (FindElement(element, func, out XElement element1))
            {
                elementout = element1;
                return true;
            }
        }

        elementout = null;
        return false;
    }

    public static bool FindElementByGuid(XElement xElement, string guid, out XElement elementout)
    {
        foreach (XElement element in xElement.Elements())
        {
            foreach (XAttribute xAttribute in element.Attributes())
            {
                if (xAttribute.Name.ToString() == "Guid" && xAttribute.Value == guid)
                {
                    elementout = element;
                    return true;
                }
            }

            if (FindElementByGuid(element, guid, out XElement element1))
            {
                elementout = element1;
                return true;
            }
        }

        elementout = null;
        return false;
    }

    public static bool HasAttribute(XElement element, Func<string, bool> func, out XAttribute xAttributeout)
    {
        foreach (XAttribute xAttribute in element.Attributes())
        {
            if (func(xAttribute.Name.LocalName))
            {
                xAttributeout = xAttribute;
                return true;
            }
        }

        xAttributeout = null;
        return false;
    }

    public static void CombineClo(XElement xElement, Stream cloorcr)
    {
        XElement mergeXml = XmlUtils.LoadXmlFromStream(cloorcr, Encoding.UTF8, true);
        foreach (XElement element in mergeXml.Elements())
        {
            if (HasAttribute(element, (name) => name.StartsWith("new-"), out XAttribute attribute))
            {
                if (HasAttribute(element, (name) => name == "Index", out XAttribute xAttribute))
                {
                    if (FindElement(xElement, (ele) => element.Attribute("Index")?.Value == xAttribute.Value,
                            out XElement element1))
                    {
                        string[] px = attribute.Name.ToString()
                            .Split(new string[] { "new-" }, StringSplitOptions.RemoveEmptyEntries);
                        if (px.Length == 1)
                        {
                            element1.SetAttributeValue(px[0], attribute.Value);
                        }
                    }
                }
            }
            else if (HasAttribute(element, (name) => { return name.StartsWith("r-"); }, out var attribute1))
            {
                if (HasAttribute(element, (name) => { return name == "Index"; }, out XAttribute xAttribute))
                {
                    if (FindElement(xElement, (ele) => { return element.Attribute("Index").Value == xAttribute.Value; },
                            out XElement element1))
                    {
                        element1.Remove();
                        element.Remove();
                    }
                }
            }

            xElement.Add(mergeXml);
        }
    }

    public static void CombineCr(XElement xElement, Stream cloorcr)
    {
        XElement MergeXml = XmlUtils.LoadXmlFromStream(cloorcr, Encoding.UTF8, true);
        CombineCrLogic(xElement, MergeXml);
    }

    public static void CombineCrLogic(XElement xElement, XElement needCombine)
    {
        foreach (XElement element in needCombine.Elements())
        {
            if (HasAttribute(element, (name) => { return name == "Result"; }, out XAttribute xAttribute1))
            {
                if (HasAttribute(element, (name) => { return name.StartsWith("new-"); }, out XAttribute attribute))
                {
                    string[] px = attribute.Name.ToString()
                        .Split(new string[] { "new-" }, StringSplitOptions.RemoveEmptyEntries);
                    string editName = "";
                    if (px.Length == 1)
                    {
                        editName = px[0];
                    }

                    if (FindElement(xElement, (ele) =>
                        {
                            //原始标签
                            foreach (XAttribute xAttribute in element.Attributes()) //待修改的标签
                            {
                                if (xAttribute.Name == attribute.Name) continue;
                                if (!HasAttribute(ele, (tname) => { return tname == xAttribute.Name; },
                                        out XAttribute attribute1))
                                {
                                    return false;
                                }
                            }

                            return true;
                        }, out XElement element1))
                    {
                        if (px.Length == 1)
                        {
                            element1.SetAttributeValue(px[0], attribute.Value);
                            element1.SetValue(element.Value);
                        }
                    }
                }
                else if (HasAttribute(element, (name) => { return name.StartsWith("r-"); }, out XAttribute attribute1))
                {
                    if (FindElement(xElement, (ele) =>
                        {
                            //原始标签
                            foreach (XAttribute xAttribute in element.Attributes()) //待修改的标签
                            {
                                if (xAttribute.Name == attribute1.Name) continue;
                                if (!HasAttribute(ele, (tname) => { return tname == xAttribute.Name; },
                                        out XAttribute attribute2))
                                {
                                    return false;
                                }
                            }

                            return true;
                        }, out XElement element1))
                    {
                        element1.Remove();
                        element.Remove();
                    }
                }
                else
                {
                    xElement.Add(element);
                }
            }

            CombineCrLogic(xElement, element);
        }
    }

    public static void Modify(XElement source, XElement change)
    {
        if (FindElement(source, (item) =>
            {
                if (item.Name.LocalName == change.Name.LocalName && item.Attribute("Guid") != null &&
                    change.Attribute("Guid") != null &&
                    item.Attribute("Guid").Value == change.Attribute("Guid").Value) return true;
                return false;
            }, out XElement xElement1))
        {
            foreach (XElement xElement in change.Elements())
            {
                Modify(xElement1, xElement);
            }
        }
        else
        {
            source.Add(change);
        }
    }

    public static void CombineDataBase(XElement DataBaseXml, Stream Xdb)
    {
        XElement MergeXml = XmlUtils.LoadXmlFromStream(Xdb, Encoding.UTF8, true);
        XElement DataObjects = DataBaseXml.Element("DatabaseObjects");
        foreach (XElement element in MergeXml.Elements())
        {
            //处理修改
            if (HasAttribute(element, (str) => { return str.Contains("new-"); }, out XAttribute attribute))
            {
                if (HasAttribute(element, (str) => { return str == "Guid"; }, out XAttribute attribute1))
                {
                    if (FindElementByGuid(DataObjects, attribute1.Value, out XElement xElement))
                    {
                        string[] px = attribute.Name.ToString()
                            .Split(new string[] { "new-" }, StringSplitOptions.RemoveEmptyEntries);
                        if (px.Length == 1)
                        {
                            xElement.SetAttributeValue(px[0], attribute.Value);
                        }
                    }
                }
            }

            Modify(DataObjects, element);
        }
    }
}