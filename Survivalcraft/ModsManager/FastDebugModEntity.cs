using System;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Engine;

namespace Game
{
	public class FastDebugModEntity : ModEntity
	{
		public Dictionary<string, FileInfo> FModFiles = [];

		public FastDebugModEntity() : base(new ZipArchive())
		{
			ModInfo = new ModInfo() { Name = "[Debug]", PackageName = "debug" };
			InitializeResources();
		}

		protected sealed override void InitializeResources()
		{
			ReadDirResources(ModsManager.ModsPath, "");
			if (!GetFile("modinfo.json", (stream) =>
			{
				ModInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(stream));
				ModInfo.Name = $"[Debug]{ModInfo.Name}";
			}))
			{
				ModInfo = new ModInfo { Name = "FastDebug", Version = "1.0.0", ApiVersion = ModsManager.ApiCurrentVersionString, Author = "Mod", Description = "调试Mod插件", ScVersion = "2.3.0.0", PackageName = "com.fastdebug" };
			}
			GetFile("icon.png", (stream) => { LoadIcon(stream); });
		}

		public void ReadDirResources(string basePath, string path)
		{
			if (string.IsNullOrEmpty(path)) path = basePath;
			foreach (string d in Storage.ListDirectoryNames(path)) ReadDirResources(basePath, path + "/" + d);
			foreach (string f in Storage.ListFileNames(path))
			{
				string absPath = path + "/" + f;
				string fileNameInZip = absPath[(basePath.Length + 1)..];
				if (fileNameInZip.StartsWith("Assets/"))
				{
					string name = fileNameInZip.Substring(7);
					ContentInfo contentInfo = new(name);
					MemoryStream memoryStream = new();
					using Stream stream = Storage.OpenFile(absPath, OpenFileMode.Read);
					stream.CopyTo(memoryStream);
					contentInfo.SetContentStream(memoryStream);
					ContentManager.Add(contentInfo);
				}
				FModFiles.Add(fileNameInZip, new FileInfo(Storage.GetSystemPath(absPath)));
			}
		}

		public override Assembly[] GetAssemblies()
		{
		    var assemblies = new List<Assembly>();
			foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
			{
				if (c.EndsWith(".dll") && !(c.StartsWith("EntitySystem") || c.StartsWith("Engine") || c.StartsWith("Survivalcraft") || c.StartsWith("OpenTK")))
				{
					var assemblyStream = Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath), OpenFileMode.Read);
					
					assemblies.Add(Assembly.Load(ModsManager.StreamToBytes(assemblyStream)));
				}
			}
			return [.. assemblies];
		}

		public override void LoadClo(ClothingBlock block, ref XElement xElement)
		{
			foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
			{
				if (c.EndsWith(".clo")) ModsManager.CombineClo(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
			}
		}

		public override void LoadCr(ref XElement xElement)
		{
			foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
			{
				if (c.EndsWith(".cr"))
					ModsManager.CombineCr(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
			}
		}

		public override void LoadLanguage()
		{
			string path = Storage.CombinePaths(ModsManager.ModsPath, "Assets/Lang");
			if (Storage.DirectoryExists(path))
			{
				foreach (string c in Storage.ListFileNames(path))
				{
					string fn = ModsManager.Configs["Language"] + ".json";
					string fpn = Storage.CombinePaths(path, c);
					if (c == fn && Storage.FileExists(fpn))
					{
						LanguageControl.loadJson(Storage.OpenFile(fpn, OpenFileMode.Read));
					}
				}
			}
		}

		public override void LoadBlocksData()
		{
			foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
			{
				if (c.EndsWith(".csv"))
					BlocksManager.LoadBlocksData(ModsManager.StreamToString(Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read)));
			}
		}

		public override void LoadXdb(ref XElement xElement)
		{
			foreach (string c in Storage.ListFileNames(ModsManager.ModsPath))
			{
				if (c.EndsWith(".xdb"))
				{
					ModsManager.CombineDataBase(xElement, Storage.OpenFile(Storage.CombinePaths(ModsManager.ModsPath, c), OpenFileMode.Read));
				}
			}

			XElement element = xElement;
			ModInterfacesManager.InvokeHooks("OnXdbLoad", (SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
			{
				modInterface.OnXdbLoad(element);
				isContinueRequired = true;
			}, this);
		}

		/// <summary>
		/// 遍历指定后缀文件列表，带 "."
		/// </summary>
		/// <param name="extension">文件后缀，为null则遍历所有文件</param>
		/// <param name="action"></param>
		/// <returns></returns>
		public override void GetFiles(string? extension, Action<string, Stream> action)
		{
			foreach ((string? fileName, FileInfo? fileInfo) in 
			         extension is null 
				         ? FModFiles 
				         : FModFiles.Where(item => item.Key.EndsWith(extension)))
			{
				if (fileInfo is null)
				{
					return;
				}
				using Stream fs = fileInfo.OpenRead();
				try
				{
					action?.Invoke(fileName, fs);
				}
				catch (Exception e)
				{
					Log.Error($"GetFile {fileName} Exception:{eS}");
				}
			}
		}
		public override bool GetFile(string filename, Action<Stream> action)
		{
			if (!FModFiles.TryGetValue(filename, out FileInfo? fileInfo) || string.IsNullOrEmpty(filename))
			{
				return false;
			}

			using Stream stream = fileInfo.OpenRead();
			try
			{
				action.Invoke(stream);
			}
			catch (Exception e)
			{
				Log.Error($"GetFile {filename} Error:{e}");
			}

			return true;
		}
		public override bool GetAssetsFile(string filename, Action<Stream> stream)
		{
			return GetFile("Assets/" + filename, stream);
		}
	}
}
