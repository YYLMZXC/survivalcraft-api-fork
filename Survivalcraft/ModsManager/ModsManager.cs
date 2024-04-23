// Game.ModsManager

using Engine;
using Game;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Engine.Serialization;
using XmlUtilities;
using Engine.Media;
using Engine.Graphics;
using System.IO.Compression;
using System.IO;
using System.Text.Json;


public static class ModsManager
{
	public const string modSuffix = ".scmod";
	public const string APIVersion = "1.70A";
	public const string SCVersion = "2.3.10.4";
	//1为api1.33 2为api1.40
	public const int Apiv = 10;

#if desktop
	public const string
		ExtPath = "app:",//ExternelPath
		DocPath = "/doc",//DocumentPath
		UserDataPath = ExtPath + DocPath + "/UserId.dat",
		CharacterSkinsDirectoryName = ExtPath + DocPath + "/CharacterSkins",
		FurniturePacksDirectoryName = ExtPath + DocPath + "/FurniturePacks",
		BlockTexturesDirectoryName = ExtPath + DocPath + "/TexturePacks",
		WorldsDirectoryName = ExtPath + "/Worlds",
		CommunityContentCachePath = ExtPath + DocPath + "/CommunityContentCache.xml",
		ModsSetPath = ExtPath + "/ModSettings.xml",
		SettingPath = ExtPath + "/Settings.xml",
		ModCachePath = ExtPath + "/Mods/Cache",
		LogPath = ExtPath + "/Bugs";
	public const bool IsAndroid = false;

#endif
#if ANDROID
	public static string ExtPath = EngineActivity.BasePath,
						 ScreenCapturePath = ExtPath + "/ScreenCapture",
						 UserDataPath = "config:/UserId.dat",
						 FurniturePacksDirectoryName = "config:/FurniturePacks",
						 CharacterSkinsDirectoryName = "config:/CharacterSkins",
						 BlockTexturesDirectoryName = "config:/TexturePacks",
						 WorldsDirectoryName = "config:/Worlds",
						 CommunityContentCachePath = "config:/CommunityContentCache.xml",
						 ModsSetPath = "config:/ModSettings.xml",
						 SettingPath = "config:/Settings.xml",
						 ModCachePath = ExtPath + "/ModsCache",
						 LogPath = ExtPath + "/Bugs";
	public const bool IsAndroid = true;

#endif
	public static string ModsPath = ExtPath + "/Mods";//移动端mods数据文件夹
	internal static ModEntity SurvivalCraftModEntity;
	internal static bool ConfigLoaded = false;

	public class ModSettings
	{
		public string languageType = string.Empty;
	}

	public class ModHook
	{
		public string HookName;
		public Dictionary<ModLoader, bool> Loaders = [];
		public Dictionary<ModLoader, string> DisableReason = [];
		public ModHook(string name)
		{
			HookName = name;
		}

		public void Add(ModLoader modLoader)
		{
			if (Loaders.TryGetValue(modLoader, out _) == false)
			{
				Loaders.Add(modLoader, true);
			}
		}

		public void Remove(ModLoader modLoader)
		{
			if (Loaders.TryGetValue(modLoader, out _))
			{
				Loaders.Remove(modLoader);
			}
		}

		public void Disable(ModLoader from, ModLoader toDisable, string reason)
		{
			if (Loaders.TryGetValue(toDisable, out _))
			{
				if (!DisableReason.TryGetValue(from, out _))
				{
					DisableReason.Add(from, reason);
				}
			}
		}
	}

	private static bool AllowContinue = true;
	public static Dictionary<string, string> Configs = [];
	public static List<ModEntity> ModListAll = [];
	public static List<ModEntity> ModList = [];
	public static List<ModLoader> ModLoaders = [];
	public static List<ModInfo> DisabledMods = [];
	public static Dictionary<string, ModHook> ModHooks = [];
	public static Dictionary<string, Assembly> Dlls = [];

	public static bool GetModEntity(string packagename, out ModEntity modEntity)
	{
		modEntity = ModList.Find(px => px.modInfo.PackageName == packagename);
		return modEntity != null;
	}

