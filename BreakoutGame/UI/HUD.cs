using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BreakoutGame.UI;

public class HUD
{
    public void Draw(SpriteBatch spriteBatch, SpriteFont font, GameManager gm)
    {
        string scoreText = $"SCORE: {gm.Score:D5}";
        string levelText = $"LEVEL {gm.CurrentLevel + 1}";
        string livesText = $"LIVES: {gm.Lives}";

        spriteBatch.DrawString(font, scoreText, new Vector2(16, 8), Color.White);

        var levelSize = font.MeasureString(levelText);
        spriteBatch.DrawString(font, levelText, new Vector2(640 - levelSize.X / 2, 8), Color.White);

        var livesSize = font.MeasureString(livesText);
        spriteBatch.DrawString(font, livesText, new Vector2(1264 - livesSize.X, 8), Color.White);
    }
}
