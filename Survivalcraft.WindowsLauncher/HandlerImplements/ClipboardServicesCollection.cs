using Game.Handlers;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class ClipboardServicesCollection : IClipboardHandler
{
    public string Text
    {
        get => Clipboard.GetText();
        set => Clipboard.SetText(value);
    }
}