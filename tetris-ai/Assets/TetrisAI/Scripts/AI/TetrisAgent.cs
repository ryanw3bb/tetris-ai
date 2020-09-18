using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TetrisAgent : Agent
{
    public bool IsTraining { get { return behaviorParameters.BehaviorType == BehaviorType.Default; } }
    public bool IsHeuristic { get { return behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly; } }
    public List<float> States { get; set; }
    public List<int> MaskedActions { get; set; }

    [SerializeField] private TetrisGame tetrisGame;
    private BehaviorParameters behaviorParameters;

    public override void Initialize()
    {
        base.Initialize();

        tetrisGame.Init(this);
        behaviorParameters = GetComponent<BehaviorParameters>();
    }

    /// <summary>
    /// Called when a new episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        tetrisGame.StartGame();
    }

    /// <summary>
    /// Read action inputs from vectorAction
    /// Action space has size 2
    /// vectorAction[0] = rotate
    /// vectorAction[1] = move
    /// </summary>
    /// <param name="vectorAction">The chosen actions</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        int currentAction = Mathf.RoundToInt(vectorAction[0]);

        if (!MaskedActions.Contains(currentAction))
        {
            // horizontal (x) position & rotation
            // 40 possible values
            int rotate = Mathf.RoundToInt(currentAction % TetrisSettings.NumRotations);
            int position = Mathf.FloorToInt(currentAction / TetrisSettings.NumRotations);

            tetrisGame.CreateBlock(position, TetrisSettings.Rotations[rotate]);
        }
        else
        {
            tetrisGame.GameOver();
        }
    }

    /// <summary>
    /// Collect possible states used by agent to make decisions
    /// Observation space has size 160
    /// for each rotation (4) and position (10) get:
    /// number of lines cleared
    /// number of holes in grid
    /// grid bumpiness
    /// grid sum height
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(States);
    }

    /// <summary>
    /// Disables unavailable actions before making a decision request
    /// </summary>
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        actionMasker.SetMask(0, MaskedActions);
    }

    /// <summary>
    /// Add a reward based on the number of lines cleared, with a
    /// multiplier for the vertical (row) position
    /// </summary>
    public void AddLineReward(GridState state)
    {
        // favour getting lines at the bottom of the grid
        // multiplier will be in range 1 - GridHeight / 5 (4.4)
        float multiplier = (TetrisSettings.GridHeight - state.LinesMinRow) / 5f;
        float reward = Mathf.Pow(state.NumLines, 2) * TetrisSettings.GridWidth * multiplier;
        AddReward(reward);
    }

    public void Log(string prefix = "")
    {
        string maskedActionsStr = "";
        foreach(float m in MaskedActions)
        {
            maskedActionsStr += m + ",";
        }

        Debug.Log(prefix + "  Masked actions: " + maskedActionsStr);
    }
}
