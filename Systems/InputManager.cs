using Microsoft.Xna.Framework.Input;

namespace BreakoutGame;

public static class InputManager
{
    private static KeyboardState _prevKb;
    private static KeyboardState _currKb;
    private static MouseState _currMouse;

    public static void Update()
    {
        _prevKb = _currKb;
        _currKb = Keyboard.GetState();
        _currMouse = Mouse.GetState();
    }

    public static bool IsKeyDown(Keys key) => _currKb.IsKeyDown(key);

    public static bool IsKeyPressed(Keys key) => _currKb.IsKeyDown(key) && !_prevKb.IsKeyDown(key);

    public static int MouseX => _currMouse.X;
}
