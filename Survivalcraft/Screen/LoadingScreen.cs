using Engine;
using Engine.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace Game
{
	public class LoadingScreen : Screen
	{
		public enum LogType
		{
			Info,
			Warning,
			Error,
			Advice
		}
		private class LogItem
		{
			public LogType LogType;
			public string Message;
			public LogItem(LogType type, string log) { LogType = type; Message = log; }
		}
		private List<Action> LoadingActoins = [];
		private List<Action> ModLoadingActoins = [];
		private CanvasWidget Canvas = new();
		private RectangleWidget Background = new() { FillColor = SettingsManager.DisplayLog ? Color.Black : Color.White, OutlineThickness = 0f, DepthWriteEnabled = true };
		private static ListPanelWidget LogList = new() { Direction = LayoutDirection.Vertical, PlayClickSound = false };
		static LoadingScreen()
		{
			LogList.ItemWidgetFactory = (obj) =>
			{
				if(obj is LogItem logItem)
				{
                    CanvasWidget canvasWidget = new() { Size = new Vector2(Display.Viewport.Width, 40), Margin = new Vector2(0, 2), HorizontalAlignment = WidgetAlignment.Near };
                    FontTextWidget fontTextWidget = new() { FontScale = 0.6f, Text = logItem.Message, Color = GetColor(logItem.LogType), VerticalAlignment = WidgetAlignment.Center, HorizontalAlignment = WidgetAlignment.Near };
                    canvasWidget.Children.Add(fontTextWidget);
                    canvasWidget.IsVisible = SettingsManager.DisplayLog;
                    LogList.IsEnabled = SettingsManager.DisplayLog;
                    return canvasWidget;
                }
				return null;
			};
			LogList.ItemSize = 30;
		}
		public static Color GetColor(LogType type)
		{
			switch (type)
			{
				case LogType.Advice: return Color.Cyan;
				case LogType.Error: return Color.Red;
				case LogType.Warning: return Color.Yellow;
				case LogType.Info: return Color.White;
				default: return Color.White;
			}
		}
		public LoadingScreen()
		{
			Canvas.Size = new Vector2(float.PositiveInfinity);
			Canvas.AddChildren(Background);
			Canvas.AddChildren(LogList);
			AddChildren(Canvas);
			Info("Initializing Mods Manager. Api Version: " + ModsManager.ApiVersionString);
		}
		public void ContentLoaded()
		{
			if (SettingsManager.DisplayLog) return;
			ClearChildren();
			RectangleWidget rectangle1 = new() { FillColor = Color.White, OutlineColor = Color.Transparent, Size = new Vector2(256f), VerticalAlignment = WidgetAlignment.Center, HorizontalAlignment = WidgetAlignment.Center };
			rectangle1.Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/CandyRufusLogo");
			RectangleWidget rectangle2 = new() { FillColor = Color.White, OutlineColor = Color.Transparent, Size = new Vector2(80), VerticalAlignment = WidgetAlignment.Far, HorizontalAlignment = WidgetAlignment.Far, Margin = new Vector2(10f) };
			rectangle2.Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/EngineLogo");
			BusyBarWidget busyBar = new() { VerticalAlignment = WidgetAlignment.Far, HorizontalAlignment = WidgetAlignment.Center, Margin = new Vector2(0, 40) };
			Canvas.AddChildren(Background);
			Canvas.AddChildren(rectangle1);
			Canvas.AddChildren(rectangle2);
			Canvas.AddChildren(busyBar);
			Canvas.AddChildren(LogList);
			AddChildren(Canvas);
		}

		public static void Error(string mesg)
		{
			Add(LogType.Error, "[Error]" + mesg);
		}
		public static void Info(string mesg)
		{
			Add(LogType.Info, "[Info]" + mesg);
		}
		public static void Warning(string mesg)
		{
			Add(LogType.Warning, "[Warning]" + mesg);
		}
		public static void Advice(string mesg)
		{
			Add(LogType.Advice, "[Advice]" + mesg);
		}
		
		public static void Add(LogType type, string mesg)
		{
			Dispatcher.Dispatch(delegate
			{
				LogItem item = new(type, mesg);
				LogList.AddItem(item);
				switch (type)
				{
					case LogType.Info:
					case LogType.Advice: Log.Information(mesg); break;
					case LogType.Error: Log.Error(mesg); break;
					case LogType.Warning: Log.Warning(mesg); break;
					default: break;
				}
				LogList.ScrollToItem(item);
			});
		}
		private void InitActions()
		{
		    var isLoadSucceed = true;
			Exception exception = null;
			AddLoadAction(delegate
			{//将所有的有效的scmod读取为ModEntity，并自动添加SurvivalCraftModEntity
				ContentManager.Initialize();
				ModsManager.Initialize();
			});
			AddLoadAction(ContentLoaded);

			AddLoadAction(delegate
			{//检查所有Mod依赖项 
			 //根据加载顺序排序后的结果
				ModsManager.ModList.Clear();
				foreach (var item in ModsManager.ModListAll)
				{
					if (item.IsDependencyChecked) continue;
					item.CheckDependencies(ModsManager.ModList);
				}
				foreach (var item in ModsManager.ModListAll) item.IsDependencyChecked = false;
			});
			AddLoadAction(delegate
			{ //初始化所有ModEntity的语言包
			  //>>>初始化语言列表
				ReadOnlyList<ContentInfo> axa = ContentManager.List("Lang");
				LanguageControl.LanguageTypes.Clear();
				foreach (ContentInfo contentInfo in axa)
				{
					string px = System.IO.Path.GetFileNameWithoutExtension(contentInfo.Filename);
					if (!LanguageControl.LanguageTypes.Contains(px)) LanguageControl.LanguageTypes.Add(px);
				}
				//<<<结束
				if (ModsManager.Configs.ContainsKey("Language") && LanguageControl.LanguageTypes.Contains(ModsManager.Configs["Language"]))
				{
					LanguageControl.Initialize(ModsManager.Configs["Language"]);
				}
				else
				{
					if (LanguageControl.LanguageTypes.Contains(Program.SystemLanguage))
					{
						LanguageControl.Initialize(Program.SystemLanguage);
					}
					else
					{
						// 如果不支持系統語言，英語是最佳選擇
						LanguageControl.Initialize("en-US");
					}
				}
				ModsManager.ModListAllDo((modEntity) => { modEntity.LoadLauguage(); });
			});
			AddLoadAction(() =>
			{
			    Dictionary<string, Assembly[]> assemblies = [];
				ModsManager.ModListAllDo((modEntity) =>
				{
					Log.Information("Get assemblies " + modEntity.modInfo.PackageName);
				    assemblies[modEntity.modInfo.PackageName] = modEntity.GetAssemblies();
				    foreach (var assembly in assemblies[modEntity.modInfo.PackageName])
				    {
					    ModsManager.Dlls.Add(assembly.GetName().FullName, assembly);
				    }
				});
				//加载 mod 程序集(.dll)文件
				//但不进行处理操作(如添加block等)
				ModsManager.ModListAllDo((modEntity) =>
				{
					if (!isLoadSucceed) return;
				    foreach(var asm in assemblies[modEntity.modInfo.PackageName])
				    {
					    Log.Information("handle assembly " + modEntity.modInfo.PackageName + " " + asm.FullName);
					    try
					    {
						    modEntity.HandleAssembly(asm);
					    }
					    catch(Exception e)
					    {
						    exception = e;
						    string separator = new('-', 10); //生成10个 '-' 连一起的字符串
						    Log.Error($"{separator}Handle assembly failed{separator}");
						    Log.Error("Loaded assembly:\n" + string.Join("\n",
							    AppDomain.CurrentDomain.GetAssemblies()
								    .Select(x => x.FullName ?? x.GetName().FullName)));
						    Log.Error(separator);
						    Log.Error("Error assembly: " + asm.FullName);
						    Log.Error("Dependencies:\n" + string.Join("\n",
							    asm.GetReferencedAssemblies().Select(x => x.FullName)));
						    Log.Error(separator);
						    Log.Error(e);
						    isLoadSucceed = false;
						    break;
					    }
				    }
				});
				if (!isLoadSucceed)
				{
					ModsManager.ModList.RemoveAll(entity =>
						entity is not SurvivalCraftModEntity && entity is not FastDebugModEntity);
					LoadingActoins.RemoveRange(1, LoadingActoins.Count - 1);
					ModLoadingActoins.Clear();
					ScreensManager.SwitchScreen(new LoadingFailedScreen(
						title: "加载失败",
						details: ["缺失模组依赖", "异常信息：", ..exception!.ToString().Split('\n')],
						solveMethods:
						[
							"检查模组是否缺失，并添加所缺失的模组", "查看模组版本与要求的模组版本是否一致",
							$"若以上方式都无法解决，请联系管理员，并发送 {Storage.GetSystemPath(ModsManager.LogPath)} 中的 Game.log "
						]
					));

				}
				//处理程序集
			});
			AddLoadAction(delegate
			{ //读取所有的ModEntity的JavaScript
				ModsManager.ModListAllDo((modEntity) => { modEntity.LoadJs(); });
				JsInterface.RegisterEvent();
			});
			AddLoadAction(delegate
			{
				Info("执行初始化任务");
				List<Action> actions = [];
				ModsManager.HookAction("OnLoadingStart", (loader) =>
				{
					loader.OnLoadingStart(actions);
					return false;
				});
				foreach (var ac in actions)
				{
					ModLoadingActoins.Add(ac);
				}
			});
			AddLoadAction(delegate
			{//初始化TextureAtlas
				Info("初始化纹理地图");
				TextureAtlasManager.Initialize();
			});
			AddLoadAction(delegate
			{ //初始化Database
				try
				{
					DatabaseManager.Initialize();
					ModsManager.ModListAllDo((modEntity) => { modEntity.LoadXdb(ref DatabaseManager.DatabaseNode); });
				}
				catch (Exception e)
				{
					Warning(e.Message);
				}
			});
			AddLoadAction(delegate
			{
				Info("读取数据库");
				try
				{
					DatabaseManager.LoadDataBaseFromXml(DatabaseManager.DatabaseNode);
				}
				catch (Exception e)
				{
					Warning(e.Message);
				}
			});
			AddLoadAction(delegate
			{ //初始化方块管理器
				Info("初始化方块管理器");
				BlocksManager.Initialize();
			});
			AddLoadAction(delegate
			{ //初始化合成谱
				CraftingRecipesManager.Initialize();
			});
			InitScreens();
			AddLoadAction(delegate
			{
				BlocksTexturesManager.Initialize();
				CharacterSkinsManager.Initialize();
				CommunityContentManager.Initialize();
				ExternalContentManager.Initialize();
				FurniturePacksManager.Initialize();
				LightingManager.Initialize();
				MotdManager.Initialize();
				VersionsManager.Initialize();
				WorldsManager.Initialize();
				ScreensManager.SwitchScreen("MainMenu");
			});
			AddLoadAction(delegate
			{
				Info("初始化Mod设置参数");
				if (Storage.FileExists(ModsManager.ModsSetPath))
				{
					using System.IO.Stream stream = Storage.OpenFile(ModsManager.ModsSetPath, OpenFileMode.Read);
					try
					{
						XElement element = XElement.Load(stream);
						ModsManager.LoadModSettings(element);
					}
					catch (Exception e)
					{
						Warning(e.Message);
					}
				}
			});
			AddLoadAction(delegate
			{
				ModsManager.ModListAllDo((modEntity) => { Info("等待剩下的任务完成:" + modEntity.modInfo?.PackageName); modEntity.Loader?.OnLoadingFinished(ModLoadingActoins); });
			});
			AddLoadAction(delegate
			{
				ScreensManager.SwitchScreen("MainMenu");
			});
		}
		private void InitScreens()
		{

			AddLoadAction(delegate
			{
				AddScreen("Nag", new NagScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("MainMenu", new MainMenuScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("Recipaedia", new RecipaediaScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("RecipaediaRecipes", new RecipaediaRecipesScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("RecipaediaDescription", new RecipaediaDescriptionScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("Bestiary", new BestiaryScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("BestiaryDescription", new BestiaryDescriptionScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("Help", new HelpScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("HelpTopic", new HelpTopicScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("Settings", new SettingsScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("SettingsPerformance", new SettingsPerformanceScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("SettingsGraphics", new SettingsGraphicsScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("SettingsUi", new SettingsUiScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("SettingsCompatibility", new SettingsCompatibilityScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("SettingsAudio", new SettingsAudioScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("SettingsControls", new SettingsControlsScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("Play", new PlayScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("NewWorld", new NewWorldScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("ModifyWorld", new ModifyWorldScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("WorldOptions", new WorldOptionsScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("GameLoading", new GameLoadingScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("Game", new GameScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("TrialEnded", new TrialEndedScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("ExternalContent", new ExternalContentScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("CommunityContent", new CommunityContentScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("Content", new ContentScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("ManageContent", new ManageContentScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("ModsManageContent", new ModsManageContentScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("ManageUser", new ManageUserScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("Players", new PlayersScreen());
			});
			AddLoadAction(delegate
			{
				AddScreen("Player", new PlayerScreen());
			});
		}
		public void AddScreen(string name, Screen screen)
		{
			ScreensManager.AddScreen(name, screen);
		}
		private void AddLoadAction(Action action)
		{
			LoadingActoins.Add(action);
		}
		public override void Leave()
		{
			LogList.ClearItems();
			Window.PresentationInterval = SettingsManager.PresentationInterval;
			ContentManager.Dispose("Textures/Gui/CandyRufusLogo");
			ContentManager.Dispose("Textures/Gui/EngineLogo");
		}
		public override void Enter(object[] parameters)
		{
			Window.PresentationInterval = 0;
			var remove = new List<string>();
			foreach (var screen in ScreensManager.m_screens)
			{
				if (screen.Value == this) continue;
				else remove.Add(screen.Key);
			}
			foreach (var screen in remove)
			{
				ScreensManager.m_screens.Remove(screen);
			}
			InitActions();
			base.Enter(parameters);
		}
		public override void Update()
		{
			if (Input.Back || Input.Cancel)
				DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Warning, "Quit?", LanguageControl.Ok,
					LanguageControl.No, (vt) =>
					{
						if (vt == MessageDialogButton.Button1) Environment.Exit(0);
						else DialogsManager.HideAllDialogs();
					}));
			if (ModsManager.GetAllowContinue() == false) return;
			if (ModLoadingActoins.Count > 0)
			{
				try
				{
					ModLoadingActoins[0].Invoke();
				}
				catch (Exception e)
				{
					Error(e.Message);
				}
				finally
				{
					ModLoadingActoins.RemoveAt(0);
				}
			}
			else
			{
				if (LoadingActoins.Count > 0)
				{
					try
					{
						LoadingActoins[0].Invoke();
					}
					catch (Exception e)
					{
						Error(e.Message);
					}
					finally
					{
						LoadingActoins.RemoveAt(0);
					}
				}
			}
		}
	}
}
