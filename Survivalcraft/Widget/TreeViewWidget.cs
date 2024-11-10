using Engine;
using Engine.Graphics;
using System.Xml.Linq;

namespace Game
{
	public class TreeViewWidget : ScrollPanelWidget
	{
		#region 字段

		private List<TreeViewNode> m_nodes = new();
		public bool m_widgetsDirty;
		public Vector2 lastActualSize = new(-1f);
		public int m_visibleItemsCount = 0;//显示出来的项的数量
		public Action<TreeViewNode> m_onNodeClicked;
		private TreeViewNode m_selectedNode;

		#endregion

		#region 属性

		public List<TreeViewNode> Nodes
		{
			get => m_nodes;
			set => m_nodes = value;
		}

		public int ItemSize { get; set; } = 70;

		public Action<TreeViewNode> OnNodeClicked
		{
			get => m_onNodeClicked;
			set => m_onNodeClicked = value;
		}

		public TreeViewNode SelectedNode => m_selectedNode;

		#endregion

		#region 方法

		public override void MeasureOverride(Vector2 parentAvailableSize)
		{
			base.MeasureOverride(parentAvailableSize);
			foreach (Widget child in Children)
			{
				if (child.IsVisible)
				{//限制子条目大小
					child.Measure(new Vector2(MathUtils.Max(parentAvailableSize.X - (2f * child.Margin.X), 0f), ItemSize));
				}
			}
			if(m_widgetsDirty)
			{
				m_widgetsDirty = false;
				UpdateNodes();
			}
		}

		public void UpdateNodes()
		{
			Children.Clear();

			foreach(var viewNode in Nodes)
			{
				AddNodeToWidget(viewNode, 0);
			}
		}

		public void AddNodeToWidget(TreeViewNode node, float xOffset)
		{
			TreeViewNodeContentItem obj = new(node, xOffset);
			obj.OnClicked = clickedNode => {
				if(clickedNode.Nodes.Count > 0) m_widgetsDirty = true;//点击到的条目有子项（即子项展开/收起了），更新列表

				//切换选中的节点
				if (m_selectedNode != null) m_selectedNode.Selected = false;
				clickedNode.Selected = true;
				m_selectedNode = clickedNode;

				OnNodeClicked?.Invoke(clickedNode);
			};
			Children.Add(obj);

			if(node.Nodes.Count > 0 && node.Expanded)
			{
				foreach(var child in node.Nodes)
				{
					AddNodeToWidget(child, xOffset + 20);
				}
			}
		}

		public override void ArrangeOverride()
		{
			if (ActualSize != lastActualSize)
			{
				m_widgetsDirty = true;
			}
			lastActualSize = ActualSize;
			int num = 0;
			foreach(Widget child in Children)
			{
				var vector2 = new Vector2(0f,(num * ItemSize) - ScrollPosition);
				ArrangeChildWidgetInCell(vector2,vector2 + new Vector2(ActualSize.X,ItemSize),child);
				num++;
			}
			m_visibleItemsCount = num;
		}

		public override float CalculateScrollAreaLength()
		{
			return m_visibleItemsCount * ItemSize;
		}

		public void AddRoot(TreeViewNode node)
		{
			m_nodes.Add(node);
			m_widgetsDirty = true;
		}

		public void RemoveRoot(TreeViewNode node)
		{
			m_nodes.Remove(node);
			m_widgetsDirty = true;
		}

		#endregion

		#region Debug

		public TreeViewWidget()
		{
			TreeViewNode treeViewNode = new("Root1","AAA");
			TreeViewNode treeViewNode2 = new("Root1->1","AAB");
			TreeViewNode treeViewNode3 = new("Root1->2","AAC");
			treeViewNode.AddChild(treeViewNode2);
			treeViewNode.AddChild(treeViewNode3);

			TreeViewNode treeViewNode4 = new("Root2","AAA");
			TreeViewNode treeViewNode5 = new("Root2->1","AAB");
			TreeViewNode treeViewNode6 = new("Root2->2","AAC");
			TreeViewNode treeViewNode7 = new("Root2->2->1","AAC");
			TreeViewNode treeViewNode8 = new("Root2->2->2","AAC");
			TreeViewNode treeViewNode9 = new("Root2->2->3","AAC");
			treeViewNode4.AddChild(treeViewNode5);
			treeViewNode4.AddChild(treeViewNode6);
			treeViewNode6.AddChild(treeViewNode7);
			treeViewNode6.AddChild(treeViewNode8);
			treeViewNode6.AddChild(treeViewNode9);

			Nodes.Add(treeViewNode);
			Nodes.Add(treeViewNode4);

			m_widgetsDirty = true;
		}

		#endregion
	}

	public class TreeViewNode
	{
		#region 字段
		private List<TreeViewNode> m_nodes;
		private bool m_expanded;
		#endregion

		#region 属性
		public List<TreeViewNode> Nodes
		{
			get => m_nodes;
			set => m_nodes = value;
		}

