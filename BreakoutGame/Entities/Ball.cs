using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame.Entities;

public class Ball
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius = 8f;
    public float Speed = 450f;
    public bool IsAttached = true;
    public Color Color = Color.White;
    public float SpeedMultiplier = 1.0f;

    private readonly Texture2D _pixel;

    public Ball(Texture2D pixel, Vector2 position, float speed)
    {
        _pixel = pixel;
        Position = position;
        Speed = speed;
    }

    public Rectangle Bounds => new Rectangle(
        (int)(Position.X - Radius), (int)(Position.Y - Radius),
        (int)(Radius * 2), (int)(Radius * 2));

    public void Update(float deltaTime)
    {
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, Bounds, Color);
    }
}
