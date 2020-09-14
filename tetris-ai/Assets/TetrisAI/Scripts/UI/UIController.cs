using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;

    private int highestPoints = -1;

    private void Awake()
    {
        Instance = this;
    }

    public void SetScore(int points)
    {
        scoreText.text = string.Format(TetrisSettings.ScoreFormat, points);
        SetHighScore(points);
    }

    public void SetHighScore(int points)
    {
        if (points > highestPoints)
        {
            Debug.Log("New High Score: " + points);
            highestPoints = points;
            highScoreText.text = string.Format(TetrisSettings.HighScoreFormat, highestPoints);
        }
    }
}
