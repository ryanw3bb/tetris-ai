public class TetrisSettings
{
    // Grid
    public const int GridWidth = 10;
    public const int GridHeight = 22;
    public const int SpawnX = 4; // 5th column
    public const int SpawnY = 20; // 21st row

    public readonly static int[] Points = new int[] { 40, 100, 300, 1200 };
    public readonly static int[] Rotations = new int[] { 0, 90, 180, 270 };

    public const float NumRotations = 4;
    public const string ScoreFormat = "Score: {0}\nLines: {1}";
    public const string HighScoreFormat = "High Score: {0}\nLines: {1}";
    public const float FallTime = 0.01f;

    public static float GridSize = GridWidth * (SpawnY + 1);
    public static int PossibleStates = Rotations.Length * GridWidth;

    public class Reward
    {
        public const float BlockPlaced = 1;
        public const float GameOver = -10f;
    }
}

public enum Tetromino { I = 0, J = 1, L = 2, O = 3, S = 4, T = 5, Z = 6 };