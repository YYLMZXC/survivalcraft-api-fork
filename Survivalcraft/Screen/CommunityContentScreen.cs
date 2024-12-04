﻿using Engine;
using System.Text.Json;
using System.Xml.Linq;

namespace Game
{
	public class CommunityContentScreen : Screen
	{
		public enum Order
		{
			ByRank,
			ByTime,
			ByBoutique,
			ByHide
		}

		public enum SearchType
		{
			ByName,
			ByAuthor,
			ByUserId
		}

		public TreeViewWidget m_treePanel;
		public LabelWidget m_orderLabel;
		public ButtonWidget m_changeOrderButton;
		public LabelWidget m_filterLabel;
		public ButtonWidget m_changeFilterButton;
		public ButtonWidget m_downloadButton;
		public ButtonWidget m_actionButton;
		public ButtonWidget m_action2Button;
		public ButtonWidget m_action3Button;
		public ButtonWidget m_moreOptionsButton;
		public ButtonWidget m_searchKey;
		public ButtonWidget m_searchTypeButton;
		public TextBoxWidget m_inputKey;
		public LabelWidget m_placeHolder;

		public object m_filter;

		public Order m_order;

		public SearchType m_searchType;

		public double m_contentExpiryTime;

		public bool m_isOwn;
		public bool m_isAdmin;
		public bool m_isCNLanguageType;

		public Dictionary<string, IEnumerable<object>> m_itemsCache = [];

		public SchubExternalContentProvider m_provider;

		public CommunityContentScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/CommunityContentScreen");
			LoadContents(this, node);
			m_treePanel = Children.Find<TreeViewWidget>("Tree");
			m_orderLabel = Children.Find<LabelWidget>("Order");
			m_changeOrderButton = Children.Find<ButtonWidget>("ChangeOrder");
			m_filterLabel = Children.Find<LabelWidget>("Filter");
			m_changeFilterButton = Children.Find<ButtonWidget>("ChangeFilter");
			m_downloadButton = Children.Find<ButtonWidget>("Download");
			m_actionButton = Children.Find<ButtonWidget>("Action");
			m_action2Button = Children.Find<ButtonWidget>("Action2");
			m_action3Button = Children.Find<ButtonWidget>("Action3");
			m_moreOptionsButton = Children.Find<ButtonWidget>("MoreOptions");
			m_inputKey = Children.Find<TextBoxWidget>("key");
			m_placeHolder = Children.Find<LabelWidget>("placeholder");
			m_searchKey = Children.Find<ButtonWidget>("Search");
			m_searchTypeButton = Children.Find<ButtonWidget>("SearchType");
			m_searchType = SearchType.ByName;
			/*
			m_treePanel.ItemWidgetFactory = delegate (object item)
			{
				var communityContentEntry = item as CommunityContentEntry;
				if (communityContentEntry != null)
				{
					XElement node2 = ContentManager.Get<XElement>("Widgets/CommunityContentItem");
					var obj = (ContainerWidget)LoadWidget(this, node2, null);
					communityContentEntry.IconInstance = obj.Children.Find<RectangleWidget>("CommunityContentItem.Icon");
					communityContentEntry.IconInstance.Subtexture = communityContentEntry.Icon == null ? ExternalContentManager.GetEntryTypeIcon(communityContentEntry.Type) : new Subtexture(communityContentEntry.Icon, Vector2.Zero, Vector2.One);
					obj.Children.Find<LabelWidget>("CommunityContentItem.Text").Text = communityContentEntry.Name;
					Color txtColor = Color.White;
					if (communityContentEntry.Boutique > 0)
					{
						txtColor = new Color(255, 215, 0);
					}
					if (m_isOwn && communityContentEntry.IsShow == 0)
					{
						txtColor = Color.Gray;
					}
					obj.Children.Find<LabelWidget>("CommunityContentItem.Text").Color = txtColor;
					obj.Children.Find<LabelWidget>("CommunityContentItem.Details").Text = $"{ExternalContentManager.GetEntryTypeDescription(communityContentEntry.Type)} {DataSizeFormatter.Format(communityContentEntry.Size)}";
					obj.Children.Find<StarRatingWidget>("CommunityContentItem.Rating").Rating = communityContentEntry.RatingsAverage;
					obj.Children.Find<StarRatingWidget>("CommunityContentItem.Rating").IsVisible = communityContentEntry.RatingsAverage > 0f;
					obj.Children.Find<LabelWidget>("CommunityContentItem.ExtraText").Text = communityContentEntry.ExtraText;
					return obj;
				}
				XElement node3 = ContentManager.Get<XElement>("Widgets/CommunityContentItemMore");
				var containerWidget = (ContainerWidget)LoadWidget(this, node3, null);
				m_moreLink = containerWidget.Children.Find<LinkWidget>("CommunityContentItemMore.Link");
				m_moreLink.Tag = item as string;
				return containerWidget;
			};
			m_treePanel.SelectionChanged += delegate
			{
				if (m_treePanel.SelectedItem != null && !(m_treePanel.SelectedItem is CommunityContentEntry))
				{
					m_treePanel.SelectedItem = null;
				}
			};
			*/
		}

