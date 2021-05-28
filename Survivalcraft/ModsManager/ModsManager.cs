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
        if (!Storage.DirectoryExists(ModsPath)) Storage.CreateDirectory(ModsPath);
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
                exceptions.Add(e);
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

    public static XElement FindElementByGuid(XElement xElement,string guid) {
        foreach (XElement element in xElement.Elements()) {
            foreach (XAttribute xAttribute in element.Attributes()) {
                if (xAttribute.Name.ToString() == "Guid"&&xAttribute.Value==guid) {
                    return element;
                }
            }
            XElement element1 = FindElementByGuid(element, guid);
            if (element1 != null) return element1;
        }
        return null;
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

    public static void CombineDataBase(XElement DataBaseXml,XElement MergeXml) {

        foreach (XElement element in MergeXml) {
            if (HasAttribute(element, (str) => { return str.Contains("new-"); }, out XAttribute attribute)) {
                if (HasAttribute(element,(str)=> { str == "Guid"; },out XAttribute attribute1)) {
                
                
                }
            }
        }
    }


}