		public bool Expanded
		{
			get => m_expanded;
			set
			{
				if (m_expanded == value) return;
				if (value && Nodes.Count == 0) return;
				m_expanded = value;
			}
		}

		public string Text
		{
			get;
			set;
		}

		public Color TextColor
		{
			get;
			set;
		}

		public string SubText
		{
			get;
			set;
		}

		public Color SubTextColor
		{
			get;
			set;
		}

		public Color SelectedColor
		{
			get;
			set;
		}

		public Texture2D Icon
		{
			get;
			set;
		}

		public TreeViewNode ParentNode
		{
			get;
			set;
		}

		public TreeViewWidget ParentTree
		{
			get;
			set;
		}

		public object Tag
		{
			get;
			set;
		}

		public bool Selected
		{
			get;
			set;
		}
		#endregion

		#region 构造函数

		public void Init(string text, Color textColor, string subText, Color subTextColor, Texture2D icon, Color selectedColor, List<TreeViewNode> children)
		{
			Nodes = children;
			Text = text;
			TextColor = textColor;
			SubText = subText;
			SubTextColor = subTextColor;
			SelectedColor = selectedColor;
			Icon = icon;
		}
		public TreeViewNode()
		{
			Init(string.Empty, Color.White, string.Empty, Color.White,null, new Color(10, 70, 0, 90), new List<TreeViewNode>());
		}

		public TreeViewNode(string text, string subText, Texture2D icon = null)
		{
			Init(text, Color.White, subText, Color.White, icon, new Color(10, 70, 0, 90), new List<TreeViewNode>());
		}

		public TreeViewNode(string text, Color textColor, string subText, Color subTextColor, Texture2D icon = null)
		{
			Init(text, textColor, subText, subTextColor, icon, new Color(10, 70, 0, 90),new List<TreeViewNode>());
		}
		#endregion

		#region 其他函数
		public void Remove() {
			if (ParentNode != null) ParentNode.Nodes.Remove(this);
			else ParentTree.Nodes.Remove(this);
		}

		public void EnsureVisible() {
			var parent = ParentNode;

			while (parent != null) {
				parent.Expanded = true;
				parent = parent.ParentNode;
			}
		}

		public void AddChild(TreeViewNode child)
		{
			m_nodes.Add(child);
		}

		public void RemoveChild(TreeViewNode child)
		{
			m_nodes.Remove(child);
		}

		public void AddChildren(List<TreeViewNode> children)
		{
			m_nodes.AddRange(children);
		}
		#endregion
	}

	public class TreeViewNodeContentItem : ContainerWidget
	{
		public TreeViewNode m_node;

		public Action<TreeViewNode> OnClicked;

		public ClickableWidget m_clickableWidget;

		public RectangleWidget m_selectedHighLight;

		public RectangleWidget m_icon;

		public RectangleWidget m_expandIcon;

		public Subtexture m_expandIconTexture;

		public Subtexture m_unexpandIconTexture;
		public TreeViewNodeContentItem(TreeViewNode node, float xOffset)
		{
			m_node = node;
			OnClicked = viewNode => { };
			XElement node2 = ContentManager.Get<XElement>("Widgets/TreeViewNodeContentItem");
			LoadContents(this, node2);
			Children.Find<LabelWidget>("TreeViewNodeContentItem.Text").Text = node.Text;
			Children.Find<LabelWidget>("TreeViewNodeContentItem.Text").Color = node.TextColor;
			Children.Find<LabelWidget>("TreeViewNodeContentItem.Details").Text = node.SubText;
			Children.Find<LabelWidget>("TreeViewNodeContentItem.Details").Color = node.SubTextColor;
			m_selectedHighLight = Children.Find<RectangleWidget>("TreeViewNodeContentItem.SelectedHighLight");
			m_selectedHighLight.FillColor = m_node.SelectedColor;
			m_icon = Children.Find<RectangleWidget>("TreeViewNodeContentItem.Icon");
			if (node.Icon != null) m_icon.Subtexture = new Subtexture(node.Icon,Vector2.Zero, Vector2.One);
			m_icon.IsVisible = node.Icon != null;
			m_expandIcon = Children.Find<RectangleWidget>("TreeViewNodeContentItem.ExpandIcon");
			m_expandIcon.FillColor = node.Nodes.Count > 0 ? new Color(128, 128, 128, 128) : Color.Transparent;

			m_expandIconTexture = ContentManager.Get<Subtexture>("Textures/Atlas/ArrowDown");
			m_unexpandIconTexture = ContentManager.Get<Subtexture>("Textures/Atlas/ArrowRight");


			Margin = new Vector2(xOffset,0);

			m_clickableWidget = new ClickableWidget();
			Children.Add(m_clickableWidget);
		}

		public override void Update()
		{
			if(m_clickableWidget.IsClicked)
			{
				m_node.Expanded = !m_node.Expanded;
				OnClicked(m_node);
			}
			m_selectedHighLight.IsVisible = m_node.Selected;
			m_expandIcon.Subtexture = m_node.Expanded ? m_expandIconTexture : m_unexpandIconTexture;
		}
	}
}