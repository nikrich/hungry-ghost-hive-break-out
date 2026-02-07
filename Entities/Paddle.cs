using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame;

public class Paddle
{
    public Vector2 Position;
    public int Width;
    public int Height;
    public Color Color;
    public float Speed;
    public float WidePowerUpTimer;

    private Texture2D _pixel;

    public Paddle(Texture2D pixel, Vector2 startPosition)
    {
        _pixel = pixel;
        Position = startPosition;
        Width = 120;
        Height = 16;
        Color = Color.White;
        Speed = 600f;
        WidePowerUpTimer = 0f;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Keyboard input
        if (InputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) ||
            InputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
        {
            Position.X -= Speed * deltaTime;
        }

        if (InputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D) ||
            InputManager.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
        {
            Position.X += Speed * deltaTime;
        }

        // Mouse input
        Position.X = InputManager.MouseX - Width / 2f;

        // Clamp to screen
        Position.X = MathHelper.Clamp(Position.X, 0, 720 - Width);

        // Update wide power-up timer
        if (WidePowerUpTimer > 0)
        {
            WidePowerUpTimer -= deltaTime;
            if (WidePowerUpTimer <= 0)
            {
                Width = 120;
                WidePowerUpTimer = 0;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle((int)Position.X, (int)Position.Y, Width, Height), Color);
    }

    public Rectangle GetBounds()
    {
        return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
    }
}
