using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BreakoutGame.Entities;
using BreakoutGame.Levels;
using BreakoutGame.Systems;
using BreakoutGame.UI;

namespace BreakoutGame;

public class BreakoutGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;
    private SpriteFont _font;
    private Paddle _paddle;
    private List<Ball> _balls;
    private List<Brick> _bricks;
    private List<PowerUp> _powerUps;
    private GameManager _gameManager;
    private HUD _hud;
    private Random _rng;
    private float _blinkTimer;
    private bool _blinkVisible;
    private bool _isWin;
    private float _speedDownTimer;

    public BreakoutGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.ApplyChanges();

        Window.AllowUserResizing = false;
        Window.Title = "Breakout";

        _gameManager = new GameManager();
        _hud = new HUD();
        _rng = new Random();
        _balls = new List<Ball>();
        _bricks = new List<Brick>();
        _powerUps = new List<PowerUp>();
        _blinkTimer = 0f;
        _blinkVisible = true;
        _isWin = false;
        _speedDownTimer = 0f;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _font = Content.Load<SpriteFont>("GameFont");

        _paddle = new Paddle(_pixel, new Vector2(580, 690));
        ResetBallOnPaddle();
    }

    private float GetBallSpeedForLevel(int levelIndex)
    {
        return Math.Min(450f + (levelIndex * 25f), 700f);
    }

    private void ResetBallOnPaddle()
    {
        _balls.Clear();
        var ball = new Ball(_pixel, Vector2.Zero, GetBallSpeedForLevel(_gameManager.CurrentLevel));
        ball.IsAttached = true;
        ball.AttachToPaddle(_paddle);
        _balls.Add(ball);
    }

    private void ClearActivePowerUps()
    {
        // Clear speed down timer and reset all ball speed multipliers
        _speedDownTimer = 0f;
        foreach (var ball in _balls)
        {
            ball.SpeedMultiplier = 1.0f;
        }

        // Reset paddle to default width
        if (_paddle.Width != 120)
        {
            float center = _paddle.Position.X + _paddle.Width / 2f;
            _paddle.Width = 120;
            _paddle.Position.X = center - _paddle.Width / 2f;
            _paddle.Position.X = MathHelper.Clamp(_paddle.Position.X, 0, 1280 - _paddle.Width);
        }
        _paddle.WidePowerUpTimer = 0f;
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
        int gap = 4;

        int gridWidth = cols * (brickW + gap) - gap;
        int offsetX = (1280 - gridWidth) / 2;
        int offsetY = 80;

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

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Global fullscreen toggle with F11
        if (InputManager.IsKeyPressed(Keys.F11))
        {
            _graphics.ToggleFullScreen();
        }

        switch (_gameManager.State)
        {
            case GameState.Title:
                UpdateTitle(dt);
                break;
            case GameState.Playing:
                UpdatePlaying(dt);
                break;
            case GameState.LevelComplete:
                UpdateLevelComplete(dt);
                break;
            case GameState.GameOver:
                UpdateGameOver();
                break;
            case GameState.Paused:
                UpdatePaused();
                break;
        }

        base.Update(gameTime);
    }

    private void UpdateTitle(float dt)
    {
        _blinkTimer += dt;
        if (_blinkTimer >= 0.5f)
        {
            _blinkTimer -= 0.5f;
            _blinkVisible = !_blinkVisible;
        }

        if (InputManager.IsKeyPressed(Keys.Space) || InputManager.IsKeyPressed(Keys.Enter))
        {
            _gameManager.Reset();
            _gameManager.State = GameState.Playing;
            _paddle.Position = new Vector2(580, 690);
            ResetBallOnPaddle();
            LoadLevel(_gameManager.CurrentLevel);
        }
    }

    private void UpdatePlaying(float dt)
    {
        _paddle.Update(dt);

        foreach (var ball in _balls)
        {
            if (ball.IsAttached)
            {
                ball.AttachToPaddle(_paddle);

                if (InputManager.IsKeyPressed(Keys.Space) || InputManager.IsKeyPressed(Keys.Enter))
                {
                    ball.Launch(_rng);
                }
            }
            else
            {
                ball.Update(dt);
            }
        }

        // Ball vs Paddle collision
        foreach (var ball in _balls)
        {
            if (ball.IsAttached) continue;
            if (ball.Velocity.Y <= 0) continue;

            if (CollisionHelper.Intersects(ball.Bounds, _paddle.Bounds))
            {
                float hitPos = (ball.Position.X - _paddle.Position.X) / _paddle.Width;
                hitPos = MathHelper.Clamp(hitPos, 0f, 1f);
                float angle = MathHelper.Lerp(-60f, 60f, hitPos) * MathF.PI / 180f;
                ball.Velocity = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * ball.Speed * ball.SpeedMultiplier;

                // Push ball above paddle to prevent re-collision
                ball.Position.Y = _paddle.Position.Y - ball.Radius;
            }
        }

        // Ball vs Brick collision (one brick per ball per frame)
        foreach (var ball in _balls)
        {
            if (ball.IsAttached) continue;

            foreach (var brick in _bricks)
            {
                if (!brick.IsAlive) continue;

                if (CollisionHelper.Intersects(ball.Bounds, brick.Bounds))
                {
                    ball.Velocity = CollisionHelper.GetBallBrickReflection(ball, brick);
                    int points = brick.Hit();
                    if (points > 0)
                    {
                        _gameManager.AddScore(points);
                        // 15% chance to spawn a power-up from destroyed brick
                        if (_rng.NextDouble() < 0.15)
                        {
                            var type = (PowerUpType)_rng.Next(4);
                            var pu = new PowerUp(_pixel,
                                new Vector2(brick.Position.X + brick.Width / 2f - 12, brick.Position.Y),
                                type);
                            _powerUps.Add(pu);
                        }
                    }
                    break; // Only one brick collision per ball per frame
                }
            }
        }

        // Update power-ups
        foreach (var pu in _powerUps)
            pu.Update(dt);

        // PowerUp vs Paddle collision
        foreach (var pu in _powerUps)
        {
            if (!pu.IsActive) continue;
            if (CollisionHelper.Intersects(pu.Bounds, _paddle.Bounds))
            {
                ApplyPowerUp(pu.Type);
                pu.IsActive = false;
            }
        }

        // Clean up inactive power-ups
        _powerUps.RemoveAll(p => !p.IsActive);

        // Speed down timer
        if (_speedDownTimer > 0)
        {
            _speedDownTimer -= dt;
            if (_speedDownTimer <= 0)
            {
                foreach (var ball in _balls)
                    ball.SpeedMultiplier = 1.0f;
            }
        }

        // Clean up dead bricks
        _bricks.RemoveAll(b => !b.IsAlive);

        // Death zone detection: remove balls that fall below screen
        _balls.RemoveAll(ball => ball.Position.Y > 720 + ball.Radius);

        // Check if all balls are lost
        if (_balls.Count == 0)
        {
            _gameManager.LoseLife();
            if (_gameManager.State != GameState.GameOver)
            {
                // Clear all active power-ups and reset to default state
                ClearActivePowerUps();
                // Reset ball on paddle if still have lives
                ResetBallOnPaddle();
            }
        }

        // Check level clear: all destructible bricks destroyed
        bool levelCleared = !_bricks.Any(b => !b.IsIndestructible);
        if (levelCleared)
        {
            _gameManager.NextLevel();
        }

        // Pause on Escape
        if (InputManager.IsKeyPressed(Keys.Escape))
        {
            _gameManager.State = GameState.Paused;
        }
    }

    private void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.WidePaddle:
                float center = _paddle.Position.X + _paddle.Width / 2f;
                _paddle.Width = 180;
                _paddle.Position.X = center - _paddle.Width / 2f;
                _paddle.Position.X = MathHelper.Clamp(_paddle.Position.X, 0, 1280 - _paddle.Width);
                _paddle.WidePowerUpTimer = 10f;
                break;

            case PowerUpType.MultiBall:
                if (_balls.Count > 0)
                {
                    var source = _balls[0];
                    for (int i = 0; i < 2; i++)
                    {
                        var newBall = new Ball(_pixel, source.Position, source.Speed);
                        float angle = (i == 0 ? -15f : 15f) * MathF.PI / 180f;
                        newBall.Velocity = RotateVector(source.Velocity, angle);
                        newBall.IsAttached = false;
                        newBall.SpeedMultiplier = source.SpeedMultiplier;
                        _balls.Add(newBall);
                    }
                }
                break;

            case PowerUpType.ExtraLife:
                if (_gameManager.Lives < 5)
                    _gameManager.Lives++;
                break;

            case PowerUpType.SpeedDown:
                foreach (var ball in _balls)
                    ball.SpeedMultiplier = 0.75f;
                _speedDownTimer = 8f;
                break;
        }
    }

    private static Vector2 RotateVector(Vector2 v, float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);
        return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
    }

    private void UpdateLevelComplete(float dt)
    {
        _gameManager.StateTimer -= dt;
        if (_gameManager.StateTimer <= 0)
        {
            if (_gameManager.CurrentLevel >= LevelData.Levels.Length)
            {
                // All levels completed — player wins
                _isWin = true;
                _gameManager.State = GameState.GameOver;
            }
            else
            {
                // Load next level
                LoadLevel(_gameManager.CurrentLevel);
                _paddle.Position = new Vector2(580, 690);
                ResetBallOnPaddle();
                _gameManager.State = GameState.Playing;
            }
        }
    }

    private void UpdateGameOver()
    {
        if (InputManager.IsKeyPressed(Keys.Space))
        {
            _gameManager.Reset();
            _isWin = false;
            _gameManager.State = GameState.Title;
        }
    }

    private void UpdatePaused()
    {
        if (InputManager.IsKeyPressed(Keys.Escape))
        {
            _gameManager.State = GameState.Playing;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        switch (_gameManager.State)
        {
            case GameState.Title:
                DrawTitle();
                break;
            case GameState.Playing:
                DrawPlaying();
                break;
            case GameState.LevelComplete:
                DrawPlaying();
                DrawLevelComplete();
                break;
            case GameState.GameOver:
                DrawGameOver();
                break;
            case GameState.Paused:
                DrawPlaying();
                DrawPaused();
                break;
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawTitle()
    {
        string title = "BREAKOUT";
        Vector2 titleSize = _font.MeasureString(title);
        float titleScale = 4f;
        Vector2 titlePos = new Vector2(
            (1280 - titleSize.X * titleScale) / 2,
            720 / 2 - 80);
        _spriteBatch.DrawString(_font, title, titlePos, Color.White,
            0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

        if (_blinkVisible)
        {
            string startText = "Press SPACE to Start";
            Vector2 startSize = _font.MeasureString(startText);
            float startScale = 1.5f;
            Vector2 startPos = new Vector2(
                (1280 - startSize.X * startScale) / 2,
                720 / 2 + 40);
            _spriteBatch.DrawString(_font, startText, startPos, Color.White,
                0f, Vector2.Zero, startScale, SpriteEffects.None, 0f);
        }
    }

    private void DrawPlaying()
    {
        // Draw order per spec: bricks, power-ups, paddle, balls, HUD
        foreach (var brick in _bricks)
            brick.Draw(_spriteBatch);

        foreach (var pu in _powerUps)
            pu.Draw(_spriteBatch, _font);

        _paddle.Draw(_spriteBatch);

        foreach (var ball in _balls)
            ball.Draw(_spriteBatch);

        _hud.Draw(_spriteBatch, _font, _gameManager);
    }

    private void DrawLevelComplete()
    {
        string text = "LEVEL COMPLETE";
        Vector2 size = _font.MeasureString(text);
        float scale = 2.5f;
        Vector2 pos = new Vector2(
            (1280 - size.X * scale) / 2,
            720 / 2 - 20);
        _spriteBatch.DrawString(_font, text, pos, Color.White,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawGameOver()
    {
        string header = _isWin ? "YOU WIN" : "GAME OVER";
        Vector2 headerSize = _font.MeasureString(header);
        float headerScale = 3f;
        Vector2 headerPos = new Vector2(
            (1280 - headerSize.X * headerScale) / 2,
            720 / 2 - 80);
        _spriteBatch.DrawString(_font, header, headerPos, Color.White,
            0f, Vector2.Zero, headerScale, SpriteEffects.None, 0f);

        string scoreText = $"Final Score: {_gameManager.Score}";
        Vector2 scoreSize = _font.MeasureString(scoreText);
        float scoreScale = 1.5f;
        Vector2 scorePos = new Vector2(
            (1280 - scoreSize.X * scoreScale) / 2,
            720 / 2 + 10);
        _spriteBatch.DrawString(_font, scoreText, scorePos, Color.White,
            0f, Vector2.Zero, scoreScale, SpriteEffects.None, 0f);

        string restartText = "Press SPACE to Restart";
        Vector2 restartSize = _font.MeasureString(restartText);
        float restartScale = 1.2f;
        Vector2 restartPos = new Vector2(
            (1280 - restartSize.X * restartScale) / 2,
            720 / 2 + 80);
        _spriteBatch.DrawString(_font, restartText, restartPos, Color.White,
            0f, Vector2.Zero, restartScale, SpriteEffects.None, 0f);
    }

    private void DrawPaused()
    {
        // Semi-transparent black overlay
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, 1280, 720), Color.Black * 0.5f);

        // "PAUSED" text centered
        string pausedText = "PAUSED";
        Vector2 pausedSize = _font.MeasureString(pausedText);
        float pausedScale = 3f;
        Vector2 pausedPos = new Vector2(
            (1280 - pausedSize.X * pausedScale) / 2,
            720 / 2 - 40);
        _spriteBatch.DrawString(_font, pausedText, pausedPos, Color.White,
            0f, Vector2.Zero, pausedScale, SpriteEffects.None, 0f);
    }
}
