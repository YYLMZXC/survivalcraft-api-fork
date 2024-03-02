using Engine.Handlers;
using Engine.Media;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class BitmapFontServicesCollection : IBitmapFontServicesCollection
{
    public BitmapFont DebugFont
    {
        get
        {
            using Stream stream =
                typeof(BitmapFont).Assembly.GetManifestResourceStream("Engine.Resources.Debugfont.png")!;
            using Stream stream2 =
                typeof(BitmapFont).Assembly.GetManifestResourceStream("Engine.Resources.Debugfont.lst")!;

            return BitmapFont.Initialize(stream, stream2);
        }
    }
}