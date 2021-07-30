using System.Xml.Linq;

namespace Game
{
    public class SettingsUiScreen : Screen
    {
        public ContainerWidget m_windowModeContainer;

        public ButtonWidget m_windowModeButton;

        public ButtonWidget m_languageButton;

        public ButtonWidget m_uiSizeButton;

        public ButtonWidget m_upsideDownButton;

        public ButtonWidget m_hideMoveLookPadsButton;

        public ButtonWidget m_showGuiInScreenshotsButton;

        public ButtonWidget m_showLogoInScreenshotsButton;

        public ButtonWidget m_screenshotSizeButton;

        public ButtonWidget m_communityContentModeButton;

        public static string fName = "SettingsUiScreen";

        public SettingsUiScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/SettingsUiScreen");
            LoadContents(this, node);
            m_windowModeContainer = Children.Find<ContainerWidget>("WindowModeContainer");
            m_languageButton = Children.Find<ButtonWidget>("LanguageButton");
            m_windowModeButton = Children.Find<ButtonWidget>("WindowModeButton");
            m_uiSizeButton = Children.Find<ButtonWidget>("UiSizeButton");
            m_upsideDownButton = Children.Find<ButtonWidget>("UpsideDownButton");
            m_hideMoveLookPadsButton = Children.Find<ButtonWidget>("HideMoveLookPads");
            m_showGuiInScreenshotsButton = Children.Find<ButtonWidget>("ShowGuiInScreenshotsButton");
            m_showLogoInScreenshotsButton = Children.Find<ButtonWidget>("ShowLogoInScreenshotsButton");
            m_screenshotSizeButton = Children.Find<ButtonWidget>("ScreenshotSizeButton");
            m_communityContentModeButton = Children.Find<ButtonWidget>("CommunityContentModeButton");
        }

        public override void Enter(object[] parameters)
        {
            m_windowModeContainer.IsVisible = true;
        }

