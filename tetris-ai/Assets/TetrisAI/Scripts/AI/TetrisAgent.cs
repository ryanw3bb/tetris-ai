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
        // Max 5000 steps if training, infinite steps if racing
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
        // rotate right/left
        // 0 - rotate left
        // 1 - none
        // 2 - rotate right
        float rotate = vectorAction[0];
        if (rotate == 0)
            controller.CurrentBlock.RotateIfValid(-90);
        else if (rotate == 2)
            controller.CurrentBlock.RotateIfValid(90);

        // move right/left
        // 0 - move left
        // 1 - none
        // 2 - move right
        float move = vectorAction[1];
        if (move == 0)
            controller.CurrentBlock.MoveIfValid(-1);
        else if (move == 2)
            controller.CurrentBlock.MoveIfValid(1);

        if(trainingMode)
        {
            // small negative reward every step
            AddReward(-1f / MaxStep);
        }
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

        // rotation of tetromino
        sensor.AddObservation(controller.CurrentBlock.Rotation);

        // position of tetromino
        sensor.AddObservation(controller.CurrentBlock.Position);

        // filled positions
        sensor.AddObservation(controller.GetFlattenedGrid());
    }
}
