using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;

    private static int highScore;
    private static int highLines;

    public void SetScore(int points, int lines)
    {
        scoreText.text = string.Format(TetrisSettings.ScoreFormat, points, lines);
        SetHighScore(points, lines);
    }

    public void SetHighScore(int points, int lines)
    {
        if (points > highScore)
        {
            highScore = points;
            highLines = lines;
            Debug.Log(string.Format(TetrisSettings.HighScoreFormat, highScore, highLines));
        }

        string txt = string.Format(TetrisSettings.HighScoreFormat, highScore, highLines);
        highScoreText.text = txt;
    }
}
