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

    public Brick(Texture2D pixel, Vector2 position, int width, int height, int hp)
    {
        _pixel = pixel;
        Position = position;
        Width = width;
        Height = height;
        IsIndestructible = hp == -1;
        MaxHP = IsIndestructible ? int.MaxValue : hp;
        HP = MaxHP;
        IsAlive = true;
        Color = IsIndestructible ? new Color(158, 158, 158) : GetColorForHP(HP);
    }

    public int Hit()
    {
        if (IsIndestructible) return 0;

        HP--;
        if (HP <= 0)
        {
            IsAlive = false;
            return PointsForMaxHP(MaxHP);
        }

        Color = GetColorForHP(HP);
        return 0;
    }

    private int PointsForMaxHP(int maxHp) => maxHp switch
    {
        1 => 10,
        2 => 20,
        3 => 30,
        4 => 50,
        _ => 50
    };

    private Color GetColorForHP(int hp) => hp switch
    {
        1 => new Color(76, 175, 80),    // Green
        2 => new Color(255, 235, 59),   // Yellow
        3 => new Color(255, 152, 0),    // Orange
        _ => new Color(244, 67, 54),    // Red (4+)
    };

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsAlive)
        {
            spriteBatch.Draw(_pixel, new Rectangle((int)Position.X, (int)Position.Y, Width, Height), Color);
        }
    }
}
