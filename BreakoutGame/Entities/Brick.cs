using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame.Entities;

public class Brick
{
    public Vector2 Position;
    public int Width = 64;
    public int Height = 24;
    public int MaxHP;
    public int HP;
    public bool IsIndestructible;
    public bool IsAlive = true;
    public Color Color;

    private readonly Texture2D _pixel;

    public Brick(Texture2D pixel, Vector2 position, int width, int height, int hpValue)
    {
        _pixel = pixel;
        Position = position;
        Width = width;
        Height = height;

        if (hpValue == -1)
        {
            IsIndestructible = true;
            MaxHP = -1;
            HP = -1;
            Color = new Color(158, 158, 158);
        }
        else
        {
            MaxHP = hpValue;
            HP = hpValue;
            Color = GetColorForHP(HP);
        }
    }

    public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

    public Color GetColorForHP(int hp) => hp switch
    {
        1 => new Color(76, 175, 80),
        2 => new Color(255, 235, 59),
        3 => new Color(255, 152, 0),
        _ => new Color(244, 67, 54),
    };

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

    public static int PointsForMaxHP(int maxHP) => maxHP switch
    {
        1 => 10,
        2 => 20,
        3 => 30,
        4 => 50,
        _ => 10,
    };

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsAlive)
            spriteBatch.Draw(_pixel, Bounds, Color);
    }
}
