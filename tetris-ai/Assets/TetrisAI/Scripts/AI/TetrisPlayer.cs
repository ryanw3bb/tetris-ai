using UnityEngine;
using UnityEngine.InputSystem;

public class TetrisPlayer : TetrisAgent
{
    [SerializeField] private int rotation;
    [SerializeField] private int position;
    [SerializeField] private InputAction confirmMove;

    /// <summary>
    /// Enable input listeners
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        confirmMove.Enable();
        confirmMove.performed += OnPlayerInput;
    }

    /// <summary>
    /// Player has pressed an input key so perform the correct action
    /// </summary>
    /// <param name="obj"></param>
    private void OnPlayerInput(InputAction.CallbackContext obj)
    {
        RequestDecision();
    }

    /// <summary>
    /// Reads player input and converts it to a vector action array
    /// </summary>
    /// <returns>An array of floats for OnActionReceived to use</returns>
    public override void Heuristic(float[] actionsOut)
    {
        float action = (position * TetrisSettings.NumRotations) + rotation;

        actionsOut[0] = action;
    }

    private void OnDestroy()
    {
        confirmMove.Disable();
    }
}
