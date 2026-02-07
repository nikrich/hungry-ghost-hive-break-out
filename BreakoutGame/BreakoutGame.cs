using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BreakoutGame.Entities;
using BreakoutGame.Levels;
using BreakoutGame.Systems;
using BreakoutGame.UI;
using System;
using System.Collections.Generic;

namespace BreakoutGame;

public class BreakoutGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Core assets
    private Texture2D _pixel;
    private SpriteFont _font;

    // Game entities
    private Paddle _paddle;
    private List<Ball> _balls;
    private List<Brick> _bricks;
    private List<PowerUp> _powerUps;

    // Game systems
    private GameManager _gameManager;
    private HUD _hud;
    private Random _rng;

    public BreakoutGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Configure window: 720x1080 portrait, non-resizable
        _graphics.PreferredBackBufferWidth = 720;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.IsFullScreen = false;
        Window.Title = "Breakout";
        Window.AllowUserResizing = false;

        // 60 FPS fixed timestep
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
    }

    protected override void Initialize()
    {
        // Initialize game systems
        _gameManager = new GameManager();
        _hud = new HUD();
        _rng = new Random();

        // Initialize entity lists
        _balls = new List<Ball>();
        _bricks = new List<Brick>();
        _powerUps = new List<PowerUp>();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create 1x1 white pixel texture
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        // Load font
        _font = Content.Load<SpriteFont>("GameFont");

        // Create paddle (positioned near bottom of screen)
        _paddle = new Paddle(_pixel, new Vector2(300, 1040));
    }

    protected override void Update(GameTime gameTime)
    {
        // Update input manager
        InputManager.Update();

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // State machine
        switch (_gameManager.State)
        {
            case GameState.Title:
                UpdateTitle(gameTime, deltaTime);
                break;

            case GameState.Playing:
                UpdatePlaying(gameTime, deltaTime);
                break;

            case GameState.LevelComplete:
                UpdateLevelComplete(gameTime, deltaTime);
                break;

            case GameState.GameOver:
                UpdateGameOver(gameTime, deltaTime);
                break;

            case GameState.Paused:
                UpdatePaused(gameTime, deltaTime);
                break;
        }

        base.Update(gameTime);
    }

    private void UpdateTitle(GameTime gameTime, float deltaTime)
    {
        // Update blink timer for "Press SPACE to Start" text
        _gameManager.StateTimer += deltaTime;

        // Check for space press to start game
        if (InputManager.IsKeyPressed(Keys.Space) || InputManager.IsKeyPressed(Keys.Enter))
        {
            _gameManager.Reset();
            LoadLevel(_gameManager.CurrentLevel);
            _gameManager.State = GameState.Playing;
        }
    }

    private void UpdatePlaying(GameTime gameTime, float deltaTime)
    {
        // Check for pause
        if (InputManager.IsKeyPressed(Keys.Escape))
        {
            _gameManager.State = GameState.Paused;
            return;
        }

        // Update entities
        _paddle.Update(gameTime);

        foreach (var ball in _balls)
            ball.Update(gameTime);

        foreach (var powerUp in _powerUps)
            powerUp.Update(gameTime);

        // TODO: Collision detection will be added in other stories
        // TODO: Win condition check will be added in BRK-007
    }

    private void UpdateLevelComplete(GameTime gameTime, float deltaTime)
    {
        _gameManager.StateTimer += deltaTime;

        // Wait 1.5 seconds before loading next level
        if (_gameManager.StateTimer >= 1.5f)
        {
            _gameManager.StateTimer = 0;
            _gameManager.CurrentLevel++;

            // Check if there are more levels
            if (_gameManager.CurrentLevel < LevelData.Levels.Length)
            {
                LoadLevel(_gameManager.CurrentLevel);

                // Reset ball on paddle
                _balls.Clear();
                // TODO: Ball creation will be handled once Ball entity is fully implemented

                _gameManager.State = GameState.Playing;
            }
            else
            {
                // No more levels - player wins
                _gameManager.State = GameState.GameOver;
            }
        }
    }

    private void UpdateGameOver(GameTime gameTime, float deltaTime)
    {
        // Check for space press to return to title
        if (InputManager.IsKeyPressed(Keys.Space) || InputManager.IsKeyPressed(Keys.Enter))
        {
            _gameManager.State = GameState.Title;
            _gameManager.StateTimer = 0;
        }
    }

    private void UpdatePaused(GameTime gameTime, float deltaTime)
    {
        // Check for escape press to unpause
        if (InputManager.IsKeyPressed(Keys.Escape))
        {
            _gameManager.State = GameState.Playing;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Only draw game entities when in Playing, Paused, or LevelComplete states
        if (_gameManager.State == GameState.Playing ||
            _gameManager.State == GameState.Paused ||
            _gameManager.State == GameState.LevelComplete)
        {
            // Draw entities based on draw order
            // 1. Bricks
            foreach (var brick in _bricks)
                brick.Draw(_spriteBatch);

            // 2. Power-ups
            foreach (var powerUp in _powerUps)
                powerUp.Draw(_spriteBatch, _font);

            // 3. Paddle
            _paddle.Draw(_spriteBatch);

            // 4. Balls
            foreach (var ball in _balls)
                ball.Draw(_spriteBatch);

            // 5. HUD
            _hud.Draw(_spriteBatch, _font, _gameManager);
        }

        // 6. Overlays (title, pause, level complete, game over)
        DrawOverlays();

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawOverlays()
    {
        switch (_gameManager.State)
        {
            case GameState.Title:
                DrawTitleScreen();
                break;

            case GameState.Paused:
                DrawPauseOverlay();
                break;

            case GameState.LevelComplete:
                DrawLevelCompleteOverlay();
                break;

            case GameState.GameOver:
                DrawGameOverScreen();
                break;
        }
    }

    private void DrawTitleScreen()
    {
        // Draw "BREAKOUT" title centered
        string title = "BREAKOUT";
        var titleSize = _font.MeasureString(title);
        var titlePos = new Vector2(360 - titleSize.X / 2, 400);
        _spriteBatch.DrawString(_font, title, titlePos, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

        // Draw "Press SPACE to Start" with blinking effect (every 0.5s)
        if (_gameManager.StateTimer % 1.0f < 0.5f)
        {
            string startText = "Press SPACE to Start";
            var startSize = _font.MeasureString(startText);
            var startPos = new Vector2(360 - startSize.X / 2, 600);
            _spriteBatch.DrawString(_font, startText, startPos, Color.White);
        }
    }

    private void DrawPauseOverlay()
    {
        // Draw semi-transparent black overlay
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 720, 1080), Color.Black * 0.5f);

        // Draw "PAUSED" text centered
        string pausedText = "PAUSED";
        var pausedSize = _font.MeasureString(pausedText);
        var pausedPos = new Vector2(360 - pausedSize.X / 2, 500);
        _spriteBatch.DrawString(_font, pausedText, pausedPos, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
    }

    private void DrawLevelCompleteOverlay()
    {
        // Draw "LEVEL COMPLETE" text centered
        string completeText = "LEVEL COMPLETE";
        var completeSize = _font.MeasureString(completeText);
        var completePos = new Vector2(360 - completeSize.X / 2, 500);
        _spriteBatch.DrawString(_font, completeText, completePos, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
    }

    private void DrawGameOverScreen()
    {
        // Determine if player won or lost
        bool isWin = _gameManager.CurrentLevel >= LevelData.Levels.Length;
        string gameOverText = isWin ? "YOU WIN" : "GAME OVER";

        // Draw game over/win text centered
        var gameOverSize = _font.MeasureString(gameOverText);
        var gameOverPos = new Vector2(360 - gameOverSize.X / 2, 400);
        _spriteBatch.DrawString(_font, gameOverText, gameOverPos, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

        // Draw final score
        string scoreText = $"Final Score: {_gameManager.Score}";
        var scoreSize = _font.MeasureString(scoreText);
        var scorePos = new Vector2(360 - scoreSize.X / 2, 500);
        _spriteBatch.DrawString(_font, scoreText, scorePos, Color.White);

        // Draw "Press SPACE to play again"
        string playAgainText = "Press SPACE to play again";
        var playAgainSize = _font.MeasureString(playAgainText);
        var playAgainPos = new Vector2(360 - playAgainSize.X / 2, 600);
        _spriteBatch.DrawString(_font, playAgainText, playAgainPos, Color.White);
    }

    private void LoadLevel(int levelIndex)
    {
        _bricks.Clear();
        _powerUps.Clear();

        int[,] layout = LevelData.Levels[levelIndex];
        int rows = layout.GetLength(0);
        int cols = layout.GetLength(1);

        int brickW = 64;
        int brickH = 24;
        int gap = 2;

        int gridWidth = cols * (brickW + gap) - gap;
        int offsetX = (720 - gridWidth) / 2;
        int offsetY = 80; // below HUD

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int val = layout[r, c];
                if (val == 0) continue;

                var pos = new Vector2(
                    offsetX + c * (brickW + gap),
                    offsetY + r * (brickH + gap));

                var brick = new Brick(_pixel, pos, brickW, brickH, val);
                _bricks.Add(brick);
            }
        }
    }
}
