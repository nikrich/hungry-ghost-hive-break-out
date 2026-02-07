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
    }

    protected override void Update(GameTime gameTime)
    {
        // Update input manager
        InputManager.Update();

        // TODO: Implement state machine based on GameManager.State

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // TODO: Draw entities based on draw order
        // 1. Bricks
        // 2. Power-ups
        // 3. Paddle
        // 4. Balls
        // 5. HUD
        // 6. Overlays

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
