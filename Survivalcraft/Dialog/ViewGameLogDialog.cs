using Engine;
using Engine.Media;
using System.Collections.Generic;
using System.Xml.Linq;
using SimpleJson;
using System;
using System.IO;
namespace Game
{
    public class ViewGameLogDialog : Dialog
    {
        public ListPanelWidget m_listPanel;

        public ButtonWidget m_copyButton;

        public ButtonWidget m_filterButton;

        public ButtonWidget m_closeButton;

        public ButtonWidget m_uploadButton;

        public LogType m_filter;

        public static string fName = "ViewGameLogDialog";

        public ViewGameLogDialog()
        {
            XElement node = ContentManager.Get<XElement>("Dialogs/ViewGameLogDialog");
            LoadContents(this, node);
            m_listPanel = Children.Find<ListPanelWidget>("ViewGameLogDialog.ListPanel");
            m_copyButton = Children.Find<ButtonWidget>("ViewGameLogDialog.CopyButton");
            m_filterButton = Children.Find<ButtonWidget>("ViewGameLogDialog.FilterButton");
            m_filterButton.Style = ContentManager.Get<XElement>("Styles/ButtonStyle_160x60");
            m_closeButton = Children.Find<ButtonWidget>("ViewGameLogDialog.CloseButton");
            m_uploadButton = Children.Find<ButtonWidget>("ViewGameLogDialog.UploadButton");
            m_listPanel.ItemClicked += delegate (object item)
            {
                if (m_listPanel.SelectedItem == item)
                {
                    DialogsManager.ShowDialog(ParentWidget, new MessageDialog("Log Item", item.ToString(), "OK", null, null));
                }
            };
            PopulateList();
        }

        public override void Update()
        {
            if (m_copyButton.IsClicked)
            {
                ClipboardManager.ClipboardString = GameLogSink.GetRecentLog(131072);
            }
            if (m_filterButton.IsClicked)
            {
                if (m_filter < LogType.Warning)
                {
                    m_filter = LogType.Warning;
                }
                else
                {
                    m_filter = m_filter < LogType.Error ? LogType.Error : LogType.Debug;
                }
                PopulateList();
            }
            if (Input.Cancel || m_closeButton.IsClicked)
            {
                DialogsManager.HideDialog(this);
            }
            if (m_filter == LogType.Debug)
            {
                m_filterButton.Text = "All";
            }
            else if (m_filter == LogType.Warning)
            {
                m_filterButton.Text = "Warnings";
            }
            else if (m_filter == LogType.Error)
            {
                m_filterButton.Text = "Errors";
            }
            if (m_uploadButton.IsClicked) {
                if (string.IsNullOrEmpty(SettingsManager.ScpboxAccessToken))
                {
                    var messageDialog = new MessageDialog(LanguageControl.Get(fName, 1), LanguageControl.Get(fName, 2), LanguageControl.Get(fName, 3), LanguageControl.Get(fName, 4),(btn)=> {
                        DialogsManager.HideAllDialogs();
                    });
                    DialogsManager.ShowDialog(this, messageDialog);
                }
                else {
                    var cancellableProgress = new CancellableProgress();
                    var dialog = new CancellableBusyDialog(LanguageControl.Get(fName, 5), true);
                    DialogsManager.ShowDialog(this, dialog);
                    var jsonObject = new JsonObject();
                    jsonObject.Add("path", "/GameLog/" + DateTime.Now.Ticks + ".log");
                    var dictionary = new Dictionary<string, string>();
                    dictionary.Add("Authorization", "Bearer " + SettingsManager.ScpboxAccessToken);
                    dictionary.Add("Content-Type", "application/octet-stream");
                    dictionary.Add("Dropbox-API-Arg", jsonObject.ToString());
                    var memoryStream = new MemoryStream();
                    GameLogSink.m_stream.Seek(0,SeekOrigin.Begin);
                    GameLogSink.m_stream.CopyTo(memoryStream);
                    memoryStream.Seek(0,SeekOrigin.Begin);
                    WebManager.Post(SPMBoxExternalContentProvider.m_redirectUri + "/com/files/upload", null, dictionary, memoryStream, cancellableProgress, delegate
                    {
                        dialog.LargeMessage = LanguageControl.Get(fName, 6);
                        dialog.m_cancelButtonWidget.Text = "OK";
                        GameLogSink.m_writer.BaseStream.SetLength(0);
                        GameLogSink.m_writer.Flush();
                        PopulateList();
                    }, delegate (Exception error)
                    {
                        dialog.LargeMessage = LanguageControl.Get(fName, 7);
                        dialog.SmallMessage = error.Message;
                    });
                }
            }
        }

        public void PopulateList()
        {
            m_listPanel.ItemWidgetFactory = delegate (object item)
            {
                string text = (item != null) ? item.ToString() : string.Empty;
                Color color = Color.Gray;
                if (text.Contains("ERROR:"))
                {
                    color = Color.Red;
                }
                else if (text.Contains("WARNING:"))
                {
                    color = Color.DarkYellow;
                }
                else if (text.Contains("INFO:"))
                {
                    color = Color.LightGray;
                }
                return new LabelWidget
                {
                    Text = text,
                    Font = BitmapFont.DebugFont,
                    HorizontalAlignment = WidgetAlignment.Near,
                    VerticalAlignment = WidgetAlignment.Center,
                    FontScale = 0.6f,
                    Color = color
                };
            };
            List<string> recentLogLines = GameLogSink.GetRecentLogLines(131072);
            m_listPanel.ClearItems();
            if (recentLogLines.Count > 1000)
            {
                recentLogLines.RemoveRange(0, recentLogLines.Count - 1000);
            }
            foreach (string item in recentLogLines)
            {
                if (m_filter == LogType.Warning)
                {
                    if (!item.Contains("WARNING:") && !item.Contains("ERROR:"))
                    {
                        continue;
                    }
                }
                else if (m_filter == LogType.Error && !item.Contains("ERROR:"))
                {
                    continue;
                }
                m_listPanel.AddItem(item);
            }
            m_listPanel.ScrollPosition = m_listPanel.Items.Count * m_listPanel.ItemSize;
        }
    }
}
