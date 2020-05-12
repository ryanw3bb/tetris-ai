using System.Collections.Generic;
using UnityEngine;

public class TetrisController : MonoBehaviour
{
    [HideInInspector] public Transform[,] Grid;

    [SerializeField] private GameObject[] tetrominoes;

    private List<GameObject> blocks = new List<GameObject>();
    private TetrisBlock currentBlock;

    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        if (currentBlock == null) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentBlock.MoveIfValid(-1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentBlock.MoveIfValid(1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            currentBlock.RotateIfValid(90);
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            currentBlock.RotateIfValid(-90);
        }

        currentBlock.Tick(Input.GetKey(KeyCode.DownArrow));
    }

    private void Reset()
    {
        foreach (GameObject block in blocks)
            Destroy(block);
        blocks = new List<GameObject>();
        Grid = new Transform[TetrisSettings.Width, TetrisSettings.Height];
        NewTetrisBlock();
    }

    public void NewTetrisBlock()
    {
        currentBlock = Instantiate(tetrominoes[Random.Range(0, tetrominoes.Length)], 
            transform.position, Quaternion.identity).GetComponent<TetrisBlock>();
        currentBlock.Init(this);
        blocks.Add(currentBlock.gameObject);
    }

    public void GameOver()
    {
        Reset();
    }
}
