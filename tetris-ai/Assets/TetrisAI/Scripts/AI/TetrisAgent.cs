using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class TetrisAgent : Agent
{
    [SerializeField] private TetrisGame controller;
    [SerializeField] private bool trainingMode;

    public override void Initialize()
    {
        base.Initialize();

        controller.Init(this);

        // override the max step set in the inspector
        // Max 5000 steps if training, infinite steps if playing
        MaxStep = trainingMode ? 5000 : 0;
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
        float rotate = vectorAction[0];
        if (rotate == 1)
            controller.CurrentBlock.RotateIfValid(90);
        else if (rotate == 2)
            controller.CurrentBlock.RotateIfValid(180);
        else if (rotate == 3)
            controller.CurrentBlock.RotateIfValid(270);

        // horizontal (x) position
        // value 0 - 9
        float position = vectorAction[1];
        controller.CurrentBlock.SetPositionIfValid(Mathf.RoundToInt(position));
    }

    /// <summary>
    /// Collect observations used by agent to make decisions
    /// Observation space has size 183
    /// Grid is 10 x 18 = 180
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        // shape of tetromino
        sensor.AddObservation(controller.CurrentBlock.Shape);

        // filled positions
        sensor.AddObservation(controller.GetFlattenedGrid());
    }
}
