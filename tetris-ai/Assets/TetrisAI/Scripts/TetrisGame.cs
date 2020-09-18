using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.MLAgents;
using UnityEngine;

public class TetrisGame : MonoBehaviour
{
    [SerializeField] private UIController ui;
    [SerializeField] private TetrisPiece[] tetrominoes;
    private TetrisGrid grid;
    private TetrisAgent agent;
    private TetrisBag bag;
    private int currentPoints = 0;
    private int totalLines = 0;
    private int currentPiece;
    private int[,] gridTemp;
    private int[] lineCount = new int[4];

    public void Init(TetrisAgent agent)
    {
        this.agent = agent;
        grid = GetComponent<TetrisGrid>();
        gridTemp = new int[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
        ui.SetHighScore(0, 0);

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i] = Instantiate(tetrominoes[i], Vector3.zero, Quaternion.identity).GetComponent<TetrisPiece>();
            tetrominoes[i].transform.SetParent(transform);
            tetrominoes[i].Init(this, grid, true);
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
        bag = new TetrisBag();
        grid.Reset();
        ResetScore();
    }

    public void BlockPlaced(GridState state)
    {
        if (state.NumLines > 0)
        {
            lineCount[state.NumLines - 1]++;
            AddToScore(state.NumLines);
            agent.AddLineReward(state);
        }
        else
        {
            agent.AddReward(TetrisSettings.Reward.BlockPlaced);
        }

        if (agent.IsHeuristic)
        {
            grid.LogState();
        }

        GetNextStates();
    }

    private void AddToScore(int lines)
    { 
        currentPoints += TetrisSettings.Points[lines - 1];
        totalLines += lines;
        ui.SetScore(currentPoints, totalLines);
    }

    private void ResetScore()
    {
        currentPoints = 0;
        totalLines = 0;
        lineCount = new int[4];
        ui.SetScore(0, 0);
    }

    public void GameOver()
    {
        UpdateStats();
        agent.AddReward(TetrisSettings.Reward.GameOver);
        agent.EndEpisode();
    }

    public void CreateBlock(int x, float rotation)
    {
        V2Int[] positions = tetrominoes[currentPiece].GetBlockPositions(x, TetrisSettings.SpawnY, rotation);
        TetrisPiece newBlock = Instantiate(tetrominoes[currentPiece]).GetComponent<TetrisPiece>();
        newBlock.transform.SetParent(transform);
        newBlock.gameObject.SetActive(true);
        newBlock.Init(this, grid, agent);
        newBlock.SetBlockPositions(positions);
    }

    private async void GetNextStates()
    {
        agent.States = new List<float>();
        agent.MaskedActions = new List<int>();

        currentPiece = bag.GetPiece();

        await Task.Run(() => PopulateStates());

        if (agent.MaskedActions.Count >= TetrisSettings.PossibleStates)
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

                agent.States.AddRange(obs);

                if (obs[0] == -1)
                {
                    agent.MaskedActions.Add(count);
                }

                count++;
            }
        }
    }

    public float[] GetState(int piece, int x, int rot)
    {
        float[] states = new float[4];

        // get the position of each square when rotated at x position
        V2Int[] p0Positions = tetrominoes[piece].GetBlockPositions(x, TetrisSettings.SpawnY, TetrisSettings.Rotations[rot]);

        gridTemp = grid.GetTempGrid();

        if (grid.CheckPositionsAreValid(p0Positions, gridTemp))
        {
            grid.MoveBlockDownToPlace(p0Positions, ref gridTemp);

            states[0] = grid.GetLines(ref gridTemp);
            grid.GetGridProperties(gridTemp, ref states[1], ref states[2], ref states[3]);

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

    private void UpdateStats()
    {
        if (agent.IsTraining)
        {
            Academy.Instance.StatsRecorder.Add("Score", currentPoints);
            Academy.Instance.StatsRecorder.Add("Lines", totalLines);
            Academy.Instance.StatsRecorder.Add("Line x1", lineCount[0]);
            Academy.Instance.StatsRecorder.Add("Line x2", lineCount[1]);
            Academy.Instance.StatsRecorder.Add("Line x3", lineCount[2]);
            Academy.Instance.StatsRecorder.Add("Line x4", lineCount[3]);
        }
    }
}