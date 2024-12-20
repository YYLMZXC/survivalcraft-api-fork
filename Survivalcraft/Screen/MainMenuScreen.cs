﻿using Engine;
using Engine.Input;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace Game
{
	public class MainMenuScreen : Screen
	{
		public string m_versionString = string.Empty;

		public bool m_versionStringTrial;

		public ButtonWidget m_showBulletinButton;

		public StackPanelWidget m_bulletinStackPanel;

		public LabelWidget m_copyrightLabel;

		public ButtonWidget m_languageSwitchButton;

		public MainMenuScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/MainMenuScreen");
			LoadContents(this, node);
			m_showBulletinButton = Children.Find<ButtonWidget>("BulletinButton");
			m_bulletinStackPanel = Children.Find<StackPanelWidget>("BulletinStackPanel");
			m_copyrightLabel = Children.Find<LabelWidget>("CopyrightLabel");
			m_languageSwitchButton = Children.Find<ButtonWidget>("LanguageSwitchButton");
			string languageType = ModsManager.Configs.GetValueOrDefault("Language", "zh-CN");
			m_bulletinStackPanel.IsVisible = languageType == "zh-CN";
			m_copyrightLabel.IsVisible = languageType != "zh-CN";
		}

		public override void Enter(object[] parameters)
		{
			MusicManager.CurrentMix = MusicManager.Mix.Menu;
			Children.Find<MotdWidget>().Restart();
			if (SettingsManager.IsolatedStorageMigrationCounter < 3)
			{
				SettingsManager.IsolatedStorageMigrationCounter++;
				VersionConverter126To127.MigrateDataFromIsolatedStorageWithDialog();
			}
			if (MotdManager.CanShowBulletin) MotdManager.ShowBulletin();
		}

		public override void Leave()
		{
			Keyboard.BackButtonQuitsApp = false;
		}

		public override void Update()
		{
			Keyboard.BackButtonQuitsApp = !MarketplaceManager.IsTrialMode;
			if (string.IsNullOrEmpty(m_versionString) || MarketplaceManager.IsTrialMode != m_versionStringTrial)
			{
				m_versionString = string.Format("Version {0}{1}", VersionsManager.Version, MarketplaceManager.IsTrialMode ? " (Day One)" : string.Empty);
				m_versionStringTrial = MarketplaceManager.IsTrialMode;
			}
			Children.Find("Buy").IsVisible = MarketplaceManager.IsTrialMode;
			Children.Find<LabelWidget>("Version").Text = m_versionString + "  API" + ModsManager.ApiVersionString;
			RectangleWidget rectangleWidget = Children.Find<RectangleWidget>("Logo");
			float num = 1f + (0.02f * MathF.Sin(1.5f * (float)MathUtils.Remainder(Time.FrameStartTime, 10000.0)));
			rectangleWidget.RenderTransform = Matrix.CreateTranslation((0f - rectangleWidget.ActualSize.X) / 2f, (0f - rectangleWidget.ActualSize.Y) / 2f, 0f) * Matrix.CreateScale(num, num, 1f) * Matrix.CreateTranslation(rectangleWidget.ActualSize.X / 2f, rectangleWidget.ActualSize.Y / 2f, 0f);
			if (m_languageSwitchButton.IsClicked)
			{
				DialogsManager.ShowDialog(null,new ListSelectionDialog(null,LanguageControl.LanguageTypes,70f,(object item) => ((KeyValuePair<string, CultureInfo>)item).Value.NativeName,delegate (object item)
				{
					LanguageControl.ChangeLanguage(((KeyValuePair<string, CultureInfo>)item).Key);
				}));
			}
			if (Children.Find<ButtonWidget>("Play").IsClicked)
			{
				ScreensManager.SwitchScreen("Play");
			}
			if (Children.Find<ButtonWidget>("Help").IsClicked)
			{
				ScreensManager.SwitchScreen("Help");
			}
			if (Children.Find<ButtonWidget>("Content").IsClicked)
			{
				ScreensManager.SwitchScreen("Content");
			}
			if (Children.Find<ButtonWidget>("Settings").IsClicked)
			{
				ScreensManager.SwitchScreen("Settings");
			}
			if (Children.Find<ButtonWidget>("Buy").IsClicked)
			{
				MarketplaceManager.ShowMarketplace();
			}
			if(Children.Find<BevelledButtonWidget>("Manage").IsClicked)
			{
				ScreensManager.m_screens.TryGetValue("Content",out Screen screen);
				ContentScreen contentScreen = screen as ContentScreen;
				contentScreen.OpenManageSelectDialog();
			}
			if (m_showBulletinButton.IsClicked)
			{
				if (MotdManager.m_bulletin != null && MotdManager.m_bulletin.Title.ToLower() != "null")
				{
					MotdManager.ShowBulletin();
				}
				else
				{
					DialogsManager.ShowDialog(null, new MessageDialog("公告获取失败", "当前暂无发布公告，\n或者没有联网获取公告信息", LanguageControl.Ok, null, null));
				}
			}
			if ((Input.Back && !Keyboard.BackButtonQuitsApp) || Input.IsKeyDownOnce(Key.Escape))
			{
				if (MarketplaceManager.IsTrialMode)
				{
					ScreensManager.SwitchScreen("Nag");
				}
				else
				{
					Window.Close();
				}
			}
			if (!String.IsNullOrEmpty(ExternalContentManager.openFilePath))
			{
				ScreensManager.SwitchScreen("ExternalContent");
			}
		}
	}
}
