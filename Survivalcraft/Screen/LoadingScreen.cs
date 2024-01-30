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

            public LogItem(LogType type, string log)
            {
                LogType = type;
                Message = log;
            }
        }

        private List<Action> m_loadingActions = [];
        private List<Action> m_modLoadingActions = [];
        private CanvasWidget m_canvas = new();

        private RectangleWidget Background = new()
        {
            FillColor = SettingsManager.DisplayLog ? Color.Black : Color.White, OutlineThickness = 0f,
            DepthWriteEnabled = true
        };

        private static ListPanelWidget m_logList = new() { Direction = LayoutDirection.Vertical, PlayClickSound = false };

        static LoadingScreen()
        {
            m_logList.ItemWidgetFactory = (obj) =>
            {
                LogItem logItem = obj as LogItem;
                CanvasWidget canvasWidget = new()
                {
                    Size = new Vector2(Display.Viewport.Width, 40), Margin = new Vector2(0, 2),
                    HorizontalAlignment = WidgetAlignment.Near
                };
                FontTextWidget fontTextWidget = new()
                {
                    FontScale = 0.6f, Text = logItem.Message, Color = GetColor(logItem.LogType),
                    VerticalAlignment = WidgetAlignment.Center, HorizontalAlignment = WidgetAlignment.Near
                };
                canvasWidget.Children.Add(fontTextWidget);
                canvasWidget.IsVisible = SettingsManager.DisplayLog;
                m_logList.IsEnabled = SettingsManager.DisplayLog;
                return canvasWidget;
            };
            m_logList.ItemSize = 30;
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
            m_canvas.Size = new Vector2(float.PositiveInfinity);
            m_canvas.AddChildren(Background);
            m_canvas.AddChildren(m_logList);
            AddChildren(m_canvas);
            Info("Initializing Mods Manager. Api Version: " + ModsManager.ApiCurrentVersionString);
        }

        public void ContentLoaded()
        {
            if (SettingsManager.DisplayLog) return;
            ClearChildren();
            RectangleWidget rectangle1 = new()
            {
                FillColor = Color.White, OutlineColor = Color.Transparent, Size = new Vector2(256f),
                VerticalAlignment = WidgetAlignment.Center, HorizontalAlignment = WidgetAlignment.Center
            };
            rectangle1.Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/CandyRufusLogo");
            RectangleWidget rectangle2 = new()
            {
                FillColor = Color.White, OutlineColor = Color.Transparent, Size = new Vector2(80),
                VerticalAlignment = WidgetAlignment.Far, HorizontalAlignment = WidgetAlignment.Far,
                Margin = new Vector2(10f)
            };
            rectangle2.Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/EngineLogo");
            BusyBarWidget busyBar = new()
            {
                VerticalAlignment = WidgetAlignment.Far, HorizontalAlignment = WidgetAlignment.Center,
                Margin = new Vector2(0, 40)
            };
            m_canvas.AddChildren(Background);
            m_canvas.AddChildren(rectangle1);
            m_canvas.AddChildren(rectangle2);
            m_canvas.AddChildren(busyBar);
            m_canvas.AddChildren(m_logList);
            AddChildren(m_canvas);
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

        public static void Add(LogType type, string message)
        {
            Dispatcher.Dispatch(() =>
            {
                LogItem item = new(type, message);
                m_logList.AddItem(item);
                switch (type)
                {
                    case LogType.Info:
                    case LogType.Advice:
                        Log.Information(message);
                        break;
                    case LogType.Error:
                        Log.Error(message);
                        break;
                    case LogType.Warning:
                        Log.Warning(message);
                        break;
                }

                m_logList.ScrollToItem(item);
            });
        }

        private void InitActions()
        {
            AddLoadAction(() =>
            {
                //将所有的有效的scmod读取为ModEntity，并自动添加SurvivalCraftModEntity
                ContentManager.Initialize();
                ModsManager.Initialize();
            });
            AddLoadAction(ContentLoaded);

            AddLoadAction(() =>
            {
                //检查所有Mod依赖项 
                //根据加载顺序排序后的结果
                ModsManager.ModList.Clear();
                foreach (var item in ModsManager.ModList)
                {
                    //item.CheckDependencies(ModsManager.ModList);
                }
            });
            AddLoadAction(() =>
            {
                //初始化所有ModEntity的语言包
                //>>>初始化语言列表
                ReadOnlyList<ContentInfo> axa = ContentManager.List("Lang");
                LanguageControl.LanguageTypes.Clear();
                foreach (ContentInfo contentInfo in axa)
                {
                    string px = System.IO.Path.GetFileNameWithoutExtension(contentInfo.FileName);
                    if (!LanguageControl.LanguageTypes.Contains(px)) LanguageControl.LanguageTypes.Add(px);
                }

                //<<<结束
                if (ModsManager.Configs.ContainsKey("Language") &&
                    LanguageControl.LanguageTypes.Contains(ModsManager.Configs["Language"]))
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

                ModsManager.ModListAllDo((modEntity) => { modEntity.LoadLanguage(); });
            });
            AddLoadAction(() =>
            {
                Dictionary<string, Assembly[]> assemblies = [];
                ModsManager.ModListAllDo((modEntity) =>
                {
                    Log.Information("Get assemblies " + modEntity.ModInfo.PackageName);
                    assemblies[modEntity.ModInfo.PackageName] = modEntity.GetAssemblies();
                });
                //加载 mod 程序集(.dll)文件
                //但不进行处理操作(如添加block等)

                ModsManager.ModListAllDo((modEntity) =>
                {
                    foreach (var asm in assemblies[modEntity.ModInfo.PackageName])
                    {
                        Log.Information("Handle assembly " + modEntity.ModInfo.PackageName + " " + asm.FullName);
                        try
                        {
                            modEntity.HandleAssembly(asm);
                        }
                        catch (Exception e)
                        {
                            string separator = new('-', 10); //生成10个 '-' 连一起的字符串
                            Log.Error($"H{separator}Handle assembly failed{separator}");
                            Log.Error("Loaded assembly:\n" + string.Join("\n",
                                AppDomain.CurrentDomain.GetAssemblies()
                                    .Select(x => x.FullName ?? x.GetName().FullName)));
                            Log.Error(separator);
                            Log.Error("Error assembly: " + asm.FullName);
                            Log.Error("Dependencies:\n" + string.Join("\n",
                                asm.GetReferencedAssemblies().Select(x => x.FullName)));
                            Log.Error(separator);
                            Log.Error(e);
                        }
                    }
                });

                //处理程序集
            });
            AddLoadAction(() =>
            {
                //读取所有的 ModEntity 的 javascript
                ModsManager.ModListAllDo((modEntity) => { modEntity.LoadJs(); });
                JsInterface.RegisterEvent();
            });
            AddLoadAction(() =>
            {
                Info("执行初始化任务");
                List<Action> actions = [];
                ModInterfacesManager.InvokeHooks("OnLoadingStart", (SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
                {
                    modInterface.OnLoadingStart(actions);
                    isContinueRequired = true;
                });
                foreach (var ac in actions)
                {
                    m_modLoadingActions.Add(ac);
                }
            });
            AddLoadAction(() =>
            {
                //初始化TextureAtlas
                Info("初始化纹理地图");
                TextureAtlasManager.Initialize();
            });
            AddLoadAction(() =>
            {
                //初始化Database
                try
                {
                    DatabaseManager.Initialize();
                    ModsManager.ModListAllDo((modEntity) =>
                    {
                        modEntity.LoadXdb(ref DatabaseManager.DatabaseNode);
                    });
                }
                catch (Exception e)
                {
                    Warning(e.Message);
                }
            });
            AddLoadAction(() =>
            {
                Info("读取数据库");
                try
                {
                    DatabaseManager.LoadDataBaseFromXml(DatabaseManager.DatabaseNode);
                }
                catch (Exception e)
                {
                    Error(e.ToString());
                }
            });
            AddLoadAction(() =>
            {
                //初始化方块管理器
                Info("初始化方块管理器");
                BlocksManager.Initialize();
            });
            AddLoadAction(() =>
            {
                //初始化合成谱
                Info("初始化配方管理器");
                CraftingRecipesManager.Initialize();
            });
            InitScreens();
            AddLoadAction(() =>
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
            });
            AddLoadAction(() =>
            {
                Info("初始化Mod设置参数");
                if (!Storage.FileExists(ModsManager.ModSettingsPath)) return;
                
                using System.IO.Stream stream = Storage.OpenFile(ModsManager.ModSettingsPath, OpenFileMode.Read);
                try
                {
                    XElement element = XElement.Load(stream);
                    ModsManager.LoadModSettings(element);
                }
                catch (Exception e)
                {
                    Warning(e.Message);
                }
            });
            AddLoadAction(() =>
            {
                ModInterfacesManager.InvokeHooks("OnLoadingFinished", (SurvivalCraftModInterface modInterface, out bool isContinueRequired) =>
                {
                    Info("等待剩下的任务完成:" + modInterface.ModEntity.ModInfo.PackageName);
                    modInterface.OnLoadingFinished(m_modLoadingActions);
                    isContinueRequired = true;
                });
            });
            AddLoadAction(() => { ScreensManager.SwitchScreen("MainMenu"); });
        }

        private void InitScreens()
        {
            AddLoadAction(() => { AddScreen("Nag", new NagScreen()); });
            AddLoadAction(() => { AddScreen("MainMenu", new MainMenuScreen()); });
            AddLoadAction(() => { AddScreen("Recipaedia", new RecipaediaScreen()); });
            AddLoadAction(() => { AddScreen("RecipaediaRecipes", new RecipaediaRecipesScreen()); });
            AddLoadAction(() => { AddScreen("RecipaediaDescription", new RecipaediaDescriptionScreen()); });
            AddLoadAction(() => { AddScreen("Bestiary", new BestiaryScreen()); });
            AddLoadAction(() => { AddScreen("BestiaryDescription", new BestiaryDescriptionScreen()); });
            AddLoadAction(() => { AddScreen("Help", new HelpScreen()); });
            AddLoadAction(() => { AddScreen("HelpTopic", new HelpTopicScreen()); });
            AddLoadAction(() => { AddScreen("Settings", new SettingsScreen()); });
            AddLoadAction(() => { AddScreen("SettingsPerformance", new SettingsPerformanceScreen()); });
            AddLoadAction(() => { AddScreen("SettingsGraphics", new SettingsGraphicsScreen()); });
            AddLoadAction(() => { AddScreen("SettingsUi", new SettingsUiScreen()); });
            AddLoadAction(() => { AddScreen("SettingsCompatibility", new SettingsCompatibilityScreen()); });
            AddLoadAction(() => { AddScreen("SettingsAudio", new SettingsAudioScreen()); });
            AddLoadAction(() => { AddScreen("SettingsControls", new SettingsControlsScreen()); });
            AddLoadAction(() => { AddScreen("Play", new PlayScreen()); });
            AddLoadAction(() => { AddScreen("NewWorld", new NewWorldScreen()); });
            AddLoadAction(() => { AddScreen("ModifyWorld", new ModifyWorldScreen()); });
            AddLoadAction(() => { AddScreen("WorldOptions", new WorldOptionsScreen()); });
            AddLoadAction(() => { AddScreen("GameLoading", new GameLoadingScreen()); });
            AddLoadAction(() => { AddScreen("Game", new GameScreen()); });
            AddLoadAction(() => { AddScreen("TrialEnded", new TrialEndedScreen()); });
            AddLoadAction(() => { AddScreen("ExternalContent", new ExternalContentScreen()); });
            AddLoadAction(() => { AddScreen("CommunityContent", new CommunityContentScreen()); });
            AddLoadAction(() => { AddScreen("Content", new ContentScreen()); });
            AddLoadAction(() => { AddScreen("ManageContent", new ManageContentScreen()); });
            AddLoadAction(() => { AddScreen("ModsManageContent", new ModsManageContentScreen()); });
            AddLoadAction(() => { AddScreen("ManageUser", new ManageUserScreen()); });
            AddLoadAction(() => { AddScreen("Players", new PlayersScreen()); });
            AddLoadAction(() => { AddScreen("Player", new PlayerScreen()); });
        }

        public void AddScreen(string name, Screen screen)
        {
            ScreensManager.AddScreen(name, screen);
        }

        private void AddLoadAction(Action action)
        {
            m_loadingActions.Add(action);
        }

        public override void Leave()
        {
            m_logList.ClearItems();
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
            if (m_modLoadingActions.Count > 0)
            {
                try
                {
                    m_modLoadingActions[0].Invoke();
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }
                finally
                {
                    m_modLoadingActions.RemoveAt(0);
                }
            }
            else
            {
                if (m_loadingActions.Count > 0)
                {
                    try
                    {
                        m_loadingActions[0].Invoke();
                    }
                    catch (Exception e)
                    {
                        Error(e.ToString());
                    }
                    finally
                    {
                        m_loadingActions.RemoveAt(0);
                    }
                }
            }
        }
    }
}