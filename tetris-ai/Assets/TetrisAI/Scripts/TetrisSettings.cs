using UnityEngine;

public class TetrisSettings
{
    public const int Width = 10;
    public const int Height = 18;

    public readonly static int[] Points = new int[]
    { 
        40, 100, 300, 1200
    };

    public const string ScoreFormat = "Score: {0}";
    public const string HighScoreFormat = "High Score: {0}";
    public const float FallTime = 0.8f;

    public static readonly Vector3 Offset = new Vector3(-1f, -0.5f, 0f);
}