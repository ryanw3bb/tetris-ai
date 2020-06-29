using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TetrisGame : MonoBehaviour
{
    public bool HumanPlayer;
    public bool RandomMode;

    public TetrisBlock CurrentBlock { get; private set; }
    public Transform[,] Grid { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;  
    [SerializeField] private GameObject[] tetrominoes;
    private TetrisAgent agent;
    private List<GameObject> blocks = new List<GameObject>();
    private int currentPoints = 0;
    private int highestPoints = 0;

    public void Init(TetrisAgent agent)
    {
        this.agent = agent;
        ResetGame();
    }

    public void StartGame()
    {
        NewTetrisBlock();
    }

    private void ResetGame()
    {
        foreach (GameObject block in blocks)
        {
            Destroy(block);
        }

        blocks = new List<GameObject>();
        Grid = new Transform[TetrisSettings.Width, TetrisSettings.Height];
        ResetScore();
    }

    public void BlockPlaced(int lines = 0)
    {
        if (lines > 0)
            AddToScore(TetrisSettings.Points[lines - 1]);
        else
            agent.AddReward(0.01f);

        NewTetrisBlock();
    }

    private void AddToScore(int points)
    { 
        agent.AddReward(points / 10);

        currentPoints += points;
        scoreText.text = string.Format(TetrisSettings.ScoreFormat, currentPoints);
    }

    private void ResetScore()
    {
        currentPoints = 0;
        scoreText.text = string.Format(TetrisSettings.ScoreFormat, currentPoints);
    }

    private void NewTetrisBlock()
    {
        CurrentBlock = Instantiate(tetrominoes[Random.Range(0, tetrominoes.Length)],
            transform.position, Quaternion.identity).GetComponent<TetrisBlock>();
        CurrentBlock.Init(this);
        blocks.Add(CurrentBlock.gameObject);
    }

    public void GameOver()
    {
        if(currentPoints > highestPoints)
        {
            highestPoints = currentPoints;
            highScoreText.text = string.Format(TetrisSettings.HighScoreFormat, highestPoints);
        }

        ResetGame();
        agent.AddReward(-5f);
        agent.EndEpisode();
    }

    public float[] GetFlattenedGrid()
    {
        float[] flatGrid = new float[TetrisSettings.Width * TetrisSettings.Height];

        for(int y = 0; y < Grid.GetLength(1); y++)
        {
            for(int x = 0; x < Grid.GetLength(0); x++)
            {
                int idx = (y * TetrisSettings.Width) + x;
                flatGrid[idx] = Grid[x, y] == null ? 0 : 1;
            }
        }

        return flatGrid;
    }   
}