	public static bool GetAllowContinue()
	{
		return AllowContinue;
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
	/// <summary>
	/// 执行Hook
	/// </summary>
	/// <param name="HookName"></param>
	/// <param name="action"></param>
	public static void HookAction(string HookName, Func<ModLoader, bool> action)
	{
		if (ModHooks.TryGetValue(HookName, out ModHook modHook))
		{
			foreach (ModLoader modLoader in modHook.Loaders.Keys)
			{
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
		if (ModHooks.TryGetValue(HookName, out ModHook modHook) == false)
		{
			modHook = new ModHook(HookName);
			ModHooks.Add(HookName, modHook);
		}
		modHook.Add(modLoader);
	}

	public static void DisableHook(ModLoader from, string HookName, string packageName, string reason)
	{
		ModEntity modEntity = ModList.Find(p => p.modInfo.PackageName == packageName);
		if (modEntity != null && ModHooks.TryGetValue(HookName, out ModHook modHook))
		{
			modHook.Disable(from, modEntity.Loader, reason);
		}
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
	public static T GetInPakOrStorageFile<T>(string filepath, string suffix = ".txt") where T : class
	{
		//string storagePath = Storage.CombinePaths(ExternelPath, filepath + prefix);
		return ContentManager.Get<T>(filepath, suffix);
	}

	public static ModInfo DeserializeJson(string json)
	{
        ModInfo modInfo = new ModInfo();
		JsonElement jsonElement = JsonDocument.Parse(json).RootElement;
		if(jsonElement.TryGetProperty("Name", out JsonElement name))
		{
			modInfo.Name = name.GetString();
        }
        if (jsonElement.TryGetProperty("Version", out JsonElement version) && version.ValueKind == JsonValueKind.String)
        {
            modInfo.Version = version.GetString();
        }
        if (jsonElement.TryGetProperty("ApiVersion", out JsonElement apiVersion) && apiVersion.ValueKind == JsonValueKind.String)
        {
            modInfo.ApiVersion = apiVersion.GetString();
        }
        if (jsonElement.TryGetProperty("Description", out JsonElement description) && description.ValueKind == JsonValueKind.String)
        {
            modInfo.Description = description.GetString();
        }
        if (jsonElement.TryGetProperty("ScVersion", out JsonElement scVersion) && scVersion.ValueKind == JsonValueKind.String)
        {
            modInfo.ScVersion = scVersion.GetString();
        }
        if (jsonElement.TryGetProperty("Link", out JsonElement link) && link.ValueKind == JsonValueKind.String)
        {
            modInfo.Link = link.GetString();
        }
        if (jsonElement.TryGetProperty("Author", out JsonElement author) && author.ValueKind == JsonValueKind.String)
        {
            modInfo.Author = author.GetString();
        }
        if (jsonElement.TryGetProperty("PackageName", out JsonElement packageName) && packageName.ValueKind == JsonValueKind.String)
        {
            modInfo.PackageName = packageName.GetString();
        }
        if (jsonElement.TryGetProperty("Dependencies", out JsonElement dependencies) && dependencies.ValueKind == JsonValueKind.Array)
        {
            modInfo.Dependencies = dependencies.EnumerateArray().Where(dependency=> dependency.ValueKind == JsonValueKind.String).Select(dependency => dependency.GetString()).ToList();
        }
        return modInfo;
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
		if (!Configs.TryAdd(key, value))
		{
			Configs[key] = value;
		}
    }

	public static string ImportMod(string name, Stream stream)
	{
		if (!Storage.DirectoryExists(ModCachePath)) Storage.CreateDirectory(ModCachePath);
		string realName = name + modSuffix;
		string path = Storage.CombinePaths(ModCachePath, realName);
		int num = 1;
		while (Storage.FileExists(path))
		{
			realName = name + "(" + num + ")"+modSuffix;
			path = Storage.CombinePaths(ModCachePath, realName);
			num++;
		}
		using (Stream fileStream = Storage.OpenFile(path, OpenFileMode.CreateOrOpen))
		{
			stream.CopyTo(fileStream);
		}
		var importModList = ScreensManager.FindScreen<ModsManageContentScreen>("ModsManageContent").m_latestScanModList;
		if (!importModList.Contains(realName)) importModList.Add(realName);
		DialogsManager.ShowDialog(null, new MessageDialog("Mod下载成功", "请到Mod管理器中进行手动安装，是否跳转", "前往", "返回", delegate (MessageDialogButton result)
		{
			if (result == MessageDialogButton.Button1)
			{
				ScreensManager.SwitchScreen("ModsManageContent");
			}
		}));
		return "Mod下载成功";
	}

	public static void ModListAllDo(Action<ModEntity> entity)
	{
		for (int i = 0; i < ModList.Count; i++)
			entity?.Invoke(ModList[i]);
	}

	public static void Initialize()
	{
		if (!Storage.DirectoryExists(ModsPath)) Storage.CreateDirectory(ModsPath);
		ModHooks.Clear();
		ModListAll.Clear();
		ModLoaders.Clear();
		SurvivalCraftModEntity = new SurvivalCraftModEntity();
		ModEntity FastDebug = new FastDebugModEntity();
		ModListAll.Add(SurvivalCraftModEntity);
		ModListAll.Add(FastDebug);
		GetScmods(ModsPath);
		List<ModInfo> ToDisable = [.. DisabledMods];
		DisabledMods.Clear();
		//float api = float.Parse(APIVersion);
		List<ModEntity> ToRemove = [];
		//读取SCMOD文件到ModListAll列表
		foreach (ModEntity modEntity1 in ModListAll)
		{
			ModInfo modInfo = modEntity1.modInfo;
			ModInfo disabledmod = ToDisable.Find(l => l.PackageName == modInfo.PackageName);
			if (disabledmod != null && disabledmod.PackageName != SurvivalCraftModEntity.modInfo.PackageName && disabledmod.PackageName != FastDebug.modInfo.PackageName)
			{
				ToDisable.Add(modInfo);
				ToRemove.Add(modEntity1);
				continue;
			}
			//float.TryParse(modInfo.ApiVersion, out float curr);
			//if (curr < api)
			//{//api版本检测
			//    ToDisable.Add(modInfo);
			//    ToRemove.Add(modEntity1);
			//    AddException(new Exception($"[{modEntity1.modInfo.PackageName}]Target version {modInfo.Version} is less than api version {APIVersion}."), true);
			//}
			List<ModEntity> modEntities = ModListAll.FindAll(px => px.modInfo.PackageName == modInfo.PackageName);
			if (modEntities.Count > 1) AddException(new Exception($"Multiple installed [{modInfo.PackageName}]"));
		}
		DisabledMods.Clear();
		foreach (var item in ToDisable)
		{
			DisabledMods.Add(item);
		}
		foreach (var item in ToRemove)
		{
			ModListAll.Remove(item);
		}
		AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
		{
			try
            {
#nullable enable
                Assembly? assembly = Dlls.GetValueOrDefault(args.Name) ??
				                     TypeCache.LoadedAssemblies.FirstOrDefault(asm => asm.GetName().FullName == args.Name);
				return assembly;
#nullable disable
			}
			catch (Exception e)
			{
				Log.Error($"加载程序集{args.Name}失败:{e.Message}");
				Log.Debug(e);
				throw;
			}
		};
	}

	public static void AddException(Exception e, bool AllowContinue_ = false)
	{
		LoadingScreen.Error(e.Message);
		AllowContinue = !SettingsManager.DisplayLog || AllowContinue_;
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
			using (Stream stream = Storage.OpenFile(ks, OpenFileMode.Read))
			{
				try
				{
					if (ms == modSuffix || ms == ".SCNEXT")
					{
						Stream keepOpenStream = ModsManageContentScreen.GetDecipherStream(stream);
						var modEntity = new ModEntity(ks, Game.ZipArchive.Open(keepOpenStream, true));
                        if (modEntity.modInfo == null)
                        {
                            LoadingScreen.Warning($"[{modEntity.ModFilePath}]缺少ModInfo文件，忽略加载");
							continue;
                        }
                        if (string.IsNullOrEmpty(modEntity.modInfo.PackageName)) continue;
						ModListAll.Add(modEntity);
					}
				}
				catch (Exception e)
				{
					AddException(e);
					stream.Close();
				}
			}
		}
		foreach (string dir in Storage.ListDirectoryNames(path))
		{
			GetScmods(Storage.CombinePaths(path, dir));
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
		byte[] data = MD5.HashData(Encoding.Default.GetBytes(input));
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
		XElement MergeXml = XmlUtils.LoadXmlFromStream(cloorcr, Encoding.UTF8, true);
		foreach (XElement element in MergeXml.Elements())
		{
			if (HasAttribute(element, (name) => { return name.StartsWith("new-"); }, out XAttribute attribute))
			{
				if (HasAttribute(element, (name) => { return name == "Index"; }, out XAttribute xAttribute))
				{
					if (FindElement(xElement, (ele) => { return element.Attribute("Index").Value == xAttribute.Value; }, out XElement element1))
					{
						string[] px = attribute.Name.ToString().Split(new string[] { "new-" }, StringSplitOptions.RemoveEmptyEntries);
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
					if (FindElement(xElement, (ele) => { return element.Attribute("Index").Value == xAttribute.Value; }, out XElement element1))
					{
						element1.Remove();
						element.Remove();
					}
				}
			}
			xElement.Add(MergeXml);
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
				else if (HasAttribute(element, (name) => { return name.StartsWith("r-"); }, out XAttribute attribute1))
				{
					if (FindElement(xElement, (ele) =>
					{//原始标签
						foreach (XAttribute xAttribute in element.Attributes())//待修改的标签
						{
							if (xAttribute.Name == attribute1.Name) continue;
							if (!HasAttribute(ele, (tname) => { return tname == xAttribute.Name; }, out XAttribute attribute2)) { return false; }
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
		if (FindElement(source, (item) => { if (item.Name.LocalName == change.Name.LocalName && item.Attribute("Guid") != null && change.Attribute("Guid") != null && item.Attribute("Guid").Value == change.Attribute("Guid").Value) return true; return false; }, out XElement xElement1))
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
						string[] px = attribute.Name.ToString().Split(new string[] { "new-" }, StringSplitOptions.RemoveEmptyEntries);
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
        for (int i=0;i<arr.Length;i++) 
            stringBuilder.Append(arr[i]+" ");
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
    public static void SaveToImage(string name, RenderTarget2D renderTarget2D)
    {
        try
        {
            Image.Save(renderTarget2D.GetData(new Rectangle(0, 0, renderTarget2D.Width, renderTarget2D.Height)), Storage.CombinePaths("app:", name + ".png"), ImageFileFormat.Png, true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
#endif
}
