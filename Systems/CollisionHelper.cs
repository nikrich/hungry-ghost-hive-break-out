using Microsoft.Xna.Framework;

namespace BreakoutGame;

public static class CollisionHelper
{
    public static bool Intersects(Rectangle a, Rectangle b) => a.Intersects(b);
}
