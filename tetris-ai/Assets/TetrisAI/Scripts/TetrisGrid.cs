using System;
using UnityEngine;

public class GridState
{
    public int NumLines;
    public int LinesMinRow;
}

public class TetrisGrid : MonoBehaviour
{
    public GridState LastState { get; private set; }

    private Transform[,] visualGrid;
    private int[,] intGrid;

    public void Awake()
    {
        LastState = new GridState();
        visualGrid = new Transform[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
        intGrid = new int[TetrisSettings.GridWidth, TetrisSettings.GridHeight];
    }

    public void Reset()
    {
        for (int y = 0; y < TetrisSettings.GridHeight; y++)
        {
            for (int x = 0; x < TetrisSettings.GridWidth; x++)
            {
                ClearCell(x, y);
            }
        }

        Array.Clear(visualGrid, 0, visualGrid.GetLength(0) * visualGrid.GetLength(1));
        Array.Clear(intGrid, 0, intGrid.GetLength(0) * intGrid.GetLength(1));
    }

    public int[,] GetTempGrid()
    {
        return intGrid.Clone() as int[,];
    }

    public void AddToGrid(Transform segment, int x, int y)
    {
        visualGrid[x, y] = segment;
        intGrid[x, y] = 1;
    }

    public bool IsCellEmpty(int x, int y)
    {
        return intGrid[x, y] == 0;
    }

    public void ClearCell(int x, int y)
    {
        if (visualGrid[x, y] != null)
        {
            if (visualGrid[x, y].parent.childCount <= 1)
            {
                Destroy(visualGrid[x, y].parent.gameObject);
            }
            else
            {
                visualGrid[x, y].parent = null;
                Destroy(visualGrid[x, y].gameObject);
            }

            visualGrid[x, y] = null;
        }
    }

    public bool CheckPositionsAreValid(V2Int[] positions, int[,] grid)
    {
        foreach (V2Int position in positions)
        {
            // outside bounds
            if (position.x < 0 || position.x >= TetrisSettings.GridWidth || position.y < 0 || position.y >= TetrisSettings.GridHeight)
            {
                return false;
            }

            // already occupied
            if (grid[position.x, position.y] != 0)
            {
                return false;
            }
        }

        return true;
    }

    public void MoveBlockDownToPlace(V2Int[] positions, ref int[,] grid)
    {
        bool placed = false;
        int shift = 0;

        while (!placed)
        {
            shift++;
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i].y--;
                if (positions[i].y < 0 || grid[positions[i].x, positions[i].y] != 0)
                {
                    placed = true;
                }
            }

            if (placed)
            {
                if (shift > 1)
                {
                    for (int i = 0; i < positions.Length; i++)
                    {
                        positions[i].y++;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        for (int i = 0; i < positions.Length; i++)
        {
            grid[positions[i].x, positions[i].y] = 1;
        }
    }

    public void GetLines()
    {
        LastState.LinesMinRow = TetrisSettings.GridHeight;
        LastState.NumLines = Mathf.RoundToInt(GetLines(ref intGrid, true));
    }

    public float GetLines(ref int[,] grid, bool isAction = false)
    {
        int numLines = 0;

        for (int i = TetrisSettings.GridHeight - 1; i >= 0; i--)
        {
            if (HasLine(grid, i))
            {
                DeleteLine(ref grid, i, isAction);
                RowDown(ref grid, i, isAction);
                numLines++;

                if (isAction)
                {
                    LastState.LinesMinRow = Math.Min(LastState.LinesMinRow, i);
                }    
            }
        }

        return numLines;
    }

    public bool HasLine(int[,] grid, int i)
    {
        for (int j = 0; j < TetrisSettings.GridWidth; j++)
        {
            if (grid[j, i] == 0)
            {
                return false;
            }
        }

        return true;
    }

    public void DeleteLine(ref int[,] grid, int i, bool isAction)
    {
        for (int j = 0; j < TetrisSettings.GridWidth; j++)
        {
            grid[j, i] = 0;

            if (isAction)
            {
                ClearCell(j, i);
            }
        }
    }

    public void RowDown(ref int[,] grid, int i, bool isAction)
    {
        for (int y = i; y < TetrisSettings.GridHeight; y++)
        {
            for (int x = 0; x < TetrisSettings.GridWidth; x++)
            {
                if (grid[x, y] == 1)
                {
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = 0;

                    if (isAction)
                    {
                        visualGrid[x, y - 1] = visualGrid[x, y];
                        visualGrid[x, y] = null;
                        visualGrid[x, y - 1].transform.position -= new Vector3(0, 1, 0);
                    }
                }
            }
        }
    }

    public void GetGridProperties(int[,] grid, ref float sumHeight, ref float bumpiness, ref float numHoles)
    {
        int[] heights = new int[TetrisSettings.GridWidth];

        for (int i = 0; i < TetrisSettings.GridWidth; i++)
        {
            bool foundHeight = false;
            for (int j = TetrisSettings.GridHeight - 1; j >= 0; j--)
            {
                if (!foundHeight)
                {
                    if (grid[i, j] == 1)
                    {
                        heights[i] = j + 1;
                        sumHeight += j + 1;
                        foundHeight = true;
                    }
                }
                else
                {
                    if (grid[i, j] == 0)
                    {
                        numHoles++;
                    }
                }
            }
        }

        for (int i = 0; i < heights.Length - 1; i++)
        {
            bumpiness += Math.Abs(heights[i] - heights[i + 1]);
        }
    }

    public void LogState()
    {
        float[] states = new float[3];
        int[,] gridTemp = intGrid.Clone() as int[,];
        GetGridProperties(gridTemp, ref states[0], ref states[1], ref states[2]);

        Debug.Log(string.Format("totalHeight:{0} bumpiness:{1} numHoles:{2}", states[0], states[1], states[2]));
    }
}