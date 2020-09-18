using System.Collections.Generic;
using UnityEngine;

public class TetrisPiece : MonoBehaviour
{
    [SerializeField] private Vector3 rotationPoint;
    private bool active = false;
    private List<Transform> segments;
    private TetrisGame controller;
    private TetrisGrid grid;
    private float previousTime;
    private Dictionary<float, V2Int[]> segmentPositions;

    public void Init(TetrisGame controller, TetrisGrid grid, bool calculatePositions = false)
    {
        this.controller = controller;
        this.grid = grid;
       
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
            Vector3 rotatedPosition = RotatePointAroundPivot(segments[i].localPosition, rotationPoint, rotation);
            positions[i] = new V2Int(Mathf.RoundToInt(rotatedPosition.x), Mathf.RoundToInt(rotatedPosition.y));
        }
        
        segmentPositions.Add(angle, positions);
    }

    private void Update()
    {
        if (!active) return;

        if (Time.time - previousTime > TetrisSettings.FallTime)
        {
            if (!MoveIfValid(0, -1))
            {
                if(!CheckForGameOver())
                {
                    AddToGrid();
                    grid.GetLines();
                    controller.BlockPlaced(grid.LastState);
                    active = false;
                }
            }

            previousTime = Time.time;
        }
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

        return positions;
    }

    public void SetBlockPositions(V2Int[] positions)
    {
        int i = 0;

        foreach (Transform segment in segments)
        {
            segment.position = controller.transform.TransformPoint(positions[i].x, positions[i].y, 0);
            i++;
        }

        active = true;
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
            if (!grid.IsCellEmpty(roundedX, roundedY)) return false;
        }

        // valid move
        transform.localPosition += new Vector3(x, y, 0);

        return true;
    }

    private bool CheckForGameOver()
    {
        foreach (Transform child in transform)
        {
            Vector3 localPos = controller.transform.InverseTransformPoint(child.transform.position);
            int roundedY = Mathf.RoundToInt(localPos.y);

            // block hasn't moved down so game over
            if (roundedY >= TetrisSettings.SpawnY)
            {
                Destroy(gameObject);
                controller.GameOver();
                active = false;
                return true;
            }
        }

        return false;
    }

    private void AddToGrid()
    {
        foreach (Transform child in transform)
        {
            Vector3 localPos = controller.transform.InverseTransformPoint(child.transform.position);
            int roundedX = Mathf.RoundToInt(localPos.x);
            int roundedY = Mathf.RoundToInt(localPos.y);

            grid.AddToGrid(child, roundedX, roundedY);
        }
    }

    private Vector3Int RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Vector3Int.RoundToInt(Quaternion.Euler(angles) * (point - pivot) + pivot);
    }
}