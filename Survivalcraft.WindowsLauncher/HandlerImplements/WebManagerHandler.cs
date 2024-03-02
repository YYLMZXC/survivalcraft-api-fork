using Engine;
using Game;
using Game.Handlers;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class WebManagerHandler : IWebManagerHandler
{
    // ReSharper disable once StringLiteralTypo
    [System.Runtime.InteropServices.DllImport("wininet.dll")]
    private static extern bool InternetGetConnectedState(out int Description, int ReservedValue);
    public bool IsInternetConnectionAvailable()
    {
        try
        {
            return InternetGetConnectedState(out _, 0);
        }
        catch (Exception e)
        {
            Log.Warning(ExceptionManager.MakeFullErrorMessage("Could not check internet connection availability.", e));
        }
        return true;
    }
}