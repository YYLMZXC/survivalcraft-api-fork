using Engine.Audio;
using Engine.Graphics;
using Engine.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using System;
using System.Drawing;
using System.Reflection;
using UIKit;

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

		private static State m_state;

        public static Point2 ScreenSize
        {
            get;
            set;
        }
		public static UIView UIView { get; set; }
        public static UIViewController uIViewController { get; set; }

        public static WindowMode WindowMode
        {
            get { return WindowMode.Fullscreen; }
            set { }
        }

        public static Point2 Size
        {
			get;
			set;
        }

        public static int PresentationInterval = 1;

		/// <summary>
		/// IOS的视口修正
		/// </summary>
		public static int PixelScale = 1;

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
			Toolkit.Init();
		}

		public static void Run(int width = 0, int height = 0, WindowMode windowMode = WindowMode.Fixed, string title = "")
		{
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
			{
				Exception ex = args.ExceptionObject as Exception;
				if (ex == null)
				{
					ex = new Exception($"Unknown exception. Additional information: {args.ExceptionObject}");
				}
				UnhandledExceptionInfo unhandledExceptionInfo = new UnhandledExceptionInfo(ex);
				UnhandledException?.Invoke(unhandledExceptionInfo);
				if (!unhandledExceptionInfo.IsHandled)
				{
					Log.Error("Application terminating due to unhandled exception {0}", unhandledExceptionInfo.Exception);
					Environment.Exit(1);
				}
			};
			WindowMode = WindowMode.Fullscreen;
			GL.GetInteger(GetPName.RedBits, out int data);
			GL.GetInteger(GetPName.RedBits, out data);
			GL.GetInteger(GetPName.GreenBits, out int data2);
			GL.GetInteger(GetPName.BlueBits, out int data3);
			GL.GetInteger(GetPName.AlphaBits, out int data4);
			GL.GetInteger(GetPName.DepthBits, out int data5);
			GL.GetInteger(GetPName.StencilBits, out int data6);
			Log.Information("OpenGL framebuffer created, R={0} G={1} B={2} A={3}, D={4} S={5}", data, data2, data3, data4, data5, data6);
		}

		public static void Close()
		{
		}

		public static void LoadHandler()
		{
			InitializeAll();
			m_state = State.Inactive;
            Created?.Invoke();
            if (m_state == State.Inactive)
			{
				m_state = State.Active;
                Activated?.Invoke();                
			}
		}

        public static void FocusedChangedHandler(bool focus)
		{
			if (focus)
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
			Touch.Clear();
		}

        public static void ClosedHandler()
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
			DisposeAll();
		}
		public static void RenderFrameHandler()
		{
			BeforeFrameAll();
			Frame?.Invoke();
            AfterFrameAll();
		}

        private static void InitializeAll()
		{
			Dispatcher.Initialize();
			Display.Initialize();
			Touch.Initialize();
			Mixer.Initialize();
		}

		private static void DisposeAll()
		{
			Dispatcher.Dispose();
			Display.Dispose();
			Touch.Dispose();
			Mixer.Dispose();
		}

		private static void BeforeFrameAll()
		{
			Time.BeforeFrame();
			Dispatcher.BeforeFrame();
			Display.BeforeFrame();
			Touch.BeforeFrame();
			Mixer.BeforeFrame();
		}

		private static void AfterFrameAll()
		{
			Time.AfterFrame();
			Dispatcher.AfterFrame();
			Display.AfterFrame();
			Touch.AfterFrame();
			Mixer.AfterFrame();
		}
	}
}
