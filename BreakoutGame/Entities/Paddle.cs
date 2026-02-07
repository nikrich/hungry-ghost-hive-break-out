using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BreakoutGame.Systems;

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
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Keyboard movement
        if (InputManager.IsKeyDown(Keys.A) || InputManager.IsKeyDown(Keys.Left))
        {
            Position = new Vector2(Position.X - Speed * deltaTime, Position.Y);
        }
        if (InputManager.IsKeyDown(Keys.D) || InputManager.IsKeyDown(Keys.Right))
        {
            Position = new Vector2(Position.X + Speed * deltaTime, Position.Y);
        }

        // Mouse movement - center paddle on mouse X
        Position = new Vector2(InputManager.MouseX - Width / 2, Position.Y);

        // Clamp paddle within play area (0 to 720)
        Position = new Vector2(MathHelper.Clamp(Position.X, 0, 720 - Width), Position.Y);

        // Handle wide power-up timer
        if (WidePowerUpTimer > 0)
        {
            WidePowerUpTimer -= deltaTime;
            if (WidePowerUpTimer <= 0)
            {
                // Reset width and re-center paddle
                float centerX = Position.X + Width / 2;
                Width = 120;
                Position = new Vector2(centerX - Width / 2, Position.Y);
                WidePowerUpTimer = 0;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle((int)Position.X, (int)Position.Y, Width, Height), Color);
    }
}
