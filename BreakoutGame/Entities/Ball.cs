using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame.Entities;

public class Ball
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Radius { get; set; }
    public float Speed { get; set; }
    public bool IsAttached { get; set; }
    public Color Color { get; set; }
    public float SpeedMultiplier { get; set; }

    private Texture2D _pixel;

    public Ball(Texture2D pixel, Vector2 position, float speed)
    {
        _pixel = pixel;
        Position = position;
        Radius = 8;
        Speed = speed;
        IsAttached = true;
        Color = Color.White;
        SpeedMultiplier = 1.0f;
        Velocity = Vector2.Zero;
    }

    public void Update(GameTime gameTime)
    {
        // TODO: Implement ball movement
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(
            (int)(Position.X - Radius), (int)(Position.Y - Radius),
            (int)(Radius * 2), (int)(Radius * 2)), Color);
    }
}
