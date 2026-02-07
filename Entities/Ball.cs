using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BreakoutGame;

public class Ball
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Radius;
    public float Speed;
    public bool IsAttached;
    public Color Color;
    public float SpeedMultiplier;

    private Texture2D _pixel;
    private Random _random;

    public Ball(Texture2D pixel, Vector2 startPosition, float speed)
    {
        _pixel = pixel;
        Position = startPosition;
        Speed = speed;
        Radius = 8f;
        Velocity = Vector2.Zero;
        IsAttached = true;
        Color = Color.White;
        SpeedMultiplier = 1.0f;
        _random = new Random();
    }

    public void Update(GameTime gameTime, Vector2 paddlePosition, int paddleWidth)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (IsAttached)
        {
            // Position ball on top of paddle, centered horizontally
            Position = new Vector2(paddlePosition.X + paddleWidth / 2f, paddlePosition.Y - Radius - 2);
            return;
        }

        // Update position
        Position += Velocity * SpeedMultiplier * deltaTime;

        // Wall collisions
        // Left wall
        if (Position.X - Radius <= 0)
        {
            Position.X = Radius;
            Velocity.X = -Velocity.X;
        }

        // Right wall
        if (Position.X + Radius >= 720)
        {
            Position.X = 720 - Radius;
            Velocity.X = -Velocity.X;
        }

        // Top wall
        if (Position.Y - Radius <= 0)
        {
            Position.Y = Radius;
            Velocity.Y = -Velocity.Y;
        }

        // Safety clamp for Y velocity
        if (Math.Abs(Velocity.Y) < 100)
        {
            Velocity.Y = Math.Sign(Velocity.Y) * 100;
            if (Velocity.Y == 0)
                Velocity.Y = 100;
        }

        // Clamp position inside play area as safety net
        Position.X = MathHelper.Clamp(Position.X, Radius, 720 - Radius);
        Position.Y = MathHelper.Clamp(Position.Y, Radius, 1080);
    }

    public void Launch()
    {
        IsAttached = false;

        // Random angle between -30 and +30 degrees from straight up
        float angleOffset = (float)(_random.NextDouble() * 60 - 30) * MathF.PI / 180f;
        float angle = -MathF.PI / 2f + angleOffset; // -90 degrees (straight up) + offset

        Velocity = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * Speed;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw as a filled square using the pixel texture
        int drawRadius = (int)(Radius * 2);
        spriteBatch.Draw(_pixel, new Rectangle(
            (int)(Position.X - Radius),
            (int)(Position.Y - Radius),
            drawRadius,
            drawRadius), Color);
    }
}
