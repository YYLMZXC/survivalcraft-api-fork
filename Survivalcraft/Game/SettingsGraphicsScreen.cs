using System.Xml.Linq;
using Engine;

namespace Game
{
	public class SettingsGraphicsScreen : Screen
	{
		private BevelledButtonWidget m_virtualRealityButton;

		private SliderWidget m_brightnessSlider;

		private ContainerWidget m_vrPanel;

		public SettingsGraphicsScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/SettingsGraphicsScreen");
			LoadContents(this, node);
			m_virtualRealityButton = Children.Find<BevelledButtonWidget>("VirtualRealityButton");
			m_brightnessSlider = Children.Find<SliderWidget>("BrightnessSlider");
			m_vrPanel = Children.Find<ContainerWidget>("VrPanel");
			m_vrPanel.IsVisible = false;
		}

		public override void Update()
		{
			if (m_virtualRealityButton.IsClicked)
			{
				if (SettingsManager.UseVr)
				{
					SettingsManager.UseVr = false;
					VrManager.StopVr();
				}
				else
				{
					SettingsManager.UseVr = true;
					VrManager.StartVr();
				}
			}
			if (m_brightnessSlider.IsSliding)
			{
				SettingsManager.Brightness = m_brightnessSlider.Value;
			}
			m_virtualRealityButton.IsEnabled = VrManager.IsVrAvailable;
			m_virtualRealityButton.Text = (SettingsManager.UseVr ? "Enabled" : "Disabled");
			m_brightnessSlider.Value = SettingsManager.Brightness;
			m_brightnessSlider.Text = MathUtils.Round(SettingsManager.Brightness * 10f).ToString();
			if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}
	}
}
