using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.UIElements;

public class TetrisGame : MonoBehaviour
{
    public TetrisBlock CurrentBlock { get; private set; }
    public Transform[,] Grid { get; private set; }
    public int[,] IntGrid { get; private set; }

    [SerializeField] private UIController uiController;
    [SerializeField] private GameObject[] tetrominoes;
    private TetrisAgent agent;
    private List<GameObject> blocks = new List<GameObject>();
    private int currentPoints = 0;
    private Vector3 spawnPosition;
    private int[,] gridTemp;
    private int linesCount;

    public void Init(TetrisAgent agent)
    {
        this.agent = agent;
        spawnPosition = transform.position + new Vector3(TetrisSettings.SpawnX, TetrisSettings.SpawnY, 0);
        Grid = new Transform[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
        IntGrid = new int[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
        gridTemp = new int[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
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
        Array.Clear(Grid, 0, Grid.GetLength(0) * Grid.GetLength(1));
        Array.Clear(IntGrid, 0, IntGrid.GetLength(0) * IntGrid.GetLength(1));
        ResetScore();
    }

    public void BlockPlaced(int lines = 0)
    {
        if (lines > 0)
        {
            linesCount += lines;
            AddToScore(TetrisSettings.Points[lines - 1]);
            agent.AddReward(Mathf.Pow(lines, 2) * TetrisSettings.GridWidth);
        }
        else
        {
            agent.AddReward(1f);
        }

        UpdateIntGrid();
        NewTetrisBlock();
    }

    private void UpdateIntGrid()
    {
        for (int i = 0; i < TetrisSettings.GridWidth; i++)
        {
            for (int j = 0; j < TetrisSettings.GridHeight; j++)
            {
                IntGrid[i, j] = Grid[i, j] == null ? 0 : 1;
            }
        }
    }

    private void AddToScore(int points)
    { 
        currentPoints += points;
        uiController.SetScore(currentPoints);
    }

    private void ResetScore()
    {
        currentPoints = 0;
        linesCount = 0;
        uiController.SetScore(0);
    }

    private void NewTetrisBlock()
    {
        CurrentBlock = Instantiate(tetrominoes[UnityEngine.Random.Range(0, tetrominoes.Length)],
            spawnPosition, Quaternion.identity).GetComponent<TetrisBlock>();
        CurrentBlock.transform.SetParent(transform);
        CurrentBlock.Init(this, agent);
        blocks.Add(CurrentBlock.gameObject);
        
        agent.RequestDecision();
    }

    public void GameOver()
    {
        uiController.SetHighScore(currentPoints);
        UpdateStats();
        ResetGame();
        agent.AddReward(-1f);
        agent.EndEpisode();
    }

    private void UpdateStats()
    {
        if (agent.IsTraining)
        {
            Academy.Instance.StatsRecorder.Add("Score", currentPoints);
            Academy.Instance.StatsRecorder.Add("Lines", linesCount);
        }    
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
    
    public float[] GetState(int rotation, int xPosition)
    {
        float[] states = new float[4];

        // get the position of each square when rotated at x position
        Vector2Int[] positions = CurrentBlock.TransformPosition(xPosition, TetrisSettings.Rotations[rotation]);

        /*if (positions != null)
        {
            Debug.Log("positions: " + positions[0] + " " + positions[1] + " " + positions[2] + " " + positions[3]);
        }*/

        bool placed = false;

        // if positions is returned as null it means the placement is invalid
        if (positions != null)
        {
            // move down until the piece is placed
            while (!placed)
            {
                // TODO: make this more elegant
                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i].y--;
                    if (positions[i].y < 0 || Grid[positions[i].x, positions[i].y] != null)
                    {
                        // TODO HERE: check to see how far down we've gone - if it's 1 square it's still invalid
                        placed = true;
                    }
                }

                if (placed)
                {
                    for (int i = 0; i < positions.Length; i++)
                    {
                        positions[i].y++;
                    }
                }
            }

            gridTemp = IntGrid.Clone() as int[,];
            for (int i = 0; i < positions.Length; i++)
            {
                gridTemp[positions[i].x, positions[i].y] = 1;
            }

            states[0] = NumLines(ref gridTemp);
            GetGridProperties(gridTemp, ref states[1], ref states[2], ref states[3]);

            // normalise state values 0 - 1
            states[0] = states[0] / 4f; // max value numlines = 4
            states[1] = states[1] / 100f; // height
            states[2] = states[2] / 100f; // bumpiness
            states[3] = states[3] / 100f; // numHoles
        }
        
        if (positions == null || !placed)
        {
            states[0] = -1;
            states[1] = -1;
            states[2] = -1;
            states[3] = -1;
        }
         
        /*if (states[0] > 0)
        {
            Debug.Log("rotation: " + rotation + " xpos: " + xPosition + " state: numLines: " + states[0] + 
                " totalHeight: " + states[1] + " bumpiness: " + states[2] + " numHoles: " + states[3]);
            Debug.Break();
        }*/

        return states;
    }

    public void LogState()
    {
        float[] states = new float[4];
        gridTemp = IntGrid.Clone() as int[,];

        states[0] = NumLines(ref gridTemp);
        GetGridProperties(gridTemp, ref states[1], ref states[2], ref states[3]);

        Debug.Log(string.Format("numLines:{0} totalHeight:{1} bumpiness:{2} numHoles:{3}", states[0], states[1], states[2], states[3]));
    }

    private float NumLines(ref int[,] grid)
    {
        int numLines = 0;

        for (int i = TetrisSettings.GridHeight - 1; i >= 0; i--)
        {
            if (HasLine(grid, i))
            {
                DeleteLine(ref grid, i);
                RowDown(ref grid, i);
                numLines++;
            }
        }

        return numLines;
    }

    private bool HasLine(int[,] grid, int i)
    {
        for (int j = 0; j < TetrisSettings.GridWidth; j++)
        {
            if (grid[j, i] == 0)
            {
                return false;
            }
        }

        return true;
    }

    private void DeleteLine(ref int[,] grid, int i)
    {
        for (int j = 0; j < TetrisSettings.GridWidth; j++)
        {
            grid[j, i] = 0;
        }
    }

    private void RowDown(ref int[,] grid, int i)
    {
        for (int y = i; y < TetrisSettings.GridHeight; y++)
        {
            for (int x = 0; x < TetrisSettings.GridWidth; x++)
            {
                if (grid[x, y] == 1)
                {
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = 0;
                }
            }
        }
    }

    private void GetGridProperties(int[,] grid, ref float totalHeight, ref float bumpiness, ref float numHoles)
    {
        int[] heights = new int[TetrisSettings.GridWidth];

        for (int i = 0; i < TetrisSettings.GridWidth; i++)
        {
            bool foundHeight = false;
            for (int j = TetrisSettings.GridHeight - 1; j >= 0; j--)
            {
                if (!foundHeight)
                {
                    if (grid[i, j] == 1)
                    {
                        heights[i] = j + 1;
                        totalHeight += j + 1;
                        foundHeight = true;
                    }
                }
                else
                {
                    if(grid[i, j] == 0)
                    {
                        numHoles++;
                    }
                }
            }
        }

        for (int i = 0; i < heights.Length - 1; i++)
        {
            bumpiness += Mathf.Abs(heights[i] - heights[i + 1]);
        }
    }
}
