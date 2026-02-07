using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame.Entities;

public class Paddle
{
    public Vector2 Position;
    public int Width = 120;
    public int Height = 16;
    public Color Color = Color.White;
    public float Speed = 600f;
    public float WidePowerUpTimer = 0f;

    private readonly Texture2D _pixel;

    public Paddle(Texture2D pixel, Vector2 position)
    {
        _pixel = pixel;
        Position = position;
    }

    public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

    public void Update(float deltaTime)
    {
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, Bounds, Color);
    }
}
