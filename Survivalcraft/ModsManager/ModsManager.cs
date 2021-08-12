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
using System.IO.Compression;


public static class ModsManager
{
    public const string APIVersion = "1.4";
    public const string SCVersion = "2.2.10.4";
    //1为api1.33 2为api1.34
    public const int Apiv = 3;
#if desktop
    public static string ExternelPath = "app:";
    public static string userDataPath = ExternelPath + "/UserId.dat";
    public static string CharacterSkinsDirectoryName = ExternelPath + "/CharacterSkins";
    public static string FurniturePacksDirectoryName = ExternelPath + "/FurniturePacks";
    public static string BlockTexturesDirectoryName = ExternelPath + "/TexturePacks";
    public static string WorldsDirectoryName = ExternelPath + "/Worlds";
    public static string communityContentCachePath = ExternelPath + "CommunityContentCache.xml";
    public static string ModsSetPath = ExternelPath + "/ModSettings.xml";
    public static string settingPath = ExternelPath + "/Settings.xml";
    public static string logPath = ExternelPath + "/Logs";
#endif
#if android
    public static string ExternelPath = EngineActivity.BasePath;
    public static string screenCapturePath =ExternelPath + "ScreenCapture";
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
    public static string ModsPath = ExternelPath + "/Mods";
    public static string path;//移动端mods数据文件夹
    internal static ModEntity SurvivalCrafModEntity;

