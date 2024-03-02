using Engine.Graphics;

namespace Game.Handlers;

public interface IScreenCaptureManagerHandler
{
    void SaveImage(RenderTarget2D renderTarget2D, string filename);
}