public class TetrisSettings
{
    // Grid
    public const int GridWidth = 10;
    public const int GridHeight = 19;
    public const int SpawnX = 4; // 5th column
    public const int SpawnY = 16; // 17th row

    public readonly static int[] Points = new int[] { 40, 100, 300, 1200 };
    public readonly static int[] Rotations = new int[] { 0, 90, 180, 270 };

    public const string ScoreFormat = "Score: {0}";
    public const string HighScoreFormat = "High Score: {0}";
    public const float TrainFallTime = 0.1f;
    public const float PlayFallTime = 0.1f; //0.8f
}