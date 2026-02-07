using System;
using Microsoft.Xna.Framework;
using BreakoutGame.Entities;

namespace BreakoutGame.Systems;

public static class CollisionHelper
{
    public static bool Intersects(Rectangle a, Rectangle b) => a.Intersects(b);

    public static Vector2 GetBallBrickReflection(Ball ball, Brick brick)
    {
        float overlapLeft = (ball.Position.X + ball.Radius) - brick.Position.X;
        float overlapRight = (brick.Position.X + brick.Width) - (ball.Position.X - ball.Radius);
        float overlapTop = (ball.Position.Y + ball.Radius) - brick.Position.Y;
        float overlapBottom = (brick.Position.Y + brick.Height) - (ball.Position.Y - ball.Radius);

        float minOverlapX = Math.Min(overlapLeft, overlapRight);
        float minOverlapY = Math.Min(overlapTop, overlapBottom);

        Vector2 velocity = ball.Velocity;
        if (minOverlapX < minOverlapY)
            velocity.X = -velocity.X;
        else
            velocity.Y = -velocity.Y;

        return velocity;
    }
}
