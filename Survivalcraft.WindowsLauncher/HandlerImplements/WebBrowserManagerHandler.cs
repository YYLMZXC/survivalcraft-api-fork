using Game.Handlers;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class WebBrowserManagerHandler : IWebBrowserManagerHandler
{
    public void LaunchBrowser(string url)
    {
        System.Diagnostics.Process.Start(url);
    }
}