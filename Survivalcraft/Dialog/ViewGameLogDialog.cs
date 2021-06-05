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


        public ViewGameLogDialog()
        {
            m_uploadButton = new BevelledButtonWidget() { Style = ContentManager.Get<XElement>("Styles/ButtonStyle_160x60") ,VerticalAlignment=WidgetAlignment.Center,Text="上传"};
            CanvasWidget canvasWidget = new CanvasWidget() { Size=new Vector2(40,0)};
            XElement node = ContentManager.Get<XElement>("Dialogs/ViewGameLogDialog");
            LoadContents(this, node);
            m_listPanel = Children.Find<ListPanelWidget>("ViewGameLogDialog.ListPanel");
            m_copyButton = Children.Find<ButtonWidget>("ViewGameLogDialog.CopyButton");
            m_filterButton = Children.Find<ButtonWidget>("ViewGameLogDialog.FilterButton");
            m_filterButton.Style = ContentManager.Get<XElement>("Styles/ButtonStyle_160x60");
            m_closeButton = Children.Find<ButtonWidget>("ViewGameLogDialog.CloseButton");
            m_closeButton.ParentWidget.Children.Add(canvasWidget);
            m_closeButton.ParentWidget.Children.Add(m_uploadButton);
            m_listPanel.ItemClicked += delegate (object item)
            {
                if (m_listPanel.SelectedItem == item)
                {
                    DialogsManager.ShowDialog(base.ParentWidget, new MessageDialog("Log Item", item.ToString(), "OK", null, null));
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
                else if (m_filter < LogType.Error)
                {
                    m_filter = LogType.Error;
                }
                else
                {
                    m_filter = LogType.Debug;
                }
                PopulateList();
            }
            if (base.Input.Cancel || m_closeButton.IsClicked)
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
                    MessageDialog messageDialog = new MessageDialog("错误", "请登陆后进行日志提交操作","确定","取消",(btn)=> {
                        DialogsManager.HideAllDialogs();
                    });
                    DialogsManager.ShowDialog(this, messageDialog);
                }
                else {
                    CancellableProgress cancellableProgress = new CancellableProgress();
                    CancellableBusyDialog dialog = new CancellableBusyDialog("上传日志中...", true);
                    DialogsManager.ShowDialog(this, dialog);
                    JsonObject jsonObject = new JsonObject();
                    jsonObject.Add("path", "/GameLog/" + DateTime.Now.Ticks + ".log");
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    dictionary.Add("Authorization", "Bearer " + SettingsManager.ScpboxAccessToken);
                    dictionary.Add("Content-Type", "application/octet-stream");
                    dictionary.Add("Dropbox-API-Arg", jsonObject.ToString());
                    MemoryStream memoryStream = new MemoryStream();
                    GameLogSink.m_stream.Seek(0,SeekOrigin.Begin);
                    GameLogSink.m_stream.CopyTo(memoryStream);
                    memoryStream.Seek(0,SeekOrigin.Begin);
                    WebManager.Post(SPMBoxExternalContentProvider.m_redirectUri + "/com/files/upload", null, dictionary, memoryStream, cancellableProgress, delegate
                    {
                        dialog.LargeMessage = "上传成功";
                        dialog.m_cancelButtonWidget.Text = "确定";
                        GameLogSink.m_writer.BaseStream.SetLength(0);
                        GameLogSink.m_writer.Flush();                        
                    }, delegate (Exception error)
                    {
                        dialog.LargeMessage = "上传失败";
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
            m_listPanel.ScrollPosition = (float)m_listPanel.Items.Count * m_listPanel.ItemSize;
        }
    }
}
