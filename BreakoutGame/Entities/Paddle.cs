using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame.Entities;

public class Paddle
{
    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Color Color { get; set; }
    public float Speed { get; set; }
    public float WidePowerUpTimer { get; set; }

    private Texture2D _pixel;

    public Paddle(Texture2D pixel, Vector2 position)
    {
        _pixel = pixel;
        Position = position;
        Width = 120;
        Height = 16;
        Color = Color.White;
        Speed = 600f;
        WidePowerUpTimer = 0;
    }

    public void Update(GameTime gameTime)
    {
        // TODO: Implement paddle movement
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle((int)Position.X, (int)Position.Y, Width, Height), Color);
    }
}
