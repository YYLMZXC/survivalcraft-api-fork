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
        public List<Action> m_loadActions = new List<Action>();

        public CanvasWidget Canvas = new CanvasWidget();

        public ListPanelWidget LogList = new ListPanelWidget() { Direction = LayoutDirection.Vertical, PlayClickSound = false };

        public XElement DatabaseNode;

        public Color GetColor(LogType type) {
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
            Canvas.Size = new Vector2(float.PositiveInfinity);
            Canvas.Children.Add(LogList);
            LogList.ItemWidgetFactory += (obj) => {
                LogItem logItem = obj as LogItem;
                CanvasWidget canvasWidget = new CanvasWidget() { Size = new Vector2(float.PositiveInfinity, 20), Margin = new Vector2(Display.Viewport.Width, 2) };
                FontTextWidget fontTextWidget = new FontTextWidget() { Text = logItem.Message, Color = GetColor(logItem.LogType), VerticalAlignment = WidgetAlignment.Center, HorizontalAlignment = WidgetAlignment.Near };
                canvasWidget.Children.Add(fontTextWidget);
                return canvasWidget;
            };
            LogList.ItemSize = 20;
            Children.Add(Canvas);
            Info("Initilizing Mods Manager. Api Version: 1.34");
        }

        public void Error(string mesg)
        {
            Add(LogType.Error,mesg);
        }
        public void Info(string mesg)
        {
            Add(LogType.Info, mesg);
        }
        public void Warning(string mesg)
        {
            Add(LogType.Warning, mesg);
        }
        public void Advice(string mesg)
        {
            Add(LogType.Advice, mesg);
        }

        public void Add(LogType type,string mesg) {
            LogItem item = new LogItem(type, mesg);
            LogList.AddItem(item);
            LogList.ScrollToItem(item);
        }

        public void InitActions()
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
        public void AddLoadAction(Action action)
        {
            m_loadActions.Add(action);
        }
        public void AddQuequeAction(Action action)
        {
            //QuequeAction.Add(action);
        }
        public override void Leave()
        {
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


        }
    }
}
