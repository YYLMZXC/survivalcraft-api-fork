using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Game
{
    public class CommunityContentScreen : Screen
    {
        public enum Order
        {
            ByRank,
            ByTime
        }

        public ListPanelWidget m_listPanel;

        public LinkWidget m_moreLink;

        public LabelWidget m_orderLabel;

        public ButtonWidget m_changeOrderButton;

        public LabelWidget m_filterLabel;

        public ButtonWidget m_changeFilterButton;

        public ButtonWidget m_downloadButton;

        public ButtonWidget m_deleteButton;

        public ButtonWidget m_moreOptionsButton;

        public ButtonWidget m_searchKey;

        public TextBoxWidget m_inputKey;

        public LabelWidget m_placeHolder;

        public object m_filter;

        public Order m_order;

        public double m_contentExpiryTime;

        public Dictionary<string, IEnumerable<object>> m_itemsCache = new Dictionary<string, IEnumerable<object>>();

        public CommunityContentScreen()
        {
            XElement node = ContentManager.Get<XElement>("Screens/CommunityContentScreen");
            LoadContents(this, node);
            m_listPanel = Children.Find<ListPanelWidget>("List");
            m_orderLabel = Children.Find<LabelWidget>("Order");
            m_changeOrderButton = Children.Find<ButtonWidget>("ChangeOrder");
            m_filterLabel = Children.Find<LabelWidget>("Filter");
            m_changeFilterButton = Children.Find<ButtonWidget>("ChangeFilter");
            m_downloadButton = Children.Find<ButtonWidget>("Download");
            m_deleteButton = Children.Find<ButtonWidget>("Delete");
            m_moreOptionsButton = Children.Find<ButtonWidget>("MoreOptions");
            m_inputKey = Children.Find<TextBoxWidget>("key");
            m_placeHolder = Children.Find<LabelWidget>("placeholder");
            m_searchKey = Children.Find<ButtonWidget>("Search");
            m_listPanel.ItemWidgetFactory = delegate (object item)
            {
                var communityContentEntry = item as CommunityContentEntry;
                if (communityContentEntry != null)
                {
                    XElement node2 = ContentManager.Get<XElement>("Widgets/CommunityContentItem");
                    var obj = (ContainerWidget)LoadWidget(this, node2, null);
                    communityContentEntry.IconInstance = obj.Children.Find<RectangleWidget>("CommunityContentItem.Icon");
                    communityContentEntry.IconInstance.Subtexture = communityContentEntry.Icon == null ? ExternalContentManager.GetEntryTypeIcon(communityContentEntry.Type) : new Subtexture(communityContentEntry.Icon, Vector2.Zero, Vector2.One);
                    obj.Children.Find<LabelWidget>("CommunityContentItem.Text").Text = communityContentEntry.Name;
                    obj.Children.Find<LabelWidget>("CommunityContentItem.Details").Text = $"{ExternalContentManager.GetEntryTypeDescription(communityContentEntry.Type)} {DataSizeFormatter.Format(communityContentEntry.Size)}";
                    obj.Children.Find<StarRatingWidget>("CommunityContentItem.Rating").Rating = communityContentEntry.RatingsAverage;
                    obj.Children.Find<StarRatingWidget>("CommunityContentItem.Rating").IsVisible = (communityContentEntry.RatingsAverage > 0f);
                    obj.Children.Find<LabelWidget>("CommunityContentItem.ExtraText").Text = communityContentEntry.ExtraText;
                    return obj;
                }
                XElement node3 = ContentManager.Get<XElement>("Widgets/CommunityContentItemMore");
                var containerWidget = (ContainerWidget)LoadWidget(this, node3, null);
                m_moreLink = containerWidget.Children.Find<LinkWidget>("CommunityContentItemMore.Link");
                m_moreLink.Tag = (item as string);
                return containerWidget;
            };
            m_listPanel.SelectionChanged += delegate
            {
                if (m_listPanel.SelectedItem != null && !(m_listPanel.SelectedItem is CommunityContentEntry))
                {
                    m_listPanel.SelectedItem = null;
                }
            };
        }

        public override void Enter(object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0].ToString() == "Mod")
            {
                m_filter = ExternalContentType.Mod;
            }
            else
            {
                m_filter = string.Empty;
            }
            m_order = Order.ByRank;
            m_inputKey.Text = string.Empty;
            PopulateList(null);
        }

        public override void Update()
        {
            m_placeHolder.IsVisible = string.IsNullOrEmpty(m_inputKey.Text);
            var communityContentEntry = m_listPanel.SelectedItem as CommunityContentEntry;
            m_downloadButton.IsEnabled = (communityContentEntry != null);
            m_deleteButton.IsEnabled = (UserManager.ActiveUser != null && communityContentEntry != null && communityContentEntry.UserId == UserManager.ActiveUser.UniqueId);
            m_orderLabel.Text = GetOrderDisplayName(m_order);
            m_filterLabel.Text = GetFilterDisplayName(m_filter);
            if (m_changeOrderButton.IsClicked)
            {
                var items = EnumUtils.GetEnumValues(typeof(Order)).Cast<Order>().ToList();
                DialogsManager.ShowDialog(null, new ListSelectionDialog(LanguageControl.Get(GetType().Name, "Order Type"), items, 60f, (object item) => GetOrderDisplayName((Order)item), delegate (object item)
                {
                    m_order = (Order)item;
                    PopulateList(null);
                }));
            }
            if (m_searchKey.IsClicked)
            {
                PopulateList(null);
            }
            if (m_changeFilterButton.IsClicked)
            {
                var list = new List<object>();
                list.Add(string.Empty);
                foreach (ExternalContentType item in from ExternalContentType t in EnumUtils.GetEnumValues(typeof(ExternalContentType))
                                                     where ExternalContentManager.IsEntryTypeDownloadSupported(t)
                                                     select t)
                {
                    list.Add(item);
                }
                if (UserManager.ActiveUser != null)
                {
                    list.Add(UserManager.ActiveUser.UniqueId);
                }
                DialogsManager.ShowDialog(null, new ListSelectionDialog(LanguageControl.Get(GetType().Name, "Filter"), list, 60f, (object item) => GetFilterDisplayName(item), delegate (object item)
                {
                    m_filter = item;
                    PopulateList(null);
                }));
            }
            if (m_downloadButton.IsClicked && communityContentEntry != null)
            {
                DownloadEntry(communityContentEntry);
            }
            if (m_deleteButton.IsClicked && communityContentEntry != null)
            {
                DeleteEntry(communityContentEntry);
            }
            if (m_moreOptionsButton.IsClicked)
            {
                DialogsManager.ShowDialog(null, new MoreCommunityLinkDialog());
            }
            if (m_moreLink != null && m_moreLink.IsClicked)
            {
                PopulateList((string)m_moreLink.Tag);
            }
            if (Input.Back || Children.Find<BevelledButtonWidget>("TopBar.Back").IsClicked)
            {
                ScreensManager.SwitchScreen("Content");
            }
            if (Input.Hold.HasValue && Input.HoldTime > 2f && Input.Hold.Value.Y < 20f)
            {
                m_contentExpiryTime = 0.0;
                Task.Delay(250).Wait();
            }
        }

        public void PopulateList(string cursor)
        {
            string text = string.Empty;
            if (SettingsManager.CommunityContentMode == CommunityContentMode.Strict)
            {
                text = "1";
            }
            if (SettingsManager.CommunityContentMode == CommunityContentMode.Normal)
            {
                text = "0";
            }
            string text2 = (m_filter is string) ? ((string)m_filter) : string.Empty;
            string text3 = (m_filter is ExternalContentType) ? LanguageControl.Get(GetType().Name, m_filter.ToString()) : string.Empty;
            string text4 = LanguageControl.Get(GetType().Name, m_order.ToString());
            string cacheKey = text2 + "\n" + text3 + "\n" + text4 + "\n" + text + "\n" + m_inputKey.Text;
            m_moreLink = null;
            if (string.IsNullOrEmpty(cursor))
            {
                m_listPanel.ClearItems();
                m_listPanel.ScrollPosition = 0f;
                if (m_contentExpiryTime != 0.0 && Time.RealTime < m_contentExpiryTime && m_itemsCache.TryGetValue(cacheKey, out IEnumerable<object> value))
                {
                    foreach (object item in value)
                    {
                        m_listPanel.AddItem(item);
                    }
                    return;
                }
            }
            var busyDialog = new CancellableBusyDialog(LanguageControl.Get(GetType().Name, 2), autoHideOnCancel: false);
            DialogsManager.ShowDialog(null, busyDialog);
            CommunityContentManager.List(cursor, text2, text3, text, text4, m_inputKey.Text, busyDialog.Progress, delegate (List<CommunityContentEntry> list, string nextCursor)
            {
                DialogsManager.HideDialog(busyDialog);
                m_contentExpiryTime = Time.RealTime + 300.0;
                while (m_listPanel.Items.Count > 0 && !(m_listPanel.Items[m_listPanel.Items.Count - 1] is CommunityContentEntry))
                {
                    m_listPanel.RemoveItemAt(m_listPanel.Items.Count - 1);
                }
                foreach (CommunityContentEntry item2 in list)
                {
                    m_listPanel.AddItem(item2);
                    if (item2.Icon == null && !string.IsNullOrEmpty(item2.IconSrc))
                    {
                        WebManager.Get(item2.IconSrc, null, null, new CancellableProgress(), delegate (byte[] data) {
                            Dispatcher.Dispatch(delegate {
                                if (data.Length > 0)
                                {
                                    try
                                    {
                                        var texture = Engine.Graphics.Texture2D.Load(Engine.Media.Image.Load(new System.IO.MemoryStream(data), Engine.Media.ImageFileFormat.Png));
                                        item2.Icon = texture;
                                        if (item2.IconInstance != null) item2.IconInstance.Subtexture = new Subtexture(texture, Vector2.Zero, Vector2.One);
                                    }
                                    catch (Exception e)
                                    {
                                        System.Diagnostics.Debug.WriteLine(e.Message);
                                    }
                                }
                            });
                        }, delegate (Exception e) { });
                    }else if (item2.IconInstance != null) item2.IconInstance.Subtexture = new Subtexture(item2.Icon, Vector2.Zero, Vector2.One);
                }
                if (list.Count > 0 && !string.IsNullOrEmpty(nextCursor))
                {
                    m_listPanel.AddItem(nextCursor);
                }
                m_itemsCache[cacheKey] = new List<object>(m_listPanel.Items);
            }, delegate (Exception error)
            {
                DialogsManager.HideDialog(busyDialog);
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, error.Message, LanguageControl.Ok, null, null));
            });
        }

        public void DownloadEntry(CommunityContentEntry entry)
        {
            string userId = (UserManager.ActiveUser != null) ? UserManager.ActiveUser.UniqueId : string.Empty;
            var busyDialog = new CancellableBusyDialog(string.Format(LanguageControl.Get(GetType().Name, 1), entry.Name), autoHideOnCancel: false);
            DialogsManager.ShowDialog(null, busyDialog);
            CommunityContentManager.Download(entry.Address, entry.Name, entry.Type, userId, busyDialog.Progress, delegate
            {
                DialogsManager.HideDialog(busyDialog);
            }, delegate (Exception error)
            {
                DialogsManager.HideDialog(busyDialog);
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, error.Message, LanguageControl.Ok, null, null));
            });
        }

        public void DeleteEntry(CommunityContentEntry entry)
        {
            if (UserManager.ActiveUser != null)
            {
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 4), LanguageControl.Get(GetType().Name, 5), LanguageControl.Yes, LanguageControl.No, delegate (MessageDialogButton button)
                {
                    if (button == MessageDialogButton.Button1)
                    {
                        var busyDialog = new CancellableBusyDialog(string.Format(LanguageControl.Get(GetType().Name, 3), entry.Name), autoHideOnCancel: false);
                        DialogsManager.ShowDialog(null, busyDialog);
                        CommunityContentManager.Delete(entry.Address, UserManager.ActiveUser.UniqueId, busyDialog.Progress, delegate
                        {
                            DialogsManager.HideDialog(busyDialog);
                            DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 6), LanguageControl.Get(GetType().Name, 7), LanguageControl.Ok, null, null));
                        }, delegate (Exception error)
                        {
                            DialogsManager.HideDialog(busyDialog);
                            DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, error.Message, LanguageControl.Ok, null, null));
                        });
                    }
                }));
            }
        }

        public static string GetFilterDisplayName(object filter)
        {
            if (filter is string)
            {
                if (!string.IsNullOrEmpty((string)filter))
                {
                    return LanguageControl.Get(typeof(CommunityContentScreen).Name, 8);
                }
                return LanguageControl.Get(typeof(CommunityContentScreen).Name, 9);
            }
            if (filter is ExternalContentType)
            {
                return ExternalContentManager.GetEntryTypeDescription((ExternalContentType)filter);
            }
            throw new InvalidOperationException(LanguageControl.Get(typeof(CommunityContentScreen).Name, 10));
        }

        public static string GetOrderDisplayName(Order order)
        {
            switch (order)
            {
                case Order.ByRank:
                    return LanguageControl.Get(typeof(CommunityContentScreen).Name, 11);
                case Order.ByTime:
                    return LanguageControl.Get(typeof(CommunityContentScreen).Name, 12);
                default:
                    throw new InvalidOperationException(LanguageControl.Get(typeof(CommunityContentScreen).Name, 13));
            }
        }
    }
}
