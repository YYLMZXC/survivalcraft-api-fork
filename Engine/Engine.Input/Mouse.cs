using OpenTK.Input;
using System;
using System.Drawing;
using Engine.Handlers;

namespace Engine.Input
{
	public static class Mouse
	{
		private static Point2? m_lastMousePosition;

		private static int? m_lastMouseWheelValue;
		
		private static bool[] m_mouseButtonsDownArray;

		private static bool[] m_mouseButtonsDownOnceArray;

		public static Point2 MouseMovement
		{
			get;
			private set;
		}

		public static int MouseWheelMovement
		{
			get;
			private set;
		}

		public static Point2? MousePosition
		{
			get;
			private set;
		}

		public static bool IsMouseVisible
		{
			get;
			set;
		}

		public static event Action<MouseEvent>? MouseMove;

		public static event Action<MouseButtonEvent>? MouseDown;

		public static event Action<MouseButtonEvent>? MouseUp;

		public static IMouseHandler? MouseServicesCollection
		{
			get;
			set;
		}

		private static string HandlerNullWarningString { get; } =
			$"{typeof(Mouse).FullName}.{nameof(MouseServicesCollection)} 未初始化";
		public static void SetMousePosition(int x, int y)
		{
			if (MouseServicesCollection is null)
			{
				Log.Warning(HandlerNullWarningString);
				return;
			}
			
			MouseServicesCollection.SetMousePosition(x, y);
		}

		internal static void Initialize()
		{
			if (MouseServicesCollection is null)
			{
				Log.Warning(HandlerNullWarningString);
				return;
			}
			#if android
			#else
			Window.GameWindow.MouseDown += MouseDownHandler;
			Window.GameWindow.MouseUp += MouseUpHandler;
			Window.GameWindow.MouseMove += MouseMoveHandler;
			#endif
		}

		private static void MouseUpHandler(object? sender, MouseButtonEventArgs e)
		{
			if (MouseServicesCollection is null)
			{
				Log.Warning(HandlerNullWarningString);
				return;
			}
			
			MouseServicesCollection.MouseUpHandler(sender, e, m_mouseButtonsDownArray, MouseUp);
		}

		private static void MouseMoveHandler(object? sender, MouseMoveEventArgs e)
		{
			if (MouseServicesCollection is null)
			{
				Log.Warning(HandlerNullWarningString);
				return;
			}
			
			MouseServicesCollection.MouseMoveHandler(sender, e, MouseMove, out var newPosition);
			MousePosition = newPosition;
		}

		private static void MouseDownHandler(object? sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			if (MouseServicesCollection is null)
			{
				Log.Warning(HandlerNullWarningString);
				return;
			}
			
			MouseServicesCollection.MouseDownHandler(sender, mouseButtonEventArgs, m_mouseButtonsDownArray,
				m_mouseButtonsDownOnceArray, MouseDown);
		}
		internal static void Dispose()
		{
		}

		internal static void BeforeFrame()
		{
			if (Window.IsActive)
			{
				#if android
				#else
				Window.GameWindow.CursorVisible = IsMouseVisible;
				#endif
				MouseState state = OpenTK.Input.Mouse.GetState();
				if (m_lastMousePosition.HasValue)
				{
					MouseMovement = new Point2(state.X - m_lastMousePosition.Value.X, state.Y - m_lastMousePosition.Value.Y);
				}
				if (m_lastMouseWheelValue.HasValue)
				{
					MouseWheelMovement = 120 * (state.Wheel - m_lastMouseWheelValue.Value);
				}
				m_lastMousePosition = new Point2(state.X, state.Y);
				m_lastMouseWheelValue = state.Wheel;
			}
			else
			{
				m_lastMousePosition = null;
				m_lastMouseWheelValue = null;
			}
		}

		static Mouse()
		{
			m_mouseButtonsDownArray = new bool[Enum.GetValues(typeof(MouseButton)).Length];
			m_mouseButtonsDownOnceArray = new bool[Enum.GetValues(typeof(MouseButton)).Length];
			IsMouseVisible = true;
		}

		public static bool IsMouseButtonDown(MouseButton mouseButton)
		{
			return m_mouseButtonsDownArray[(int)mouseButton];
		}

		public static bool IsMouseButtonDownOnce(MouseButton mouseButton)
		{
			return m_mouseButtonsDownOnceArray[(int)mouseButton];
		}

		public static void Clear()
		{
			for (int i = 0; i < m_mouseButtonsDownArray.Length; i++)
			{
				m_mouseButtonsDownArray[i] = false;
				m_mouseButtonsDownOnceArray[i] = false;
			}
		}

		internal static void AfterFrame()
		{
			for (int i = 0; i < m_mouseButtonsDownOnceArray.Length; i++)
			{
				m_mouseButtonsDownOnceArray[i] = false;
			}
			if (!IsMouseVisible)//处于三维模式
			{
				MousePosition = null;
				if (Window.IsActive)//by把红色赋予黑海 1003705691
					SetMousePosition(Window.Size.X / 2, Window.Size.Y / 2);//鼠标自动归位
			}
		}
	}
}