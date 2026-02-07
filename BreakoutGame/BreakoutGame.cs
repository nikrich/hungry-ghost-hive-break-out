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

        // TODO: Implement state machine based on GameManager.State
        // For now, just update entities and handle collisions

        // Update balls
        foreach (var ball in _balls)
            ball.Update(gameTime);

        // Ball vs Brick collision
        foreach (var ball in _balls)
        {
            Rectangle ballRect = new Rectangle(
                (int)(ball.Position.X - ball.Radius),
                (int)(ball.Position.Y - ball.Radius),
                (int)(ball.Radius * 2),
                (int)(ball.Radius * 2));

            // Check collision with each alive brick
            foreach (var brick in _bricks)
            {
                if (!brick.IsAlive) continue;

                Rectangle brickRect = new Rectangle(
                    (int)brick.Position.X,
                    (int)brick.Position.Y,
                    brick.Width,
                    brick.Height);

                if (CollisionHelper.Intersects(ballRect, brickRect))
                {
                    // Hit the brick and get points
                    int points = brick.Hit();
                    if (points > 0)
                        _gameManager.AddScore(points);

                    // Reflect ball velocity
                    ball.Velocity = CollisionHelper.GetBallBrickReflection(ball, brick);

                    // Break after first brick collision to prevent multi-hit glitches
                    break;
                }
            }
        }

        // Remove dead bricks
        _bricks.RemoveAll(b => !b.IsAlive);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        // Draw entities based on draw order
        // 1. Bricks
        foreach (var brick in _bricks)
            brick.Draw(_spriteBatch);

        // 2. Power-ups (TODO)
        // 3. Paddle (TODO)
        // 4. Balls (TODO)
        // 5. HUD (TODO)
        // 6. Overlays (TODO)

        _spriteBatch.End();

        base.Draw(gameTime);
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
