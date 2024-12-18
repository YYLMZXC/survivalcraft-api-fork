﻿using Engine;
using Engine.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using XmlUtilities;
namespace Game
{
	public static class SettingsManager
	{
		public static float m_soundsVolume;

		public static float m_musicVolume;

		public static float m_brightness;

		public static ResolutionMode m_resolutionMode;

		public static WindowMode m_windowMode;

		public static Point2 m_resizableWindowPosition;

		public static Point2 m_resizableWindowSize;

		public static bool UsePrimaryMemoryBank { get; set; }

		public static bool AllowInitialIntro { get; set; }
		public static bool DeleteWorldNeedToText { get; set; }

		public static bool CreativeDragMaxStacking { get; set; }

		public static float Touchoffset { get; set; }

		public static float SoundsVolume
		{
			get
			{
				return m_soundsVolume;
			}
			set
			{
				m_soundsVolume = MathUtils.Saturate(value);
			}
		}

		public static float MusicVolume
		{
			get
			{
				return m_musicVolume;
			}
			set
			{
				m_musicVolume = MathUtils.Saturate(value);
			}
		}

		public static int VisibilityRange
		{
			get;
			set;
		}

		public static bool UseVr
		{
			get;
			set;
		}

		public static float UIScale { get; set; }

		public static ResolutionMode ResolutionMode
		{
			get
			{
				return m_resolutionMode;
			}
			set
			{
				if (value != m_resolutionMode)
				{
					m_resolutionMode = value;
					SettingChanged?.Invoke("ResolutionMode");
				}
			}
		}

		public static float ViewAngle
		{
			get;
			set;
		}

		public static SkyRenderingMode SkyRenderingMode
		{
			get;
			set;
		}

		public static bool TerrainMipmapsEnabled
		{
			get;
			set;
		}

		public static bool ObjectsShadowsEnabled
		{
			get;
			set;
		}

		public static float Brightness
		{
			get
			{
				return m_brightness;
			}
			set
			{
				value = Math.Clamp(value, 0f, 1f);
				if (value != m_brightness)
				{
					m_brightness = value;
					SettingChanged?.Invoke("Brightness");
				}
			}
		}

		public static int PresentationInterval
		{
			get;
			set;
		}

		public static bool ShowGuiInScreenshots
		{
			get;
			set;
		}

		public static bool ShowLogoInScreenshots
		{
			get;
			set;
		}

		public static ScreenshotSize ScreenshotSize
		{
			get;
			set;
		}

		public static WindowMode WindowMode
		{
			get
			{
				return m_windowMode;
			}
			set
			{
				if (value != m_windowMode)
				{
					if (value == WindowMode.Borderless)
					{
						m_resizableWindowSize = Window.Size;
						Window.Position = Point2.Zero;
						Window.Size = Window.ScreenSize;
					}
					else if (value == WindowMode.Resizable)
					{
						Window.Position = m_resizableWindowPosition;
						Window.Size = m_resizableWindowSize;
					}
					Window.WindowMode = value;
					m_windowMode = value;
				}
				
				ModsManager.HookAction("WindowModeChanged", loader =>
				{
					loader.WindowModeChanged(value);
					return false;
				});
			}
		}
		#region 简单设置项
		public static GuiSize GuiSize
		{
			get;
			set;
		}

		public static bool HideMoveLookPads
		{
			get;
			set;
		}

		public static string BlocksTextureFileName
		{
			get;
			set;
		}

		public static MoveControlMode MoveControlMode
		{
			get;
			set;
		}

		public static LookControlMode LookControlMode
		{
			get;
			set;
		}

		public static bool LeftHandedLayout
		{
			get;
			set;
		}

		public static bool FlipVerticalAxis
		{
			get;
			set;
		}

		public static float MoveSensitivity
		{
			get;
			set;
		}

		public static float LookSensitivity
		{
			get;
			set;
		}

		public static float GamepadDeadZone
		{
			get;
			set;
		}

		public static float GamepadCursorSpeed
		{
			get;
			set;
		}

		public static float CreativeDigTime
		{
			get;
			set;
		}

		public static float CreativeReach
		{
			get;
			set;
		}

		public static float MinimumHoldDuration
		{
			get;
			set;
		}

		public static float MinimumDragDistance
		{
			get;
			set;
		}

		public static bool AutoJump
		{
			get;
			set;
		}

		public static bool HorizontalCreativeFlight
		{
			get;
			set;
		}

		public static string DropboxAccessToken
		{
			get;
			set;
		}

		public static string MotdUpdateUrl
		{
			get;
			set;
		}

		public static string MotdUpdateCheckUrl
		{
			get;
			set;
		}
		public static string ScpboxAccessToken
		{
			get;
			set;
		}

		public static string ScpboxUserInfo
		{
			get;
			set;
		}

