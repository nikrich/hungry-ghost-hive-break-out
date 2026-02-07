using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame.Entities;

public enum PowerUpType
{
    WidePaddle,
    MultiBall,
    ExtraLife,
    SpeedDown
}

public class PowerUp
{
    public Vector2 Position;
    public int Size = 24;
    public float FallSpeed = 200f;
    public PowerUpType Type;
    public bool IsActive = true;
    public Color Color;
    public string Label;

    private readonly Texture2D _pixel;

    public PowerUp(Texture2D pixel, Vector2 position, PowerUpType type)
    {
        _pixel = pixel;
        Position = position;
        Type = type;

        (Color, Label) = type switch
        {
            PowerUpType.WidePaddle => (new Color(33, 150, 243), "W"),
            PowerUpType.MultiBall => (new Color(156, 39, 176), "M"),
            PowerUpType.ExtraLife => (new Color(233, 30, 99), "+"),
            PowerUpType.SpeedDown => (new Color(0, 188, 212), "S"),
            _ => (Color.White, "?"),
        };
    }

    public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Size, Size);

    public void Update(float deltaTime)
    {
        Position.Y += FallSpeed * deltaTime;
        if (Position.Y > 720)
            IsActive = false;
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font)
    {
        if (!IsActive) return;
        spriteBatch.Draw(_pixel, Bounds, Color);
        Vector2 labelSize = font.MeasureString(Label);
        Vector2 labelPos = new Vector2(
            Position.X + (Size - labelSize.X) / 2,
            Position.Y + (Size - labelSize.Y) / 2);
        spriteBatch.DrawString(font, Label, labelPos, Color.White);
    }
}
