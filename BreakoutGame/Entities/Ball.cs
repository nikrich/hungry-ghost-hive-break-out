using System;
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

    public void AttachToPaddle(Paddle paddle)
    {
        Position.X = paddle.Position.X + paddle.Width / 2f;
        Position.Y = paddle.Position.Y - Radius;
    }

    public void Launch(Random rng)
    {
        IsAttached = false;
        float angleDeg = (float)(rng.NextDouble() * 60.0 - 30.0);
        float angle = angleDeg * MathF.PI / 180f;
        Velocity = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * Speed;
    }

    public void Update(float deltaTime)
    {
        if (IsAttached) return;

        Position += Velocity * SpeedMultiplier * deltaTime;

        // Left wall
        if (Position.X <= Radius)
        {
            Position.X = Radius;
            Velocity.X = MathF.Abs(Velocity.X);
        }
        // Right wall
        if (Position.X >= BreakoutGame.ScreenWidth - Radius)
        {
            Position.X = BreakoutGame.ScreenWidth - Radius;
            Velocity.X = -MathF.Abs(Velocity.X);
        }
        // Top wall
        if (Position.Y <= Radius)
        {
            Position.Y = Radius;
            Velocity.Y = MathF.Abs(Velocity.Y);
        }

        // Safety clamp: prevent near-horizontal travel
        if (MathF.Abs(Velocity.Y) < 100f)
        {
            Velocity.Y = (Velocity.Y >= 0 ? 1f : -1f) * 100f;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_pixel, Bounds, Color);
    }
}
