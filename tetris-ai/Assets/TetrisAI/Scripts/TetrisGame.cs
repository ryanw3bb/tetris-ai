using System.Collections.Generic;
using UnityEngine;

public class TetrisGame : MonoBehaviour
{
    public TetrisBlock CurrentBlock { get; private set; }
    public Transform[,] Grid { get; private set; }

    [SerializeField] private UIController uiController;
    [SerializeField] private GameObject[] tetrominoes;
    private TetrisAgent agent;
    private List<GameObject> blocks = new List<GameObject>();
    private int currentPoints = 0;
    private Vector3 spawnPosition;

    public void Init(TetrisAgent agent)
    {
        this.agent = agent;
        spawnPosition = transform.position + new Vector3(TetrisSettings.SpawnX, TetrisSettings.SpawnY, 0);
        uiController.SetHighScore(0);
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
        Grid = new Transform[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
        ResetScore();
    }

    public void BlockPlaced(int lines = 0)
    {
        if (lines > 0)
        {
            AddToScore(TetrisSettings.Points[lines - 1]);
            agent.AddReward(Mathf.Pow(lines, 2) * TetrisSettings.GridWidth);
        }
        else
        {
            agent.AddReward(1);
        }

        NewTetrisBlock();
    }

    private void AddToScore(int points)
    { 
        currentPoints += points;
        uiController.SetScore(currentPoints);
    }

    private void ResetScore()
    {
        currentPoints = 0;
        uiController.SetScore(0);
    }

    private void NewTetrisBlock()
    {
        CurrentBlock = Instantiate(tetrominoes[Random.Range(0, tetrominoes.Length)],
            spawnPosition, Quaternion.identity).GetComponent<TetrisBlock>();
        CurrentBlock.transform.SetParent(transform);
        CurrentBlock.Init(this);
        blocks.Add(CurrentBlock.gameObject);
        
        agent.RequestDecision();
    }

    public void GameOver()
    {
        uiController.SetHighScore(currentPoints);
        ResetGame();
        agent.AddReward(-1f);
        agent.EndEpisode();
    }

    public float[] GetFlattenedGrid()
    {
        float[] flatGrid = new float[TetrisSettings.GridWidth * TetrisSettings.GridHeight];

        for(int y = 0; y < Grid.GetLength(1); y++)
        {
            for(int x = 0; x < Grid.GetLength(0); x++)
            {
                int idx = (y * TetrisSettings.GridWidth) + x;
                flatGrid[idx] = Grid[x, y] == null ? 0 : 1;
            }
        }

        return flatGrid;
    }   
}