		public override void Enter(object[] parameters)
		{
			foreach (var provider in ExternalContentManager.m_providers)
			{
				if (provider is SchubExternalContentProvider)
				{
					m_provider = (SchubExternalContentProvider)provider;
					break;
				}
			}
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
			m_isOwn = false;
			string languageType = (!ModsManager.Configs.TryGetValue("Language",out string value)) ? "zh-CN" : value;
			m_isCNLanguageType = languageType == "zh-CN";
			CommunityContentManager.IsAdmin(new CancellableProgress(), delegate (bool isAdmin)
			{
				m_isAdmin = isAdmin;
			}, delegate (Exception e)
			{
				DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, e.Message, LanguageControl.Ok, null, null));
			});
			PopulateList(null);
		}

		public override void Update()
		{
			m_placeHolder.IsVisible = string.IsNullOrEmpty(m_inputKey.Text);
			m_actionButton.IsVisible = m_isAdmin || m_isOwn;
			m_action2Button.IsVisible = m_isAdmin || m_isOwn;
			if (!m_isCNLanguageType)
			{
				m_actionButton.IsVisible = false;
				m_action2Button.IsVisible = false;
				m_action3Button.IsVisible = false;
			}
			var communityContentEntry = m_treePanel.SelectedNode?.Tag as CommunityContentEntry;
			m_downloadButton.IsEnabled = communityContentEntry != null;
			if (communityContentEntry != null)
			{
				m_actionButton.IsEnabled = m_isAdmin || m_isOwn;
				m_actionButton.Text = m_order == Order.ByHide || m_isOwn
					? LanguageControl.Get(GetType().Name, 23)
					: (communityContentEntry.Boutique == 0) ? LanguageControl.Get(GetType().Name, 15) : LanguageControl.Get(GetType().Name, 16);
				m_action2Button.IsEnabled = (m_filter.ToString() != "Mod") && (m_isAdmin || m_isOwn);
			}
			else
			{
				m_actionButton.IsEnabled = false;
				m_action2Button.IsEnabled = false;
				m_actionButton.Text = LanguageControl.Get(GetType().Name, 17);
			}
			if (m_isOwn)
			{
				m_searchType = SearchType.ByName;
				m_searchTypeButton.IsEnabled = false;
			}
			else
			{
				m_searchTypeButton.IsEnabled = true;
			}
			m_action2Button.Text = (communityContentEntry != null && communityContentEntry.IsShow == 0) ? LanguageControl.Get(GetType().Name, 24) : LanguageControl.Get(GetType().Name, 25);
			m_orderLabel.Text = GetOrderDisplayName(m_order);
			m_filterLabel.Text = GetFilterDisplayName(m_filter);
			m_searchTypeButton.Text = GetSearchTypeDisplayName(m_searchType);
			if (m_changeOrderButton.IsClicked)
			{
				var items = EnumUtils.GetEnumValues(typeof(Order)).Cast<Order>().ToList();
				if (!m_isAdmin)
				{
					items.Remove(Order.ByHide);
				}
				DialogsManager.ShowDialog(null, new ListSelectionDialog(LanguageControl.Get(GetType().Name, "Order Type"), items, 60f, (object item) => GetOrderDisplayName((Order)item), delegate (object item)
				{
					m_order = (Order)item;
					PopulateList(null, true);
				}));
			}
			if (m_searchKey.IsClicked)
			{
				PopulateList(null);
			}
			if (m_changeFilterButton.IsClicked)
			{
				var list = new List<object>
				{
					string.Empty
				};
				foreach (ExternalContentType item in from ExternalContentType t in EnumUtils.GetEnumValues(typeof(ExternalContentType))
													 where ExternalContentManager.IsEntryTypeDownloadSupported(t)
													 select t)
				{
					list.Add(item);
				}
				if (!string.IsNullOrEmpty(SettingsManager.ScpboxAccessToken))
				{
					list.Add(SettingsManager.ScpboxAccessToken);
				}
				DialogsManager.ShowDialog(null, new ListSelectionDialog(LanguageControl.Get(GetType().Name, "Filter"), list, 60f, (object item) => GetFilterDisplayName(item), delegate (object item)
				{
					m_filter = item;
					m_isOwn = GetFilterDisplayName(item) == "只看自己";
					PopulateList(null, true);
				}));
			}
			if (m_downloadButton.IsClicked && communityContentEntry != null)
			{
				DownloadEntry(communityContentEntry);
			}
			if (m_actionButton.IsClicked && communityContentEntry != null)
			{
				if (m_order == Order.ByHide || m_isOwn)
				{
					DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 26), communityContentEntry.Name, LanguageControl.Ok, LanguageControl.Cancel, delegate (MessageDialogButton button)
					{
						if (button == MessageDialogButton.Button1)
						{
							var busyDialog = new CancellableBusyDialog(LanguageControl.Get(GetType().Name, 2), autoHideOnCancel: false);
							DialogsManager.ShowDialog(null, busyDialog);
							CommunityContentManager.DeleteFile(communityContentEntry.Index, busyDialog.Progress, delegate (byte[] data)
							{
								DialogsManager.HideDialog(busyDialog);
								m_treePanel.RemoveAtTag(communityContentEntry);
                                JsonElement result = JsonDocument.Parse(data).RootElement;
                                string msg = result[0].GetInt32() == 200 ? LanguageControl.Get(GetType().Name, 27) + communityContentEntry.Name : result[1].GetString();
								DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 20), msg, LanguageControl.Ok, null, null));
							}, delegate (Exception e)
							{
								DialogsManager.HideDialog(busyDialog);
								DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, e.Message, LanguageControl.Ok, null, null));
							});
						}
					}));
				}
				else
				{
					if (communityContentEntry.Boutique == 0)
					{
						DialogsManager.ShowDialog(null, new TextBoxDialog(LanguageControl.Get(GetType().Name, 18), "5", 4, delegate (string s)
						{
							if (!string.IsNullOrEmpty(s))
							{
								int boutique = 5;
								try
								{
									boutique = int.Parse(s);
								}
								catch
								{
								}
								var busyDialog = new CancellableBusyDialog(LanguageControl.Get(GetType().Name, 2), autoHideOnCancel: false);
								DialogsManager.ShowDialog(null, busyDialog);
								CommunityContentManager.UpdateBoutique(communityContentEntry.Type.ToString(), communityContentEntry.Index, boutique, busyDialog.Progress, delegate (byte[] data)
								{
									DialogsManager.HideDialog(busyDialog);
									m_order = Order.ByBoutique;
									PopulateList(null, true);
                                    JsonElement result = JsonDocument.Parse(data).RootElement;
                                    string msg = result[0].GetInt32() == 200 ? LanguageControl.Get(GetType().Name, 19) + communityContentEntry.Name : result[1].GetString();
									DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 20), msg, LanguageControl.Ok, null, null));
								}, delegate (Exception e)
								{
									DialogsManager.HideDialog(busyDialog);
									DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, e.Message, LanguageControl.Ok, null, null));
								});
							}
						}));
					}
					else
					{
						DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 21), communityContentEntry.Name, LanguageControl.Ok, LanguageControl.Cancel, delegate (MessageDialogButton button)
						{
							if (button == MessageDialogButton.Button1)
							{
								var busyDialog = new CancellableBusyDialog(LanguageControl.Get(GetType().Name, 2), autoHideOnCancel: false);
								DialogsManager.ShowDialog(null, busyDialog);
								CommunityContentManager.UpdateBoutique(communityContentEntry.Type.ToString(), communityContentEntry.Index, 0, busyDialog.Progress, delegate (byte[] data)
								{
									DialogsManager.HideDialog(busyDialog);
									PopulateList(null, true);
                                    JsonElement result = JsonDocument.Parse(data).RootElement;
                                    string msg = result[0].GetInt32() == 200 ? LanguageControl.Get(GetType().Name, 22) + communityContentEntry.Name : result[1].GetString();
									DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 20), msg, LanguageControl.Ok, null, null));
								}, delegate (Exception e)
								{
									DialogsManager.HideDialog(busyDialog);
									DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, e.Message, LanguageControl.Ok, null, null));
								});
							}
						}));
					}
				}
			}
			if (m_action2Button.IsClicked && communityContentEntry != null)
			{
				var busyDialog = new CancellableBusyDialog(LanguageControl.Get(GetType().Name, 2), autoHideOnCancel: false);
				DialogsManager.ShowDialog(null, busyDialog);
				int isShow = (communityContentEntry.IsShow + 1) % 2;
				string sucessMsg = (isShow == 1) ? LanguageControl.Get(GetType().Name, 28) : LanguageControl.Get(GetType().Name, 29);
				CommunityContentManager.UpdateHidePara(communityContentEntry.Index, isShow, busyDialog.Progress, delegate (byte[] data)
				{
					DialogsManager.HideDialog(busyDialog);
					if (!m_isOwn)
					{
						m_treePanel.RemoveAtTag(communityContentEntry);
					}
					else
					{
						PopulateList(null, true);
					}
                    JsonElement result = JsonDocument.Parse(data).RootElement;
                    string msg = result[0].GetInt32() == 200 ? sucessMsg + communityContentEntry.Name : result[1].GetString();
					DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(GetType().Name, 20), msg, LanguageControl.Ok, null, null));
				}, delegate (Exception e)
				{
					DialogsManager.HideDialog(busyDialog);
					DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, e.Message, LanguageControl.Ok, null, null));
				});
			}
			m_action3Button.Text = "申精";
			if (m_action3Button.IsClicked)
			{
				string msg = "如果你觉得你的作品足够优秀，\n可以申请加入精品区，让更多人看到。\n加精作品将会是社区认证的作品，是有机会上游戏公告推广的。\n\n具体申精方式\n请加[SC中文社区存档交流群(745540296)]了解。\n同时，如果你对某个作品有异议，\n也可加群举报，本群会受理作品归属问题，守护玩家的劳动成果！\n";
				DialogsManager.ShowDialog(null, new MessageDialog("作品如何申精？", msg, LanguageControl.Ok, null, null));
			}
			if (m_searchTypeButton.IsClicked)
			{
				if (m_isAdmin)
				{
					if (m_searchType == SearchType.ByName) m_searchType = SearchType.ByAuthor;
					else if (m_searchType == SearchType.ByAuthor) m_searchType = SearchType.ByUserId;
					else if (m_searchType == SearchType.ByUserId) m_searchType = SearchType.ByName;
				}
				else
				{
					if (m_searchType == SearchType.ByName) m_searchType = SearchType.ByAuthor;
					else if (m_searchType == SearchType.ByAuthor) m_searchType = SearchType.ByName;
					else if (m_searchType == SearchType.ByUserId) m_searchType = SearchType.ByName;
				}
			}
			if (m_moreOptionsButton.IsClicked)
			{
				//DialogsManager.ShowDialog(null, new MoreCommunityLinkDialog());
				if (m_provider.IsLoggedIn)
				{
					string info = string.IsNullOrEmpty(SettingsManager.ScpboxUserInfo) ? "暂无用户信息" : SettingsManager.ScpboxUserInfo;
					DialogsManager.ShowDialog(null, new MessageDialog("账号已登录,是否登出?", info, LanguageControl.Yes, LanguageControl.No, delegate (MessageDialogButton button)
					{
						if (button == MessageDialogButton.Button1)
						{
							m_provider.Logout();
						}
					}));
				}
				else
				{
					ExternalContentManager.ShowLoginUiIfNeeded(m_provider, showWarningDialog: false, delegate
					{
					});
				}
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

		public void PopulateList(string cursor, bool force = false)
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
			string text4 = m_order.ToString();
			string cacheKey = text2 + "\n" + text3 + "\n" + text4 + "\n" + text + "\n" + m_inputKey.Text;
			if (string.IsNullOrEmpty(cursor) && !force)
			{
				m_treePanel.Clear();
				m_treePanel.ScrollPosition = 0f;
				if (m_contentExpiryTime != 0.0 && Time.RealTime < m_contentExpiryTime && m_itemsCache.TryGetValue(cacheKey, out IEnumerable<object> value))
				{
					foreach (object item in value)//添加
					{
						if (item is TreeViewNode treeViewNode) m_treePanel.AddRoot(treeViewNode);
					}
					return;
				}
			}
			if (force)
			{
				m_treePanel.Clear();
			}
			var busyDialog = new CancellableBusyDialog(LanguageControl.Get(GetType().Name, 2), autoHideOnCancel: false);
			DialogsManager.ShowDialog(null, busyDialog);
			CommunityContentManager.List(cursor, text2, text3, text, text4, m_inputKey.Text, m_searchType.ToString(), busyDialog.Progress, delegate (List<CommunityContentEntry> list, string nextCursor)
			{
				DialogsManager.HideDialog(busyDialog);
				m_contentExpiryTime = Time.RealTime + 300.0;
				while (m_treePanel.Nodes.Count > 0 && m_treePanel.Nodes[^1].Tag is "Load More")//移除原先添加的"加载更多"节点
				{
					m_treePanel.Nodes.RemoveAt(m_treePanel.Nodes.Count - 1);
				}
				foreach (CommunityContentEntry item2 in list)
				{
					var rootNode = m_treePanel.Nodes.FirstOrDefault(x => x.Tag is int id && id == item2.CollectionID);
					if(rootNode == null)
					{
						rootNode = CreateCollectionNode(item2);
						m_treePanel.AddRoot(rootNode);
					}
					rootNode.AddChild(ContentToNode(item2));
					m_treePanel.m_widgetsDirty = true;

					if (item2.Icon == null && !string.IsNullOrEmpty(item2.IconSrc))
					{
						WebManager.Get(item2.IconSrc, null, null, new CancellableProgress(), delegate (byte[] data)
						{
							Dispatcher.Dispatch(delegate
							{
								if (data.Length > 0)
								{
									try
									{
										var texture = Engine.Graphics.Texture2D.Load(Image.Load(new System.IO.MemoryStream(data)));
										item2.Icon = texture;
										if(item2.LinkedNode != null)
										{
											//item2.LinkedNode.Icon = texture; //资源节点的图标
											if(item2.LinkedNode.ParentNode is {
													Tag: int
												}) //合集节点的图标
											{
												item2.LinkedNode.ParentNode.Icon = texture;
											}
										}
									}
									catch (Exception)
									{
										//System.Diagnostics.Debug.WriteLine(e.Message);
									}
								}
							});
						}, delegate (Exception e) { });
					}
					else if (item2.LinkedNode  != null) item2.LinkedNode.Icon = item2.Icon;
				}
				if (list.Count > 0 && !string.IsNullOrEmpty(nextCursor))
				{
					//加载更多节点
					TreeViewNode loadMoreNode = new("加载更多",new Color(64,192,64),string.Empty,Color.Transparent);
					loadMoreNode.Selectable = false;
					loadMoreNode.Tag = "Load More";
					loadMoreNode.OnClicked = () => PopulateList(nextCursor);
					m_treePanel.AddRoot(loadMoreNode);
				}
				m_itemsCache[cacheKey] = new List<object>(m_treePanel.Nodes);
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

		public string GetFilterDisplayName(object filter)
		{
			return filter is string
				? !string.IsNullOrEmpty((string)filter)
					? LanguageControl.Get(nameof(CommunityContentScreen), 8)
					: LanguageControl.Get(nameof(CommunityContentScreen), 9)
				: filter is ExternalContentType
				? ExternalContentManager.GetEntryTypeDescription((ExternalContentType)filter)
				:            throw new InvalidOperationException(LanguageControl.Get(nameof(CommunityContentScreen), 10));
		}

		public string GetOrderDisplayName(Order order)
		{
			return order switch
			{
				Order.ByRank => m_isCNLanguageType ? "评分最高" : "ByRank",
				Order.ByTime => m_isCNLanguageType ? "最新发布" : "ByTime",
				Order.ByBoutique => m_isCNLanguageType ? "精品推荐" : "ByBoutique",
				Order.ByHide => m_isCNLanguageType ? "尚未发布" : "ByHide",
				_ => throw new InvalidOperationException(LanguageControl.Get(nameof(CommunityContentScreen),13)),
			};
		}

		public string GetSearchTypeDisplayName(SearchType searchType)
		{
			return searchType switch
			{
				SearchType.ByName => m_isCNLanguageType ? "资源名" : "Name",
				SearchType.ByAuthor => m_isCNLanguageType ? "用户名" : "User",
				SearchType.ByUserId => m_isCNLanguageType ? "用户ID" : "UID",
				_ => "null",
			};
		}

		public TreeViewNode ContentToNode(CommunityContentEntry contentEntry)
		{
			string title = contentEntry.Name;
			string desc = $"{ExternalContentManager.GetEntryTypeDescription(contentEntry.Type)} {DataSizeFormatter.Format(contentEntry.Size)} {contentEntry.ExtraText}";
			TreeViewNode node = new (title, Color.White, desc, new Color(128, 128, 128),contentEntry.Icon) { Tag = contentEntry };
			contentEntry.LinkedNode = node;
			return node;
		}

		public TreeViewNode CreateCollectionNode(CommunityContentEntry contentEntry)
		{
			TreeViewNode node = new (contentEntry.CollectionName, Color.White, contentEntry.CollectionDetails, new Color(128, 128, 128)) { Tag = contentEntry.CollectionID };
			return node;
		}
	}
}
