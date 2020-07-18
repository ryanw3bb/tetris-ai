using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    public int Shape { get { return shape; } }
    public bool Active { get; private set; }

    [SerializeField] private Vector3 rotationPoint;
    [SerializeField] private int shape;

    private TetrisGame controller;
    private TetrisAgent agent;
    private float previousTime;
    private Dictionary<Transform, Vector3> offsets;

    public void Init(TetrisGame controller, TetrisAgent agent)
    {
        this.controller = controller;
        this.agent = agent;
        Active = true;

        offsets = new Dictionary<Transform, Vector3>();
        foreach (Transform child in transform)
        {
            offsets.Add(child, child.localPosition);
        }
    }

    private void FixedUpdate()
    {
        if (!Active) return;

        //float speed = downPressed ? TetrisSettings.FallTime / 10 : TetrisSettings.FallTime;
        float speed = agent.IsTraining ? TetrisSettings.TrainFallTime : TetrisSettings.PlayFallTime;

        if (Time.time - previousTime > speed)
        {
            if (!MoveIfValid(0, -1))
            {
                if(!CheckForGameOver())
                {
                    AddToGrid();
                    int lines = CheckForLines();
                    controller.BlockPlaced(lines);
                    Active = false;
                }
            }

            previousTime = Time.time;
        }
    }

    public void SetPosition(int x)
    {
        foreach (Transform child in transform)
        {
            float offset = child.transform.position.x - transform.position.x;
            int roundedX = Mathf.RoundToInt(x + offset);

            // outside bounds
            if (roundedX < 0 || roundedX >= TetrisSettings.GridWidth)
            {
                controller.GameOver();
                return;
            }
        }

        // valid move
        transform.localPosition = new Vector3(x, transform.localPosition.y, 0);
    }

    public bool MoveIfValid(int x, int y = 0)
    {
        foreach (Transform child in transform)
        {
            Vector3 localPosition = controller.transform.InverseTransformPoint(child.transform.position);
            int roundedX = Mathf.RoundToInt(localPosition.x + x);
            int roundedY = Mathf.RoundToInt(localPosition.y + y);

            // outside bounds
            if (roundedX < 0 || roundedX >= TetrisSettings.GridWidth || roundedY < 0 || roundedY >= TetrisSettings.GridHeight) return false;

            // already occupied
            if (controller.Grid[roundedX, roundedY] != null) return false;
        }

        // valid move
        transform.localPosition += new Vector3(x, y, 0);

        return true;
    }

    public void RotateIfValid(float angle)
    {
        Vector3 rotation = new Vector3(0, 0, angle);
        Vector3 pivot = transform.TransformPoint(rotationPoint);

        foreach (Transform child in transform)
        {
            // rotate each child around point
            Vector3 newPos = RotatePointAroundPivot(child.position, pivot, rotation);

            // convert to local space
            Vector3 localPos = controller.transform.InverseTransformPoint(newPos);
            int roundedX = Mathf.RoundToInt(localPos.x);
            int roundedY = Mathf.RoundToInt(localPos.y);

            // outside bounds
            if (roundedX < 0 || roundedX >= TetrisSettings.GridWidth || roundedY < 0 || roundedY >= TetrisSettings.GridHeight) return;

            // already occupied
            if (controller.Grid[roundedX, roundedY] != null) return;
        }

        transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), angle);
    }

    public Vector2Int[] TransformPosition(int xpos, float angle, bool set = false)
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
                if (set)
                {
                    agent.PrintStatus("Placed block outside bounds");
                }
                return null;
            }

            // already occupied
            if (controller.Grid[roundedX, roundedY] != null)
            {
                if (set)
                {
                    agent.PrintStatus("Placed block on another block");
                }
                return null;
            }

            if (set)
            {
                child.Key.position = controller.transform.TransformPoint(roundedX, roundedY, 0);
            }

            positions[count] = new Vector2Int(roundedX, roundedY);
            count++;
        }

        return positions;
    }


    private bool CheckForGameOver()
    {
        foreach (Transform child in transform)
        {
            Vector3 localPos = controller.transform.InverseTransformPoint(child.transform.position);
            int roundedY = Mathf.RoundToInt(localPos.y);

            if (roundedY >= TetrisSettings.SpawnY)
            {
                controller.GameOver();
                Active = false;
                return true;
            }
        }

        return false;
    }

    private int CheckForLines()
    {
        int numLines = 0;

        for (int i = TetrisSettings.GridHeight - 1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                RowDown(i);
                numLines++;
            }
        }

        return numLines;
    }

    private bool HasLine(int i)
    {
        for (int j = 0; j < TetrisSettings.GridWidth; j++)
        {
            if (controller.Grid[j, i] == null)
                return false;
        }

        return true;
    }

    private void DeleteLine(int i)
    {
        for (int j = 0; j < TetrisSettings.GridWidth; j++)
        {
            Destroy(controller.Grid[j, i].gameObject);
            controller.Grid[j, i] = null;
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < TetrisSettings.GridHeight; y++)
        {
            for (int j = 0; j < TetrisSettings.GridWidth; j++)
            {
                if(controller.Grid[j, y] != null)
                {
                    controller.Grid[j, y - 1] = controller.Grid[j, y];
                    controller.Grid[j, y] = null;
                    controller.Grid[j, y - 1].transform.position -= new Vector3(0, 1, 0);
                }
            }
        }
    }

    private void AddToGrid()
    {
        foreach (Transform child in transform)
        {
            Vector3 localPos = controller.transform.InverseTransformPoint(child.transform.position);
            int roundedX = Mathf.RoundToInt(localPos.x);
            int roundedY = Mathf.RoundToInt(localPos.y);

            controller.Grid[roundedX, roundedY] = child;
        }
    }

    private Vector3Int RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Vector3Int.RoundToInt(Quaternion.Euler(angles) * (point - pivot) + pivot);
    }
}