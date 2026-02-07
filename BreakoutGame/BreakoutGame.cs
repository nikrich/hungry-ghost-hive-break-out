using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BreakoutGame.Systems;

namespace BreakoutGame;

public class BreakoutGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;
    private SpriteFont _font;
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
}
