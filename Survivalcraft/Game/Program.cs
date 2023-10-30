using Engine;
using Engine.Graphics;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Game
{
	public static class Program
	{
		public static double m_frameBeginTime;

		public static double m_cpuEndTime;

		public static List<Uri> m_urisToHandle = new List<Uri>();

		public static string SystemLanguage { get; set; }

		public static float LastFrameTime
		{
			get;
			set;
		}

		public static float LastCpuFrameTime
		{
			get;
			set;
		}

		public static event Action<Uri> HandleUri;

		[STAThread]
		public static void Main()
		{
			try
			{
				SystemLanguage = CultureInfo.CurrentUICulture.Name;
			}
			catch { }
			if (string.IsNullOrEmpty(SystemLanguage))
			{
				Log.Debug(RegionInfo.CurrentRegion.DisplayName);
				if (RegionInfo.CurrentRegion.DisplayName != "United States")
				{
					SystemLanguage = "zh-CN";
				}
				else
				{
					SystemLanguage = "en-US";
				}
			}
				

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
			Log.RemoveAllLogSinks();
			Log.AddLogSink(new GameLogSink());
			Window.HandleUri += HandleUriHandler;
			Window.Deactivated += DeactivatedHandler;
			Window.Frame += FrameHandler;
			Display.DeviceReset += ContentManager.Display_DeviceReset;
			Window.UnhandledException += delegate (UnhandledExceptionInfo e)
			{
				ExceptionManager.ReportExceptionToUser("Unhandled exception.", e.Exception);
				e.IsHandled = true;
			};
			JsInterface.Initiate();
			string Error = "正常运行中";
			if(AL.GetError()!=0)
			{
				Error = "OPENAL疑似未安装!";
				WebBrowserManager.LaunchBrowser("http://www.openal.org/downloads/oalinst.zip");
			}
			Window.Run((int)(Window.ScreenSize.X / 1.2f), (int)(Window.ScreenSize.Y * 0.85f), WindowMode.Resizable, "生存战争2.3插件版NEXT" + ModsManager.APIVersion + " #" + Error);
		}

		public static void HandleUriHandler(Uri uri)
		{
			m_urisToHandle.Add(uri);
		}

		public static void DeactivatedHandler()
		{
			GC.Collect();
		}

		public static void FrameHandler()
		{
			if (Time.FrameIndex < 0)
			{
				Display.Clear(Vector4.Zero, 1f);
			}
			else if (Time.FrameIndex == 0)
			{
				Initialize();
			}
			else
			{
				Run();
			}
		}

		public static void Initialize()
		{

			Log.Information($"Survivalcraft starting up at {DateTime.Now}, Version={VersionsManager.Version}, BuildConfiguration={VersionsManager.BuildConfiguration}, Platform={VersionsManager.Platform}, Storage.AvailableFreeSpace={Storage.FreeSpace / 1024 / 1024}MB, ApproximateScreenDpi={ScreenResolutionManager.ApproximateScreenDpi:0.0}, ApproxScreenInches={ScreenResolutionManager.ApproximateScreenInches:0.0}, ScreenResolution={Window.Size}, ProcessorsCount={Environment.ProcessorCount}, RAM={Utilities.GetTotalAvailableMemory() / 1024 / 1024}MB, 64bit={Marshal.SizeOf<IntPtr>() == 8}");
			try
			{
				SettingsManager.Initialize();
				VersionsManager.Initialize();
				ExternalContentManager.Initialize();
				MusicManager.Initialize();
				ScreensManager.Initialize();
				Log.Information("Program Initialize Success");
			}
			catch (Exception e)
			{
				Log.Error(e.Message);
			}
		}

		public static void Run()
		{
			LastFrameTime = (float)(Time.RealTime - m_frameBeginTime);
			LastCpuFrameTime = (float)(m_cpuEndTime - m_frameBeginTime);
			m_frameBeginTime = Time.RealTime;
			if (Engine.Input.Keyboard.IsKeyDown(Engine.Input.Key.F11))
			{
				SettingsManager.WindowMode = SettingsManager.WindowMode == WindowMode.Fullscreen ? WindowMode.Resizable : WindowMode.Fullscreen;
			}
			try
			{
				if (ExceptionManager.Error == null)
				{
					while (m_urisToHandle.Count > 0)
					{
						Uri obj = m_urisToHandle[0];
						m_urisToHandle.RemoveAt(0);
						HandleUri?.Invoke(obj);
					}
					PerformanceManager.Update();
					MotdManager.Update();
					MusicManager.Update();
					ScreensManager.Update();
					DialogsManager.Update();
				}
				else
				{
					ExceptionManager.UpdateExceptionScreen();
				}
			}
			catch (Exception e)
			{
				ModsManager.AddException(e);
				ScreensManager.SwitchScreen("MainMenu");
			}
			m_cpuEndTime = Time.RealTime;
			try
			{
				Display.RenderTarget = null;
				if (ExceptionManager.Error == null)
				{
					ScreensManager.Draw();
					PerformanceManager.Draw();
					ScreenCaptureManager.Run();
				}
				else
				{
					ExceptionManager.DrawExceptionScreen();
				}
			}
			catch (Exception e2)
			{
				if (GameManager.Project != null) GameManager.DisposeProject();
				ExceptionManager.ReportExceptionToUser(null, e2);
				ScreensManager.SwitchScreen("MainMenu");
			}
		}
	}
}
