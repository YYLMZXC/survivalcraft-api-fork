using System;
using Engine.Input;
using OpenTK.Input;

namespace Engine.Handlers;

public interface IMouseHandler
{
    void SetMousePosition(int x, int y);

    void MouseMoveHandler(object? sender, MouseMoveEventArgs args, Action<MouseEvent>? mouseMove,
        out Point2? mousePosition);

    void MouseUpHandler(object? sender, MouseButtonEventArgs args, bool[] mouseButtonsDownArray,
        Action<MouseButtonEvent>? mouseUp);
    
    void MouseDownHandler(object? sender, MouseButtonEventArgs args, bool[] mouseButtonsDownArray, bool[] mouseButtonsDownOnceArray, Action<MouseButtonEvent>? mouseDown);
}