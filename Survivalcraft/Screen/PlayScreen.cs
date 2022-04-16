using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Game
{
    public class PlayScreen : Screen
    {
        public ListPanelWidget m_worldsListWidget;

        public ButtonWidget m_playButton;

        public ButtonWidget m_newWorldButton;

        public ButtonWidget m_propertiesButton;

        public static int MaxWorlds = 300;

        public static string fName = "PlayScreen";

        public PlayScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/PlayScreen");
            LoadContents(this, node);
            m_worldsListWidget = Children.Find<ListPanelWidget>("WorldsList");
            m_playButton = Children.Find<ButtonWidget>("Play");
            m_newWorldButton = Children.Find<ButtonWidget>("NewWorld");
            m_propertiesButton = Children.Find<ButtonWidget>("Properties");
            ListPanelWidget worldsListWidget = m_worldsListWidget;
            worldsListWidget.ItemWidgetFactory = (Func<object, Widget>)Delegate.Combine(worldsListWidget.ItemWidgetFactory, (Func<object, Widget>)delegate (object item)
            {
                var worldInfo = (WorldInfo)item;
                XElement node2 = ContentManager.Get<XElement>("Widgets/SavedWorldItem");
                var containerWidget = (ContainerWidget)LoadWidget(this, node2, null);
                LabelWidget labelWidget = containerWidget.Children.Find<LabelWidget>("WorldItem.Name");
                LabelWidget labelWidget2 = containerWidget.Children.Find<LabelWidget>("WorldItem.Details");
                containerWidget.Tag = worldInfo;
                labelWidget.Text = worldInfo.WorldSettings.Name;
                labelWidget2.Text = string.Format("{0} | {1:dd MMM yyyy HH:mm} | {2} | {3} | {4}", DataSizeFormatter.Format(worldInfo.Size),
                    worldInfo.LastSaveTime.ToLocalTime(),
                    (worldInfo.PlayerInfos.Count > 1) ? string.Format(LanguageControl.GetContentWidgets(fName, 9), worldInfo.PlayerInfos.Count) : string.Format(LanguageControl.GetContentWidgets(fName, 10), 1),
                    LanguageControl.Get("GameMode", worldInfo.WorldSettings.GameMode.ToString()),
                    LanguageControl.Get("EnvironmentBehaviorMode", worldInfo.WorldSettings.EnvironmentBehaviorMode.ToString()));
                if (worldInfo.SerializationVersion != VersionsManager.SerializationVersion)
                {
                    labelWidget2.Text = labelWidget2.Text + " | " + (string.IsNullOrEmpty(worldInfo.SerializationVersion) ? LanguageControl.GetContentWidgets("Usual", "Unknown") : ("(" + worldInfo.SerializationVersion + ")"));
                }
                return containerWidget;
            });
            m_worldsListWidget.ScrollPosition = 0f;
            m_worldsListWidget.ScrollSpeed = 0f;
            m_worldsListWidget.ItemClicked += delegate (object item)
            {
                if (item != null && m_worldsListWidget.SelectedItem == item)
                {
                    Play(item);
                }
            };
        }

        public override void Enter(object[] parameters)
        {
            var dialog = new BusyDialog(LanguageControl.GetContentWidgets(fName, 5), null);
            DialogsManager.ShowDialog(null, dialog);
            Task.Run(delegate
            {
                var selectedItem = (WorldInfo)m_worldsListWidget.SelectedItem;
                WorldsManager.UpdateWorldsList();
                var worldInfos = new List<WorldInfo>(WorldsManager.WorldInfos);
                worldInfos.Sort((WorldInfo w1, WorldInfo w2) => DateTime.Compare(w2.LastSaveTime, w1.LastSaveTime));
                Dispatcher.Dispatch(delegate
                {
                    m_worldsListWidget.ClearItems();
                    foreach (WorldInfo item in worldInfos)
                    {
                        m_worldsListWidget.AddItem(item);
                    }
                    if (selectedItem != null)
                    {
                        m_worldsListWidget.SelectedItem = worldInfos.FirstOrDefault((WorldInfo wi) => wi.DirectoryName == selectedItem.DirectoryName);
                    }
                    DialogsManager.HideDialog(dialog);
                });
            });
        }

        public override void Update()
        {
            Vector2 size = new Vector2(310, 60);
            if(SettingsManager.UIScale > 1f) size = new Vector2(250, 60);
            m_playButton.Size = size;
            m_newWorldButton.Size = size;
            if (m_worldsListWidget.SelectedItem != null && WorldsManager.WorldInfos.IndexOf((WorldInfo)m_worldsListWidget.SelectedItem) < 0)
            {
                m_worldsListWidget.SelectedItem = null;
            }
            Children.Find<LabelWidget>("TopBar.Label").Text = string.Format(LanguageControl.GetContentWidgets(fName, 6), m_worldsListWidget.Items.Count);
            m_playButton.IsEnabled = (m_worldsListWidget.SelectedItem != null);
            m_propertiesButton.IsEnabled = (m_worldsListWidget.SelectedItem != null);
            if (m_playButton.IsClicked && m_worldsListWidget.SelectedItem != null)
            {
                Play(m_worldsListWidget.SelectedItem);
            }
            if (m_newWorldButton.IsClicked)
            {
                if (WorldsManager.WorldInfos.Count >= MaxWorlds)
                {
                    DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.GetContentWidgets(fName, 7), string.Format(LanguageControl.GetContentWidgets(fName, 8), MaxWorlds), LanguageControl.GetContentWidgets("Usual", "ok"), null, null));
                }
                else
                {
                    ScreensManager.SwitchScreen("NewWorld");
                    m_worldsListWidget.SelectedItem = null;
                }
            }
            if (m_propertiesButton.IsClicked && m_worldsListWidget.SelectedItem != null)
            {
                var worldInfo = (WorldInfo)m_worldsListWidget.SelectedItem;
                ScreensManager.SwitchScreen("ModifyWorld", worldInfo.DirectoryName, worldInfo.WorldSettings);
            }
            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
            {
                ScreensManager.SwitchScreen("MainMenu");
                m_worldsListWidget.SelectedItem = null;
            }
        }

        public void Play(object item)
        {
            ModsManager.HookAction("BeforeGameLoading", loader => {
                item = loader.BeforeGameLoading(this, item);
                return true;
            });
            ScreensManager.SwitchScreen("GameLoading", item, null);
            m_worldsListWidget.SelectedItem = null;
        }
    }
}
