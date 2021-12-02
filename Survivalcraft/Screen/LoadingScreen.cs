using Engine;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine.Graphics;
namespace Game
{
    public class LoadingScreen : Screen
    {
        public enum LogType { 
            Info,
            Warning,
            Error,
            Advice
        }
        private class LogItem {
            public LogType LogType;
            public string Message;
            public LogItem(LogType type, string log) { LogType = type; Message = log; }
        }
        private List<Action> LoadingActoins = new List<Action>();
        private List<Action> ModLoadingActoins = new List<Action>();
        private CanvasWidget Canvas = new CanvasWidget();
        private static ListPanelWidget LogList = new ListPanelWidget() { Direction = LayoutDirection.Vertical, PlayClickSound = false };
        static LoadingScreen() {
            LogList.ItemWidgetFactory = (obj) => {
                LogItem logItem = obj as LogItem;
                CanvasWidget canvasWidget = new CanvasWidget() { Size = new Vector2(Display.Viewport.Width, 40), Margin = new Vector2(0, 2),HorizontalAlignment=WidgetAlignment.Near };
                FontTextWidget fontTextWidget = new FontTextWidget() { FontScale = 0.7f, Text = logItem.Message, Color = GetColor(logItem.LogType), VerticalAlignment = WidgetAlignment.Center, HorizontalAlignment = WidgetAlignment.Near };
                canvasWidget.Children.Add(fontTextWidget);
                return canvasWidget;
            };
            LogList.ItemSize = 30;
        }
        public static Color GetColor(LogType type) {
            switch (type) {
                case LogType.Advice:return Color.Cyan;
                case LogType.Error:return Color.Red;
                case LogType.Warning:return Color.Yellow;
                case LogType.Info:return Color.White;
                default:return Color.White;
            }
        }
        public LoadingScreen()
        {
            RectangleWidget rectangle = new RectangleWidget() { FillColor = Color.Black, OutlineThickness = 0f };
            Canvas.Size = new Vector2(float.PositiveInfinity);
            Canvas.Children.Add(rectangle);
            Canvas.Children.Add(LogList);
            Children.Add(Canvas);
            Info("Initilizing Mods Manager. Api Version: " + ModsManager.APIVersion);
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
        public static void Add(LogType type,string mesg) {
            Dispatcher.Dispatch(delegate {
                LogItem item = new LogItem(type, mesg);
                LogList.AddItem(item);
                switch (type) {
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
            AddLoadAction(delegate {//将所有的有效的scmod读取为ModEntity，并自动添加SurvivalCraftModEntity
                ContentManager.Initialize();
                MusicManager.CurrentMix = MusicManager.Mix.Menu;
                ModsManager.Initialize();
            });
            AddLoadAction(delegate {//检查所有Mod依赖项 
                ModsManager.ModListAllDo((modEntity) => { modEntity.CheckDependencies(); });            
            });
            AddLoadAction(delegate { //初始化所有ModEntity的语言包
                //>>>初始化语言列表
                ReadOnlyList<ContentInfo> axa = ContentManager.List("Lang");
                LanguageControl.LanguageTypes.Clear();
                foreach (ContentInfo contentInfo in axa) {
                    string px = System.IO.Path.GetFileNameWithoutExtension(contentInfo.Filename);
                    if(!LanguageControl.LanguageTypes.Contains(px))LanguageControl.LanguageTypes.Add(px);
                }
                //<<<结束
                if (ModsManager.Configs.ContainsKey("Language")) LanguageControl.Initialize(ModsManager.Configs["Language"]);
                else LanguageControl.Initialize("zh-CN");
                ModsManager.ModListAllDo((modEntity) => { modEntity.LoadLauguage(); });
            });
            AddLoadAction(delegate { //读取所有的ModEntity的dll，并分离出ModLoader，保存Blocks
                ModsManager.ModListAllDo((modEntity) => { modEntity.LoadDll(); });
            });
            AddLoadAction(delegate {//初始化TextureAtlas
                Info("初始化纹理地图");
                TextureAtlasManager.Initialize();
            });
            AddLoadAction(delegate { //初始化Database
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
            AddLoadAction(delegate {
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
            AddLoadAction(delegate { //初始化方块管理器
                Info("初始化方块管理器");
                BlocksManager.Initialize();
            });
            AddLoadAction(delegate { //初始化合成谱
                CraftingRecipesManager.Initialize();
            });
            InitScreens();
            AddLoadAction(delegate {
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
            AddLoadAction(delegate {
                Info("初始化Mod设置参数");
                if (Storage.FileExists(ModsManager.ModsSetPath)) {
                    using (System.IO.Stream stream = Storage.OpenFile(ModsManager.ModsSetPath, OpenFileMode.Read))
                    {
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
                }
            });
            AddLoadAction(()=> {
                ModsManager.ModListAllDo((modEntity) => { Info("等待剩下的任务完成:" + modEntity.modInfo?.PackageName); modEntity.OnLoadingFinished(ModLoadingActoins); });
            });
            AddLoadAction(()=> {
                ScreensManager.SwitchScreen("MainMenu");
            });
        }
        private void InitScreens() {

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
            if (Input.Back || Input.Cancel) DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Warning, "Quit?", LanguageControl.Ok, LanguageControl.No, (vt) => {
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
                finally {
                    ModLoadingActoins.RemoveAt(0);
                }

            }
            else {
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
                    finally {
                        LoadingActoins.RemoveAt(0);
                    }
                }
            }
        }
    }
}
