using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
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

#if desktop
		private static void Main(string[] args)
		{
			EntryPoint();
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


            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Log.RemoveAllLogSinks();
            Log.AddLogSink(new GameLogSink());
            Window.HandleUri += HandleUriHandler;
            Window.Deactivated += DeactivatedHandler;
            Window.Frame += FrameHandler;
            Display.DeviceReset += ContentManager.Display_DeviceReset;
            Window.UnhandledException += delegate(UnhandledExceptionInfo e)
            {
                ExceptionManager.ReportExceptionToUser("Unhandled exception.", e.Exception);
                e.IsHandled = true;
            };
            JsInterface.Initiate();
            Window.Run(0, 0, WindowMode.Resizable,
                "生存战争2.3插件版_" + ModsManager.APIVersion);
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