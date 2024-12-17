using System.Collections.Generic;
using System.Xml.Linq;
using TemplatesDatabase;

namespace Game
{
	public class ModifyWorldScreen : Screen
	{
		public TextBoxWidget m_nameTextBox;

		public LabelWidget m_seedLabel;

		public ButtonWidget m_gameModeButton;

		public ButtonWidget m_worldOptionsButton;

		public LabelWidget m_errorLabel;

		public LabelWidget m_descriptionLabel;

		public ButtonWidget m_applyButton;

		public ButtonWidget m_deleteButton;

		public ButtonWidget m_uploadButton;

		public string m_directoryName;

		public WorldSettings m_worldSettings;

		private WorldSettings m_originalWorldSettings;

		public ValuesDictionary m_worldSettingsData = [];

		public ValuesDictionary m_originalWorldSettingsData = [];

		public bool m_changingGameModeAllowed;

		public ModifyWorldScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/ModifyWorldScreen");
			LoadContents(this, node);
			m_nameTextBox = Children.Find<TextBoxWidget>("Name");
			m_seedLabel = Children.Find<LabelWidget>("Seed");
			m_gameModeButton = Children.Find<ButtonWidget>("GameMode");
			m_worldOptionsButton = Children.Find<ButtonWidget>("WorldOptions");
			m_errorLabel = Children.Find<LabelWidget>("Error");
			m_descriptionLabel = Children.Find<LabelWidget>("Description");
			m_applyButton = Children.Find<ButtonWidget>("Apply");
			m_deleteButton = Children.Find<ButtonWidget>("Delete");
			m_uploadButton = Children.Find<ButtonWidget>("Upload");
			m_nameTextBox.TextChanged += delegate
			{
				m_worldSettings.Name = m_nameTextBox.Text;
			};
			m_nameTextBox.MaximumLength = 128;
		}

		public override void Enter(object[] parameters)
		{
			if (ScreensManager.PreviousScreen.GetType() != typeof(WorldOptionsScreen))
			{
				m_directoryName = (string)parameters[0];
				m_worldSettings = (WorldSettings)parameters[1];
				m_originalWorldSettingsData.Clear();
				m_worldSettings.Save(m_originalWorldSettingsData, liveModifiableParametersOnly: true);
				m_originalWorldSettings = new WorldSettings();
				m_originalWorldSettings.Load(m_originalWorldSettingsData);
				m_changingGameModeAllowed = m_worldSettings.GameMode != GameMode.Cruel;
			}
		}

		public override void Update()
		{
			if (m_gameModeButton.IsClicked && m_changingGameModeAllowed)
			{
				DialogsManager.ShowDialog(null, new SelectGameModeDialog(string.Empty, allowAdventure: true, allowCruel: false, delegate (GameMode gameMode)
				{
					m_worldSettings.GameMode = gameMode;
				}));
			}
			m_worldSettingsData.Clear();
			m_worldSettings.Save(m_worldSettingsData, liveModifiableParametersOnly: true);
			bool flag = !CompareValueDictionaries(m_originalWorldSettingsData, m_worldSettingsData);
			bool flag2 = WorldsManager.ValidateWorldName(m_worldSettings.Name);
			m_nameTextBox.Text = m_worldSettings.Name;
			m_seedLabel.Text = m_worldSettings.Seed;
			m_gameModeButton.Text = LanguageControl.Get("GameMode", m_worldSettings.GameMode.ToString());
			m_gameModeButton.IsEnabled = m_changingGameModeAllowed;
			m_errorLabel.IsVisible = !flag2;
			m_descriptionLabel.IsVisible = flag2;
			m_uploadButton.IsEnabled = flag2 && !flag;
			m_applyButton.IsEnabled = flag2 && flag;
			m_descriptionLabel.Text = StringsManager.GetString("GameMode." + m_worldSettings.GameMode.ToString() + ".Description");
			if (m_worldOptionsButton.IsClicked)
			{
				ScreensManager.SwitchScreen("WorldOptions", m_worldSettings, true);
			}
			if (m_deleteButton.IsClicked)
			{
				Dialog dialog = null;
				if(SettingsManager.DeleteWorldNeedToText)
				{
					TextBoxDialog textBoxDialog;
					textBoxDialog = new TextBoxDialog(LanguageControl.Get(GetType().Name, 1) + LanguageControl.Get(GetType().Name, 5), string.Empty, 3, delegate (string content)
                    {
                        if (content?.ToLower() == "yes")
                        {
                            WorldsManager.DeleteWorld(m_directoryName);
                            ScreensManager.SwitchScreen("Play");
                            DialogsManager.HideDialog(dialog);
                        }
                        else
                        {
                            DialogsManager.HideDialog(dialog);
                        }
                    });
                    dialog = textBoxDialog;
                    textBoxDialog.Children.Find<LabelWidget>("TextBoxDialog.Title").Color = Color.Red;
                    textBoxDialog.AutoHide = false;
                }
				else
				{
                    dialog = new MessageDialog(LanguageControl.Get(GetType().Name, 1), LanguageControl.Get(GetType().Name, 2), LanguageControl.Yes, LanguageControl.No, delegate (MessageDialogButton button)
                    {
                        if (button == MessageDialogButton.Button1)
                        {
                            WorldsManager.DeleteWorld(m_directoryName);
                            ScreensManager.SwitchScreen("Play");
                            DialogsManager.HideDialog(dialog);
                        }
                        else
                        {
                            DialogsManager.HideDialog(dialog);
                        }
                    });
                }

				DialogsManager.ShowDialog(null, dialog);
			}
			if (m_uploadButton.IsClicked && flag2 && !flag)
			{
				ExternalContentManager.ShowUploadUi(ExternalContentType.World, m_directoryName);
			}
			if (m_applyButton.IsClicked && flag2 && flag)
			{
				if (m_worldSettings.GameMode != 0 && m_worldSettings.GameMode != GameMode.Adventure)
				{
					m_worldSettings.ResetOptionsForNonCreativeMode(null);
				}
				WorldsManager.ChangeWorld(m_directoryName, m_worldSettings);
				ScreensManager.SwitchScreen("Play");
			}
			if (!base.Input.Back && !base.Input.Cancel && !Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				return;
			}
			if (flag)
			{
				DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 3), LanguageControl.Get(GetType().Name, 4), LanguageControl.Yes, LanguageControl.No, delegate (MessageDialogButton button)
				{
					if (button == MessageDialogButton.Button1)
					{
						ScreensManager.SwitchScreen("Play");
					}
				}));
			}
			else
			{
				ScreensManager.SwitchScreen("Play");
			}
		}

		public static bool CompareValueDictionaries(ValuesDictionary d1, ValuesDictionary d2)
		{
			if (d1.Count != d2.Count)
			{
				return false;
			}
			foreach (KeyValuePair<string, object> item in d1)
			{
				object value = d2.GetValue<object>(item.Key, null);
				var valuesDictionary = value as ValuesDictionary;
				if (valuesDictionary != null)
				{
					var valuesDictionary2 = item.Value as ValuesDictionary;
					if (valuesDictionary2 == null || !CompareValueDictionaries(valuesDictionary, valuesDictionary2))
					{
						return false;
					}
				}
				else if (!Equals(value, item.Value))
				{
					return false;
				}
			}
			return true;
		}
	}
}