		public static bool MotdUseBackupUrl
		{
			get;
			set;
		}

		public static double MotdUpdatePeriodHours
		{
			get;
			set;
		}

		public static DateTime MotdLastUpdateTime
		{
			get;
			set;
		}

		public static string MotdLastDownloadedData
		{
			get;
			set;
		}

		public static string UserId
		{
			get;
			set;
		}

		public static string LastLaunchedVersion
		{
			get;
			set;
		}

		public static CommunityContentMode CommunityContentMode
		{
			get;
			set;
		}

		public static bool MultithreadedTerrainUpdate
		{
			get;
			set;
		}

		public static int IsolatedStorageMigrationCounter
		{
			get;
			set;
		}

		public static bool DisplayFpsCounter
		{
			get;
			set;
		}

		public static bool DisplayFpsRibbon
		{
			get;
			set;
		}

		public static int NewYearCelebrationLastYear
		{
			get;
			set;
		}

		public static ScreenLayout ScreenLayout1
		{
			get;
			set;
		}

		public static ScreenLayout ScreenLayout2
		{
			get;
			set;
		}

		public static ScreenLayout ScreenLayout3
		{
			get;
			set;
		}

		public static ScreenLayout ScreenLayout4
		{
			get;
			set;
		}

		public static bool UpsideDownLayout
		{
			get;
			set;
		}
		#endregion
		public static bool FullScreenMode
		{
			get
			{
				return Window.WindowMode == WindowMode.Fullscreen;
			}
			set
			{
				Window.WindowMode = value ? WindowMode.Fullscreen : WindowMode.Resizable;
				ModsManager.HookAction("WindowModeChanged", loader =>
				{
					loader.WindowModeChanged(WindowMode);
					return false;
				});
			}
		}

		public static bool DisplayLog
		{
			get;
			set;
		}

		public static string BulletinTime
		{
			get;
			set;
		}

		public static bool DragHalfInSplit
		{
			get;
			set;
		}

		public static float LowFPSToTimeDeceleration
		{
			get;
			set;
		}

		public static bool UseAPISleepTimeAcceleration
		{
			get;
			set;
		}

        public static event Action<string> SettingChanged;

