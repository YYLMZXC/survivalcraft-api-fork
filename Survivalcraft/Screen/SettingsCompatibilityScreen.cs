using System.Xml.Linq;

namespace Game
{
	public class SettingsCompatibilityScreen : Screen
	{
		public ButtonWidget m_singlethreadedTerrainUpdateButton;

		public ButtonWidget m_useAudioTrackCachingButton;

		public ContainerWidget m_disableAudioTrackCachingContainer;

		public ButtonWidget m_useReducedZRangeButton;

		public ContainerWidget m_useReducedZRangeContainer;

		public ButtonWidget m_viewGameLogButton;

		public ButtonWidget m_resetDefaultsButton;

		public LabelWidget m_descriptionLabel;

		public ButtonWidget m_使用内置路径;

		public SettingsCompatibilityScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/SettingsCompatibilityScreen");
			LoadContents(this, node);
			m_singlethreadedTerrainUpdateButton = Children.Find<ButtonWidget>("SinglethreadedTerrainUpdateButton");
			m_useAudioTrackCachingButton = Children.Find<ButtonWidget>("UseAudioTrackCachingButton");
			m_disableAudioTrackCachingContainer = Children.Find<ContainerWidget>("DisableAudioTrackCachingContainer");
			m_useReducedZRangeButton = Children.Find<ButtonWidget>("UseReducedZRangeButton");
			m_useReducedZRangeContainer = Children.Find<ContainerWidget>("UseReducedZRangeContainer");
			m_viewGameLogButton = Children.Find<ButtonWidget>("ViewGameLogButton");
			m_resetDefaultsButton = Children.Find<ButtonWidget>("ResetDefaultsButton");
			m_descriptionLabel = Children.Find<LabelWidget>("Description");
			m_使用内置路径 = Children.Find<ButtonWidget>("使用内置路径");
		}

		public override void Enter(object[] parameters)
		{
			m_descriptionLabel.Text = string.Empty;
			m_disableAudioTrackCachingContainer.IsVisible = false;
			m_useAudioTrackCachingButton.IsVisible = false;
			m_useReducedZRangeContainer.IsVisible = false;
		}

		public override void Update()
		{
			//if (m_singlethreadedTerrainUpdateButton.IsClicked)
			//{
			//	SettingsManager.MultithreadedTerrainUpdate = !SettingsManager.MultithreadedTerrainUpdate;
			//	m_descriptionLabel.Text = StringsManager.GetString("Settings.Compatibility.SinglethreadedTerrainUpdate.Description");
			//}
			if (m_useReducedZRangeButton.IsClicked)
			{
				SettingsManager.UseReducedZRange = !SettingsManager.UseReducedZRange;
				m_descriptionLabel.Text = StringsManager.GetString("Settings.Compatibility.UseReducedZRange.Description");
			}
			if (m_useAudioTrackCachingButton.IsClicked)
			{
				SettingsManager.EnableAndroidAudioTrackCaching = !SettingsManager.EnableAndroidAudioTrackCaching;
				m_descriptionLabel.Text = StringsManager.GetString("Settings.Compatibility.UseAudioTrackCaching.Description");
			}

			if (m_viewGameLogButton.IsClicked)
			{
				DialogsManager.ShowDialog(null, new ViewGameLogDialog());
			}
			if (m_resetDefaultsButton.IsClicked)
			{
				SettingsManager.MultithreadedTerrainUpdate = true;
				SettingsManager.UseReducedZRange = false;
			}
			if (m_使用内置路径.IsClicked)
			{
				SettingsManager.使用内置路径 = !SettingsManager.使用内置路径;

			}
			m_singlethreadedTerrainUpdateButton.Text = "已弃用";
			m_useAudioTrackCachingButton.Text = SettingsManager.EnableAndroidAudioTrackCaching ? LanguageControl.On : LanguageControl.Off;
			m_useReducedZRangeButton.Text = SettingsManager.UseReducedZRange ? LanguageControl.On : LanguageControl.Off;
			m_使用内置路径.Text = SettingsManager.使用内置路径 ? LanguageControl.On : LanguageControl.Off;

			m_resetDefaultsButton.IsEnabled = !SettingsManager.MultithreadedTerrainUpdate || SettingsManager.UseReducedZRange;
			if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}
	}
}
