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
        public List<Action> QuequeAction = new List<Action>();

        public int m_index;

        public bool m_loadingStarted;

        public bool m_loadingFinished;

        public bool m_pauseLoading;

        public bool m_loadingErrorsSuppressed;

        public StackPanelWidget panelWidget = new StackPanelWidget() { Direction = LayoutDirection.Vertical, HorizontalAlignment = WidgetAlignment.Center, VerticalAlignment = WidgetAlignment.Far, Margin = new Vector2(0, 80) };

        public LabelWidget labelWidget = new LabelWidget() { Text = "API v1.34", Color = Color.Red, VerticalAlignment = WidgetAlignment.Far, HorizontalAlignment = WidgetAlignment.Center };

        public static LabelWidget labelWidget2 = new LabelWidget() { Color=Color.Red, VerticalAlignment = WidgetAlignment.Far,HorizontalAlignment = WidgetAlignment.Near,Margin=new Vector2(300,0)};

        public XElement DatabaseNode;

        public bool IsConfirm = false;

        public LoadingScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/LoadingScreen");
            LoadContents(this, node);
            panelWidget.Children.Add(labelWidget);
            panelWidget.Children.Add(labelWidget2);
            Children.Add(panelWidget);
            InitActions();            
        }

        public void InitActions() {
            AddLoadAction(() => {
                SetMsg("初始化ModsManager");
                ModsManager.Initialize();
            });
            AddLoadAction(() => {
                foreach (ModEntity entity in ModsManager.WaitToLoadMods)
                {
                    AddQuequeAction(() => {
                        SetMsg($"检查Mod依赖项:{entity.modInfo.Name}");
                        entity.CheckDependencies();
                    });
                }
            });
            AddLoadAction(() => {
                foreach (ModEntity entity in ModsManager.CacheToLoadMods)
                {
                    AddQuequeAction(() => {
                        SetMsg($"加载Dll:{entity.modInfo.Name}");
                        try
                        {
                            entity.LoadDll();
                        }
                        catch (Exception e)
                        {
                            entity.HasException = true;
                            ModsManager.AddException(e);
                        }
                    });
                }
            });
            AddLoadAction(() => {
                LoadingScreen.SetMsg("初始化语言包:[SurvivalCraft]");
                LanguageControl.Initialize(ModsManager.modSettings.languageType);
                foreach (ModEntity modEntity in ModsManager.CacheToLoadMods)
                {
                    AddQuequeAction(() => {
                        try
                        {
                            LoadingScreen.SetMsg($"初始化语言包:[{modEntity.modInfo.Name}]");
                            modEntity.LoadLauguage();                            
                        }
                        catch (Exception e)
                        {
                            modEntity.HasException = true;
                            ModsManager.AddException(e);
                        }

                    });
                }
            });
            AddLoadAction(()=> {
                
                foreach (ModEntity modEntity in ModsManager.CacheToLoadMods)
                {
                    AddQuequeAction(()=> {
                        LoadingScreen.SetMsg($"初始化Pak:[{modEntity.modInfo.Name}]");
                        modEntity.InitPak();
                    });
                }
            });
            AddLoadAction(() => {
                foreach (ContentInfo item in ContentManager.List())
                {
                    ContentInfo localContentInfo = item;
                    AddQuequeAction(delegate
                    {
                        try
                        {
                            SetMsg($"检查Pak:{localContentInfo.Name}");
                            ContentManager.Get(localContentInfo.Name);
                        }
                        catch (Exception e)
                        {
                            ModsManager.AddException(e);
                        }
                    });
                }
            });
            AddLoadAction(() => {
                SetMsg("初始化DatabaseManager:[SurvivalCraft]");
                DatabaseManager.Initialize();
                foreach (ModEntity entity in ModsManager.CacheToLoadMods)
                {
                    AddQuequeAction(() => {
                        SetMsg($"初始化DatabaseManager:[{entity.modInfo.Name}]");
                        entity.LoadXdb(ref DatabaseManager.xElement);
                    });
                }
            });
            AddLoadAction(() => {
                SetMsg("加载Database:[SurvivalCraft]");
                DatabaseManager.LoadDataBaseFromXml(DatabaseManager.xElement);
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化CommunityContentManager");
                CommunityContentManager.Initialize();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化MotdManager");
                MotdManager.Initialize();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化LightingManager");
                LightingManager.Initialize();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化StringsManager");
                StringsManager.LoadStrings();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化TextureAtlasManager");
                TextureAtlasManager.LoadAtlases();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化WorldsManager");
                WorldsManager.Initialize();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化BlocksTexturesManager");
                BlocksTexturesManager.Initialize();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化CharacterSkinsManager");
                CharacterSkinsManager.Initialize();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化FurniturePacksManager");
                FurniturePacksManager.Initialize();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化BlocksManager");
                BlocksManager.Initialize();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化CraftingRecipesManager");
                CraftingRecipesManager.Initialize();
            });
            AddLoadAction(() =>
            {
                SetMsg("初始化MusicManager");
                MusicManager.CurrentMix = MusicManager.Mix.Menu;
            });
            AddLoadAction(() => {
                ModsManager.CacheToLoadMods.Clear();
                ModsManager.LoadedMods.Clear();
                foreach (ModEntity modEntity in ModsManager.CacheToLoadMods)
                {
                    if(!modEntity.HasException)ModsManager.LoadedMods.Add(modEntity);
                }

            });
        }

        public void AddLoadAction(Action action)
        {
            m_loadActions.Add(action);
        }
        public void AddQuequeAction(Action action)
        {
            QuequeAction.Add(action);
        }
        public static void SetMsg(string text) {
            labelWidget2.Text = text;
        }
        public override void Leave()
        {
            ContentManager.Dispose("Textures/Gui/CandyRufusLogo");
            ContentManager.Dispose("Textures/Gui/EngineLogo");
            Window.PresentationInterval = SettingsManager.PresentationInterval;
        }
        public override void Enter(object[] parameters)
        {
            Window.PresentationInterval = 0;
            base.Enter(parameters);
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
                    bool flag2=false;
                    while (QuequeAction.Count > 0)
                    {
                        QuequeAction[0].Invoke();
                        QuequeAction.RemoveAt(0);
                        flag2 = true;
                        break;
                    }
                    if (flag2) break;
                    if (ModsManager.exceptions.Count > 0)
                    {
                        m_pauseLoading = true;
                    }
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
                if (ModsManager.exceptions.Count > 0&&!IsConfirm) {

                    DialogsManager.ShowDialog(ScreensManager.RootWidget, new MessageDialog("Mod加载出错", ExceptionManager.MakeFullErrorMessage(ModsManager.exceptions[0]), "确定", "忽略", delegate (MessageDialogButton b)
                    {
                        IsConfirm = true;
                        switch (b)
                        {
                            case MessageDialogButton.Button1:
                                m_pauseLoading = false;
                                ModsManager.exceptions.RemoveAt(0);
                                break;
                            case MessageDialogButton.Button2:
                                m_loadingErrorsSuppressed = true;                                
                                break;
                        }
                    }));

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
