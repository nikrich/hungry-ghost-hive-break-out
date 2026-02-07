using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BreakoutGame.Entities;
using BreakoutGame.Systems;

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
    private Random _rng;
    private float _blinkTimer;
    private bool _blinkVisible;

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
        _graphics.PreferredBackBufferWidth = 720;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.ApplyChanges();

        Window.AllowUserResizing = false;
        Window.Title = "Breakout";

        _gameManager = new GameManager();
        _rng = new Random();
        _balls = new List<Ball>();
        _bricks = new List<Brick>();
        _powerUps = new List<PowerUp>();
        _blinkTimer = 0f;
        _blinkVisible = true;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _font = Content.Load<SpriteFont>("GameFont");

        _paddle = new Paddle(_pixel, new Vector2(300, 1040));
        ResetBallOnPaddle();
    }

    private void ResetBallOnPaddle()
    {
        _balls.Clear();
        var ball = new Ball(_pixel, Vector2.Zero, 450f);
        ball.IsAttached = true;
        ball.AttachToPaddle(_paddle);
        _balls.Add(ball);
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (_gameManager.State)
        {
            case GameState.Title:
                UpdateTitle(dt);
                break;
            case GameState.Playing:
                UpdatePlaying(dt);
                break;
            case GameState.LevelComplete:
                break;
            case GameState.GameOver:
                break;
            case GameState.Paused:
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
            _paddle.Position = new Vector2(300, 1040);
            ResetBallOnPaddle();
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
                break;
            case GameState.GameOver:
                break;
            case GameState.Paused:
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
            (720 - titleSize.X * titleScale) / 2,
            1080 / 2 - 80);
        _spriteBatch.DrawString(_font, title, titlePos, Color.White,
            0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

        if (_blinkVisible)
        {
            string startText = "Press SPACE to Start";
            Vector2 startSize = _font.MeasureString(startText);
            float startScale = 1.5f;
            Vector2 startPos = new Vector2(
                (720 - startSize.X * startScale) / 2,
                1080 / 2 + 40);
            _spriteBatch.DrawString(_font, startText, startPos, Color.White,
                0f, Vector2.Zero, startScale, SpriteEffects.None, 0f);
        }
    }

    private void DrawPlaying()
    {
        _paddle.Draw(_spriteBatch);

        foreach (var ball in _balls)
            ball.Draw(_spriteBatch);
    }
}