		public static void Initialize()
		{
			if(!LoadSettings())
			{
				DisplayLog = false;
				DragHalfInSplit = true;
				m_resolutionMode = ResolutionMode.High;
				VisibilityRange = 128;
				ViewAngle = 1f;
				TerrainMipmapsEnabled = true;
				SkyRenderingMode = SkyRenderingMode.Full;
				ObjectsShadowsEnabled = true;
				PresentationInterval = 1;
				m_soundsVolume = 1.0f;
				m_musicVolume = 0.2f;
				m_brightness = 0.8f;
				ShowGuiInScreenshots = false;
				ShowLogoInScreenshots = true;
				ScreenshotSize = ScreenshotSize.ScreenSize;
				MoveControlMode = MoveControlMode.Buttons;
				HideMoveLookPads = false;
				AllowInitialIntro = true;
				DeleteWorldNeedToText = false;
				BlocksTextureFileName = string.Empty;
				LookControlMode = LookControlMode.EntireScreen;
				FlipVerticalAxis = false;
#if ANDROID
			UIScale = 0.9f;
			AutoJump = true;
#else
				UIScale = 0.75f;
				AutoJump = false;
#endif
				MoveSensitivity = 0.5f;
				LookSensitivity = 0.5f;
				GamepadDeadZone = 0.16f;
				GamepadCursorSpeed = 1f;
				CreativeDigTime = 0.1f;
				CreativeReach = 7.5f;
				MinimumHoldDuration = 0.15f;
				MinimumDragDistance = 10f;
				HorizontalCreativeFlight = false;
				DropboxAccessToken = string.Empty;
				ScpboxAccessToken = string.Empty;
				MotdUpdateUrl = "https://m.schub.top/com/motd?v={0}&l={1}";
				MotdUpdateCheckUrl = "https://m.schub.top/com/motd?v={0}&cmd=version_check&platform={1}&apiv={2}&l={3}";
				MotdUpdatePeriodHours = 12.0;
				MotdLastUpdateTime = DateTime.MinValue;
				MotdLastDownloadedData = string.Empty;
				UserId = string.Empty;
				LastLaunchedVersion = string.Empty;
				CommunityContentMode = CommunityContentMode.Normal;
				MultithreadedTerrainUpdate = true;
				NewYearCelebrationLastYear = 2035;
				ScreenLayout1 = ScreenLayout.Single;
				ScreenLayout2 = (Window.ScreenSize.X / (float)Window.ScreenSize.Y > 1.33333337f) ? ScreenLayout.DoubleVertical : ScreenLayout.DoubleHorizontal;
				ScreenLayout3 = (Window.ScreenSize.X / (float)Window.ScreenSize.Y > 1.33333337f) ? ScreenLayout.TripleVertical : ScreenLayout.TripleHorizontal;
				ScreenLayout4 = ScreenLayout.Quadruple;
				BulletinTime = string.Empty;
				ScpboxUserInfo = string.Empty;
				HorizontalCreativeFlight = true;
				CreativeDragMaxStacking = true;
				LowFPSToTimeDeceleration = 10;
				UseAPISleepTimeAcceleration = false;
			}
			Window.Deactivated += delegate
			{
				SaveSettings();
			};
		}
		/// <summary>
		/// 文件存在则读取并返回真否则返回假
		/// </summary>
		public static bool LoadSettings()
		{
			try
			{
				if (Storage.FileExists(ModsManager.SettingPath))
				{
					using (Stream stream = Storage.OpenFile(ModsManager.SettingPath, OpenFileMode.Read))
					{
						ModsManager.DisabledMods.Clear();
						XElement xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
						ModsManager.LoadSettings(xElement);
						foreach (XElement item in xElement.Elements())
						{
							string name = "<unknown>";
							try
							{
								if (item.Name.LocalName == "Setting")
								{
									name = XmlUtils.GetAttributeValue<string>(item, "Name");
									string attributeValue = XmlUtils.GetAttributeValue<string>(item, "Value");
									PropertyInfo propertyInfo = (from pi in typeof(SettingsManager).GetRuntimeProperties()
																 where pi.Name == name && pi.GetMethod.IsStatic && pi.GetMethod.IsPublic && pi.SetMethod.IsPublic
																 select pi).FirstOrDefault();
									if (propertyInfo is not null)
									{
										object value = HumanReadableConverter.ConvertFromString(propertyInfo.PropertyType, attributeValue);
										propertyInfo.SetValue(null, value, null);
									}

								}
								else if (item.Name.LocalName == "DisableMods")
								{
									foreach (XElement xElement1 in item.Elements())
									{
										var modInfo = new ModInfo();
										modInfo.PackageName = xElement1.Attribute("PackageName").Value;
										modInfo.Version = xElement1.Attribute("Version").Value;
										ModsManager.DisabledMods.Add(modInfo);
									}
								}
							}
							catch (Exception ex)
							{
								Log.Warning(string.Format("Setting \"{0}\" could not be loaded. Reason: {1}", new object[2]
								{
									name,
									ex.Message
								}));
							}
						}

					}
					Log.Information("Loaded settings.");
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception e)
			{
				ExceptionManager.ReportExceptionToUser("Loading settings failed.", e);
				return false;
			}
		}

		public static void SaveSettings()
		{
			try
			{
				var xElement = new XElement("Settings");
				foreach (PropertyInfo item in from pi in typeof(SettingsManager).GetRuntimeProperties()
											  where pi.GetMethod.IsStatic && pi.GetMethod.IsPublic && pi.SetMethod.IsPublic
											  select pi)
				{
					try
					{
						string value = HumanReadableConverter.ConvertToString(item.GetValue(null, null));
						XElement node = XmlUtils.AddElement(xElement, "Setting");
						XmlUtils.SetAttributeValue(node, "Name", item.Name);
						XmlUtils.SetAttributeValue(node, "Value", value);
					}
					catch (Exception ex)
					{
						Log.Warning(string.Format("Setting \"{0}\" could not be saved. Reason: {1}",
						[
							item.Name,
							ex.Message
						]));
					}
				}
				var xElement1 = new XElement("DisableMods");
				var xElement2 = new XElement("ModSettings");
				foreach (ModEntity modEntity in ModsManager.ModListAll)
				{
					if (ModsManager.DisabledMods.Contains(modEntity.modInfo))
					{
						var element = new XElement("Mod");
						element.SetAttributeValue("PackageName", modEntity.modInfo.PackageName);
						element.SetAttributeValue("Version", modEntity.modInfo.Version);
						xElement1.Add(element);
					}
				}
				xElement.Add(xElement1);
				ModsManager.SaveSettings(xElement);
				ModsManager.SaveModSettings(xElement2);
				using (Stream stream = Storage.OpenFile(ModsManager.SettingPath, OpenFileMode.Create))
				{
					XmlUtils.SaveXmlToStream(xElement, stream,Encoding.UTF8, throwOnError: true);
				}
				using (Stream stream = Storage.OpenFile(ModsManager.ModsSetPath, OpenFileMode.Create))
				{
					XmlUtils.SaveXmlToStream(xElement2,stream,Encoding.UTF8,throwOnError: true);
				}
				Log.Information("Saved settings");
			}
			catch (Exception e)
			{
				ExceptionManager.ReportExceptionToUser("Saving settings failed.", e);
			}
		}
	}
}
