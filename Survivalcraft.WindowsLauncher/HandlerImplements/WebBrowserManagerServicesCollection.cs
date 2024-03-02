using Game.Handlers;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class WebBrowserManagerServicesCollection : IWebBrowserManagerHandler
{
    public void LaunchBrowser(string url)
    {
        System.Diagnostics.Process.Start(url);
    }
}