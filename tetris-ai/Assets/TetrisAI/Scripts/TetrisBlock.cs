using System.Collections;
using UnityEditor;
using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    public int Shape { get { return shape; } }
    public int Rotation { get; private set; }
    public Vector2 Position { get { return transform.position + TetrisSettings.Offset; } }
    public bool Active { get; private set; }

    [SerializeField] private Vector3 rotationPoint;
    [SerializeField] private int shape;

    private TetrisGame controller;
    private float previousTime;
    private bool decisionStarted;
    private bool decisionFinished;

    public void Init(TetrisGame controller)
    {
        this.controller = controller;
        Active = true;
    }

    //public void Tick(bool downPressed)
    private void FixedUpdate()
    {
        if (!Active) return;

        //float speed = downPressed || decisionFinished ? TetrisSettings.FallTime / 10 : TetrisSettings.FallTime;
        //float speed = downPressed ? TetrisSettings.FallTime / 10 : TetrisSettings.FallTime;
        float speed = TetrisSettings.FallTime;

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

            /*if(!controller.HumanPlayer && !decisionStarted)
            {
                RequestDecision();
            }*/

            previousTime = Time.time;
        }
    }

    public bool MoveIfValid(int x, int y = 0)
    {
        foreach (Transform child in transform)
        {
            int roundedX = Mathf.RoundToInt(child.transform.position.x + x);
            int roundedY = Mathf.RoundToInt(child.transform.position.y + y);

            // outside bounds
            if (roundedX < 0 || roundedX >= TetrisSettings.Width || roundedY < 0 || roundedY >= TetrisSettings.Height) return false;

            // already occupied
            if (controller.Grid[roundedX, roundedY] != null) return false;
        }

        // valid move
        transform.position += new Vector3(x, y, 0);
        return true;
    }

    // TODO: helper class?
    private Vector3Int RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Vector3Int.RoundToInt(Quaternion.Euler(angles) * (point - pivot) + pivot);
    }

    public void RotateIfValid(float angle)
    {
        Vector3 rotation = new Vector3(0, 0, angle);
        Vector3 pivot = transform.TransformPoint(rotationPoint);

        foreach (Transform child in transform)
        {
            // rotate each child around point
            Vector3Int newPos = RotatePointAroundPivot(child.position, pivot, rotation);

            // outside bounds
            if (newPos.x < 0 || newPos.x >= TetrisSettings.Width || newPos.y < 0 || newPos.y >= TetrisSettings.Height) return;

            // already occupied
            if (controller.Grid[newPos.x, newPos.y] != null) return;
        }

        transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), angle);

        // TODO: do we need this? Just send transform.rotation.z
        int idx = (angle == 90) ? 1 : -1;
        Rotation += idx;
        if (Rotation > 3) Rotation = 0;
        else if (Rotation < 0) Rotation = 3;
    }

    private bool CheckForGameOver()
    {
        foreach (Transform child in transform)
        {
            int roundedY = Mathf.RoundToInt(child.transform.position.y);

            if (roundedY >= TetrisSettings.Height)
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

        for (int i = TetrisSettings.Height - 1; i >= 0; i--)
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
        for (int j = 0; j < TetrisSettings.Width; j++)
        {
            if (controller.Grid[j, i] == null)
                return false;
        }

        return true;
    }

    private void DeleteLine(int i)
    {
        for (int j = 0; j < TetrisSettings.Width; j++)
        {
            Destroy(controller.Grid[j, i].gameObject);
            controller.Grid[j, i] = null;
        }
    }

    private void RowDown(int i)
    {
        for (int y = i; y < TetrisSettings.Height; y++)
        {
            for (int j = 0; j < TetrisSettings.Width; j++)
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
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            controller.Grid[roundedX, roundedY] = children;
        }
    }

    /*private void RequestDecision()
    {
        decisionStarted = true;
        StartCoroutine(RequestDecisionRoutine());
    }

    private IEnumerator RequestDecisionRoutine()
    {
        for (int i = 0; i < TetrisSettings.MovesPerBlock; i++)
        {
            if (!Active) yield break;

            if (!controller.RandomMode)
            {
                agent.RequestDecision();
            }
            else
            {
                // Random action
                float[] actions = new float[2];
                actions[0] = Mathf.Round(Random.Range(0, 2f));
                actions[1] = Mathf.Round(Random.Range(0, 2f));
                agent.OnActionReceived(actions);
            }

            yield return new WaitForEndOfFrame();
        }

        decisionFinished = true;
    }*/

}