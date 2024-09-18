using System.Diagnostics;
using Engine;
using Engine.Graphics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Engine.Input;

#if desktop
using ImeSharp;
#endif

namespace Game
{
	public static class Program
	{
		public static double m_frameBeginTime;

		public static double m_cpuEndTime;

		public static List<Uri> m_urisToHandle = [];
#nullable enable
		public static string? SystemLanguage { get; set; }
#nullable disable
		public static float LastFrameTime { get; set; }

		public static float LastCpuFrameTime { get; set; }

		public static event Action<Uri> HandleUri;
		public static string Title = "生存战争2.3插件版_";

#if desktop
		private static void Main(string[] args)
		{
			
			if(args != null && args.Length > 0)
			{
				//拖动到exe的文件解析
				ExternalContentManager.openFilePath = args[0];
			}
			
			
			// Process.Start("C:\\Windows\\System32\\msg.exe",  "/server:127.0.0.1 * \"此版本为预览版 不建议长期使用");
			Window.Created += () =>
			{
				InputMethod.Initialize(Process.GetCurrentProcess().MainWindowHandle, true);
				InputMethod.Enabled = false;
			};
			EntryPoint();
			AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
				//在程序目录下面寻找dll,解决部分设备找不到目录下程序集的问题
				var location = new FileInfo(typeof(Program).Assembly.Location).Directory!.FullName;
				return Assembly.LoadFrom(Path.Combine(location, e.Name));
			};

			//RootCommand rootCommand =
			//[
			//    new Option<string>(["-m", "--mod-import"], ""),
			//    new Option<string>(["-l", "--language"], "")
			//];
		}
#endif
		
		[STAThread]
		public static void EntryPoint()
		{
			try
			{
				SystemLanguage = CultureInfo.CurrentUICulture.Name;
			}
			catch
			{
			}

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


			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
			Log.RemoveAllLogSinks();
			Log.AddLogSink(new GameLogSink());
#if DEBUG
			Log.AddLogSink(new ConsoleLogSink());
			Title = "[DEBUG]" + Title;
#endif
			Window.HandleUri += HandleUriHandler;
			Window.Deactivated += DeactivatedHandler;
			Window.Frame += FrameHandler;
			Display.DeviceReset += ContentManager.Display_DeviceReset;
			Window.UnhandledException += delegate(UnhandledExceptionInfo e)
			{
				ExceptionManager.ReportExceptionToUser("Unhandled exception.", e.Exception);
				e.IsHandled = true;
			};
			Window.Run(0, 0, WindowMode.Resizable,
				 Title+ ModsManager.ApiVersionString);
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
			Log.Information(
				$"Survivalcraft starting up at {DateTime.Now}, Version={VersionsManager.Version}, BuildConfiguration={VersionsManager.BuildConfiguration}, Platform={VersionsManager.Platform}, Storage.AvailableFreeSpace={Storage.FreeSpace / 1024 / 1024}MB, ApproximateScreenDpi={ScreenResolutionManager.ApproximateScreenDpi:0.0}, ApproxScreenInches={ScreenResolutionManager.ApproximateScreenInches:0.0}, ScreenResolution={Window.Size}, ProcessorsCount={Environment.ProcessorCount}, RAM={Utilities.GetTotalAvailableMemory() / 1024 / 1024}MB, 64bit={Marshal.SizeOf<IntPtr>() == 8}");
			try
			{
				SettingsManager.Initialize();
				JsInterface.Initiate();
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
#if android
			// TODO: 待完成。
			// EngineInputConnection.Implement = new SurvivalcraftInputConnection();
#endif
			
			LastFrameTime = (float)(Time.RealTime - m_frameBeginTime);
			LastCpuFrameTime = (float)(m_cpuEndTime - m_frameBeginTime);
			m_frameBeginTime = Time.RealTime;
			if (Keyboard.IsKeyDown(Key.F11))
			{
				SettingsManager.WindowMode = SettingsManager.WindowMode == WindowMode.Fullscreen
					? WindowMode.Resizable
					: WindowMode.Fullscreen;
			}

			try
			{
				if (ExceptionManager.Error == null)
				{
					foreach (Uri obj in m_urisToHandle)
					{
						HandleUri?.Invoke(obj);
					}

					m_urisToHandle.Clear();
					PerformanceManager.Update();
					MotdManager.Update();
					MusicManager.Update();
					ScreensManager.Update();
					DialogsManager.Update();
					JsInterface.Update();
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
				Log.Error("sine:" + e2);
				ScreensManager.SwitchScreen("MainMenu");
			}
		}
	}
}