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
        Color = GetColorForType(type);
        Label = GetLabelForType(type);
    }

    private Color GetColorForType(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.WidePaddle => new Color(33, 150, 243),    // Blue #2196F3
            PowerUpType.MultiBall => new Color(156, 39, 176),     // Purple #9C27B0
            PowerUpType.ExtraLife => new Color(233, 30, 99),      // Pink #E91E63
            PowerUpType.SpeedDown => new Color(0, 188, 212),      // Cyan #00BCD4
            _ => Color.White
        };
    }

    private string GetLabelForType(PowerUpType type)
    {
        return type switch
        {
            PowerUpType.WidePaddle => "W",
            PowerUpType.MultiBall => "M",
            PowerUpType.ExtraLife => "+",
            PowerUpType.SpeedDown => "S",
            _ => "?"
        };
    }

    public void Update(GameTime gameTime)
    {
        // Placeholder - to be implemented in BRK-008
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font)
    {
        // Placeholder - to be implemented in BRK-008
    }
}
