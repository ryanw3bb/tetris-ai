using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TetrisAgent : Agent
{
    public bool IsTraining { get { return behaviorParameters.BehaviorType == BehaviorType.Default; } }
    public bool IsHeuristic { get { return behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly; } }

    [SerializeField] protected TetrisGame controller;

    protected BehaviorParameters behaviorParameters;
    //protected List<float> observations = new List<float>();
    //protected List<int> maskedActions = new List<int>();
    //protected int currentAction;

    public override void Initialize()
    {
        base.Initialize();

        controller.Init(this);
        behaviorParameters = GetComponent<BehaviorParameters>();
    }

    /// <summary>
    /// Called when a new episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        controller.StartGame();
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

        if (!controller.MaskedActions.Contains(currentAction))
        {
            // horizontal (x) position & rotation
            // 40 possible values
            int rotate = Mathf.RoundToInt(currentAction % TetrisSettings.NumRotations);
            int position = Mathf.FloorToInt(currentAction / TetrisSettings.NumRotations);

            controller.CreateBlock(position, TetrisSettings.Rotations[rotate]);
            //controller.CurrentBlock.TransformPosition(position, TetrisSettings.Rotations[rotate], true);
        }
        else
        {
            controller.GameOver();
        }
    }

    /// <summary>
    /// Collect possible states used by agent to make decisions
    /// Observation space has size 40
    /// for each rotation (4) and position (10) get:
    /// number of lines cleared
    /// number of holes in board
    /// bumpiness
    /// max height
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        /*maskedActions.Clear();
        observations.Clear();
        int count = 0;*/

        sensor.AddObservation(controller.States);

        // get array of possible states when dropping this piece
        // add impossible moves to the action mask
        /*for (int j = 0; j < TetrisSettings.GridWidth; j++)
        {
            for (int i = 0; i < TetrisSettings.Rotations.Length; i++)
            {
                float[] obs = controller.GetState(i, j);
                sensor.AddObservation(obs);
                observations.AddRange(obs);

                if (Mathf.Approximately(obs[0], -1))
                {
                    maskedActions.Add(count);
                }

                count++;
            }
        }*/

        /*int nextPiece = controller.GetNextPiece();
        sensor.AddObservation(nextPiece);

        sensor.AddObservation(controller.GetFlattenedGrid());*/
    }

    /// <summary>
    /// Disables unavailable actions before making a decision request
    /// </summary>
    /// <param name="actionMasker"></param>
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        actionMasker.SetMask(0, controller.MaskedActions);
    }

    public void Log(string prefix = "")
    {
        string maskedActionsStr = "";
        foreach(float m in controller.MaskedActions)
        {
            maskedActionsStr += m + ",";
        }

        Debug.Log(prefix + "  Masked actions: " + maskedActionsStr);
    }
}
