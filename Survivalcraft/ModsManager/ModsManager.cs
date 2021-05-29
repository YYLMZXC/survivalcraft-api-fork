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
using SimpleJson;
using SimpleJson.Reflection;



public static class ModsManager
{
    public static Dictionary<string, ZipArchive> Archives;
    public const string APIVersion = "1.34";
    public const string SCVersion = "2.2.10.4";

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
    public static List<Exception> exceptions = new List<Exception>();
    public static List<ModEntity> LoadedMods = new List<ModEntity>();
    public static List<ModEntity> CacheToLoadMods = new List<ModEntity>();
    public static List<ModEntity> WaitToLoadMods = new List<ModEntity>();
    public static List<ModLoader> ModLoaders = new List<ModLoader>();

    public static T DeserializeJson<T>(string text) where T : class
    {
        JsonObject obj = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(text, typeof(JsonObject));
        T outobj = Activator.CreateInstance(typeof(T)) as T;
        Type outtype = outobj.GetType();
        foreach (var c in obj)
        {
            FieldInfo[] fieldInfos = outtype.GetFields();
            FieldInfo field = outtype.GetField(c.Key, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) continue;
            if (c.Value is JsonArray)
            {
                JsonArray jsonArray = c.Value as JsonArray;
                Type[] types = field.FieldType.GetGenericArguments();
                var list1 = Activator.CreateInstance(typeof(List<>).MakeGenericType(types));
                foreach (var item in jsonArray)
                {
                    Type type = list1.GetType();
                    MethodInfo methodInfo = type.GetMethod("Add");
                    if (types.Length == 1)
                    {
                        string tn = types[0].Name.ToLower();
                        switch (tn)
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
                    field.SetValue(outobj, list1);
                }
            }
            else
            {
                field.SetValue(outobj, c.Value);
            }
        }
        return outobj;
    }
    public static void SaveSettings(XElement xElement)
    {
        foreach (ModEntity modEntity in CacheToLoadMods) {
            modEntity.SaveSettings(xElement);
        }
    }
    public static void LoadSettings(XElement xElement)
    {
        foreach (ModEntity modEntity in CacheToLoadMods)
        {
            modEntity.SaveSettings(xElement);
        }
    }
    public static string ImportMod(string name,Stream stream) {
        string path = Storage.CombinePaths(ModsPath,name);
        Stream fileStream = Storage.OpenFile(path,OpenFileMode.CreateOrOpen);
        stream.CopyTo(fileStream);
        fileStream.Close();
        return "下载成功,重启游戏生效";

    }
    public static void Initialize()
    {
        if (!Storage.DirectoryExists(ModsPath)) Storage.CreateDirectory(ModsPath);
        WaitToLoadMods.Clear();
        WaitToLoadMods.Add(new SurvivalCrafModEntity());
        ModLoaders.Clear();
        GetAllFiles(ModsManager.ModsPath);
    }
    public static void AddException(Exception e) {
        exceptions.Add(e);
    }
    /// <summary>
    /// 获取所有文件
    /// </summary>
    /// <param name="path"></param>
    public static void GetAllFiles(string path)
    {
        foreach (string item in Storage.ListFileNames(path))
        {
            string ms = Storage.GetExtension(item);
            string ks = Storage.CombinePaths(path, item);
            Stream stream = Storage.OpenFile(ks, OpenFileMode.Read);
            try
            {
                if (ms == ".zip" || ms == ".scmod")
                {
                    ModEntity modEntity = new ModEntity(ZipArchive.Open(stream, true));
                    WaitToLoadMods.Add(modEntity);
                }
            }
            catch (Exception e)
            {
                AddException(e);
            }
        }
        foreach (string dir in Storage.ListDirectoryNames(path))
        {
            GetAllFiles(Storage.CombinePaths(path, dir));
        }
    }
    public static string StreamToString(Stream stream)
    {
        stream.Seek(0,SeekOrigin.Begin);
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
        Type[] types = asm.GetTypes();
        for (int i = 0; i < types.Length; i++)
        {
            MethodInfo method;
            if ((method = types[i].GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) != null)
            {
                method.Invoke(Activator.CreateInstance(types[i]), null);
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
    public static bool FindElement(XElement xElement,Func<XElement,bool> func, out XElement elementout)
    {
        foreach (XElement element in xElement.Elements())
        {
            if (func(element)) {
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
    public static bool FindElementByGuid(XElement xElement,string guid,out XElement elementout) {
        foreach (XElement element in xElement.Elements()) {
            foreach (XAttribute xAttribute in element.Attributes()) {
                if (xAttribute.Name.ToString() == "Guid" && xAttribute.Value == guid) {
                    elementout = element;
                    return true;
                }
            }
            if (FindElementByGuid(element, guid, out XElement element1)){
                elementout = element1;
                return true;
            }
        }
        elementout = null;
        return false;
    }
    public static bool HasAttribute(XElement element,Func<string,bool> func,out XAttribute xAttributeout) {
        foreach (XAttribute xAttribute in element.Attributes())
        {
            if (func(xAttribute.Name.ToString())) {
                xAttributeout = xAttribute;
                return true;
            }
        }
        xAttributeout = null;
        return false;
    }
    public static void CombineClo(XElement xElement,Stream cloorcr) {
        XElement MergeXml = XmlUtilities.XmlUtils.LoadXmlFromStream(cloorcr, Encoding.UTF8,true);
        foreach (XElement element in MergeXml.Elements()) {
            if (HasAttribute(element, (name) => { return name.StartsWith("new-"); }, out XAttribute attribute)) {
                if (HasAttribute(element, (name) => { return name == "Index"; }, out XAttribute xAttribute)) {
                    if (FindElement(xElement, (ele) => { return element.Attribute("Index").Value == xAttribute.Value; }, out XElement element1)) {
                        string[] px = attribute.Name.ToString().Split(new string[] { "new-" }, StringSplitOptions.RemoveEmptyEntries);
                        if (px.Length == 1)
                        {
                            element1.SetAttributeValue(px[0], attribute.Value);
                        }
                    }
                }
            }
            xElement.Add(MergeXml);
        }
    }
    public static void CombineCr(XElement xElement, Stream cloorcr)
    {
        XElement MergeXml = XmlUtilities.XmlUtils.LoadXmlFromStream(cloorcr, Encoding.UTF8, true);
        CombineCrLogic(xElement,MergeXml);
    }
    public static void CombineCrLogic(XElement xElement, XElement needCombine) {

        foreach (XElement element in needCombine.Elements())
        {
            if (HasAttribute(element, (name) => { return name == "Result"; }, out XAttribute xAttribute1))
            {
                if (HasAttribute(element, (name) => { return name.StartsWith("new-"); }, out XAttribute attribute))
                {
                    string[] px = attribute.Name.ToString().Split(new string[] { "new-" }, StringSplitOptions.RemoveEmptyEntries);
                    string editName = "";
                    if (px.Length == 1)
                    {
                        editName = px[0];
                    }
                    if (FindElement(xElement, (ele) => {//原始标签
                        foreach (XAttribute xAttribute in element.Attributes())//待修改的标签
                        {
                            if (xAttribute.Name == attribute.Name) continue;
                            if (!HasAttribute(ele, (tname) => { return tname == xAttribute.Name; }, out XAttribute attribute1)) { return false; }
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
            }
            CombineCrLogic(xElement, element);
        }


    }
    public static void CombineDataBase(XElement DataBaseXml,Stream Xdb) {
        XElement MergeXml=XmlUtilities.XmlUtils.LoadXmlFromStream(Xdb,Encoding.UTF8,true);
        XElement DataObjects = DataBaseXml.Element("DatabaseObjects");
        foreach (XElement element in MergeXml.Elements()) {
            //处理修改
            if (HasAttribute(element, (str) => { return str.Contains("new-"); }, out XAttribute attribute)) {
                if (HasAttribute(element,(str)=> {return str == "Guid"; },out XAttribute attribute1)) {
                    if (FindElementByGuid(DataObjects, attribute1.Value, out XElement xElement)) {
                        string[] px = attribute.Name.ToString().Split(new string[] { "new-"},StringSplitOptions.RemoveEmptyEntries);
                        if (px.Length == 1) {
                            xElement.SetAttributeValue(px[0], attribute.Value);
                        }
                    }                
                }
            }
            if (element.Name.ToString() == "Folder") {
                if (ModsManager.HasAttribute(element, (name) => { return name == "Guid"; }, out XAttribute xAttribute)) {
                    if (FindElementByGuid(DataObjects, xAttribute.Value, out XElement xElement))
                    {
                        foreach (XElement element1 in element.Elements()) {
                            xElement.Add(element1);
                        }
                    }
                }            
            }
        }
    }


}
