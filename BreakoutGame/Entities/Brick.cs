using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame.Entities;

public class Brick
{
    public Vector2 Position { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int MaxHP { get; set; }
    public int HP { get; set; }
    public bool IsIndestructible { get; set; }
    public bool IsAlive { get; set; }
    public Color Color { get; set; }

    private Texture2D _pixel;

    public Brick(Texture2D pixel, Vector2 position, int width, int height, int maxHP)
    {
        _pixel = pixel;
        Position = position;
        Width = width;
        Height = height;
        MaxHP = maxHP;
        HP = maxHP;
        IsIndestructible = maxHP == -1;
        IsAlive = true;
        Color = Color.White;
    }

    public int Hit()
    {
        // Placeholder - to be implemented in BRK-006
        return 0;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Placeholder - to be implemented in BRK-005
    }
}
