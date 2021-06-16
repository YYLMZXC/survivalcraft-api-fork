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
using Engine.Graphics;
using Engine.Media;

public static class ModsManager
{
    public static Dictionary<string, ZipArchive> Archives;
    public const string APIVersion = "1.34";
    public const string SCVersion = "2.2.10.4";
    //1为api1.33 2为api1.34
    public const int Apiv = 2;
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
    public static List<ModEntity> ModList = new List<ModEntity>();
    public static List<ModLoader> ModLoaders = new List<ModLoader>();
    public static List<ModInfo> DisabledMods = new List<ModInfo>();

    public static T DeserializeJson<T>(string text) where T : class
    {
        JsonObject obj = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(text, typeof(JsonObject));
        T outobj = Activator.CreateInstance(typeof(T)) as T;
        Type outtype = outobj.GetType();
        foreach (var c in obj)
        {
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
        foreach (ModEntity modEntity in ModList) {
           if(modEntity.IsLoaded&&!modEntity.IsDisabled) modEntity.SaveSettings(xElement);
        }
    }
    public static void LoadSettings(XElement xElement)
    {
        foreach (ModEntity modEntity in ModList)
        {
            if (modEntity.IsLoaded && !modEntity.IsDisabled) modEntity.SaveSettings(xElement);
        }
    }
    public static string ImportMod(string name,Stream stream) {
        string path = Storage.CombinePaths(ModsPath,name);
        Stream fileStream = Storage.OpenFile(path,OpenFileMode.CreateOrOpen);
        stream.CopyTo(fileStream);
        fileStream.Close();
        return "下载成功";

    }
    public static void Initialize()
    {
        if (!Storage.DirectoryExists(ModsPath)) Storage.CreateDirectory(ModsPath);
        ModList.Clear();
        ModLoaders.Clear();
        ModList.Add(new SurvivalCrafModEntity());
        ModList.Add(new FastDebugModEntity());
        GetScmods(ModsPath);
        foreach (ModEntity modEntity1 in ModList) {
            ModInfo modInfo = modEntity1.modInfo;
            ModInfo disabledmod = DisabledMods.Find(l=>l.PackageName==modInfo.PackageName&&l.Version==modInfo.Version);
            if (disabledmod != null) {
                modEntity1.IsDisabled = true;
                modEntity1.IsLoaded = false;
                continue;
            }
            if (modEntity1.IsChecked) continue;
            List<ModEntity> modEntities = ModsManager.ModList.FindAll(px => px.IsLoaded && !px.IsDisabled && px.modInfo.PackageName == modInfo.PackageName);
            Version version = new Version();
            foreach (ModEntity modEntity in modEntities)
            {
                if (version <= new Version(modEntity.modInfo.Version)) version = new Version(modEntity.modInfo.Version);
            }
            List<ModEntity> entities = ModsManager.ModList.FindAll(px => px.modInfo.PackageName == modInfo.PackageName && new Version(px.modInfo.Version) != new Version(modInfo.Version) && new Version(px.modInfo.Version) == version);
            if (entities.Count>1)
            {
                ModsManager.AddException(new InvalidOperationException($"检测到已安装多个[{modEntity1.modInfo.Name}]，已加载版本:{version}"));
                foreach (ModEntity modEntity in modEntities)
                {
                    if (version != new Version(modEntity.modInfo.Version))
                    {
                        modEntity1.IsLoaded = false;
                        modEntity1.IsDisabled = true;
                    }
                    modEntity1.IsChecked = true;
                }
            }
        }
    }
    public static void AddException(Exception e) {
        exceptions.Add(e);
    }
    /// <summary>
    /// 获取所有文件
    /// </summary>
    /// <param name="path"></param>
    public static void GetScmods(string path)
    {
        foreach (string item in Storage.ListFileNames(path))
        {
            string ms = Storage.GetExtension(item);
            string ks = Storage.CombinePaths(path, item);
            Stream stream = Storage.OpenFile(ks, OpenFileMode.Read);
            try
            {
                if (ms == ".scmod")
                {
                    ModEntity modEntity = new ModEntity(ZipArchive.Open(stream, true));
                    if (modEntity.modInfo == null) continue;
                    if (string.IsNullOrEmpty(modEntity.modInfo.PackageName)) continue;
                    ModList.Add(modEntity);
                }
            }
            catch (Exception e)
            {
                AddException(e);
            }
        }
        foreach (string dir in Storage.ListDirectoryNames(path))
        {
            GetScmods(Storage.CombinePaths(path, dir));
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
            if (func(xAttribute.Name.LocalName)) {
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
        XElement MergeXml = XmlUtils.LoadXmlFromStream(cloorcr, Encoding.UTF8, true);
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
                    if (FindElement(xElement, (ele) =>
                    {//原始标签
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
                }else {
                    xElement.Add(element);
                }
            }
            CombineCrLogic(xElement, element);
        }
    }
    public static void Modify(XElement source,XElement change) {
        if (FindElement(source, (item) => { if (item.Name.LocalName == change.Name.LocalName && item.Attribute("Guid")!=null && change.Attribute("Guid") != null && item.Attribute("Guid").Value == change.Attribute("Guid").Value) return true;return false; }, out XElement xElement1)){
            foreach (XElement xElement in change.Elements()) {
                Modify(xElement1,xElement);
            }
        }
        else
        {
            source.Add(change);
        }

    }
    public static void CombineDataBase(XElement DataBaseXml,Stream Xdb) {
        XElement MergeXml = XmlUtils.LoadXmlFromStream(Xdb, Encoding.UTF8, true);
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
            Modify(DataObjects,element);
        }
    }
    public enum SourceType{
        positions,
        normals,
        map,
        vertices,
        TEXCOORD,
        VERTEX,
        NORMAL
    }
    public static string ObjectsToStr<T>(T[] arr) {
        if (arr == null) return string.Empty;
        StringBuilder stringBuilder = new StringBuilder();
        for (int i=0;i<arr.Length;i++) {
            stringBuilder.Append(arr[i]+" ");
        }
        string res = stringBuilder.ToString();
        return res.Substring(0,res.Length-1);
    }
    /// <summary>
    /// 计算三点成面的法向量
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <returns></returns>
    public static Vector3 Cal_Normal_3D(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float na = (v2.Y - v1.Y) * (v3.Z - v1.Z) - (v2.Z - v1.Z) * (v3.Y - v1.Y);
        float nb = (v2.Z - v1.Z) * (v3.X - v1.X) - (v2.X - v1.Z) * (v3.Z - v1.Z);
        float nc = (v2.X - v1.X) * (v3.Y - v1.Y) - (v2.Y - v1.Y) * (v3.X - v1.X);
        return new Vector3(na, nb, nc);
    }
    public static void SaveToImage(string name,RenderTarget2D renderTarget2D)
    {
        Image image = new Image(renderTarget2D.Width, renderTarget2D.Height);
        renderTarget2D.GetData(image.Pixels, 0, new Rectangle(0, 0, renderTarget2D.Width, renderTarget2D.Height));
        try
        {
            Image.Save(image, Storage.CombinePaths("app:", name + ".png"), ImageFileFormat.Png, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
