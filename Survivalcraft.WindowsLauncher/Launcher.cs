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
        BitmapFont.BitmapFontHandler = new BitmapFontHandler();
        ClipboardManager.ClipboardManagerHandler = new ClipboardHandler();
        ExternalContentManager.ExternalContentManagerHandler = new ExternalContentManagerHandler();
        HardwareManager.HardwareManagerHandler = new HardwareManagerHandler();
        LitShader.LitShaderHandler = new LitShaderHandler();
        Mouse.MouseHandler = new MouseHandler();
        MusicManager.MusicManagerHandler = new MusicManagerHandler();
        ScreenCaptureManager.ScreenCaptureManagerHandler = new ScreenCaptureManagerHandler();
        Touch.TouchHandler = new TouchHandler();
        UnlitShader.UnlitShaderHandler = new UnlitShaderHandler();
        Utilities.UtilitiesHandler = new UtilitiesHandler();
        WebBrowserManager.WebBrowserManagerHandler = new WebBrowserManagerHandler();
        WebManager.WebManagerHandler = new WebManagerHandler();
    }
}