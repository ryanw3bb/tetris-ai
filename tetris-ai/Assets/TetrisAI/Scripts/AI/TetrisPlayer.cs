using UnityEngine;
using UnityEngine.InputSystem;

public class TetrisPlayer : TetrisAgent
{
    [Header("Input Bindings")]
    public InputAction moveHorizInput;
    public InputAction moveVertInput;
    public InputAction rotateInput;

    /// <summary>
    /// Enable input listeners
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        moveHorizInput.Enable();
        moveHorizInput.performed += OnPlayerInput;

        moveVertInput.Enable();
        moveVertInput.performed += OnPlayerInput;

        rotateInput.Enable();
        rotateInput.performed += OnPlayerInput;
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
        // Rotate: -1 == left, 0 == none, 1 == right
        float rotateValue = Mathf.Round(rotateInput.ReadValue<float>());
        rotateValue++;

        // Horizontal: -1 == left, 0 == none, 1 == right
        float horizValue = Mathf.Round(moveHorizInput.ReadValue<float>());
        horizValue++;

        // Vertical: 0 == none, 1 == down
        float vertValue = Mathf.Round(moveVertInput.ReadValue<float>());

        actionsOut[0] = rotateValue;
        actionsOut[1] = horizValue;
    }

    private void OnDestroy()
    {
        moveHorizInput.Disable();
        moveVertInput.Disable();
        rotateInput.Disable();
    }
}
