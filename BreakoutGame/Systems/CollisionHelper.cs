using Microsoft.Xna.Framework;
using BreakoutGame.Entities;
using System;

namespace BreakoutGame.Systems;

public static class CollisionHelper
{
    /// <summary>
    /// Returns true if two rectangles overlap.
    /// </summary>
    public static bool Intersects(Rectangle a, Rectangle b) => a.Intersects(b);

    /// <summary>
    /// Determines which face of the brick the ball hit.
    /// Returns a normalized reflection vector.
    /// </summary>
    public static Vector2 GetBallBrickReflection(Ball ball, Brick brick)
    {
        // Calculate overlap depths from each side
        float overlapLeft = (ball.Position.X + ball.Radius) - brick.Position.X;
        float overlapRight = (brick.Position.X + brick.Width) - (ball.Position.X - ball.Radius);
        float overlapTop = (ball.Position.Y + ball.Radius) - brick.Position.Y;
        float overlapBottom = (brick.Position.Y + brick.Height) - (ball.Position.Y - ball.Radius);

        // Find minimum overlap to determine collision face
        float minOverlapX = Math.Min(overlapLeft, overlapRight);
        float minOverlapY = Math.Min(overlapTop, overlapBottom);

        Vector2 velocity = ball.Velocity;
        if (minOverlapX < minOverlapY)
            velocity.X = -velocity.X; // Side hit
        else
            velocity.Y = -velocity.Y; // Top/bottom hit

        return velocity;
    }
}
