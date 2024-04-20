using Engine;
using Engine.Graphics;

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Game
{
	public class ModEntity
	{
		public ModInfo modInfo;
		public Texture2D Icon;
		public ZipArchive ModArchive;
		public Dictionary<string, ZipArchiveEntry> ModFiles = [];
		public List<Block> Blocks = [];
		public string ModFilePath;
		public bool IsDependencyChecked;
		public ModLoader Loader { get { return ModLoader_; } set { ModLoader_ = value; } }
		private ModLoader ModLoader_;

		public ModEntity() { }

		public ModEntity(ZipArchive zipArchive)
		{
			ModFilePath = ModsManager.ModsPath;
			ModArchive = zipArchive;
			InitResources();
		}
		public ModEntity(string FileName, ZipArchive zipArchive)
		{
			ModFilePath = FileName;
			ModArchive = zipArchive;
			InitResources();
		}

		public virtual void LoadIcon(Stream stream)
		{
			Icon = Texture2D.Load(stream);
			stream.Close();
		}
		/// <summary>
		/// 获取指定后缀文件列表，带.
		/// </summary>
		/// <param name="extension"></param>
		/// <param name="action">参数1文件名参数，2打开的文件流</param>
		public virtual void GetFiles(string extension, Action<string, Stream> action)
		{
			var files = new List<Stream>();
			//将每个zip里面的文件读进内存中
			foreach (ZipArchiveEntry zipArchiveEntry in ModArchive.ReadCentralDir())
			{
				if (Storage.GetExtension(zipArchiveEntry.FilenameInZip) == extension)
				{
					var stream = new MemoryStream();
					ModArchive.ExtractFile(zipArchiveEntry, stream);
					stream.Position = 0L;
					try
					{
						action.Invoke(zipArchiveEntry.FilenameInZip, stream);
					}
					catch (Exception e)
					{
						Log.Error(string.Format("获取文件[{0}]失败：{1}", zipArchiveEntry.FilenameInZip, e.Message));
					}
					finally
					{
						stream.Dispose();
					}
				}
			}
		}
		/// <summary>
		/// 获取指定文件
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="stream">参数1打开的文件流</param>
		/// <returns></returns>
		public virtual bool GetFile(string filename, Action<Stream> stream)
		{
			if (ModFiles.TryGetValue(filename, out ZipArchiveEntry entry))
			{
				using (MemoryStream ms = new())
				{
					ModArchive.ExtractFile(entry, ms);
					ms.Position = 0L;
					try
					{
						stream?.Invoke(ms);
					}
					catch (Exception e)
					{
						LoadingScreen.Error($"[{modInfo.Name}]获取文件[{filename}]失败：" + e.Message);
					}
				}
			}
			return false;
		}
		public virtual bool GetAssetsFile(string filename, Action<Stream> stream)
		{
			return GetFile("Assets/" + filename, stream);
		}
		/// <summary>
		/// 初始化语言包
		/// </summary>
		public virtual void LoadLauguage()
		{
			LoadingScreen.Info($"[{modInfo.Name}]加载Lang语言目录");
			GetAssetsFile($"Lang/{ModsManager.Configs["Language"]}.json", (stream) => { LanguageControl.loadJson(stream); });
		}
		/// <summary>
		/// Mod初始化
		/// </summary>
		public virtual void ModInitialize()
		{
			LoadingScreen.Info($"[{modInfo.Name}]初始化Mod方法");
			ModLoader_?.__ModInitialize();
		}
		/// <summary>
		/// 初始化Content资源
		/// </summary>
		public virtual void InitResources()
		{
			ModFiles.Clear();
			if (ModArchive == null) return;
			List<ZipArchiveEntry> entries = ModArchive.ReadCentralDir();
			foreach (ZipArchiveEntry zipArchiveEntry in entries)
			{
				if (zipArchiveEntry.FileSize > 0)
				{
					ModFiles.Add(zipArchiveEntry.FilenameInZip, zipArchiveEntry);
				}
			}
			GetFile("modinfo.json", (stream) =>
			{
				modInfo = ModsManager.DeserializeJson(ModsManager.StreamToString(stream));
			});
			if (modInfo == null) return;
			GetFile("icon.png", (stream) =>
			{
				LoadIcon(stream);
			});
			foreach (var c in ModFiles)
			{
				ZipArchiveEntry zipArchiveEntry = c.Value;
				string filename = zipArchiveEntry.FilenameInZip;
				if (!zipArchiveEntry.IsFilenameUtf8)
				{
					ModsManager.AddException(new Exception($"[{modInfo.Name}]中的[{zipArchiveEntry.FilenameInZip}]文件名称编码不是UTF-8，请进行修正"));
				}
				if (filename.StartsWith("Assets/"))
				{
					MemoryStream memoryStream = new();
					ContentInfo contentInfo = new(filename.Substring(7));
					ModArchive.ExtractFile(zipArchiveEntry, memoryStream);
					contentInfo.SetContentStream(memoryStream);
					ContentManager.Add(contentInfo);
				}
			}
			LoadingScreen.Info($"[{modInfo.Name}]加载资源文件数:{ModFiles.Count}");
		}
		/// <summary>
		/// 初始化BlocksData资源
		/// </summary>
		public virtual void LoadBlocksData()
		{
			LoadingScreen.Info($"[{modInfo.Name}]加载.csv方块数据文件");
			GetFiles(".csv", (filename, stream) =>
			{
				BlocksManager.LoadBlocksData(ModsManager.StreamToString(stream));
			});
		}
		/// <summary>
		/// 初始化Database数据
		/// </summary>
		/// <param name="xElement"></param>
		public virtual void LoadXdb(ref XElement xElement)
		{
			XElement element = xElement;
			LoadingScreen.Info($"[{modInfo.Name}]加载.xdb数据库文件");
			GetFiles(".xdb", (filename, stream) =>
			{
				ModsManager.CombineDataBase(element, stream);
			});
			Loader?.OnXdbLoad(xElement);
		}
		/// <summary>
		/// 初始化Clothing数据
		/// </summary>
		/// <param name="block"></param>
		/// <param name="xElement"></param>
		public virtual void LoadClo(ClothingBlock block, ref XElement xElement)
		{
			XElement element = xElement;
			LoadingScreen.Info($"[{modInfo.Name}]加载.clo衣物数据文件");
			GetFiles(".clo", (filename, stream) =>
			{
				ModsManager.CombineClo(element, stream);
			});
		}
		/// <summary>
		/// 初始化CraftingRecipe
		/// </summary>
		/// <param name="xElement"></param>
		public virtual void LoadCr(ref XElement xElement)
		{
			XElement element = xElement;
			LoadingScreen.Info($"[{modInfo.Name}]加载.cr合成谱文件");
			GetFiles(".cr", (filename, stream) => { ModsManager.CombineCr(element, stream); });
		}
		
		/// <summary>
		/// 加载mod程序集
		/// </summary>
		public virtual Assembly[] GetAssemblies()
		{
			LoadingScreen.Info($"[{modInfo.Name}]加载 .NET .dll 程序集文件");
			
			var assemblies = new List<Assembly>();
			
			GetFiles(".dll", (filename, stream) =>
			{
			    if(!filename.StartsWith("Assets/"))
				    assemblies.Add(Assembly.Load(ModsManager.StreamToBytes(stream)));
			});//获取mod文件内的dll文件（不包括Assets文件夹内的dll）
			
			return [.. assemblies];
		}
		public virtual void HandleAssembly(Assembly assembly)
		{
			var blockTypes = new List<Type>();
			Type[] types = assembly.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				Type type = types[i];
				if (type.IsSubclassOf(typeof(ModLoader)) && !type.IsAbstract)
				{
					var modLoader = Activator.CreateInstance(types[i]) as ModLoader;
					modLoader.Entity = this;
					Loader = modLoader;
					modLoader.__ModInitialize();
					ModsManager.ModLoaders.Add(modLoader);
				}
				if (type.IsSubclassOf(typeof(IContentReader.IContentReader)) && !type.IsAbstract)
				{
					IContentReader.IContentReader reader = Activator.CreateInstance(type) as IContentReader.IContentReader;
					if (!ContentManager.ReaderList.ContainsKey(reader.Type)) ContentManager.ReaderList.Add(reader.Type, reader);
				}
				if (type.IsSubclassOf(typeof(Block)) && !type.IsAbstract)
				{
					blockTypes.Add(type);
				}
			}
			for (int i = 0; i < blockTypes.Count; i++)
			{
				Type type = blockTypes[i];
				FieldInfo fieldInfo = type.GetRuntimeFields().FirstOrDefault(p => p.Name == "Index" && p.IsPublic && p.IsStatic);
				if (fieldInfo == null || fieldInfo.FieldType != typeof(int))
				{
					LoadingScreen.Warning($"Block type \"{type.FullName}\" does not have static field Index of type int.");
				}
				else
				{
					int staticIndex = (int)fieldInfo.GetValue(null);
					var block = (Block)Activator.CreateInstance(type.GetTypeInfo().AsType());
					block.BlockIndex = staticIndex;
					Blocks.Add(block);
				}
			}
		}
		public virtual void LoadJs()
		{
			LoadingScreen.Info($"[{modInfo.Name}]加载.js脚本文件");
			GetFiles(".js", (filename, stream) =>
			{
				JsInterface.Execute(new StreamReader(stream).ReadToEnd());
			});
		}
		/// <summary>
		/// 检查依赖项
		/// </summary>
		public virtual void CheckDependencies(List<ModEntity> modEntities)
		{
			LoadingScreen.Info($"[{modInfo.Name}]检查依赖项");
			for (int j = 0; j < modInfo.Dependencies.Count; j++)
			{
				int k = j;
				string name = modInfo.Dependencies[k];
				string dn = "";
				var dnversion = new Version();
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
				ModEntity entity = ModsManager.ModListAll.Find(px => px.modInfo.PackageName == dn && new Version(px.modInfo.Version) == dnversion);
				if (entity != null)
				{
					//依赖项最先被加载
					if (!entity.IsDependencyChecked) entity.CheckDependencies(modEntities);
				}
				else
				{
					var e = new Exception($"[{modInfo.Name}]缺少依赖项{name}");
					throw e;
				}
			}
			IsDependencyChecked = true;
			modEntities.Add(this);
		}
		/// <summary>
		/// 保存设置
		/// </summary>
		/// <param name="xElement"></param>
		public virtual void SaveSettings(XElement xElement)
		{
			Loader?.SaveSettings(xElement);
		}
		/// <summary>
		/// 加载设置
		/// </summary>
		/// <param name="xElement"></param>
		public virtual void LoadSettings(XElement xElement)
		{
			Loader?.LoadSettings(xElement);
		}
		/// <summary>
		/// BlocksManager初始化完毕
		/// </summary>
		/// <param name="categories"></param>
		public virtual void OnBlocksInitalized()
		{
			Loader?.BlocksInitalized();
		}
		//释放资源
		public virtual void Dispose()
		{
			try { Loader?.ModDispose(); } catch { }
			ModArchive?.ZipFileStream.Close();
		}
		public override bool Equals(object obj)
		{
			if (obj is ModEntity px)
			{
				return px.modInfo.PackageName == modInfo.PackageName && new Version(px.modInfo.Version) == new Version(modInfo.Version);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return modInfo.GetHashCode();
		}
	}
}

