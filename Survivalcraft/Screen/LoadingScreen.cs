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
        private bool ActionFinished = true;
        private object ActionLock = new object();
        private List<Action> LoadingActoins = new List<Action>();
        private List<Action> ModLoadingActoins = new List<Action>();
        private CanvasWidget Canvas = new CanvasWidget();
        private static ListPanelWidget LogList = new ListPanelWidget() { Direction = LayoutDirection.Vertical, PlayClickSound = false };
        static LoadingScreen() {
            LogList.ItemWidgetFactory = (obj) => {
                LogItem logItem = obj as LogItem;
                CanvasWidget canvasWidget = new CanvasWidget() { Size = new Vector2(Display.Viewport.Width, 40), Margin = new Vector2(0, 2),HorizontalAlignment=WidgetAlignment.Near };
                FontTextWidget fontTextWidget = new FontTextWidget() { FontScale = 1f, Text = logItem.Message, Color = GetColor(logItem.LogType), VerticalAlignment = WidgetAlignment.Center, HorizontalAlignment = WidgetAlignment.Near };
                canvasWidget.Children.Add(fontTextWidget);
                return canvasWidget;
            };
            LogList.ItemSize = 20;
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
                LanguageControl.Initialize(ModsManager.modSettings.languageType);
                System.Threading.Tasks.Task.Run(delegate {
                    lock (ActionLock)
                    {
                        ModsManager.ModListAllDo((modEntity) => { modEntity.LoadLauguage(); });
                    }
                });
            });
            AddLoadAction(delegate { //读取所有的ModEntity的dll，并分离出ModLoader，保存Blocks
                ActionFinished = false;
                lock (ActionLock)
                {
                    ModsManager.ModListAllDo((modEntity) => { modEntity.LoadDll(); });
                    ActionFinished = true;
                }
            });
            AddLoadAction(delegate {//初始化TextureAtlas
                Info("TextureAtlas Initialize");
                TextureAtlasManager.Initialize();
            });
            AddLoadAction(delegate { //初始化Database
                Info("DatabaseManager Initialize");
                System.Threading.Tasks.Task.Run(delegate {
                    lock (ActionLock)
                    {
                        try
                        {
                            DatabaseManager.Initialize();
                            ModsManager.ModListAllDo((modEntity) => { modEntity.LoadXdb(ref DatabaseManager.DatabaseNode); });
                            DatabaseManager.LoadDataBaseFromXml(DatabaseManager.DatabaseNode);
                        }
                        catch (Exception e)
                        {
                            Warning(e.Message);
                        }
                    }
                });
            });
            AddLoadAction(delegate { //初始化方块管理器
                ActionFinished = false;
                lock (ActionLock) {
                    Info("BlocksManager Initialize");
                    BlocksManager.Initialize();
                    ActionFinished = true;
                }
            });

            AddLoadAction(delegate { //初始化合成谱
                Info("CraftingRecipesManager Initialize");
                CraftingRecipesManager.Initialize();
            });
            InitScreens();

            AddLoadAction(delegate {
                BlocksTexturesManager.Initialize();
                AnalyticsManager.Initialize();
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
                Info("Loading ModSettings");
                if (Storage.FileExists(ModsManager.SettingPath)) {
                    using (System.IO.Stream stream = Storage.OpenFile(ModsManager.SettingPath, OpenFileMode.Read))
                    {
                        try
                        {
                            XElement element = XElement.Load(stream);
                            ModsManager.ModListAllDo((modEntity) => {
                                modEntity.LoadSettings(element);
                            });
                        }
                        catch (Exception e)
                        {
                            Warning(e.Message);
                        }
                    }
                }
            });
            AddLoadAction(()=> {
                ModsManager.ModListAllDo((modEntity) => { Info("Invoke OnLoadingFinished:" + modEntity.modInfo?.PackageName); modEntity.OnLoadingFinished(ModLoadingActoins); });
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
                    if (ActionFinished) ModLoadingActoins[0].Invoke();
                    ModLoadingActoins.RemoveAt(0);
                }
                catch (Exception e)
                {
                    ModsManager.AddException(e, false);
                }

            }
            else {
                if (LoadingActoins.Count > 0)
                {
                    try
                    {
                        LoadingActoins[0].Invoke();
                        if(ActionFinished)LoadingActoins.RemoveAt(0);
                    }
                    catch (Exception e)
                    {
                        ModsManager.AddException(e, false);
                    }
                }
            }
        }
    }
}
