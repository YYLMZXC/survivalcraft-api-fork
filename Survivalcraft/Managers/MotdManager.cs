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
        public static void UpdateVersion() {
            string url = string.Format(SettingsManager.MotdUpdateCheckUrl, VersionsManager.SerializationVersion, VersionsManager.Platform, ModsManager.APIVersion, LanguageControl.LName());
            WebManager.Get(url, null, null, new CancellableProgress(), data => {
                UpdateResult = SimpleJson.SimpleJson.DeserializeObject<SimpleJson.JsonObject>(System.Text.Encoding.UTF8.GetString(data));
            }, ex => {
                Log.Error("Failed processing Update check. Reason: " + ex.Message);
            });
        }
        public static void DownloadMotd()
        {
            Log.Information("Downloading MOTD");
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
            if (Time.PeriodicEvent(1.0, 0.0) && ModsManager.ConfigLoaded)
            {
                var t = TimeSpan.FromHours(SettingsManager.MotdUpdatePeriodHours);
                DateTime now = DateTime.Now;
                if (now >= SettingsManager.MotdLastUpdateTime + t)
                {
                    SettingsManager.MotdLastUpdateTime = now;
                    DownloadMotd();
                    UpdateVersion();
                }
            }
            if (MessageOfTheDay == null && !string.IsNullOrEmpty(SettingsManager.MotdLastDownloadedData))
            {
                MessageOfTheDay = ParseMotd(SettingsManager.MotdLastDownloadedData);
                if (MessageOfTheDay == null)
                {
                    SettingsManager.MotdLastDownloadedData = string.Empty;
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
                return message;
            }
            catch (Exception ex)
            {
                Log.Warning("Failed extracting MOTD string. Reason: " + ex.Message);
            }
            return null;
        }
        public static string GetMotdUrl() => string.Format(SettingsManager.MotdUpdateUrl, VersionsManager.SerializationVersion, ModsManager.Configs["Language"]);
    }
}
