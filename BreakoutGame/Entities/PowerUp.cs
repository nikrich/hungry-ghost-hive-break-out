using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame.Entities;

public enum PowerUpType
{
    WidePaddle,   // Blue #2196F3, label "W"
    MultiBall,    // Purple #9C27B0, label "M"
    ExtraLife,    // Pink #E91E63, label "+"
    SpeedDown     // Cyan #00BCD4, label "S"
}

public class PowerUp
{
    public Vector2 Position { get; set; }
    public int Size { get; set; }
    public float FallSpeed { get; set; }
    public PowerUpType Type { get; set; }
    public bool IsActive { get; set; }
    public Color Color { get; set; }
    public string Label { get; set; }

    private Texture2D _pixel;

    public PowerUp(Texture2D pixel, Vector2 position, PowerUpType type)
    {
        _pixel = pixel;
        Position = position;
        Size = 24;
        FallSpeed = 200f;
        Type = type;
        IsActive = true;

        (Color, Label) = type switch
        {
            PowerUpType.WidePaddle => (new Color(33, 150, 243), "W"),
            PowerUpType.MultiBall => (new Color(156, 39, 176), "M"),
            PowerUpType.ExtraLife => (new Color(233, 30, 99), "+"),
            PowerUpType.SpeedDown => (new Color(0, 188, 212), "S"),
            _ => (Color.White, "?")
        };
    }

    public void Update(GameTime gameTime)
    {
        // TODO: Implement power-up falling
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font)
    {
        if (IsActive)
        {
            spriteBatch.Draw(_pixel, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), Color);

            var labelSize = font.MeasureString(Label);
            var labelPos = new Vector2(
                Position.X + Size / 2 - labelSize.X / 2,
                Position.Y + Size / 2 - labelSize.Y / 2);
            spriteBatch.DrawString(font, Label, labelPos, Color.White);
        }
    }
}
