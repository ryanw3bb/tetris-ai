using UnityEngine;

public class TetrisBlock : MonoBehaviour
{
    public int Shape { get { return shape; } }
    public int Rotation { get; private set; }
    public Vector2 Position { get { return transform.position + TetrisSettings.Offset; } }

    [SerializeField] private Vector3 rotationPoint;
    [SerializeField] private int shape;

    private TetrisController controller;
    private float previousTime;

    public void Init(TetrisController controller)
    {
        this.controller = controller;

        Vector3 t = transform.position + transform.position;
    }

    public void Tick(bool downPressed)
    {
        if (Time.time - previousTime > (downPressed ? TetrisSettings.FallTime / 10 : TetrisSettings.FallTime))
        {
            if (!MoveIfValid(0, -1))
            {
                if(!CheckForGameOver())
                {
                    AddToGrid();
                    int lines = CheckForLines();
                    controller.BlockPlaced(lines);
                }
            }

            previousTime = Time.time;
        }
    }

    public bool MoveIfValid(int x = 0, int y = 0)
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x + x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y + y);

            // outside bounds
            if (roundedX < 0 || roundedX >= TetrisSettings.Width || roundedY < 0 || roundedY > TetrisSettings.Height) return false;

            // already occupied
            if (controller.Grid[roundedX, roundedY] != null) return false;
        }

        // valid move
        transform.position += new Vector3(x, y, 0);
        return true;
    }

    // TODO: make this better
    public void RotateIfValid(float angle)
    {
        transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), angle);
        
        if (!MoveIfValid())
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), angle);
        }
        else
        {
            int idx = (angle == 90) ? 1 : -1;
            Rotation += idx;
            if (Rotation > 3) Rotation = 0;
            else if (Rotation < 0) Rotation = 3;
        }
    }

    private bool CheckForGameOver()
    {
        foreach (Transform children in transform)
        {
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if (roundedY >= TetrisSettings.Height)
            {
                controller.GameOver();
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
}