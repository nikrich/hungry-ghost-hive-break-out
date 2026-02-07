using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BreakoutGame.Systems;

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
        if (InputManager.IsKeyDown(Keys.A) || InputManager.IsKeyDown(Keys.Left))
            Position.X -= Speed * deltaTime;
        if (InputManager.IsKeyDown(Keys.D) || InputManager.IsKeyDown(Keys.Right))
            Position.X += Speed * deltaTime;

        Position.X = InputManager.MouseX - Width / 2f;

        Position.X = MathHelper.Clamp(Position.X, 0, 720 - Width);

        if (WidePowerUpTimer > 0)
        {
            WidePowerUpTimer -= deltaTime;
            if (WidePowerUpTimer <= 0)
            {
                float center = Position.X + Width / 2f;
                Width = 120;
                Position.X = center - Width / 2f;
                Position.X = MathHelper.Clamp(Position.X, 0, 720 - Width);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, Bounds, Color);
    }
}
