using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

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
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Update();

        if (InputManager.IsKeyPressed(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Draw basic HUD for now
        _hud.Draw(_spriteBatch, _font, _gameManager);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
