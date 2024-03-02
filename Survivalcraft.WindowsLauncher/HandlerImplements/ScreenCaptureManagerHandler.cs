using Engine;
using Engine.Graphics;
using Engine.Media;
using Game;
using Game.Handlers;
using Image = Engine.Media.Image;
using Rectangle = Engine.Rectangle;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class ScreenCaptureManagerHandler : IScreenCaptureManagerHandler
{
    public void SaveImage(RenderTarget2D renderTarget2D, string filename)
    {
        var image = new Image(renderTarget2D.Width, renderTarget2D.Height); 
        renderTarget2D.GetData(image.Pixels, 0, new Rectangle(0, 0, renderTarget2D.Width, renderTarget2D.Height));
        if (!Storage.DirectoryExists(ScreenCaptureManager.ScreenshotDir))
        { 
            Storage.CreateDirectory(ScreenCaptureManager.ScreenshotDir);
        }
        string path = Storage.CombinePaths(ScreenCaptureManager.ScreenshotDir, filename);
        using (Stream stream = Storage.OpenFile(path, OpenFileMode.CreateOrOpen))
        {
            Image.Save(image, stream, ImageFileFormat.Jpg, saveAlpha: false);
        }
    }
}