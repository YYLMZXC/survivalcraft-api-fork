using Engine;
using Engine.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using XmlUtilities;
using System.Collections.Generic;
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

        public static bool AllowInitialIntro { get; set; }

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

        public static ViewAngleMode ViewAngleMode
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
                value = MathUtils.Clamp(value, 0f, 1f);
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
            }
        }

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

        public static string MotdBackupUpdateUrl
        {
            get;
            set;
        }
        public static string ScpboxAccessToken { get; set; }
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

        public static bool EnableAndroidAudioTrackCaching
        {
            get;
            set;
        }

        public static bool UseReducedZRange
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

        public static bool FullScreenMode
        {
            get
            {
                return Window.WindowMode == WindowMode.Fullscreen;
            }
            set
            {
                Window.WindowMode = value ? WindowMode.Fullscreen : WindowMode.Resizable;
            }
        }

        public static event Action<string> SettingChanged;

        public static void Initialize()
        {
            VisibilityRange = 128;
            m_resolutionMode = ResolutionMode.High;
            ViewAngleMode = ViewAngleMode.Normal;
            SkyRenderingMode = SkyRenderingMode.Full;
            TerrainMipmapsEnabled = false;
            ObjectsShadowsEnabled = true;
            m_soundsVolume = 0.5f;
            m_musicVolume = 0.5f;
            m_brightness = 0.5f;
            PresentationInterval = 1;
            ShowGuiInScreenshots = false;
            ShowLogoInScreenshots = true;
            ScreenshotSize = ScreenshotSize.ScreenSize;
            MoveControlMode = MoveControlMode.Pad;
            HideMoveLookPads = false;
            AllowInitialIntro = true;
            BlocksTextureFileName = string.Empty;
            LookControlMode = LookControlMode.EntireScreen;
            FlipVerticalAxis = false;
#if android
            EnableAndroidAudioTrackCaching = true;
            GuiSize = GuiSize.Normal;
#endif
#if desktop
            GuiSize = GuiSize.Smallest;
            EnableAndroidAudioTrackCaching = false;
#endif
            MoveSensitivity = 0.5f;
            LookSensitivity = 0.5f;
            GamepadDeadZone = 0.16f;
            GamepadCursorSpeed = 1f;
            CreativeDigTime = 0.2f;
            CreativeReach = 7.5f;
            MinimumHoldDuration = 0.5f;
            MinimumDragDistance = 10f;
            AutoJump = true;
            HorizontalCreativeFlight = false;
            DropboxAccessToken = string.Empty;
            ScpboxAccessToken = string.Empty;
            MotdUpdateUrl = "https://m.schub.top/com/motd?v={0}&l={1}";
            MotdBackupUpdateUrl = "https://m.schub.top/com/motd?v={0}&l={1}";
            MotdUpdatePeriodHours = 12.0;
            MotdLastUpdateTime = DateTime.MinValue;
            MotdLastDownloadedData = string.Empty;
            UserId = string.Empty;
            LastLaunchedVersion = string.Empty;
            CommunityContentMode = CommunityContentMode.Normal;
            MultithreadedTerrainUpdate = true;
            NewYearCelebrationLastYear = 2015;
            ScreenLayout1 = ScreenLayout.Single;
            ScreenLayout2 = ((Window.ScreenSize.X / (float)Window.ScreenSize.Y > 1.33333337f) ? ScreenLayout.DoubleVertical : ScreenLayout.DoubleHorizontal);
            ScreenLayout3 = ((Window.ScreenSize.X / (float)Window.ScreenSize.Y > 1.33333337f) ? ScreenLayout.TripleVertical : ScreenLayout.TripleHorizontal);
            ScreenLayout4 = ScreenLayout.Quadruple;
            
            HorizontalCreativeFlight = true;
            TerrainMipmapsEnabled = true;
            LoadSettings();
            VersionsManager.CompareVersions(LastLaunchedVersion, "1.29");
            _ = 0;
            if (VersionsManager.CompareVersions(LastLaunchedVersion, "2.1") < 0)
            {
                MinimumDragDistance = 10f;
            }
            if (VersionsManager.CompareVersions(LastLaunchedVersion, "2.2") < 0)
            {
                if (Utilities.GetTotalAvailableMemory() < 524288000)
                {
                    VisibilityRange = MathUtils.Min(64, VisibilityRange);
                }
                else if (Utilities.GetTotalAvailableMemory() < 1048576000)
                {
                    VisibilityRange = MathUtils.Min(112, VisibilityRange);
                }
            }
            Window.Deactivated += delegate
            {
                SaveSettings();
            };
        }

        public static void LoadSettings()
        {
            try
            {
                if (Storage.FileExists(ModsManager.settingPath))
                {
                    using (Stream stream = Storage.OpenFile(ModsManager.settingPath, OpenFileMode.Read))
                    {
                        ModsManager.DisabledMods.Clear();
                        XElement xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
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
                                    if ((object)propertyInfo != null)
                                    {
                                        object value = HumanReadableConverter.ConvertFromString(propertyInfo.PropertyType, attributeValue);
                                        propertyInfo.SetValue(null, value, null);
                                    }

                                }
                                else if (item.Name.LocalName == "ModSet" && item.Attribute("Name").Value == "Language")
                                {
                                    ModsManager.modSettings.languageType = (LanguageControl.LanguageType)int.Parse(item.Attribute("Value").Value);
                                }
                                else if(item.Name.LocalName == "DisableMods")
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
                }
            }
            catch (Exception e)
            {
                ExceptionManager.ReportExceptionToUser("Loading settings failed.", e);
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
                        Log.Warning(string.Format("Setting \"{0}\" could not be saved. Reason: {1}", new object[2]
                        {
                            item.Name,
                            ex.Message
                        }));
                    }
                }
                var la = new XElement("ModSet");
                la.SetAttributeValue("Name", "Language");
                la.SetAttributeValue("Value", (int)ModsManager.modSettings.languageType);
                var xElement1 = new XElement("DisableMods");
                foreach (ModEntity modEntity in ModsManager.ModList)
                {
                    var element = new XElement("Mod");
                    element.SetAttributeValue("PackageName", modEntity.modInfo.PackageName);
                    element.SetAttributeValue("Version", modEntity.modInfo.Version);
                    xElement1.Add(element);
                }
                xElement.Add(xElement1);
                ModsManager.SaveSettings(xElement);
                using (Stream stream = Storage.OpenFile(ModsManager.settingPath, OpenFileMode.Create))
                {
                    XmlUtils.SaveXmlToStream(xElement, stream, null, throwOnError: true);
                }
                using (Stream stream = Storage.OpenFile(ModsManager.ModsSetPath, OpenFileMode.Create))
                {
                    var xElement2 = new XElement("ModSettings");
                    ModsManager.SaveSettings(xElement2);
                    XmlUtils.SaveXmlToStream(xElement2, stream, null, throwOnError: true);
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
