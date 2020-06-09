using MLAgents;
using UnityEngine;

public class TetrisAgent : Agent
{
    private TetrisController controller;

    private void Start()
    {
        controller = GetComponent<TetrisController>();    
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
    }

    /// <summary>
    /// Read action inputs from vectorAction
    /// vectorAction[0] = rotate
    /// vectorAction[1] = move
    /// </summary>
    /// <param name="vectorAction">The chosen actions</param>
    public override void AgentAction(float[] vectorAction)
    {
        // rotation right/left
        if (vectorAction[0] == 1f)
            controller.CurrentBlock.RotateIfValid(90);
        else if (vectorAction[0] == -1f)
            controller.CurrentBlock.RotateIfValid(-90);

        // move right/left
        if (vectorAction[1] == 1f)
            controller.CurrentBlock.MoveIfValid(1);
        else if (vectorAction[1] == -1f)
            controller.CurrentBlock.MoveIfValid(-1);
    }

    /// <summary>
    /// Collect observations used by agent to make decisions
    /// </summary>
    public override void CollectObservations()
    {
        // shape of tetromino
        AddVectorObs(controller.CurrentBlock.Shape);

        // rotation of tetromino
        AddVectorObs(controller.CurrentBlock.Rotation);

        // position of tetromino
        AddVectorObs(controller.CurrentBlock.Position);

        // filled positions
        AddVectorObs(controller.GetFlattenedGrid());
    }

    public override void AgentReset()
    {
        
    }
}
