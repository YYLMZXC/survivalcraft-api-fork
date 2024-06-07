using System.Xml.Linq;
using Engine;
using Game;
using GameEntitySystem;

public class GameWidget : CanvasWidget
{
    public List<Camera> m_cameras = new List<Camera>();

    public Camera m_activeCamera;

	public ViewWidget ViewWidget { get; set; }

	public ContainerWidget GuiWidget { get; set; }

	public int GameWidgetIndex { get; set; }

	public SubsystemGameWidgets SubsystemGameWidgets { get; set; }

	public PlayerData PlayerData { get; set; }

	public ReadOnlyList<Camera> Cameras => new ReadOnlyList<Camera>(m_cameras);

	public Camera ActiveCamera
	{
		get => m_activeCamera;
		set
		{
			if (value == null || value.GameWidget != this)
			{
				throw new InvalidOperationException("Invalid camera.");
			}
			if (!IsCameraAllowed(value))
			{
				value = FindCamera<FppCamera>();
			}
			if (value != m_activeCamera)
			{
				Camera activeCamera = m_activeCamera;
				m_activeCamera = value;
				m_activeCamera.Activate(activeCamera);
			}
		}
	}

	public ComponentCreature Target { get; set; }

	public GameWidget(PlayerData playerData, int gameViewIndex)
	{
		PlayerData = playerData;
		GameWidgetIndex = gameViewIndex;
		SubsystemGameWidgets = playerData.SubsystemGameWidgets;
		LoadContents(this, ContentManager.Get<XElement>("Widgets/GameWidget"));
		ViewWidget = Children.Find<ViewWidget>("View");
		GuiWidget = Children.Find<ContainerWidget>("Gui");
		m_cameras.Add(new FppCamera(this));
		m_cameras.Add(new DeathCamera(this));
		m_cameras.Add(new IntroCamera(this));
		m_cameras.Add(new TppCamera(this));
		m_cameras.Add(new DebugCamera(this));
		m_cameras.Add(new OrbitCamera(this));
		m_cameras.Add(new FixedCamera(this));
		m_cameras.Add(new LoadingCamera(this));
		m_activeCamera = FindCamera<LoadingCamera>();
	}

	public T FindCamera<T>(bool throwOnError = true) where T : Camera
	{
		T val = (T)m_cameras.FirstOrDefault((Camera c) => c is T);
		if (val != null || !throwOnError)
		{
			return val;
		}
		throw new InvalidOperationException($"Camera with type \"{typeof(T).Name}\" not found.");
	}

	public bool IsEntityTarget(Entity entity)
	{
		if (Target != null)
		{
			return Target.Entity == entity;
		}
		return false;
	}

	public bool IsEntityFirstPersonTarget(Entity entity)
	{
		if (IsEntityTarget(entity))
		{
			return ActiveCamera is FppCamera;
		}
		return false;
	}

	public override void Update()
	{
		WidgetInputDevice widgetInputDevice = DetermineInputDevices();
		if (WidgetsHierarchyInput == null || WidgetsHierarchyInput.Devices != widgetInputDevice)
		{
			WidgetsHierarchyInput = new WidgetInput(widgetInputDevice);
		}
		if ((widgetInputDevice & WidgetInputDevice.MultiMice) != 0 && (widgetInputDevice & WidgetInputDevice.Mouse) == 0)
		{
			WidgetsHierarchyInput.UseSoftMouseCursor = true;
		}
		else
		{
			WidgetsHierarchyInput.UseSoftMouseCursor = false;
		}
		if (GuiWidget.ParentWidget == null)
		{
			UpdateWidgetsHierarchy(GuiWidget);
		}
	}

    public WidgetInputDevice DetermineInputDevices()
	{
		bool flag = false;
		foreach (PlayerData playersDatum in PlayerData.SubsystemPlayers.PlayersData)
		{
			if ((playersDatum.InputDevice & WidgetInputDevice.MultiMice) != 0)
			{
				flag = true;
			}
		}
		WidgetInputDevice widgetInputDevice = WidgetInputDevice.None;
		foreach (WidgetInputDevice allInputDevice in PlayerScreen.AllInputDevices)
		{
			if (!flag || allInputDevice != (WidgetInputDevice.Keyboard | WidgetInputDevice.Mouse))
			{
				widgetInputDevice |= allInputDevice;
			}
		}
		if (PlayerData.SubsystemPlayers.PlayersData.Count > 0 && PlayerData == PlayerData.SubsystemPlayers.PlayersData[0])
		{
			WidgetInputDevice widgetInputDevice2 = WidgetInputDevice.None;
			foreach (PlayerData playersDatum2 in PlayerData.SubsystemPlayers.PlayersData)
			{
				if (playersDatum2 != PlayerData)
				{
					widgetInputDevice2 |= playersDatum2.InputDevice;
				}
			}
			return (widgetInputDevice & ~widgetInputDevice2) | WidgetInputDevice.Touch | PlayerData.InputDevice;
		}
		WidgetInputDevice widgetInputDevice3 = WidgetInputDevice.None;
		foreach (PlayerData playersDatum3 in PlayerData.SubsystemPlayers.PlayersData)
		{
			if (playersDatum3 == PlayerData)
			{
				break;
			}
			widgetInputDevice3 |= playersDatum3.InputDevice;
		}
		return (PlayerData.InputDevice & ~widgetInputDevice3) | WidgetInputDevice.Touch;
	}

	public bool IsCameraAllowed(Camera camera)
	{
		if (camera is LoadingCamera)
		{
			return false;
		}
		return true;
	}
}
