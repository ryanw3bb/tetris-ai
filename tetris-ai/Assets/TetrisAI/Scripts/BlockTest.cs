using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BlockTest : MonoBehaviour
{
    [SerializeField] private Vector3 rotationPoint;
    [SerializeField] private InputAction changeState;

    private int count = 0;
    Dictionary<Transform, Vector3> offsets = new Dictionary<Transform, Vector3>();

    private void Start()
    {
        foreach (Transform child in transform)
        {
            offsets.Add(child, child.localPosition);
        }

        changeState.Enable();
        changeState.performed += OnChangeState;
    }

    private void OnChangeState(InputAction.CallbackContext obj)
    {
        int rotate = Mathf.FloorToInt(count / 10f);
        int position = Mathf.RoundToInt(count % 10f);

        float angle = 0;
        if (rotate == 1)
            angle = 90;
        else if (rotate == 2)
            angle = 180;
        else if (rotate == 3)
            angle = 270;

        GetTransformedPosition(position, angle, true);

        count++;

        if (count >= 40)
        {
            count = 0;
        }
    }

    public Vector2Int[] GetTransformedPosition(int xpos, float angle, bool set = false)
    {
        Vector2Int[] positions = new Vector2Int[4];

        Vector3 rotation = new Vector3(0, 0, angle);
        int count = 0;

        foreach (KeyValuePair<Transform, Vector3> child in offsets)
        {
            // rotate each child around rotation point
            Vector3 rotatedOffset = RotatePointAroundPivot(child.Value, rotationPoint, rotation);

            // convert to local space and shift to x position
            int roundedX = Mathf.RoundToInt(xpos + rotatedOffset.x);
            int roundedY = Mathf.RoundToInt(transform.localPosition.y + rotatedOffset.y);

            // outside bounds
            if (roundedX < 0 || roundedX >= TetrisSettings.GridWidth || roundedY < 0 || roundedY >= TetrisSettings.GridHeight)
            {
                if (set) { Debug.LogError("ERROR"); }
                //return null;
            }

            // already occupied
            //if (controller.Grid[roundedX, roundedY] != null) return null;

            if (set)
            {
                child.Key.position = transform.parent.transform.TransformPoint(roundedX, roundedY, 0);
            }

            positions[count] = new Vector2Int(roundedX, roundedY);
            count++;
        }

        return positions;
    }

    private Vector3Int RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Vector3Int.RoundToInt(Quaternion.Euler(angles) * (point - pivot) + pivot);
    }
}
