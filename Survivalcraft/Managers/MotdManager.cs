using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
    public static class MotdManager
    {
        public class Message
        {
            public List<Line> Lines = new List<Line>();
        }

        public class Line
        {
            public float Time;

            public XElement Node;

            public string Text;
        }

        public class Bulletin
        {
            public string Title = string.Empty;

            public string EnTitle = string.Empty;

            public string Time = string.Empty;

            public string Content = string.Empty;

            public string EnContent = string.Empty;
        }

        public class FilterMod
        {
            public string Name = string.Empty;

            public string PackageName = string.Empty;

            public string Version = string.Empty;

            public string FilterAPIVersion = string.Empty;

            public string Explanation = string.Empty;
        }

        public static Bulletin m_bulletin;

        public static bool CanShowBulletin = false;

        public static bool CanDownloadMotd = true;

        public static List<FilterMod> FilterModAll = new List<FilterMod>();

        public static Message m_message;

        public static SimpleJson.JsonObject UpdateResult = null;

        public static Message MessageOfTheDay
        {
            get
            {
                return m_message;
            }
            set
            {
                m_message = value;
                MessageOfTheDayUpdated?.Invoke();
            }
        }

        public static event Action MessageOfTheDayUpdated;

        public static void ForceRedownload()
        {
            SettingsManager.MotdLastUpdateTime = DateTime.MinValue;
        }

        public static void Initialize()
        {
            if (VersionsManager.Version != VersionsManager.LastLaunchedVersion)
            {
                ForceRedownload();
            }
        }

        public static void UpdateVersion() 
        {
            string url = string.Format(SettingsManager.MotdUpdateCheckUrl, VersionsManager.SerializationVersion, VersionsManager.Platform, ModsManager.APIVersion, LanguageControl.LName());
            WebManager.Get(url, null, null, new CancellableProgress(), data => {
                UpdateResult = SimpleJson.SimpleJson.DeserializeObject<SimpleJson.JsonObject>(System.Text.Encoding.UTF8.GetString(data));
            }, ex => {
                Log.Error("Failed processing Update check. Reason: " + ex.Message);
            });
        }

        public static void DownloadMotd()
        {
            string url = GetMotdUrl();
            WebManager.Get(url, null, null, null, delegate (byte[] result)
            {
                try
                {
                    string motdLastDownloadedData = UnpackMotd(result);
                    MessageOfTheDay = null;
                    SettingsManager.MotdLastDownloadedData = motdLastDownloadedData;
                    Log.Information("Downloaded MOTD");
                }
                catch (Exception ex)
                {
                    Log.Error("Failed processing MOTD string. Reason: " + ex.Message);
                }
            }, delegate (Exception error)
            {
                Log.Error("Failed downloading MOTD. Reason: {0}", error.Message);
            });
        }

        public static void Update()
        {
            //if (Time.PeriodicEvent(1.0, 0.0) && ModsManager.ConfigLoaded)
            //{
                //var t = TimeSpan.FromHours(SettingsManager.MotdUpdatePeriodHours);
                //DateTime now = DateTime.Now;
                //if (now >= SettingsManager.MotdLastUpdateTime + t)
                //{
                //    SettingsManager.MotdLastUpdateTime = now;
                //    DownloadMotd();
                //    UpdateVersion();
                //}
            //}
            if (CanDownloadMotd)
            {
                DownloadMotd();
                CanDownloadMotd = false;
            }
            if (MessageOfTheDay == null && !string.IsNullOrEmpty(SettingsManager.MotdLastDownloadedData))
            {
                MessageOfTheDay = ParseMotd(SettingsManager.MotdLastDownloadedData);
                if (MessageOfTheDay == null)
                {
                    SettingsManager.MotdLastDownloadedData = string.Empty;
                }
                if (m_bulletin != null && SettingsManager.BulletinTime != m_bulletin.Time)
                {
                    if (IsCNLanguageType() && m_bulletin.Title.ToLower() != "null")
                    {
                        CanShowBulletin = true;
                    }
                    else if (!IsCNLanguageType() && m_bulletin.EnTitle.ToLower() != "null")
                    {
                        CanShowBulletin = true;
                    }
                }
            }
        }

        public static string UnpackMotd(byte[] data)
        {
            using (var stream = new MemoryStream(data))
                return new StreamReader(stream).ReadToEnd();
            throw new InvalidOperationException($"\"motd.xml\" file not found in Motd zip archive.");
        }

        public static Message ParseMotd(string dataString)
        {
            try
            {
                int num = dataString.IndexOf("<Motd");
                if (num < 0)
                {
                    throw new InvalidOperationException("Invalid MOTD data string.");
                }
                int num2 = dataString.IndexOf("</Motd>");
                if (num2 >= 0 && num2 > num)
                {
                    num2 += 7;
                }
                XElement xElement = XmlUtils.LoadXmlFromString(dataString.Substring(num, num2 - num), throwOnError: true);
                SettingsManager.MotdUpdatePeriodHours = XmlUtils.GetAttributeValue(xElement, "UpdatePeriodHours", 24);
                SettingsManager.MotdUpdateUrl = XmlUtils.GetAttributeValue(xElement, "UpdateUrl", SettingsManager.MotdUpdateUrl);
                var message = new Message();
                foreach (XElement item2 in xElement.Elements())
                {
                    if (Widget.IsNodeIncludedOnCurrentPlatform(item2))
                    {
                        var item = new Line
                        {
                            Time = XmlUtils.GetAttributeValue<float>(item2, "Time"),
                            Node = item2.Elements().FirstOrDefault(),
                            Text = item2.Value
                        };
                        message.Lines.Add(item);
                    }
                }
                LoadBulletin(dataString);
                LoadFilterMods(dataString);
                return message;
            }
            catch (Exception ex)
            {
                Log.Warning("Failed extracting MOTD string. Reason: " + ex.Message);
            }
            return null;
        }

        public static void LoadBulletin(string dataString)
        {
            int num = dataString.IndexOf("<Motd2");
            if (num < 0)
            {
                throw new InvalidOperationException("Invalid MOTD2 data string.");
            }
            int num2 = dataString.IndexOf("</Motd2>");
            if (num2 >= 0 && num2 > num)
            {
                num2 += 8;
            }
            XElement xElement = XmlUtils.LoadXmlFromString(dataString.Substring(num, num2 - num), throwOnError: true);
            string languageType = (!ModsManager.Configs.ContainsKey("Language")) ? "zh-CN" : ModsManager.Configs["Language"];
            foreach (XElement item in xElement.Elements())
            {
                if (item.Name.LocalName == "Bulletin")
                {
                    m_bulletin = new Bulletin();
                    m_bulletin.Title = item.Attribute("Title").Value;
                    m_bulletin.EnTitle = item.Attribute("EnTitle").Value;
                    m_bulletin.Time = languageType + "$" + item.Attribute("Time").Value;
                    m_bulletin.Content = item.Element("Content").Value;
                    m_bulletin.EnContent = item.Element("EnContent").Value;
                    break;
                }
            }
        }

        public static void LoadFilterMods(string dataString)
        {
            int num = dataString.IndexOf("<Motd3");
            if (num < 0)
            {
                throw new InvalidOperationException("Invalid MOTD3 data string.");
            }
            int num2 = dataString.IndexOf("</Motd3>");
            if (num2 >= 0 && num2 > num)
            {
                num2 += 8;
            }
            XElement xElement = XmlUtils.LoadXmlFromString(dataString.Substring(num, num2 - num), throwOnError: true);
            FilterModAll.Clear();
            foreach (XElement item in xElement.Elements())
            {
                if (item.Name.LocalName == "FilterMod")
                {
                    FilterMod filterMod = new FilterMod();
                    filterMod.Name = item.Attribute("Name").Value;
                    filterMod.PackageName = item.Attribute("PackageName").Value;
                    filterMod.Version = item.Attribute("Version").Value;
                    filterMod.FilterAPIVersion = item.Attribute("FilterAPIVersion").Value;
                    filterMod.Explanation = item.Value;
                    FilterModAll.Add(filterMod);
                }
            }
        }

        public static void ShowBulletin()
        {
            try
            {
                string time = m_bulletin.Time.Contains("$") ? m_bulletin.Time.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries)[1] : string.Empty;
                if (!string.IsNullOrEmpty(time)) time = (IsCNLanguageType() ? "公告发布时间: " : "Time: ") + time;
                string title = IsCNLanguageType() ? m_bulletin.Title : m_bulletin.EnTitle;
                string content = IsCNLanguageType() ? m_bulletin.Content : m_bulletin.EnContent;
                DialogsManager.ShowDialog(null, new BulletinDialog(title, time + "\n" + content, delegate
                {
                    SettingsManager.BulletinTime = m_bulletin.Time;
                }));
                CanShowBulletin = false;
            }
            catch (Exception ex)
            {
                Log.Warning("Failed ShowBulletin. Reason: " + ex.Message);
            }
        }

        public static bool IsCNLanguageType()
        {
            string languageType = (!ModsManager.Configs.ContainsKey("Language")) ? "zh-CN" : ModsManager.Configs["Language"];
            return (languageType == "zh-CN");
        }

        public static string GetMotdUrl()
        {
            string languageType = (!ModsManager.Configs.ContainsKey("Language")) ? "zh-CN" : ModsManager.Configs["Language"];
            return string.Format(SettingsManager.MotdUpdateUrl, VersionsManager.SerializationVersion, languageType);
        }
    }
}
