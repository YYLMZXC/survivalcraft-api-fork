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
		public ModInfo ModInfo { get; protected set; }
		public Texture2D Icon;
		public ZipArchive? ModArchive;
		public Dictionary<string, ZipArchiveEntry> ModFiles = [];
		public List<Block> Blocks = [];
		public string ModFilePath;
		public List<IModLoader> Loaders { get; } = [];

		public ModEntity(ZipArchive? zipArchive): this(ModsManager.ModsPath, zipArchive)
		{
		}
		public ModEntity(string fileName, ZipArchive? zipArchive)
		{
			ModFilePath = fileName;
			ModArchive = zipArchive;
			InitializeResources();
		}

		public void LoadIcon(Stream stream)
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
			//将每个zip里面的文件读进内存中
			foreach (ZipArchiveEntry zipArchiveEntry in ModArchive.ReadCentralDir())
			{
				if (string.Equals(Storage.GetExtension(zipArchiveEntry.FilenameInZip) , extension, StringComparison.InvariantCultureIgnoreCase)) continue;
				var stream = new MemoryStream();
				ModArchive.ExtractFile(zipArchiveEntry, stream);
				stream.Position = 0L;
				try
				{
					action.Invoke(zipArchiveEntry.FilenameInZip, stream);
				}
				catch (Exception e)
				{
					Log.Error($"获取文件[{zipArchiveEntry.FilenameInZip}]失败：{e.Message}");
				}
				finally
				{
					stream.Dispose();
				}
			}
		}

		/// <summary>
		/// 获取指定文件
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public virtual bool GetFile(string filename, Action<Stream> action)
		{
			if (!ModFiles.TryGetValue(filename, out ZipArchiveEntry entry)) return false;
			using MemoryStream stream = new();
			
			ModArchive.ExtractFile(entry, stream);
			stream.Position = 0L;
			try
			{
				action?.Invoke(stream);
			}
			catch (Exception e)
			{
				LoadingScreen.Error($"[{ModInfo.Name}]获取文件[{filename}]失败：" + e.Message);
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
		public virtual void LoadLanguage()
		{
			LoadingScreen.Info($"[{ModInfo.Name}]加载Lang语言目录");
			GetAssetsFile($"Lang/{ModsManager.Configs["Language"]}.json", (stream) => { LanguageControl.loadJson(stream); });
		}
		/// <summary>
		/// Mod初始化
		/// </summary>
		public void ModInitialize()
		{
			LoadingScreen.Info($"[{ModInfo.Name}]初始化Mod方法");
			Loaders.ForEach(loader => loader?._OnLoaderInitialize());
		}
		/// <summary>
		/// 初始化Content资源
		/// </summary>
		protected virtual void InitializeResources()
		{
			ModFiles.Clear();
			if (ModArchive == null) return;
			List<ZipArchiveEntry> entries = ModArchive.ReadCentralDir();
			foreach (ZipArchiveEntry zipArchiveEntry in entries.Where(zipArchiveEntry => zipArchiveEntry.FileSize > 0))
			{
				ModFiles.Add(zipArchiveEntry.FilenameInZip, zipArchiveEntry);
			}
			GetFile("modinfo.json", (stream) =>
			{
				ModInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(stream));
			});
			if (ModInfo == null) return;
			GetFile("icon.png", LoadIcon);
			foreach (var c in ModFiles)
			{
				ZipArchiveEntry zipArchiveEntry = c.Value;
				string filename = zipArchiveEntry.FilenameInZip;
				if (!zipArchiveEntry.IsFilenameUtf8)
				{
					var gbk = Encoding.GetEncoding("GBK");
					var utf = Encoding.UTF8;
					var p = utf.GetString(Encoding.Convert(gbk, utf, gbk.GetBytes(zipArchiveEntry.FilenameInZip)));
					ModsManager.AddException(new Exception($"[{ModInfo.Name}]文件名[{zipArchiveEntry.FilenameInZip}]编码不是UTF-8，请进行修正，GBK编码为[{p}]"));
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
			LoadingScreen.Info($"[{ModInfo.Name}]加载资源文件数:{ModFiles.Count}");
		}
		/// <summary>
		/// 初始化BlocksData资源
		/// </summary>
		public virtual void LoadBlocksData()
		{
			LoadingScreen.Info($"[{ModInfo.Name}]加载.csv方块数据文件");
			GetFiles(".csv", (_, stream) =>
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
			LoadingScreen.Info($"[{ModInfo.Name}]加载.xdb数据库文件");
			GetFiles(".xdb", (_, stream) =>
			{
				ModsManager.CombineDataBase(element, stream);
			});
			ModInterfacesManager.InvokeHooks("OnXdbLoad",
				(SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
				{
					modInterface.OnXdbLoad(element);
					isContinueRequired = true;
				}, this);
		}
		/// <summary>
		/// 初始化Clothing数据
		/// </summary>
		/// <param name="block"></param>
		/// <param name="xElement"></param>
		public virtual void LoadClo(ClothingBlock block, ref XElement xElement)
		{
			XElement element = xElement;
			LoadingScreen.Info($"[{ModInfo.Name}]加载.clo衣物数据文件");
			GetFiles(".clo", (_, stream) =>
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
			LoadingScreen.Info($"[{ModInfo.Name}]加载.cr合成谱文件");
			GetFiles(".cr", (_, stream) => { ModsManager.CombineCr(element, stream); });
		}
		
		/// <summary>
		/// 加载mod程序集
		/// </summary>
		public virtual Assembly[] GetAssemblies()
		{
			LoadingScreen.Info($"[{ModInfo.Name}]加载 .NET .dll 程序集文件");
			
			var assemblies = new List<Assembly>();
			
			GetFiles(".dll", (filename, stream) =>
			{
				if (!filename.StartsWith("Assets/"))
				{
					assemblies.Add(Assembly.Load(ModsManager.StreamToBytes(stream)));
				}
			});//获取mod文件内的dll文件（不包括Assets文件夹内的dll）
			
			return [.. assemblies];
		}
		public virtual void HandleAssembly(Assembly assembly)
		{
			ModsManager.Assemblies.Add(assembly.GetName().FullName, assembly);
			var blockTypes = new List<Type>();
			Type[] types = assembly.GetTypes();
			Type typeOfModLoader = typeof(IModLoader);
			
			foreach (Type type in types)
			{
				if (type.GetInterfaces().Any(@interface => @interface == typeOfModLoader) && !type.IsAbstract && type.IsClass)
				{
					if(Activator.CreateInstance(type) is not IModLoader modLoader) continue;
					modLoader.ModEntity = this;
					modLoader._OnLoaderInitialize();
					ModsManager.ModLoaders.Add(modLoader);
					continue;
				}
				if (type.IsSubclassOf(typeof(Block)) && !type.IsAbstract)
				{
					blockTypes.Add(type);
				}
			}
			foreach (Type type in blockTypes)
			{
				FieldInfo? fieldInfo = type.GetRuntimeFields()
					.FirstOrDefault(p => p is { Name: "Index", IsPublic: true, IsStatic: true });
				if (fieldInfo == null || fieldInfo.FieldType != typeof(int))
				{
					LoadingScreen.Warning($"Block type \"{type.FullName}\" does not have static field Index of type int.");
				}
				else
				{
					var staticIndex = (int)fieldInfo.GetValue(null)!;
					var block = (Block?)Activator.CreateInstance(type.GetTypeInfo().AsType());
					if (block is null)
					{
						Log.Error($"无法实例化类型 {type.FullName}");
						continue;
					}
					block.BlockIndex = staticIndex;
					Blocks.Add(block);
				}
			}
		}
		public void LoadJs()
		{
			LoadingScreen.Info($"[{ModInfo.Name}]加载.js脚本文件");
			GetFiles(".js", (_, stream) =>
			{
				JsInterface.Execute(new StreamReader(stream).ReadToEnd());
			});
		}
		/// <summary>
		/// 检查依赖项
		/// </summary>
		public void CheckDependencies(List<ModEntity> _)
		{
			//此方法可能是不需要的
			/*
			LoadingScreen.Info($"[{modInfo.Name}]检查依赖项");
			for (int j = 0; j < modInfo.Dependencies.Count; j++)
			{
				int k = j;
				string name = modInfo.Dependencies[k];
				string dependencyName = null;
				var dependencyVersion = new Version();
				if (name.Contains(":"))
				{
					string[] tmp1 = name.Split([':']);
					if (tmp1.Length == 2)
					{
						dependencyName = tmp1[0];
						dependencyVersion = new Version(tmp1[1]);
					}
				}
				else
				{
					dependencyName = name;
				}
				ModEntity entity = ModsManager.ModListAll.Find(px => px.modInfo.PackageName == dependencyName && new Version(px.modInfo.Version) == dependencyVersion);
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
			*/
		}
		/// <summary>
		/// 保存设置
		/// </summary>
		/// <param name="xElement"></param>
		public virtual void SaveSettings(XElement xElement)
		{
			ModInterfacesManager.InvokeHooks("SaveSettings", (SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
			{
				modInterface.SaveSettings(xElement);
				isContinueRequired = true;
			}, this);
		}
		/// <summary>
		/// 加载设置
		/// </summary>
		/// <param name="xElement"></param>
		public virtual void LoadSettings(XElement xElement)
		{
			ModInterfacesManager.InvokeHooks("LoadSettings", (SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
			{
				modInterface.LoadSettings(xElement);
				isContinueRequired = true;
			}, this);
		}
		/// <summary>
		/// BlocksManager初始化完毕
		/// </summary>
		public virtual void OnBlocksInitialized()
		{
			ModInterfacesManager.InvokeHooks("BlocksInitialized", (SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
			{
				modInterface.BlocksInitialized();
				isContinueRequired = true;
			}, this);
		}
		//释放资源
		public virtual void Dispose()
		{
			ModInterfacesManager.InvokeHooks("ModDispose", (SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
			{
				modInterface.ModDispose();
				isContinueRequired = true;
			}, this);
			ModArchive?.ZipFileStream.Close();
		}
		public override bool Equals(object? obj)
		{
			if (obj is ModEntity px)
			{
				return px.ModInfo.PackageName == ModInfo.PackageName && new Version(px.ModInfo.Version) == new Version(ModInfo.Version);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ModInfo.GetHashCode();
		}
	}
}

