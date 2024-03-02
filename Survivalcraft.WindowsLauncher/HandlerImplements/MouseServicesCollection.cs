using Engine;
using Engine.Handlers;
using Engine.Input;
using OpenTK.Input;
using Keyboard = Engine.Input.Keyboard;
using Mouse = Engine.Input.Mouse;
using MouseButton = Engine.Input.MouseButton;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class MouseServicesCollection : IMouseHandler
{
    public void SetMousePosition(int x, int y)
    {
        Point point = Window.GameWindow.PointToScreen(new Point(x, y));
        OpenTK.Input.Mouse.SetPosition(point.X, point.Y);
    }

    public void MouseMoveHandler(object? sender, MouseMoveEventArgs args,  Action<MouseEvent>? mouseMove, out Point2? mousePosition)
    {
        ProcessMouseMove(new Point2(args.Position.X, args.Position.Y), mouseMove, out mousePosition);
    }

    public void MouseUpHandler(object? sender, MouseButtonEventArgs args, bool[] mouseButtonsDownArray, Action<MouseButtonEvent>? mouseUp)
    {
        var mouseButton = TranslateMouseButton(args.Button);
        if ((int)mouseButton != -1)
        {
            ProcessMouseUp(mouseButton, new Point2(args.Position.X, args.Position.Y), mouseButtonsDownArray, mouseUp);
        }
    }

    public void MouseDownHandler(object? sender, MouseButtonEventArgs args, bool[] mouseButtonsDownArray, bool[] mouseButtonsDownOnceArray, Action<MouseButtonEvent>? mouseDown)
    {
        var mouseButton = TranslateMouseButton(args.Button);
        if ((int)mouseButton != -1)
        {
            ProcessMouseDown(mouseButton, new Point2(args.Position.X, args.Position.Y), mouseButtonsDownArray, mouseButtonsDownOnceArray, mouseDown);
        }
    }

    private static MouseButton TranslateMouseButton(OpenTK.Input.MouseButton mouseButton)
    {
        return mouseButton switch
        {
            OpenTK.Input.MouseButton.Left => MouseButton.Left,
            OpenTK.Input.MouseButton.Right => MouseButton.Right,
            OpenTK.Input.MouseButton.Middle => MouseButton.Middle,
            _ => (MouseButton)(-1)
        };
    }

    private static void ProcessMouseDown(MouseButton mouseButton, Point2 position, bool[] mouseButtonsDownArray, bool[] mouseButtonsDownOnceArray, Action<MouseButtonEvent>? buttonDown)
    {
        if (Window.IsActive && !Keyboard.IsKeyboardVisible)
        {
            mouseButtonsDownArray[(int)mouseButton] = true;
            mouseButtonsDownOnceArray[(int)mouseButton] = true;
            if (Mouse.IsMouseVisible)
            {
                buttonDown?.Invoke(new MouseButtonEvent
                {
                    Button = mouseButton,
                    Position = position
                });
            }
        }
    }

    private static void ProcessMouseUp(MouseButton mouseButton, Point2 position, bool[] mouseButtonsDownArray, Action<MouseButtonEvent>? mouseUp)
    {
        if (!Window.IsActive || Keyboard.IsKeyboardVisible) return;
        
        mouseButtonsDownArray[(int)mouseButton] = false;
        if (Mouse.IsMouseVisible)
        {
            mouseUp?.Invoke(new MouseButtonEvent
            {
                Button = mouseButton,
                Position = position
            });
        }
    }

    private static void ProcessMouseMove(Point2 position, Action<MouseEvent>? mouseMove, out Point2? mousePosition)
    {
        if (!Window.IsActive || Keyboard.IsKeyboardVisible || !Mouse.IsMouseVisible)
        {
            mousePosition = Mouse.MousePosition;
        }
        
        mousePosition = position;
        mouseMove?.Invoke(new MouseEvent
        {
            Position = position
        });
    }
}