        public override void Update()
        {
            if (m_windowModeButton.IsClicked)
            {
                SettingsManager.WindowMode = (Engine.WindowMode)((int)(SettingsManager.WindowMode + 1) % EnumUtils.GetEnumValues(typeof(Engine.WindowMode)).Count);
            }
            if (m_languageButton.IsClicked)
            {
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 1), LanguageControl.Get(fName, 2), LanguageControl.Yes, LanguageControl.No, delegate (MessageDialogButton button)
                {
                    if (button == MessageDialogButton.Button1)
                    {
                        ModsManager.modSettings.languageType = (LanguageControl.LanguageType)((int)(ModsManager.modSettings.languageType + 1) % EnumUtils.GetEnumValues(typeof(LanguageControl.LanguageType)).Count);
                        LanguageControl.Initialize(ModsManager.modSettings.languageType);
                        ScreensManager.m_screens["MainMenu"] = new MainMenuScreen();
                        ScreensManager.m_screens["Recipaedia"] = new RecipaediaScreen();
                        ScreensManager.m_screens["RecipaediaRecipes"] = new RecipaediaRecipesScreen();
                        ScreensManager.m_screens["RecipaediaDescription"] = new RecipaediaDescriptionScreen();
                        ScreensManager.m_screens["Bestiary"] = new BestiaryScreen();
                        ScreensManager.m_screens["BestiaryDescription"] = new BestiaryDescriptionScreen();
                        ScreensManager.m_screens["Help"] = new HelpScreen();
                        ScreensManager.m_screens["HelpTopic"] = new HelpTopicScreen();
                        ScreensManager.m_screens["Settings"] = new SettingsScreen();
                        ScreensManager.m_screens["SettingsPerformance"] = new SettingsPerformanceScreen();
                        ScreensManager.m_screens["SettingsGraphics"] = new SettingsGraphicsScreen();
                        ScreensManager.m_screens["SettingsUi"] = new SettingsUiScreen();
                        ScreensManager.m_screens["SettingsCompatibility"] = new SettingsCompatibilityScreen();
                        ScreensManager.m_screens["SettingsAudio"] = new SettingsAudioScreen();
                        ScreensManager.m_screens["SettingsControls"] = new SettingsControlsScreen();
                        ScreensManager.m_screens["Play"] = new PlayScreen();
                        ScreensManager.m_screens["NewWorld"] = new NewWorldScreen();
                        ScreensManager.m_screens["ModifyWorld"] = new ModifyWorldScreen();
                        ScreensManager.m_screens["WorldOptions"] = new WorldOptionsScreen();
                        ScreensManager.m_screens["GameLoading"] = new GameLoadingScreen();
                        ScreensManager.m_screens["Game"] = new GameScreen();
                        ScreensManager.m_screens["ExternalContent"] = new ExternalContentScreen();
                        ScreensManager.m_screens["CommunityContent"] = new CommunityContentScreen();
                        ScreensManager.m_screens["Content"] = new ContentScreen();
                        ScreensManager.m_screens["ManageContent"] = new ManageContentScreen();
                        ScreensManager.m_screens["Players"] = new PlayersScreen();
                        ScreensManager.m_screens["Player"] = new PlayerScreen();
                        ScreensManager.SwitchScreen("MainMenu");
                    }
                }));
            }
            if (m_uiSizeButton.IsClicked)
            {
                SettingsManager.GuiSize = (GuiSize)((int)(SettingsManager.GuiSize + 1) % EnumUtils.GetEnumValues(typeof(GuiSize)).Count);
            }
            if (m_upsideDownButton.IsClicked)
            {
                SettingsManager.UpsideDownLayout = !SettingsManager.UpsideDownLayout;
            }
            if (m_hideMoveLookPadsButton.IsClicked)
            {
                SettingsManager.HideMoveLookPads = !SettingsManager.HideMoveLookPads;
            }
            if (m_showGuiInScreenshotsButton.IsClicked)
            {
                SettingsManager.ShowGuiInScreenshots = !SettingsManager.ShowGuiInScreenshots;
            }
            if (m_showLogoInScreenshotsButton.IsClicked)
            {
                SettingsManager.ShowLogoInScreenshots = !SettingsManager.ShowLogoInScreenshots;
            }
            if (m_screenshotSizeButton.IsClicked)
            {
                SettingsManager.ScreenshotSize = (ScreenshotSize)((int)(SettingsManager.ScreenshotSize + 1) % EnumUtils.GetEnumValues(typeof(ScreenshotSize)).Count);
            }
            if (m_communityContentModeButton.IsClicked)
            {
                SettingsManager.CommunityContentMode = (CommunityContentMode)((int)(SettingsManager.CommunityContentMode + 1) % EnumUtils.GetEnumValues(typeof(CommunityContentMode)).Count);
            }
            m_windowModeButton.Text = LanguageControl.Get("WindowMode", SettingsManager.WindowMode.ToString());
            m_uiSizeButton.Text = LanguageControl.Get("GuiSize", SettingsManager.GuiSize.ToString());
            m_languageButton.Text = LanguageControl.Get("Language","Name");
            m_upsideDownButton.Text = (SettingsManager.UpsideDownLayout ? LanguageControl.Yes : LanguageControl.No);
            m_hideMoveLookPadsButton.Text = (SettingsManager.HideMoveLookPads ? LanguageControl.Yes : LanguageControl.No);
            m_showGuiInScreenshotsButton.Text = (SettingsManager.ShowGuiInScreenshots ? LanguageControl.Yes : LanguageControl.No);
            m_showLogoInScreenshotsButton.Text = (SettingsManager.ShowLogoInScreenshots ? LanguageControl.Yes : LanguageControl.No);
            m_screenshotSizeButton.Text = LanguageControl.Get("ScreenshotSize", SettingsManager.ScreenshotSize.ToString());
            m_communityContentModeButton.Text = LanguageControl.Get("CommunityContentMode", SettingsManager.CommunityContentMode.ToString());
            if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
            {
                ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
            }
        }
    }
}
