using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BreakoutGame.Entities;
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

        // Create initial ball
        var ball = new Ball(_pixel, new Vector2(360, 1032), 450f);
        _balls.Add(ball);
    }

    protected override void Update(GameTime gameTime)
    {
        // Update input manager
        InputManager.Update();

        // TODO: Implement state machine based on GameManager.State
        // For now, just update the paddle and balls
        _paddle?.Update(gameTime);

        // Update all balls
        foreach (var ball in _balls)
        {
            ball.Update(gameTime, _paddle);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Draw entities based on draw order
        // 1. Bricks (TODO)
        // 2. Power-ups (TODO)
        // 3. Paddle
        _paddle?.Draw(_spriteBatch);
        // 4. Balls
        foreach (var ball in _balls)
        {
            ball.Draw(_spriteBatch);
        }
        // 5. HUD (TODO)
        // 6. Overlays (TODO)

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
