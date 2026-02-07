namespace BreakoutGame;

public class GameManager
{
    public int Score { get; set; }
    public int Lives { get; set; }
    public int CurrentLevel { get; set; }
    public GameState State { get; set; }
    public float StateTimer { get; set; }

    public GameManager()
    {
        Reset();
    }

    public void AddScore(int points)
    {
        Score += points;
    }

    public void LoseLife()
    {
        Lives--;
        if (Lives <= 0)
        {
            State = GameState.GameOver;
        }
    }

    public void NextLevel()
    {
        CurrentLevel++;
        State = GameState.LevelComplete;
    }

    public void Reset()
    {
        Score = 0;
        Lives = 3;
        CurrentLevel = 0;
        State = GameState.Title;
        StateTimer = 0;
    }
}
