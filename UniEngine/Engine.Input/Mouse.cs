using OpenTK.Input;
using System;
using System.Drawing;

namespace Engine.Input
{
	public static class Mouse
	{
#if desktop
		private static Point2? m_lastMousePosition;

		private static int? m_lastMouseWheelValue;
#endif
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

		public static event Action<MouseEvent> MouseMove;

		public static event Action<MouseButtonEvent> MouseDown;

		public static event Action<MouseButtonEvent> MouseUp;

		public static void SetMousePosition(int x, int y)
		{
#if desktop
			Point point = Window.m_gameWindow.PointToScreen(new Point(x, y));
			OpenTK.Input.Mouse.SetPosition(point.X, point.Y);
#endif
		}

		internal static void Initialize()
		{
#if desktop
			Window.m_gameWindow.MouseDown += MouseDownHandler;
			Window.m_gameWindow.MouseUp += MouseUpHandler;
			Window.m_gameWindow.MouseMove += MouseMoveHandler;
#endif
		}

		internal static void Dispose()
		{
		}

		internal static void BeforeFrame()
		{
#if desktop
			if (Window.IsActive)
			{
				Window.m_gameWindow.CursorVisible = IsMouseVisible;
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
#endif
		}

#if desktop
		private static void MouseDownHandler(object sender, MouseButtonEventArgs e)
		{
			MouseButton mouseButton = TranslateMouseButton(e.Button);
			if (mouseButton != (MouseButton)(-1))
			{
				ProcessMouseDown(mouseButton, new Point2(e.Position.X, e.Position.Y));
			}
		}

		private static void MouseUpHandler(object sender, MouseButtonEventArgs e)
		{
			MouseButton mouseButton = TranslateMouseButton(e.Button);
			if (mouseButton != (MouseButton)(-1))
			{
				ProcessMouseUp(mouseButton, new Point2(e.Position.X, e.Position.Y));
			}
		}

		private static void MouseMoveHandler(object sender, MouseMoveEventArgs e)
		{
			ProcessMouseMove(new Point2(e.Position.X, e.Position.Y));
		}

		private static MouseButton TranslateMouseButton(OpenTK.Input.MouseButton mouseButton)
		{
			switch (mouseButton)
			{
				case OpenTK.Input.MouseButton.Left:
					return MouseButton.Left;
				case OpenTK.Input.MouseButton.Right:
					return MouseButton.Right;
				case OpenTK.Input.MouseButton.Middle:
					return MouseButton.Middle;
				default:
					return (MouseButton)(-1);
			}
		}
#endif

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
			if (!IsMouseVisible)//������άģʽ
			{
				MousePosition = null;
				if (Engine.Window.IsActive)//by�Ѻ�ɫ����ں� 1003705691
					Mouse.SetMousePosition(Window.Size.X / 2, Window.Size.Y / 2);//����Զ���λ
			}
		}

		private static void ProcessMouseDown(MouseButton mouseButton, Point2 position)
		{
			if (Window.IsActive && !Keyboard.IsKeyboardVisible)
			{
				m_mouseButtonsDownArray[(int)mouseButton] = true;
				m_mouseButtonsDownOnceArray[(int)mouseButton] = true;
				if (IsMouseVisible && Mouse.MouseDown != null)
				{
					Mouse.MouseDown(new MouseButtonEvent
					{
						Button = mouseButton,
						Position = position
					});
				}
			}
		}

		private static void ProcessMouseUp(MouseButton mouseButton, Point2 position)
		{
			if (Window.IsActive && !Keyboard.IsKeyboardVisible)
			{
				m_mouseButtonsDownArray[(int)mouseButton] = false;
				if (IsMouseVisible && Mouse.MouseUp != null)
				{
					Mouse.MouseUp(new MouseButtonEvent
					{
						Button = mouseButton,
						Position = position
					});
				}
			}
		}

		private static void ProcessMouseMove(Point2 position)
		{
			if (Window.IsActive && !Keyboard.IsKeyboardVisible && IsMouseVisible)
			{
				MousePosition = position;
				Mouse.MouseMove?.Invoke(new MouseEvent
				{
					Position = position
				});
			}
		}
	}
}