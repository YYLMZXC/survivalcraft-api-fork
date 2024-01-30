using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;

using Engine;
using Game.IContentReader;

namespace Game
{
	public sealed class SurvivalCraftModEntity : ModEntity
	{
		public static SurvivalCraftModEntity Instance = null!;
		public SurvivalCraftModEntity() : base(null!)
		{
			if (Instance != null)
			{
				throw new InvalidOperationException($"{nameof(SurvivalCraftModEntity)} 被重复实例化。");
			}

			Instance = this;
			
			var readers = new List<IContentReader.IContentReader>();
			readers.AddRange(new IContentReader.IContentReader[]
			{
				new AssemblyReader(),
				new BitmapFontReader(),
				new DaeModelReader(),
				new ImageReader(),
				new JsonArrayReader(),
				new JsonObjectReader(),
				new IContentReader.JsonModelReader(),
				new MtllibStructReader(),
				new IContentReader.ObjModelReader(),
				new ShaderReader(),
				new SoundBufferReader(),
				new StreamingSourceReader(),
				new IContentReader.StringReader(),
				new SubtextureReader(),
				new Texture2DReader(),
				new XmlReader()
			});
			for (int i = 0; i < readers.Count; i++)
			{
				ContentManager.ReaderList.Add(readers[i].Type, readers[i]);
			}

			Stream stream = Storage.OpenFile("app:/Content.zip", OpenFileMode.Read);
			MemoryStream memoryStream = new();
			stream.CopyTo(memoryStream);
			stream.Close();
			memoryStream.Position = 0L;
			ModArchive = ZipArchive.Open(memoryStream, true);
			InitializeResources();
			LabelWidget.BitmapFont = ContentManager.Get<Engine.Media.BitmapFont>("Fonts/Pericles");
			LoadingScreen.Info("加载资源:" + ModInfo?.Name);
		}
		public override void LoadBlocksData()
		{
			LoadingScreen.Info("加载方块数据:" + ModInfo?.Name);
			BlocksManager.LoadBlocksData(ContentManager.Get<string>("BlocksData"));
			ContentManager.Dispose("BlocksData");
		}
		public override Assembly[] GetAssemblies()
		{
			return [typeof(BlocksManager).Assembly];
		}
		public override void HandleAssembly(Assembly assembly)
		{
			Type[] types = assembly.GetTypes();
			foreach (var type in types)
			{
				if (type.IsSubclassOf(typeof(IModLoader)) && !type.IsAbstract)
				{
					if(Activator.CreateInstance(type) is not IModLoader modLoader) continue;
					modLoader.ModEntity = this;
					modLoader._OnLoaderInitialize();
					ModsManager.ModLoaders.Add(modLoader);
					continue;
				}

				if (!type.IsSubclassOf(typeof(Block)) || type.IsAbstract)
				{
					continue;
				}

				var fieldInfo = type.GetRuntimeFields().FirstOrDefault(p => p is { Name: "Index", IsPublic: true, IsStatic: true });
				if (fieldInfo == null || fieldInfo.FieldType != typeof(int))
				{
					ModsManager.AddException(new InvalidOperationException(
						$"Block type \"{type.FullName}\" does not have static field Index of type int."));
					continue;
				}

				var index = (int)fieldInfo.GetValue(null)!;
				var block = (Block?)Activator.CreateInstance(type.GetTypeInfo().AsType());
				if (block is null)
				{
					Log.Error($"Cannot construct block {type.FullName}");
				}
				else
				{
					block.BlockIndex = index;
					Blocks.Add(block);
				}
			}
		}
		public override void LoadXdb(ref XElement xElement)
		{
			LoadingScreen.Info("加载数据库:" + ModInfo?.Name);
			xElement = ContentManager.Get<XElement>("Database");
			ContentManager.Dispose("Database");
		}
		public override void LoadCr(ref XElement xElement)
		{
			LoadingScreen.Info("加载合成谱:" + ModInfo?.Name);
			xElement = ContentManager.Get<XElement>("CraftingRecipes");
			ContentManager.Dispose("CraftingRecipes");
		}
		public override void LoadClo(ClothingBlock block, ref XElement xElement)
		{
			LoadingScreen.Info("加载衣物数据:" + ModInfo?.Name);
			xElement = ContentManager.Get<XElement>("Clothes");
			ContentManager.Dispose("Clothes");
		}
		public override void SaveSettings(XElement xElement)
		{


		}
		public override void LoadSettings(XElement xElement)
		{
		}
		public override void OnBlocksInitialized()
		{
			BlocksManager.AddCategory("Terrain");
			BlocksManager.AddCategory("Plants");
			BlocksManager.AddCategory("Construction");
			BlocksManager.AddCategory("Items");
			BlocksManager.AddCategory("Tools");
			BlocksManager.AddCategory("Weapons");
			BlocksManager.AddCategory("Clothes");
			BlocksManager.AddCategory("Electrics");
			BlocksManager.AddCategory("Food");
			BlocksManager.AddCategory("Spawner Eggs");
			BlocksManager.AddCategory("Painted");
			BlocksManager.AddCategory("Dyed");
			BlocksManager.AddCategory("Fireworks");
		}
	}
}
