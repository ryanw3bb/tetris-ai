using UnityEngine;
using UnityEngine.InputSystem;

public class TetrisDebug : MonoBehaviour
{
    [SerializeField] private InputAction triggerDebug;

    private TetrisGame tetrisGame;

    private void Start()
    {
        tetrisGame = GetComponent<TetrisGame>();

        triggerDebug.Enable();
        triggerDebug.performed += LogOutput;
    }

    private void LogOutput(InputAction.CallbackContext obj)
    {
        tetrisGame.LogState();
        Debug.Break();
    }

    private void OnDestroy()
    {
        triggerDebug.Disable();
    }
}
