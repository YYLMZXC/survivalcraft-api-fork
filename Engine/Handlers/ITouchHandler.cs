namespace Engine.Handlers;

public interface ITouchHandler
{
    void HandleTouchEvent(object motionEvent, out int i, out Point2 point2);
}