using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TetrisAgent : Agent
{
    public bool IsTraining;

    [SerializeField] private TetrisGame controller;

    private List<int> maskedActions;
    private float currentAction;

    public override void Initialize()
    {
        base.Initialize();

        controller.Init(this);

        // override the max step set in the inspector
        // Max 5000 steps if training, infinite steps if playing
        MaxStep = IsTraining ? 5000 : 0;
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
        // rotation
        // 0 - 0
        // 1 - 90
        // 2 - 180
        // 3 - 270
        /*float rotate = vectorAction[0];
        if (rotate == 1)
            controller.CurrentBlock.RotateIfValid(90);
        else if (rotate == 2)
            controller.CurrentBlock.RotateIfValid(180);
        else if (rotate == 3)
            controller.CurrentBlock.RotateIfValid(270);

        // horizontal (x) position
        // possible value 0 - 9
        float position = vectorAction[1];
        controller.CurrentBlock.SetPosition(Mathf.RoundToInt(position));*/

        currentAction = vectorAction[0];
        int rotate = Mathf.RoundToInt(currentAction % 4f);
        int position = Mathf.FloorToInt(currentAction / 4f);

        /*if (rotate == 1)
            controller.CurrentBlock.RotateIfValid(90);
        else if (rotate == 2)
            controller.CurrentBlock.RotateIfValid(180);
        else if (rotate == 3)
            controller.CurrentBlock.RotateIfValid(270);
        
         controller.CurrentBlock.SetPosition(position);*/

        float angle = 0;
        if (rotate == 1)
            angle = 90;
        else if (rotate == 2)
            angle = 180;
        else if (rotate == 3)
            angle = 270;

        controller.CurrentBlock.TransformPosition(position, angle, true);
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
        maskedActions = new List<int>();
        int count = 0;

        // get array of possible states when dropping this piece
        // add impossible moves to the action mask
        for (int j = 0; j < TetrisSettings.GridWidth; j++)
        {
            for (int i = 0; i < TetrisSettings.Rotations.Length; i++)
            {
                float[] obs = controller.GetState(i, j);
                sensor.AddObservation(obs);

                if(obs[0] == -1)
                {
                    maskedActions.Add(count);
                }

                count++;
            }
        }

        if(maskedActions.Count >= 40)
        {
            Debug.Log("Can't go");
            controller.GameOver();
        }
    }

    /// <summary>
    /// Disables unavailable actions before making a decision request
    /// </summary>
    /// <param name="actionMasker"></param>
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        actionMasker.SetMask(0, maskedActions);
    }

    public void PrintStatus(string prefix = "")
    {
        string maskedActionsStr = "";
        foreach(float m in maskedActions)
        {
            maskedActionsStr += m + ",";
        }
        Debug.Log(prefix + " Current action: " + currentAction + "  Masked actions: " + maskedActionsStr);
    }
}
