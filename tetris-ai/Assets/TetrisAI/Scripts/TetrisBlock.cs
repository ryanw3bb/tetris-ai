using System;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    public int Shape { get { return shape; } }
    public bool Active { get; private set; }

    /*public Dictionary<Transform, Vector3> Offsets
    {
        get
        {
            if (offsets == null)
            {
                offsets = new Dictionary<Transform, Vector3>();
                foreach (Transform child in transform)
                {
                    offsets.Add(child, child.localPosition);
                }
            }

            return offsets;
        }
    }*/

    public Vector3 RotationPoint;
    [SerializeField] private int shape;

    private List<Transform> segments;

    private TetrisGame controller;
    private TetrisAgent agent;
    private float previousTime;
    //private Dictionary<Transform, Vector3> offsets;
    private Dictionary<float, V2Int[]> segmentPositions;

    public void Init(TetrisGame controller, TetrisAgent agent, bool calculatePositions = false)
    {
        this.controller = controller;
        this.agent = agent;
        Active = false;

        segments = new List<Transform>();
        foreach (Transform child in transform)
        {
            segments.Add(child);
        }

        if (calculatePositions)
        {
            segmentPositions = new Dictionary<float, V2Int[]>();
            CalculatePositions(0);
            CalculatePositions(90);
            CalculatePositions(180);
            CalculatePositions(270);
        }
    }

    public void CalculatePositions(float angle)
    {
        int len = segments.Count;
        V2Int[] positions = new V2Int[len];

        Vector3 rotation = new Vector3(0, 0, -angle);

        for (int i = 0; i < segments.Count; i++)
        {
            // rotate each child around rotation point
            Vector3 rotatedPosition = RotatePointAroundPivot(segments[i].localPosition, RotationPoint, rotation);
            positions[i] = new V2Int(Mathf.RoundToInt(rotatedPosition.x), Mathf.RoundToInt(rotatedPosition.y));
        }
        
        segmentPositions.Add(angle, positions);
    }

    private void Update()
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
                    int minRow = TetrisSettings.GridHeight;
                    int lines = CheckForLines(ref minRow);
                    controller.BlockPlaced(lines, minRow);
                    Active = false;
                }
            }

            previousTime = Time.time;
        }
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

    public V2Int[] GetBlockPositions(int x, int y, float angle)
    {
        int len = segmentPositions[angle].Length;
        V2Int[] positions = new V2Int[len];

        for (int i = 0; i < len; i++)
        {
            positions[i] = new V2Int();
            positions[i].x = segmentPositions[angle][i].x + x;
            positions[i].y = segmentPositions[angle][i].y + y;
        }

        /*foreach (V2Int position in positions)
        {
            position.x += x;
            position.y += y;
        }*/

        return positions;
    }

    /*public Vector2Int[] GetBlockPositions(int x, int y, float angle)
    {
        Vector2Int[] positions = new Vector2Int[4];

        Vector3 rotation = new Vector3(0, 0, -angle);
        int count = 0;

        foreach (KeyValuePair<Transform, Vector3> child in Offsets)
        {
            // rotate each child around rotation point
            Vector3 rotatedOffset = RotatePointAroundPivot(child.Value, RotationPoint, rotation);

            // convert to local space and shift to x position
            int roundedX = Mathf.RoundToInt(x + rotatedOffset.x);
            int roundedY = Mathf.RoundToInt(y + rotatedOffset.y);

            // outside bounds
            /*if (roundedX < 0 || roundedX >= TetrisSettings.GridWidth || roundedY < 0 || roundedY >= TetrisSettings.GridHeight)
            {
                if (set)
                {
                    agent.LogError("Placed block " + shape + " outside bounds pos:" + x + " angle:" + angle + " x:" + roundedX + " y:" + roundedY);
                    controller.GameOver();
                }
                return null;
            }

            // already occupied
            if (controller.Grid[roundedX, roundedY] != null)
            {
                if (set)
                {
                    agent.LogError("Placed block on another block");
                    controller.GameOver();
                }
                return null;
            }*/

            /*if (set)
            {
                child.Key.position = controller.transform.TransformPoint(roundedX, roundedY, 0);
            }*/

            /*positions[count] = new Vector2Int(roundedX, roundedY);
            count++;
        }

        //Active = set;
        //previousTime = Time.time;

        return positions;
    }*/

    public void SetBlockPositions(V2Int[] positions)
    {
        int i = 0;

        foreach (Transform segment in segments)
        {
            segment.position = controller.transform.TransformPoint(positions[i].x, positions[i].y, 0);
            i++;
        }

        Active = true;
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

    private int CheckForLines(ref int minRow)
    {
        int numLines = 0;

        for (int i = TetrisSettings.GridHeight - 1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                RowDown(i);
                numLines++;
                minRow = Math.Min(minRow, i);
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

    /*public void SetPosition(int x)
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
    }*/
}