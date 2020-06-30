using UnityEngine;

public class TetrisSettings
{
    public const int GridWidth = 10;
    public const int GridHeight = 19;
    public const int SpawnX = 4; // 5th column
    public const int SpawnY = 16; // 17th row

    public readonly static int[] Points = new int[]
    { 
        40, 100, 300, 1200
    };

    public const string ScoreFormat = "Score: {0}";
    public const string HighScoreFormat = "High Score: {0}";
    public const float FallTime = 0.1f; // 0.8f
}