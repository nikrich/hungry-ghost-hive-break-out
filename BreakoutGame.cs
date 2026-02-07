using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace BreakoutGame;

public class BreakoutGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _pixel;
    private SpriteFont _font;
    private GameManager _gameManager;
    private HUD _hud;
    private Random _rng;

    private Paddle _paddle;
    private List<Ball> _balls;

    public BreakoutGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Set window size to 720x1080 portrait
        _graphics.PreferredBackBufferWidth = 720;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();

        // Set window title
        Window.Title = "Breakout";

        // Fixed timestep at 60 FPS
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);

        _gameManager = new GameManager();
        _hud = new HUD();
        _rng = new Random();

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

        // Create paddle
        _paddle = new Paddle(_pixel, new Vector2(300, 1050));

        // Create initial ball
        _balls = new List<Ball>();
        var ball = new Ball(_pixel, Vector2.Zero, 450f);
        _balls.Add(ball);
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();

        if (InputManager.IsKeyPressed(Keys.Escape))
            Exit();

        // Update paddle
        _paddle.Update(gameTime);

        // Update balls
        foreach (var ball in _balls)
        {
            ball.Update(gameTime, _paddle.Position, _paddle.Width);
        }

        // Handle space/enter to launch ball
        if (InputManager.IsKeyPressed(Keys.Space) || InputManager.IsKeyPressed(Keys.Enter))
        {
            foreach (var ball in _balls)
            {
                if (ball.IsAttached)
                {
                    ball.Launch();
                }
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Draw paddle
        _paddle.Draw(_spriteBatch);

        // Draw balls
        foreach (var ball in _balls)
        {
            ball.Draw(_spriteBatch);
        }

        // Draw HUD
        _hud.Draw(_spriteBatch, _font, _gameManager);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
