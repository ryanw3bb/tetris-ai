using MLAgents;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TetrisController : MonoBehaviour
{
    public TetrisBlock CurrentBlock { get; private set; }
    public Transform[,] Grid { get; private set; }

    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;  
    [SerializeField] private GameObject[] tetrominoes;
    private TetrisAgent agent;
    private List<GameObject> blocks = new List<GameObject>();
    private int currentPoints = 0;
    private int highestPoints = 0;

    private void Start()
    {
        agent = GetComponent<TetrisAgent>();
        Reset();
    }

    private void Update()
    {
        if (CurrentBlock == null) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CurrentBlock.MoveIfValid(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CurrentBlock.MoveIfValid(1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            CurrentBlock.RotateIfValid(90);
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            CurrentBlock.RotateIfValid(-90);
        }

        CurrentBlock.Tick(Input.GetKey(KeyCode.DownArrow));
    }

    private void Reset()
    {
        agent.Done();
        foreach (GameObject block in blocks)
            Destroy(block);
        blocks = new List<GameObject>();
        Grid = new Transform[TetrisSettings.Width, TetrisSettings.Height];
        ResetScore();
        NewTetrisBlock();
    }

    public void BlockPlaced(int lines = 0)
    {
        if (lines > 0) AddToScore(TetrisSettings.Points[lines - 1]);
        NewTetrisBlock();
    }

    private void AddToScore(int points)
    { 
        agent.AddReward(points / 100);

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
        CurrentBlock.Init(this, agent);
        blocks.Add(CurrentBlock.gameObject);
    }

    public void GameOver()
    {
        if(currentPoints > highestPoints)
        {
            highestPoints = currentPoints;
            highScoreText.text = string.Format(TetrisSettings.HighScoreFormat, highestPoints);
        }
        Reset();
    }

    public float[] GetFlattenedGrid()
    {
        float[] flatGrid = new float[TetrisSettings.Width * TetrisSettings.Height];
        
        for(int i = 0; i < this.Grid.GetLength(0); i++)
        {
            for(int j = 0; j < this.Grid.GetLength(1); j++)
            {
                int idx = (i * TetrisSettings.Width) + j;
                flatGrid[idx] = this.Grid[i, j] == null ? 0 : 1;
            }
        }

        return flatGrid;
    }   
}
