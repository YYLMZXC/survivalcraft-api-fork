using Engine;
using Engine.Handlers;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class TouchHandler : ITouchHandler
{
    public void HandleTouchEvent(object motionEvent, out int id, out Point2 position)
    {
        id = 0;
        position = -Point2.One;
    }
}