using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.MLAgents;
using UnityEngine;

public class TetrisGame : MonoBehaviour
{
    public Transform[,] Grid { get; private set; }
    private int[,] intGrid;

    public List<float> States;
    public List<int> MaskedActions;

    [SerializeField] private UIController uiController;
    [SerializeField] private TetrisBlock[] tetrominoes;
    private TetrisAgent agent;
    private TetrisBag bag;
    private List<GameObject> blocks = new List<GameObject>();
    private int currentPoints = 0;
    private int currentPiece;
    private int[,] gridTemp;
    private int[] lineCount = new int[4];

    public void Init(TetrisAgent agent)
    {
        this.agent = agent;
        Grid = new Transform[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
        intGrid = new int[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
        gridTemp = new int[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
        uiController.SetHighScore(0);

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i] = Instantiate(tetrominoes[i], Vector3.zero, Quaternion.identity).GetComponent<TetrisBlock>();
            tetrominoes[i].transform.SetParent(transform);
            tetrominoes[i].Init(this, agent, true);
            tetrominoes[i].gameObject.SetActive(false);
        }
    }
    
    public void StartGame()
    {
        ResetGame();
        GetNextStates();
    }

    private void ResetGame()
    {
        foreach (GameObject block in blocks)
        {
            Destroy(block);
        }

        bag = new TetrisBag();
        blocks = new List<GameObject>();
        Array.Clear(Grid, 0, Grid.GetLength(0) * Grid.GetLength(1));
        Array.Clear(intGrid, 0, intGrid.GetLength(0) * intGrid.GetLength(1));
        ResetScore();
    }

    public void BlockPlaced(int lines, int minRow)
    {
        if (lines > 0)
        {
            lineCount[lines - 1]++;
            AddToScore(TetrisSettings.Points[lines - 1]);
            AddReward(lines, minRow);
        }
        else
        {
            agent.AddReward(TetrisSettings.Reward.BlockPlaced);
        }

        UpdateIntGrid();
        GetNextStates();
    }

    private void AddReward(int lines, int minRow)
    {
        // favour getting lines at the bottom of the grid
        // multiplier will be in range 1 - GridHeight (22)
        float multiplier = TetrisSettings.GridHeight - minRow;
        float reward = lines * lines * TetrisSettings.GridWidth * multiplier;
        agent.AddReward(reward);
    }

    private void UpdateIntGrid()
    {
        for (int i = 0; i < TetrisSettings.GridWidth; i++)
        {
            for (int j = 0; j < TetrisSettings.GridHeight; j++)
            {
                intGrid[i, j] = Grid[i, j] == null ? 0 : 1;
            }
        }

        if (agent.IsHeuristic)
        {
            LogGridState();
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
        lineCount = new int[4];
        uiController.SetScore(0);
    }

    public void CreateBlock(int x, float rotation)
    {
        V2Int[] positions = tetrominoes[currentPiece].GetBlockPositions(x, TetrisSettings.SpawnY, rotation);
        TetrisBlock newBlock = Instantiate(tetrominoes[currentPiece]).GetComponent<TetrisBlock>();
        newBlock.transform.SetParent(transform);
        newBlock.gameObject.SetActive(true);
        newBlock.Init(this, agent);
        newBlock.SetBlockPositions(positions);
        blocks.Add(newBlock.gameObject);
    }

    private async void GetNextStates()
    {
        States = new List<float>();
        MaskedActions = new List<int>();

        currentPiece = bag.GetPiece();

        await Task.Run(() => PopulateStates());

        if (MaskedActions.Count >= TetrisSettings.PossibleStates)
        {
            GameOver();
        }
        else if (!agent.IsHeuristic)
        {
            agent.RequestDecision();
        }
    }

    private void PopulateStates()
    {
        int count = 0;

        for (int i = 0; i < TetrisSettings.GridWidth; i++)
        {
            for (int j = 0; j < TetrisSettings.Rotations.Length; j++)
            {
                float[] obs = GetState(currentPiece, i, j);

                States.AddRange(obs);

                if (obs[0] == -1)
                {
                    MaskedActions.Add(count);
                }

                count++;
            }
        }
    }

    /*private void PopulateStates()
    {
        int nextPiece = bag.PeekNextPiece();

        int count = 0;
        for (int i = 0; i < TetrisSettings.GridWidth; i++)
        {
            for (int j = 0; j < TetrisSettings.Rotations.Length; j++)
            {
                bool first = true;

                for (int k = 0; k < TetrisSettings.GridWidth; k++)
                {
                    for (int l = 0; l < TetrisSettings.Rotations.Length; l++)
                    {
                        float[] obs = GetState(currentPiece, i, j, nextPiece, k, l);

                        States.AddRange(obs);

                        if (first)
                        {
                            first = false;

                            if (obs[0] == -1)
                            {
                                MaskedActions.Add(count);
                            }

                            count++;
                        }
                    }
                }
            }
        }
    }*/

    /*private void PopulateStates()
    {
        int nextPiece = bag.PeekNextPiece();

        int count = 0;
        for (int i = 0; i < TetrisSettings.GridWidth; i++)
        {
            for (int j = 0; j < TetrisSettings.Rotations.Length; j++)
            {
                float[] obs = GetState(currentPiece, i, j);

                if (obs[0] == -1)
                {
                    MaskedActions.Add(count);
                }
                else
                {
                    for (int k = 0; k < TetrisSettings.GridWidth; k++)
                    {
                        for (int l = 0; l < TetrisSettings.Rotations.Length; l++)
                        {
                            float maxLines = GetNumLines(currentPiece, i, j, nextPiece, k, l);
                            obs[0] = Math.Max(obs[0], maxLines);
                        }
                    }
                }

                States.AddRange(obs);

                count++;
            }
        }
    }*/

    public void GameOver()
    {
        UpdateStats();
        agent.AddReward(TetrisSettings.Reward.GameOver);
        agent.EndEpisode();
    }

    public float[] GetState(int p0, int p0x, int p0Rot)
    {
        float[] states = new float[4];

        // get the position of each square when rotated at x position
        V2Int[] p0Positions = tetrominoes[p0].GetBlockPositions(p0x, TetrisSettings.SpawnY, TetrisSettings.Rotations[p0Rot]);

        gridTemp = intGrid.Clone() as int[,];

        if (CheckPositionsAreValid(p0Positions, gridTemp))
        {
            MoveBlockDownToPlace(p0Positions, ref gridTemp);

            states[0] = NumLines(ref gridTemp);
            GetGridProperties(gridTemp, ref states[1], ref states[2], ref states[3]);

            // normalise state values 0 - 1
            states[0] = states[0] / 4f; // max value numlines = 4
            states[1] = states[1] / TetrisSettings.GridSize; // sum height
            states[2] = states[2] / TetrisSettings.GridSize; // bumpiness
            states[3] = states[3] / TetrisSettings.GridSize; // numHoles
        }
        else
        {
            states[0] = -1;
            states[1] = -1;
            states[2] = -1;
            states[3] = -1;
        }

        return states;
    }

    public float GetNumLines(int p0, int p0x, int p0Rot, int p1, int p1x, int p1Rot)
    {
        float maxLines = 0;

        // get the position of each square when rotated at x position
        V2Int[] p0Positions = tetrominoes[p0].GetBlockPositions(p0x, TetrisSettings.SpawnY, TetrisSettings.Rotations[p0Rot]);
        V2Int[] p1Positions = tetrominoes[p1].GetBlockPositions(p1x, TetrisSettings.SpawnY, TetrisSettings.Rotations[p1Rot]);

        gridTemp = intGrid.Clone() as int[,];

        if (CheckPositionsAreValid(p0Positions, gridTemp))
        {
            MoveBlockDownToPlace(p0Positions, ref gridTemp);

            if (CheckPositionsAreValid(p1Positions, gridTemp))
            {
                MoveBlockDownToPlace(p1Positions, ref gridTemp);

                // 8 is maximum number of lines from 2 moves
                maxLines = (int)NumLines(ref gridTemp) / 8f;
            }
        }

        return maxLines;
    }


    //public float[] GetState(int p0, int p0x, int p0Rot, int p1, int p1x, int p1Rot)
    //{
    //    float[] states = new float[4];

    //    // get the position of each square when rotated at x position
    //    V2Int[] p0Positions = tetrominoes[p0].GetBlockPositions(p0x, TetrisSettings.SpawnY, TetrisSettings.Rotations[p0Rot]);
    //    V2Int[] p1Positions = tetrominoes[p1].GetBlockPositions(p1x, TetrisSettings.SpawnY, TetrisSettings.Rotations[p1Rot]);

    //    gridTemp = intGrid.Clone() as int[,];

    //    if (CheckPositionsAreValid(p0Positions, gridTemp))
    //    {
    //        MoveBlockDownToPlace(p0Positions, ref gridTemp);

    //        if (CheckPositionsAreValid(p1Positions, gridTemp))
    //        {
    //            MoveBlockDownToPlace(p1Positions, ref gridTemp);
    //        }

    //        states[0] = NumLines(ref gridTemp);
    //        GetGridProperties(gridTemp, ref states[1], ref states[2], ref states[3]);

    //        // normalise state values 0 - 1
    //        states[0] = states[0] / 8f; // max value numlines = 4
    //        states[1] = states[1] / TetrisSettings.GridSize; // sum height
    //        states[2] = states[2] / TetrisSettings.GridSize; // bumpiness
    //        states[3] = states[3] / TetrisSettings.GridSize; // numHoles
    //    }
    //    else
    //    {
    //        states[0] = -1;
    //        states[1] = -1;
    //        states[2] = -1;
    //        states[3] = -1;
    //    }

    //    return states;
    //}

    private bool CheckPositionsAreValid(V2Int[] positions, int[,] grid)
    {
        foreach (V2Int position in positions)
        {
            // outside bounds
            if (position.x < 0 || position.x >= TetrisSettings.GridWidth || position.y < 0 || position.y >= TetrisSettings.GridHeight)
            {
                return false;
            }

            // already occupied
            if (grid[position.x, position.y] != 0)
            {
                return false;
            }
        }

        return true;
    }

    private void MoveBlockDownToPlace(V2Int[] positions, ref int[,] grid)
    {
        bool placed = false;
        int shift = 0;

        while (!placed)
        {
            shift++;
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i].y--;
                if (positions[i].y < 0 || grid[positions[i].x, positions[i].y] != 0)
                {
                    placed = true;
                }
            }

            if (placed)
            {
                if (shift > 1)
                {
                    for (int i = 0; i < positions.Length; i++)
                    {
                        positions[i].y++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        for (int i = 0; i < positions.Length; i++)
        {
            grid[positions[i].x, positions[i].y] = 1;
        }
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

    private void GetGridProperties(int[,] grid, ref float sumHeight, ref float bumpiness, ref float numHoles)
    {
        int[] heights = new int[TetrisSettings.GridWidth];

        float maxHeight = 0; // not used for now

        for (int i = 0; i < TetrisSettings.GridWidth; i++)
        {
            bool foundHeight = false;
            for (int j = TetrisSettings.GridHeight - 1; j >= 0; j--)
            {
                if (!foundHeight)
                {
                    if (grid[i, j] == 1)
                    {
                        maxHeight = Mathf.Max(maxHeight, j + 1);
                        heights[i] = j + 1;
                        sumHeight += j + 1;
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
            bumpiness += Math.Abs(heights[i] - heights[i + 1]);
        }
    }

    public void LogGridState()
    {
        float[] states = new float[3];
        gridTemp = intGrid.Clone() as int[,];
        GetGridProperties(gridTemp, ref states[0], ref states[1], ref states[2]);

        Debug.Log(string.Format("totalHeight:{0} bumpiness:{1} numHoles:{2} maxHeight:{3}", states[0], states[1], states[2], states[3]));
    }

    private void UpdateStats()
    {
        if (agent.IsTraining)
        {
            Academy.Instance.StatsRecorder.Add("Score", currentPoints);
            Academy.Instance.StatsRecorder.Add("Line x1", lineCount[0]);
            Academy.Instance.StatsRecorder.Add("Line x2", lineCount[1]);
            Academy.Instance.StatsRecorder.Add("Line x3", lineCount[2]);
            Academy.Instance.StatsRecorder.Add("Line x4", lineCount[3]);
        }
    }

    /*public float[] GetFlattenedGrid()
    {
        float[] flatGrid = new float[TetrisSettings.GridWidth * TetrisSettings.GridHeight];

        for(int y = 0; y < Grid.GetLength(1); y++)
        {
            for(int x = 0; x < Grid.GetLength(0); x++)
            {
                int idx = (y * TetrisSettings.GridWidth) + x;
                flatGrid[idx] = intGrid[x, y];
            }
        }

        return flatGrid;
    }*/
}