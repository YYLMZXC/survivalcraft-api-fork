﻿using System;
using System.Drawing;
using OpenTK.Input;

namespace Engine.Input
{
	public static class Mouse
	{
#if !ANDROID
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
#if !ANDROID
			Point point = Window.m_gameWindow.PointToScreen(new Point(x, y));
			OpenTK.Input.Mouse.SetPosition(point.X, point.Y);
#endif
		}

        internal static void Initialize()
		{
#if !ANDROID
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
#if !ANDROID
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

#if !ANDROID
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

		public static MouseButton TranslateMouseButton(OpenTK.Input.MouseButton mouseButton)
		{
            return mouseButton switch
            {
                OpenTK.Input.MouseButton.Left => MouseButton.Left,
                OpenTK.Input.MouseButton.Right => MouseButton.Right,
                OpenTK.Input.MouseButton.Middle => MouseButton.Middle,
                OpenTK.Input.MouseButton.Button1=>MouseButton.Ext1,
                OpenTK.Input.MouseButton.Button2 => MouseButton.Ext2,
                _ => (MouseButton)(-1),
            };
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
			if (!IsMouseVisible)
			{
				MousePosition = null;
#if !ANDROID
                if (Window.m_gameWindow.Focused)
                {
                    Window.m_gameWindow.CursorGrabbed = true;
                }
            }
            else
            {
                Window.m_gameWindow.CursorGrabbed = false;
#endif
            }
        }

        public static void ProcessMouseDown(MouseButton mouseButton, Point2 position)
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

        public static void ProcessMouseUp(MouseButton mouseButton, Point2 position)
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

        public static void ProcessMouseMove(Point2 position)
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