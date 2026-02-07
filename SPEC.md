# Breakout Game — MonoGame Implementation Spec

## Overview

A classic Breakout/Arkanoid-style game built in **MonoGame (C#)**. The player controls a paddle at the bottom of the screen, bouncing a ball to destroy bricks arranged in rows at the top. The game features multiple levels, power-ups, a lives system, and a score tracker.

---

## Project Setup

- **Framework:** MonoGame 3.8+ (Desktop GL or DirectX)
- **Language:** C# (.NET 8+)
- **Project template:** `dotnet new mgdesktopgl -o BreakoutGame`
- **Resolution:** 720×1080 (portrait orientation)
- **Window:** Non-resizable, title "Breakout"
- **Target framerate:** 60 FPS (fixed timestep)
- **Assets:** All visuals are drawn programmatically using `SpriteBatch` with a **1×1 white pixel texture** (created at runtime via `new Texture2D(GraphicsDevice, 1, 1)` and `SetData(new[] { Color.White })`). Shapes are drawn by scaling and coloring this texture. **No external content pipeline assets** except for one `.spritefont` for text rendering.
- **Font:** Add a single `GameFont.spritefont` to the Content project (14pt Arial or similar). This is the only Content Pipeline asset.

---

## Architecture

### Solution Structure

```
BreakoutGame/
├── BreakoutGame.csproj
├── Program.cs
├── BreakoutGame.cs            (main Game1 class)
├── Content/
│   └── GameFont.spritefont
├── GameState.cs               (enum: Title, Playing, LevelComplete, GameOver, Paused)
├── GameManager.cs             (score, lives, current level, state transitions)
├── Entities/
│   ├── Paddle.cs
│   ├── Ball.cs
│   ├── Brick.cs
│   └── PowerUp.cs
├── Levels/
│   └── LevelData.cs           (static level definitions)
├── Systems/
│   ├── CollisionHelper.cs     (AABB and circle-rect collision)
│   └── InputManager.cs        (keyboard + mouse abstraction)
└── UI/
    └── HUD.cs
```

### Design Principles

- **No entity base class required.** Each entity is a standalone class with `Update(GameTime)` and `Draw(SpriteBatch)` methods.
- **Game1 (`BreakoutGame.cs`) orchestrates everything:** holds lists of entities, calls update/draw, manages state.
- **Collision is checked in Game1.Update()**, not inside individual entities.

---

## Core Classes

### `BreakoutGame.cs` (Game1)

Responsibilities:
- Owns the `Paddle`, `List<Ball>`, `List<Brick>`, `List<PowerUp>`, `GameManager`, `HUD`
- Creates the shared 1×1 white pixel `Texture2D` in `LoadContent()` and passes it to all entities
- Runs a state machine each frame based on `GameManager.State`
- Performs all collision detection in `Update()`
- Calls `Draw()` on all active entities

Key fields:
```csharp
Texture2D _pixel;
SpriteFont _font;
Paddle _paddle;
List<Ball> _balls;
List<Brick> _bricks;
List<PowerUp> _powerUps;
GameManager _gameManager;
HUD _hud;
Random _rng;
```

### `GameState.cs`

```csharp
public enum GameState
{
    Title,
    Playing,
    LevelComplete,
    GameOver,
    Paused
}
```

### `GameManager.cs`

Fields:
- `int Score`
- `int Lives` (starts at 3, max 5)
- `int CurrentLevel` (0-indexed)
- `GameState State`
- `float StateTimer` (used for level-complete delay, blink timers, etc.)

Methods:
- `void AddScore(int points)`
- `void LoseLife()` → decrements lives, sets state to `GameOver` if lives reach 0
- `void NextLevel()` → increments level, sets state to `LevelComplete`
- `void Reset()` → resets score, lives, level to starting values

---

## Game Objects

### Paddle

**Fields:**
- `Vector2 Position` (top-left corner)
- `int Width` = 120 (default), 180 (wide power-up)
- `int Height` = 16
- `Color Color` = `Color.White`
- `float Speed` = 600f
- `float WidePowerUpTimer` = 0

**Update logic:**
1. If left key held: move left by `Speed * deltaTime`
2. If right key held: move right by `Speed * deltaTime`
3. Also track mouse: `Position.X = mouseX - Width / 2`
4. Clamp X so paddle stays within play area (walls at x=0 and x=720)
5. If `WidePowerUpTimer > 0`: decrement timer. On expiry, reset `Width` to 120 and re-center.

**Draw:** `spriteBatch.Draw(_pixel, new Rectangle((int)Position.X, (int)Position.Y, Width, Height), Color)`

**Collision rectangle:** `new Rectangle((int)Position.X, (int)Position.Y, Width, Height)`

### Ball

**Fields:**
- `Vector2 Position` (center)
- `Vector2 Velocity`
- `float Radius` = 8
- `float Speed` = 450f (magnitude of velocity vector, capped at 700)
- `bool IsAttached` = true (stuck to paddle before launch)
- `Color Color` = `Color.White`
- `float SpeedMultiplier` = 1.0f (for Speed Down power-up)

**Update logic:**
1. If `IsAttached`: position follows paddle center (centered horizontally, sitting on top of paddle).
2. If launched: `Position += Velocity * SpeedMultiplier * deltaTime`
3. **Wall collisions (handled internally):**
   - Left wall (x ≤ Radius): reflect X velocity, clamp position
   - Right wall (x ≥ 720 - Radius): reflect X velocity, clamp position
   - Top wall (y ≤ Radius): reflect Y velocity, clamp position
4. **Safety clamp:** If `Math.Abs(Velocity.Y) < 100`, set `Velocity.Y = Math.Sign(Velocity.Y) * 100` (or 100 if sign is 0).

**Launch:**
When player presses Space/Enter while `IsAttached`:
- `IsAttached = false`
- Set velocity to a random upward angle: angle between -30° and +30° from straight up
- `Velocity = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * Speed`

**Draw:** Draw as a filled square (good enough) or draw a circle by drawing a scaled pixel. Simplest approach:
```csharp
spriteBatch.Draw(_pixel, new Rectangle(
    (int)(Position.X - Radius), (int)(Position.Y - Radius),
    (int)(Radius * 2), (int)(Radius * 2)), Color);
```

**Bounding box for collision:** `new Rectangle((int)(Position.X - Radius), (int)(Position.Y - Radius), (int)(Radius * 2), (int)(Radius * 2))`

### Brick

**Fields:**
- `Vector2 Position` (top-left)
- `int Width` = 64
- `int Height` = 24
- `int MaxHP` (set on creation from level data)
- `int HP`
- `bool IsIndestructible` (true if level data value is -1)
- `bool IsAlive` = true
- `Color Color` (derived from current HP)

**Color mapping method:**
```csharp
public Color GetColorForHP(int hp) => hp switch
{
    1 => new Color(76, 175, 80),    // Green
    2 => new Color(255, 235, 59),   // Yellow
    3 => new Color(255, 152, 0),    // Orange
    _ => new Color(244, 67, 54),    // Red (4+)
};
// Indestructible: new Color(158, 158, 158) // Gray
```

**Hit method:**
```csharp
public int Hit()
{
    if (IsIndestructible) return 0;
    HP--;
    if (HP <= 0)
    {
        IsAlive = false;
        return PointsForMaxHP(MaxHP); // score value
    }
    Color = GetColorForHP(HP);
    return 0;
}
```

**Point values by MaxHP:**

| MaxHP | Points |
|-------|--------|
| 1     | 10     |
| 2     | 20     |
| 3     | 30     |
| 4     | 50     |

**Draw:** `spriteBatch.Draw(_pixel, new Rectangle(...), Color)` — only if `IsAlive`.

**Collision rectangle:** `new Rectangle((int)Position.X, (int)Position.Y, Width, Height)`

### PowerUp

**Fields:**
- `Vector2 Position` (top-left)
- `int Size` = 24
- `float FallSpeed` = 200f
- `PowerUpType Type` (enum)
- `bool IsActive` = true
- `Color Color`
- `string Label`

**PowerUpType enum:**
```csharp
public enum PowerUpType
{
    WidePaddle,   // Blue #2196F3, label "W"
    MultiBall,    // Purple #9C27B0, label "M"
    ExtraLife,    // Pink #E91E63, label "+"
    SpeedDown     // Cyan #00BCD4, label "S"
}
```

**Update:** `Position.Y += FallSpeed * deltaTime`. If `Position.Y > 1080`, set `IsActive = false`.

**Draw:** Draw the colored square, then draw the label letter centered on it using the `SpriteFont`.

**Collision rectangle:** `new Rectangle((int)Position.X, (int)Position.Y, Size, Size)`

---

## Collision Detection

All collision is done in `BreakoutGame.Update()`. Use AABB (axis-aligned bounding box) checks.

### `CollisionHelper.cs`

```csharp
public static class CollisionHelper
{
    /// Returns true if two rectangles overlap.
    public static bool Intersects(Rectangle a, Rectangle b) => a.Intersects(b);

    /// Determines which face of the brick the ball hit.
    /// Returns a normalized reflection vector.
    public static Vector2 GetBallBrickReflection(Ball ball, Brick brick)
    {
        // Calculate overlap depths from each side
        float overlapLeft = (ball.Position.X + ball.Radius) - brick.Position.X;
        float overlapRight = (brick.Position.X + brick.Width) - (ball.Position.X - ball.Radius);
        float overlapTop = (ball.Position.Y + ball.Radius) - brick.Position.Y;
        float overlapBottom = (brick.Position.Y + brick.Height) - (ball.Position.Y - ball.Radius);

        // Find minimum overlap to determine collision face
        float minOverlapX = Math.Min(overlapLeft, overlapRight);
        float minOverlapY = Math.Min(overlapTop, overlapBottom);

        Vector2 velocity = ball.Velocity;
        if (minOverlapX < minOverlapY)
            velocity.X = -velocity.X; // Side hit
        else
            velocity.Y = -velocity.Y; // Top/bottom hit

        return velocity;
    }
}
```

### Collision order each frame:

1. **Ball vs. Paddle:**
   - Check AABB overlap.
   - If colliding and ball is moving downward (`Velocity.Y > 0`):
     - Calculate normalized hit position: `hitPos = (ball.Position.X - paddle.Position.X) / paddle.Width` (clamped to 0–1)
     - Map to angle: `angle = MathHelper.Lerp(-60°, 60°, hitPos)` (in radians)
     - Set `ball.Velocity = new Vector2(MathF.Sin(angle), -MathF.Cos(angle)) * ball.Speed * ball.SpeedMultiplier`

2. **Ball vs. Bricks (for each ball, for each alive brick):**
   - Check AABB overlap.
   - If colliding:
     - Call `brick.Hit()` → get points, add to score
     - Reflect ball velocity using `CollisionHelper.GetBallBrickReflection()`
     - If brick was destroyed: roll power-up spawn (15% chance)
     - **Break after first brick collision per ball per frame** to avoid multi-hit glitches

3. **PowerUp vs. Paddle:**
   - Check AABB overlap.
   - If colliding: apply power-up effect, set `powerUp.IsActive = false`

4. **Ball vs. Death Zone (y > 1080 + ball.Radius):**
   - Remove ball from list.
   - If no balls remain: lose a life, reset ball on paddle.

---

## Power-Up Application

Implemented in `BreakoutGame.cs`:

```csharp
void ApplyPowerUp(PowerUpType type)
{
    switch (type)
    {
        case PowerUpType.WidePaddle:
            _paddle.Width = 180;
            _paddle.WidePowerUpTimer = 10f;
            break;

        case PowerUpType.MultiBall:
            // Spawn 2 new balls from the first ball's position
            var source = _balls[0];
            for (int i = 0; i < 2; i++)
            {
                var newBall = new Ball(_pixel, source.Position, source.Speed);
                float angle = (i == 0 ? -30f : 30f) * MathF.PI / 180f;
                // Rotate source velocity by angle
                newBall.Velocity = RotateVector(source.Velocity, angle);
                newBall.IsAttached = false;
                newBall.SpeedMultiplier = source.SpeedMultiplier;
                _balls.Add(newBall);
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
```

When `_speedDownTimer` expires, reset all balls' `SpeedMultiplier` to 1.0f.

---

## Game Flow

### State: `Title`

- Clear screen to black.
- Draw "BREAKOUT" centered, large (scale the font or draw with big characters).
- Draw "Press SPACE to Start" below, blinking (toggle every 0.5s using a timer).
- On Space press → `GameManager.Reset()`, load level 0, set state to `Playing`.

### State: `Playing`

1. Update paddle, balls, power-ups.
2. Run collision checks.
3. Clean up dead bricks and inactive power-ups from lists.
4. Check win condition: if all non-indestructible bricks are destroyed → `GameManager.NextLevel()`.
5. On Escape press → set state to `Paused`.

### State: `LevelComplete`

- Display "LEVEL COMPLETE" centered on screen.
- Wait 1.5 seconds (`StateTimer` countdown).
- Then load next level. If no more levels → set state to `GameOver` with win flag.
- Reset ball to paddle.

### State: `GameOver`

- Display "GAME OVER" or "YOU WIN" centered.
- Display `"Final Score: {score}"` below.
- Display "Press SPACE to play again".
- On Space → transition to `Title`.

### State: `Paused`

- Draw a semi-transparent black overlay (`Color.Black * 0.5f`, full screen rectangle).
- Draw "PAUSED" centered.
- On Escape press → set state back to `Playing`.
- **Do not call Update on any entities while paused.**

---

## Level Data

### `LevelData.cs`

```csharp
public static class LevelData
{
    // 0 = empty, 1-4 = HP, -1 = indestructible
    public static readonly int[][,] Levels = new[]
    {
        // Level 1 — Introduction
        new int[,]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        },

        // Level 2 — Mixed Durability
        new int[,]
        {
            { 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        },

        // Level 3 — Fortress
        new int[,]
        {
            { 0, 0, 3, 3, 3, 3, 3, 0, 0 },
            { 0, 3, 2, 2, 2, 2, 2, 3, 0 },
            { 3, 2, 1, 1, 1, 1, 1, 2, 3 },
            { 3, 2, 1, 1, 1, 1, 1, 2, 3 },
            { 0, 3, 2, 2, 2, 2, 2, 3, 0 },
            { 0, 0, 3, 3, 3, 3, 3, 0, 0 },
        },

        // Level 4 — Indestructible Maze
        new int[,]
        {
            {  4,  4,  4,  4,  4,  4,  4,  4,  4 },
            {  1,  1,  1,  1,  1,  1,  1,  1,  1 },
            { -1,  0, -1,  0, -1,  0, -1,  0, -1 },
            {  1,  1,  1,  1,  1,  1,  1,  1,  1 },
            {  2,  2,  2,  2,  2,  2,  2,  2,  2 },
        },

        // Level 5 — Final Stand
        new int[,]
        {
            {  4,  4,  4,  4,  4,  4,  4,  4,  4 },
            {  3,  3,  3,  3,  3,  3,  3,  3,  3 },
            {  2,  2,  2,  2,  2,  2,  2,  2,  2 },
            {  1,  1,  1,  1,  1,  1,  1,  1,  1 },
            { -1,  0,  0, -1,  0, -1,  0,  0, -1 },
            {  1,  1,  1,  1,  1,  1,  1,  1,  1 },
            {  2,  2,  2,  2,  2,  2,  2,  2,  2 },
        },
    };
}
```

### Loading a Level

In `BreakoutGame.cs`:
```csharp
void LoadLevel(int levelIndex)
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
```

---

## HUD

### `HUD.cs`

Draws three text elements each frame:

| Element | Position | Format |
|---------|----------|--------|
| Score   | Top-left (16, 8) | `"SCORE: 00000"` (zero-padded to 5 digits) |
| Level   | Top-center | `"LEVEL 1"` |
| Lives   | Top-right (right-aligned) | `"LIVES: 3"` |

```csharp
public void Draw(SpriteBatch spriteBatch, SpriteFont font, GameManager gm)
{
    string scoreText = $"SCORE: {gm.Score:D5}";
    string levelText = $"LEVEL {gm.CurrentLevel + 1}";
    string livesText = $"LIVES: {gm.Lives}";

    spriteBatch.DrawString(font, scoreText, new Vector2(16, 8), Color.White);

    var levelSize = font.MeasureString(levelText);
    spriteBatch.DrawString(font, levelText, new Vector2(360 - levelSize.X / 2, 8), Color.White);

    var livesSize = font.MeasureString(livesText);
    spriteBatch.DrawString(font, livesText, new Vector2(704 - livesSize.X, 8), Color.White);
}
```

---

## Scoring

| Event | Points |
|-------|--------|
| Destroy 1-HP brick | 10 |
| Destroy 2-HP brick | 20 |
| Destroy 3-HP brick | 30 |
| Destroy 4-HP brick | 50 |
| Clear level bonus | 500 × (level number, 1-indexed) |

---

## Input

### `InputManager.cs`

Thin wrapper over `Keyboard.GetState()` and `Mouse.GetState()`:

```csharp
public static class InputManager
{
    private static KeyboardState _prevKb, _currKb;
    private static MouseState _currMouse;

    public static void Update()
    {
        _prevKb = _currKb;
        _currKb = Keyboard.GetState();
        _currMouse = Mouse.GetState();
    }

    public static bool IsKeyDown(Keys key) => _currKb.IsKeyDown(key);
    public static bool IsKeyPressed(Keys key) => _currKb.IsKeyDown(key) && !_prevKb.IsKeyDown(key);
    public static int MouseX => _currMouse.X;
}
```

**Key bindings:**

| Action | Keys |
|--------|------|
| Move left | `A`, `Left` |
| Move right | `D`, `Right` |
| Launch / Confirm | `Space`, `Enter` |
| Pause | `Escape` |
| Mouse | Paddle follows `Mouse.X` |

---

## Ball Speed Progression

After each level clear, the base ball speed increases:

```csharp
float newSpeed = Math.Min(450f + (levelIndex * 25f), 700f);
```

New balls created (on reset or multi-ball) use this speed.

---

## Edge Cases & Robustness

1. **Ball stuck in brick:** Only process one brick collision per ball per frame. After reflecting, skip remaining bricks for that ball this frame.
2. **Multi-ball life loss:** Only call `LoseLife()` when `_balls.Count == 0` after removing dead balls, not on each individual ball death.
3. **Power-up stacking:** Wide Paddle and Speed Down timers reset on re-collection (don't stack). Multi-Ball and Extra Life are instant.
4. **Near-horizontal ball:** Enforce `Math.Abs(Velocity.Y) >= 100` after every reflection.
5. **Ball outside bounds:** After each position update, clamp ball inside the play area as a safety net.
6. **Frame-rate independence:** Always multiply velocities by `(float)gameTime.ElapsedGameTime.TotalSeconds`.

---

## Draw Order

In `BreakoutGame.Draw()`:

```csharp
GraphicsDevice.Clear(Color.Black);

_spriteBatch.Begin();

// 1. Bricks (back)
foreach (var brick in _bricks)
    brick.Draw(_spriteBatch);

// 2. Power-ups
foreach (var pu in _powerUps)
    pu.Draw(_spriteBatch, _font);

// 3. Paddle
_paddle.Draw(_spriteBatch);

// 4. Balls (front)
foreach (var ball in _balls)
    ball.Draw(_spriteBatch);

// 5. HUD (topmost)
_hud.Draw(_spriteBatch, _font, _gameManager);

// 6. Overlays (pause, level complete, etc.)
DrawOverlays();

_spriteBatch.End();
```

---

## Implementation Order for Claude Code

Build and test in this order:

1. **Scaffold:** `dotnet new mgdesktopgl`, create folder structure, set up 720×1080 window, create 1×1 pixel texture, load font.
2. **Paddle:** Render, move with keyboard + mouse, clamp to screen.
3. **Ball:** Render on paddle, launch with Space, bounce off walls.
4. **Ball ↔ Paddle collision:** Angle-based reflection.
5. **Bricks:** Load Level 1 grid, render bricks.
6. **Ball ↔ Brick collision:** Reflection, HP decrement, destruction, scoring.
7. **Level progression:** Clear condition, load next level, ball speed increase.
8. **Power-ups:** Spawn on brick death, fall, collect, apply effects, timers.
9. **Lives system:** Death zone, life loss, ball reset, game over.
10. **Game states:** Title screen, game over screen, pause overlay.
11. **Polish:** HUD formatting, level complete transition, win condition.

---

## Minimum Viable Output

After step 6, you should have a playable single-level Breakout game. Everything after that is incremental improvement. Prioritize getting the core loop (paddle → ball → bricks → score) working first.