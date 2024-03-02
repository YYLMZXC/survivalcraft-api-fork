using Engine;
using Engine.Graphics;
using Engine.Handlers;
using Engine.Input;
using Engine.Media;
using Game;
using Survivalcraft.WindowsLauncher.HandlerImplements;

namespace Survivalcraft.WindowsLauncher;

internal static class Launcher
{
    [STAThread]
    private static void Main()
    {
        InitializeHandlers();
        Program.Main();
    }

    private static void InitializeHandlers()
    {
        BitmapFont.BitmapFontServicesCollection = new BitmapFontServicesCollection();
        ClipboardManager.ClipboardManagerServicesCollection = new ClipboardServicesCollection();
        ExternalContentManager.ExternalContentManagerServicesCollection = new ExternalContentManagerServicesCollection();
        HardwareManager.HardwareManagerServicesCollection = new HardwareManagerServicesCollection();
        LitShader.LitShaderServicesCollection = new LitShaderServicesCollection();
        Mouse.MouseServicesCollection = new MouseServicesCollection();
        MusicManager.MusicManagerServicesCollection = new MusicManagerServicesCollection();
        ScreenCaptureManager.ScreenCaptureManagerServicesCollection = new ScreenCaptureManagerServicesCollection();
        Touch.TouchServicesCollection = new TouchServicesCollection();
        UnlitShader.UnlitShaderServicesCollection = new UnlitShaderServicesCollection();
        Utilities.UtilitiesServicesCollection = new UtilitiesServicesCollection();
        WebBrowserManager.WebBrowserManagerServicesCollection = new WebBrowserManagerServicesCollection();
        WebManager.WebManagerServicesCollection = new WebManagerServicesCollection();
    }
}