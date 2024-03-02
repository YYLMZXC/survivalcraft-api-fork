using Engine.Audio;
using Engine.Graphics;
using Engine.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using System;
using System.Drawing;
using System.Reflection;

namespace Engine
{
	public static class Window
	{
		private enum State
		{
			Uncreated,
			Inactive,
			Active
		}

		public static GameWindow GameWindow;

		private static bool m_closing;

		private static float m_dpiScale;

		private static int? m_swapInterval;

		private static State m_state;

		public static Point2 ScreenSize
		{
			get
			{
				DisplayDevice @default = DisplayDevice.Default;
				if (Configuration.RunningOnMacOS)
				{
					return new Point2((int)MathUtils.Round((float)@default.Width * m_dpiScale), (int)MathUtils.Round((float)@default.Height * m_dpiScale));
				}
				return new Point2(@default.Width, @default.Height);
			}
		}

		public static WindowMode WindowMode
		{
			get
			{
				VerifyWindowOpened();
				if (GameWindow.WindowState == WindowState.Fullscreen)
				{
					return WindowMode.Fullscreen;
				}
				if (GameWindow.WindowBorder != 0)
				{
					return WindowMode.Fixed;
				}
				return WindowMode.Resizable;
			}
			set
			{
				VerifyWindowOpened();
				switch (value)
				{
					case WindowMode.Fixed:
						GameWindow.WindowBorder = WindowBorder.Fixed;
						if (GameWindow.WindowState == WindowState.Fullscreen)
						{
							GameWindow.WindowState = WindowState.Normal;
						}
						break;
					case WindowMode.Resizable:
						GameWindow.WindowBorder = WindowBorder.Resizable;
						if (GameWindow.WindowState == WindowState.Fullscreen)
						{
							GameWindow.WindowState = WindowState.Normal;
						}
						break;
					case WindowMode.Borderless:
						GameWindow.WindowBorder = WindowBorder.Hidden;
						if (GameWindow.WindowState == WindowState.Fullscreen)
						{
							GameWindow.WindowState = WindowState.Normal;
						}
						break;
					case WindowMode.Fullscreen:
						GameWindow.WindowState = WindowState.Fullscreen;
						break;
				}
			}
		}

		public static Point2 Position
		{
			get
			{
				VerifyWindowOpened();
				return new Point2(GameWindow.Location.X, GameWindow.Location.Y);
			}
			set
			{
				VerifyWindowOpened();
				GameWindow.Location = new Point(value.X, value.Y);
			}
		}

		public static Point2 Size
		{
			get
			{
				VerifyWindowOpened();
				return new Point2(GameWindow.ClientSize.Width, GameWindow.ClientSize.Height);
			}
			set
			{
				VerifyWindowOpened();
				GameWindow.ClientSize = new Size(value.X, value.Y);
			}
		}

		public static string Title
		{
			get
			{
				VerifyWindowOpened();
				return GameWindow.Title;
			}
			set
			{
				VerifyWindowOpened();
				GameWindow.Title = value;
			}
		}

		public static Icon Icon
		{
			get
			{
				VerifyWindowOpened();
				return GameWindow.Icon;
			}
			set
			{
				VerifyWindowOpened();
				GameWindow.Icon = value;
			}
		}

		public static int PresentationInterval
		{
			get
			{
				VerifyWindowOpened();
				if (!m_swapInterval.HasValue)
				{
					m_swapInterval = GameWindow.Context.SwapInterval;
				}
				return m_swapInterval.Value;
			}
			set
			{
				VerifyWindowOpened();
				value = MathUtils.Clamp(value, 0, 4);
				if (value != PresentationInterval)
				{
					GameWindow.Context.SwapInterval = value;
					m_swapInterval = value;
				}
			}
		}

		public static bool IsCreated => m_state != State.Uncreated;

		public static bool IsActive => m_state == State.Active;

		public static event Action Created;

		public static event Action Resized;

		public static event Action Activated;

		public static event Action Deactivated;

		public static event Action Closed;

		public static event Action Frame;

		public static event Action<UnhandledExceptionInfo> UnhandledException;

		public static event Action<Uri> HandleUri;

		static Window()
		{
			m_dpiScale = 1f;
			Toolkit.Init();
		}

