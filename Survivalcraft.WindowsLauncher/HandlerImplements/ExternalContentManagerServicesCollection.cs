using Game;
using Game.Handlers;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class ExternalContentManagerServicesCollection : IExternalContentManagerHandler
{
    public void Initialize()
    {
        ExternalContentManager.m_providers =
            [..ExternalContentManager.m_providers, new DiskExternalContentProvider()];
    }
}