    public class ModSettings
    {
        public LanguageControl.LanguageType languageType;
    }
    public class ModHook {
        public string HookName;
        public Dictionary<ModLoader, bool> Loaders = new Dictionary<ModLoader, bool>();
        public Dictionary<ModLoader, string> DisableReason = new Dictionary<ModLoader, string>();
        public ModHook(string name)
        {
            HookName = name;
        }
        public void Add(ModLoader modLoader)
        {
            if (Loaders.TryGetValue(modLoader, out bool k) == false)
            {
                Loaders.Add(modLoader, true);
            }
        }
        public void Remove(ModLoader modLoader)
        {
            if (Loaders.TryGetValue(modLoader, out bool k))
            {
                Loaders.Remove(modLoader);
            }
        }
        public void Disable(ModLoader from,ModLoader toDisable ,string reason)
        {
            if (Loaders.TryGetValue(toDisable, out bool k))
            {
                k = false;
                if (DisableReason.TryGetValue(from, out string res))
                {
                    res = reason;
                }
                else
                {
                    DisableReason.Add(from, reason);
                }
            }
        }
    }
    private static bool AllowContinue = true;
    public static ModSettings modSettings=new ModSettings();
    public static List<ModEntity> ModList = new List<ModEntity>();
    public static List<ModLoader> ModLoaders = new List<ModLoader>();
    public static List<ModInfo> DisabledMods = new List<ModInfo>();
    public static Dictionary<string, ModHook> ModHooks = new Dictionary<string, ModHook>();
    public static bool GetModEntity(string packagename,out ModEntity modEntity) {
        modEntity = ModList.Find(px=>px.modInfo.PackageName==packagename);
        return modEntity != null;
    }
    public static bool GetAllowContinue() { return AllowContinue; }
    /// <summary>
    /// 执行Hook
    /// </summary>
    /// <param name="HookName"></param>
    /// <param name="action"></param>
    public static void HookAction(string HookName,Func<ModLoader,bool> action) {
        if (ModHooks.TryGetValue(HookName, out ModHook modHook)) {
            foreach (ModLoader modLoader in modHook.Loaders.Keys) {
                if (action.Invoke(modLoader)) break;
            }
        }    
    }
    /// <summary>
    /// 注册Hook
    /// </summary>
    /// <param name="HookName"></param>
    /// <param name="modLoader"></param>
    public static void RegisterHook(string HookName, ModLoader modLoader)
    {
        if (ModHooks.TryGetValue(HookName, out ModHook modHook)==false)
        {
            modHook = new ModHook(HookName);
            ModHooks.Add(HookName, modHook);
        }
        modHook.Add(modLoader);
    }
    public static void DisableHook(ModLoader from, string HookName,string packageName,string reason) {
        ModEntity modEntity = ModList.Find(p => p.modInfo.PackageName == packageName);
        if (ModHooks.TryGetValue(HookName, out ModHook modHook))
        {
            modHook.Disable(from, modEntity.ModLoader_, reason);
        }
    }
    public static void StreamCompress(Stream input, MemoryStream data)
    {
        byte[] dat = data.ToArray();
        using (var stream = new GZipStream(input, CompressionMode.Compress)) {
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
            outStream.Seek(0,SeekOrigin.Begin);
            return outStream;
        }
    }
    public static T GetInPakOrStorageFile<T>(string filepath,string prefix=".txt") where T :class {
        string storagePath = Storage.CombinePaths(ExternelPath, filepath + prefix);
        if (Storage.FileExists(storagePath))
        {
            object obj = null;
            using (Stream stream = Storage.OpenFile(storagePath, OpenFileMode.Read))
            {
                obj = ContentManager.StreamConvertType(typeof(T).GetType(), stream);
            }
            return obj as T;
        }
        else return ContentManager.Get<T>(filepath);

    }
    public static T DeserializeJson<T>(string text) where T : class
    {
        var obj = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(text, typeof(JsonObject));
        var outobj = Activator.CreateInstance(typeof(T)) as T;
        Type outtype = outobj.GetType();
        foreach (var c in obj)
        {
            FieldInfo field = outtype.GetField(c.Key, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) continue;
            if (c.Value is JsonArray)
            {
                var jsonArray = c.Value as JsonArray;
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
           modEntity.SaveSettings(xElement);
        }
    }
    public static void LoadSettings(XElement xElement)
    {
        foreach (ModEntity modEntity in ModList)
        {
            modEntity.SaveSettings(xElement);
        }
    }
    public static string ImportMod(string name,Stream stream) {
        string path = Storage.CombinePaths(ModsPath,name);
        using (Stream fileStream = Storage.OpenFile(path, OpenFileMode.CreateOrOpen)) {
            stream.CopyTo(fileStream);
            stream.Close();
        }
        return "下载成功";

    }
    public static void ModListAllDo(Action<ModEntity> entity) {
        for (int i=0;i<ModList.Count;i++) {
            entity?.Invoke(ModList[i]);
        }
    }
    public static void Initialize()
    {
        if (!Storage.DirectoryExists(ModsPath)) Storage.CreateDirectory(ModsPath);
        ModHooks.Clear();
        ModList.Clear();
        ModLoaders.Clear();
        SurvivalCrafModEntity = new SurvivalCrafModEntity();
        ModList.Add(SurvivalCrafModEntity);
        ModList.Add(new FastDebugModEntity());
        GetScmods(ModsPath);
        DisabledMods.Clear();
        List<ModEntity> ToRemove = new List<ModEntity>();
        List<ModInfo> ToDisable = new List<ModInfo>();
        ToDisable.AddRange(DisabledMods);
        foreach (ModEntity modEntity1 in ModList) {
            ModInfo modInfo = modEntity1.modInfo;
            ModInfo disabledmod = ToDisable.Find(l=>l.PackageName==modInfo.PackageName&&l.Version==modInfo.Version);
            if (disabledmod != null)
            {
                ToDisable.Add(modEntity1.modInfo);
                ToRemove.Add(modEntity1);
                continue;
            }
            if (modEntity1.IsChecked) continue;
            List<ModEntity> modEntities = ModList.FindAll(px => px.modInfo.PackageName == modInfo.PackageName);
            var version = new Version();
            foreach (ModEntity modEntity in modEntities)
            {
                if (version <= new Version(modEntity.modInfo.Version)) version = new Version(modEntity.modInfo.Version);
            }
            List<ModEntity> entities = ModList.FindAll(px => px.modInfo.PackageName == modInfo.PackageName && new Version(px.modInfo.Version) != new Version(modInfo.Version) && new Version(px.modInfo.Version) == version);
            if (entities.Count>1)
            {
                AddException(new InvalidOperationException($"检测到已安装多个[{modEntity1.modInfo.Name}]，已加载版本:{version}"));
                foreach (ModEntity modEntity in modEntities)
                {
                    if (version != new Version(modEntity.modInfo.Version))
                    {
                        ToDisable.Add(modEntity1.modInfo);
                        ToRemove.Add(modEntity1);
                    }
                    modEntity1.IsChecked = true;
                }
            }
        }
        foreach (var item in ToRemove) {
            ModList.Remove(item);
        }
    }
    public static void AddException(Exception e,bool AllowContinue_=false) {
        LoadingScreen.Error(e.Message);
        AllowContinue = AllowContinue_;
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
                    var modEntity = new ModEntity(ZipArchive.Open(stream, true));
                    if (modEntity.modInfo == null) continue;
                    if (string.IsNullOrEmpty(modEntity.modInfo.PackageName)) continue;
                    ModList.Add(modEntity);
                }
            }
            catch (Exception e)
            {
                stream.Close();
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
    public static bool FindElement(XElement xElement, Func<XElement,bool> func, out XElement elementout)
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
        XElement MergeXml = XmlUtils.LoadXmlFromStream(cloorcr, Encoding.UTF8,true);
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
                }
                else
                {
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
#if DEBUG
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
        var stringBuilder = new StringBuilder();
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
        var image = new Image(renderTarget2D.Width, renderTarget2D.Height);
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
#endif
}
