using Game.Handlers;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class ClipboardHandler : IClipboardHandler
{
    public string Text
    {
        get => Clipboard.GetText();
        set => Clipboard.SetText(value);
    }
}