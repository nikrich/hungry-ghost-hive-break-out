using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BreakoutGame.Systems;
using System;

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
    private Random _rng;

    public Ball(Texture2D pixel, Vector2 position, float speed)
    {
        _pixel = pixel;
        Position = position;
        Velocity = Vector2.Zero;
        Radius = 8;
        Speed = speed;
        IsAttached = true;
        Color = Color.White;
        SpeedMultiplier = 1.0f;
        _rng = new Random();
    }

    public void Update(GameTime gameTime, Paddle paddle)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // If attached to paddle, follow paddle center
        if (IsAttached)
        {
            Position = new Vector2(paddle.Position.X + paddle.Width / 2, paddle.Position.Y - Radius);

            // Launch on Space or Enter
            if (InputManager.IsKeyPressed(Keys.Space) || InputManager.IsKeyPressed(Keys.Enter))
            {
                IsAttached = false;
                LaunchBall();
            }
        }
        else
        {
            // Update position based on velocity
            Position += Velocity * SpeedMultiplier * deltaTime;

            // Wall collision detection and reflection
            // Left wall
            if (Position.X - Radius <= 0)
            {
                Position = new Vector2(Radius, Position.Y);
                Velocity = new Vector2(Math.Abs(Velocity.X), Velocity.Y);
            }

            // Right wall
            if (Position.X + Radius >= 720)
            {
                Position = new Vector2(720 - Radius, Position.Y);
                Velocity = new Vector2(-Math.Abs(Velocity.X), Velocity.Y);
            }

            // Top wall
            if (Position.Y - Radius <= 0)
            {
                Position = new Vector2(Position.X, Radius);
                Velocity = new Vector2(Velocity.X, Math.Abs(Velocity.Y));
            }

            // Enforce minimum Y velocity to prevent near-horizontal balls
            if (Math.Abs(Velocity.Y) < 100)
            {
                Velocity = new Vector2(Velocity.X, Math.Sign(Velocity.Y) * 100);
            }

            // Safety clamp to keep ball in bounds
            Position = new Vector2(
                MathHelper.Clamp(Position.X, Radius, 720 - Radius),
                MathHelper.Clamp(Position.Y, Radius, 1080)
            );
        }
    }

    private void LaunchBall()
    {
        // Random angle between -30° and +30° from straight up
        float angleOffset = (float)((_rng.NextDouble() - 0.5) * 60 * Math.PI / 180.0);
        float angle = -MathF.PI / 2 + angleOffset; // -90° + offset

        Velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * Speed;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, new Rectangle(
            (int)(Position.X - Radius), (int)(Position.Y - Radius),
            (int)(Radius * 2), (int)(Radius * 2)), Color);
    }
}