		public static void Run(int width = 0, int height = 0, WindowMode windowMode = WindowMode.Fixed, string title = "")
		{
			if (GameWindow != null)
			{
				throw new InvalidOperationException("Window is already opened.");
			}
			if ((width != 0 || height != 0) && (width <= 0 || height <= 0))
			{
				throw new ArgumentOutOfRangeException("size");
			}
			AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
			{
				Exception ex = args.ExceptionObject as Exception;
				if (ex == null)
				{
					ex = new Exception($"Unknown exception. Additional information: {args.ExceptionObject}");
				}
				UnhandledExceptionInfo unhandledExceptionInfo = new(ex);
				UnhandledException?.Invoke(unhandledExceptionInfo);
				if (!unhandledExceptionInfo.IsHandled)
				{
					Log.Error("Application terminating due to unhandled exception {0}", unhandledExceptionInfo.Exception);
					Environment.Exit(1);
				}
			};
			GraphicsMode mode = new(new OpenTK.Graphics.ColorFormat(24), 16, 0, 0, OpenTK.Graphics.ColorFormat.Empty, 2);
			width = (width == 0) ? (ScreenSize.X * 4 / 5) : width;
			height = (height == 0) ? (ScreenSize.Y * 4 / 5) : height;
			GameWindow = new GameWindow(width, height, mode, title, GameWindowFlags.Default, DisplayDevice.Default, 2, 0, GraphicsContextFlags.Default);
			GameWindow.Icon = new Icon(typeof(Window).GetTypeInfo().Assembly.GetManifestResourceStream("Engine.Resources.icon.ico"), new Size(32, 32));
			m_dpiScale = GameWindow.ClientSize.Width / 400f;
			GameWindow.ClientSize = new Size(width, height);
			if (Configuration.RunningOnMacOS)
			{
				Point2 point = new((int)MathUtils.Round((float)ScreenSize.X / m_dpiScale), (int)MathUtils.Round((float)ScreenSize.Y / m_dpiScale));
				GameWindow.Location = new Point(MathUtils.Max((point.X - GameWindow.Size.Width) / 2, 0), MathUtils.Max((point.Y - GameWindow.Size.Height) / 2, 0));
			}
			else
			{
				GameWindow.Location = new Point(MathUtils.Max((ScreenSize.X - GameWindow.Size.Width) / 2, 0), MathUtils.Max((ScreenSize.Y - GameWindow.Size.Height) / 2, 0));
			}
			WindowMode = windowMode;
			GameWindow.Load += LoadHandler;
			GL.GetInteger(GetPName.RedBits, out int data);
			GL.GetInteger(GetPName.RedBits, out data);
			GL.GetInteger(GetPName.GreenBits, out int data2);
			GL.GetInteger(GetPName.BlueBits, out int data3);
			GL.GetInteger(GetPName.AlphaBits, out int data4);
			GL.GetInteger(GetPName.DepthBits, out int data5);
			GL.GetInteger(GetPName.StencilBits, out int data6);
			Log.Information("OpenGL framebuffer created, R={0} G={1} B={2} A={3}, D={4} S={5}", data, data2, data3, data4, data5, data6);
			GameWindow.Run();
		}

		public static void Close()
		{
			VerifyWindowOpened();
			m_closing = true;
		}

		private static void LoadHandler(object sender, EventArgs args)
		{
			InitializeAll();
			SubscribeToEvents();
			m_state = State.Inactive;
			Created?.Invoke();
			if (m_state == State.Inactive)
			{
				m_state = State.Active;
				Activated?.Invoke();
			}
		}

		private static void FocusedChangedHandler(object sender, EventArgs args)
		{
			if (GameWindow.Focused)
			{
				if (m_state == State.Inactive)
				{
					m_state = State.Active;
					Activated?.Invoke();
				}
				return;
			}
			if (m_state == State.Active)
			{
				m_state = State.Inactive;
				Deactivated?.Invoke();
			}
			Keyboard.Clear();
			Mouse.Clear();
			Touch.Clear();
		}

		private static void ClosedHandler(object sender, EventArgs args)
		{
			if (m_state == State.Active)
			{
				m_state = State.Inactive;
				Deactivated?.Invoke();
			}
			if (m_state == State.Inactive)
			{
				m_state = State.Uncreated;
				Closed?.Invoke();
			}
			UnsubscribeFromEvents();
			DisposeAll();
			GameWindow.Dispose();
			GameWindow = null;
		}

		private static void ResizeHandler(object sender, EventArgs args)
		{
			Display.Resize();
			Resized?.Invoke();
		}

		private static void RenderFrameHandler(object sender, EventArgs args)
		{
			BeforeFrameAll();
			Frame?.Invoke();
			AfterFrameAll();
			if (!m_closing)
			{
				GameWindow.Context.SwapBuffers();
			}
			else
			{
				GameWindow.Close();
			}
		}

		private static void VerifyWindowOpened()
		{
			if (GameWindow == null)
			{
				throw new InvalidOperationException("Window is not opened.");
			}
		}

		private static void SubscribeToEvents()
		{
			GameWindow.FocusedChanged += FocusedChangedHandler;
			GameWindow.Closed += ClosedHandler;
			GameWindow.Resize += ResizeHandler;
			GameWindow.RenderFrame += RenderFrameHandler;
		}

		private static void UnsubscribeFromEvents()
		{
			GameWindow.FocusedChanged -= FocusedChangedHandler;
			GameWindow.Closed -= ClosedHandler;
			GameWindow.Resize -= ResizeHandler;
			GameWindow.RenderFrame -= RenderFrameHandler;
		}

		private static void InitializeAll()
		{
			try
			{
				Dispatcher.Initialize();
				Display.Initialize();
				Keyboard.Initialize();
				Mouse.Initialize();
				Touch.Initialize();
				GamePad.Initialize();
				Mixer.Initialize();
			}
			catch (Exception ex)
			{
				Log.Error("初始化时出错: " + ex.Message);
			}

		}

		private static void DisposeAll()
		{
			Dispatcher.Dispose();
			Display.Dispose();
			Keyboard.Dispose();
			Mouse.Dispose();
			Touch.Dispose();
			GamePad.Dispose();
			Mixer.Dispose();
		}

		private static void BeforeFrameAll()
		{
			Time.BeforeFrame();
			Dispatcher.BeforeFrame();
			Display.BeforeFrame();
			Keyboard.BeforeFrame();
			Mouse.BeforeFrame();
			Touch.BeforeFrame();
			GamePad.BeforeFrame();
			Mixer.BeforeFrame();
		}

		private static void AfterFrameAll()
		{
			Time.AfterFrame();
			Dispatcher.AfterFrame();
			Display.AfterFrame();
			Keyboard.AfterFrame();
			Mouse.AfterFrame();
			Touch.AfterFrame();
			GamePad.AfterFrame();
			Mixer.AfterFrame();
		}
	}
}
