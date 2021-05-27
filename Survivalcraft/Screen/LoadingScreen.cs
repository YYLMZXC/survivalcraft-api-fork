using Engine;
using Engine.Content;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game
{
    public class LoadingScreen : Screen
    {
        public List<Action> m_loadActions = new List<Action>();

        public int m_index;

        public bool m_loadingStarted;

        public bool m_loadingFinished;

        public bool m_pauseLoading;

        public bool m_loadingErrorsSuppressed;

        public StackPanelWidget panelWidget = new StackPanelWidget() { Direction=LayoutDirection.Vertical,HorizontalAlignment=WidgetAlignment.Center,VerticalAlignment=WidgetAlignment.Far};

        public LabelWidget labelWidget = new LabelWidget() { Text="API v1.34",Color=Color.Red,VerticalAlignment=WidgetAlignment.Far, HorizontalAlignment=WidgetAlignment.Center};

        public LabelWidget labelWidget2 = new LabelWidget() { Color=Color.Red, VerticalAlignment = WidgetAlignment.Far,HorizontalAlignment = WidgetAlignment.Near,Margin=new Vector2(300,0)};

        public LoadingScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/LoadingScreen");
            LoadContents(this, node);
            panelWidget.Children.Add(labelWidget);
            panelWidget.Children.Add(labelWidget2);
            Children.Add(panelWidget);
            AddLoadAction(delegate
            {
                SetMsg("初始化DatabaseManager");
                DatabaseManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化CommunityContentManager");
                CommunityContentManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化MotdManager");
                MotdManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化LightingManager");
                LightingManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化StringsManager");
                StringsManager.LoadStrings();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化TextureAtlasManager");
                TextureAtlasManager.LoadAtlases();
            });

            AddLoadAction(delegate
            {
                SetMsg("初始化WorldsManager");
                WorldsManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化BlocksTexturesManager");
                BlocksTexturesManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化CharacterSkinsManager");
                CharacterSkinsManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化FurniturePacksManager");
                FurniturePacksManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化BlocksManager");
                BlocksManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化CraftingRecipesManager");
                CraftingRecipesManager.Initialize();
            });
            AddLoadAction(delegate
            {
                SetMsg("初始化MusicManager");
                MusicManager.CurrentMix = MusicManager.Mix.Menu;
            });
            foreach (ContentInfo item in ContentManager.List())
            {
                ContentInfo localContentInfo = item;
                AddLoadAction(delegate
                {
                    SetMsg("检查文件" + localContentInfo.Name);
                    ContentManager.Get(localContentInfo.Name);
                });
            }

        }

        public void AddLoadAction(Action action)
        {
            m_loadActions.Add(action);
        }
        public void SetMsg(string text) {
            labelWidget2.Text = text;
        }
        public override void Leave()
        {
            ContentManager.Dispose("Textures/Gui/CandyRufusLogo");
            ContentManager.Dispose("Textures/Gui/EngineLogo");
        }

        public override void Update()
        {
            if (!m_loadingStarted)
            {
                m_loadingStarted = true;
            }
            else
            {
                if (m_loadingFinished)
                {
                    return;
                }
                double realTime = Time.RealTime;
                while (!m_pauseLoading && m_index < m_loadActions.Count)
                {
                    try
                    {
                        m_loadActions[m_index++]();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Loading error. Reason: " + ex.Message);
                        if (!m_loadingErrorsSuppressed)
                        {
                            m_pauseLoading = true;
                            DialogsManager.ShowDialog(ScreensManager.RootWidget, new MessageDialog("Loading Error", ExceptionManager.MakeFullErrorMessage(ex), "确定", "Suppress", delegate (MessageDialogButton b)
                            {
                                switch (b)
                                {
                                    case MessageDialogButton.Button1:
                                        m_pauseLoading = false;
                                        break;
                                    case MessageDialogButton.Button2:
                                        m_loadingErrorsSuppressed = true;
                                        break;
                                }
                            }));
                        }
                    }
                    if (Time.RealTime - realTime > 0.1)
                    {
                        break;
                    }
                }
                if (m_index >= m_loadActions.Count)
                {
                    m_loadingFinished = true;
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    ScreensManager.SwitchScreen("MainMenu");
                }
            }
        }
    }
}
