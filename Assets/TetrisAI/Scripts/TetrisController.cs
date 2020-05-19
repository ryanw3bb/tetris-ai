using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TetrisController : MonoBehaviour
{
    [HideInInspector] public Transform[,] Grid;

    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;  
    [SerializeField] private GameObject[] tetrominoes;

    private List<GameObject> blocks = new List<GameObject>();
    private TetrisBlock currentBlock;
    private int currentPoints = 0;
    private int highestPoints = 0;

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
        ResetScore();
        NewTetrisBlock();
    }

    public void BlockPlaced(int lines = 0)
    {
        if (lines > 0) AddToScore(TetrisSettings.Points[lines - 1]);
        NewTetrisBlock();
    }

    private void AddToScore(int points)
    {
        currentPoints += points;
        scoreText.text = string.Format(TetrisSettings.ScoreFormat, currentPoints);
    }

    private void ResetScore()
    {
        currentPoints = 0;
        scoreText.text = string.Format(TetrisSettings.ScoreFormat, currentPoints);
    }

    private void NewTetrisBlock()
    {
        currentBlock = Instantiate(tetrominoes[Random.Range(0, tetrominoes.Length)],
            transform.position, Quaternion.identity).GetComponent<TetrisBlock>();
        currentBlock.Init(this);
        blocks.Add(currentBlock.gameObject);
    }

    public void GameOver()
    {
        if(currentPoints > highestPoints)
        {
            highestPoints = currentPoints;
            highScoreText.text = string.Format(TetrisSettings.HighScoreFormat, highestPoints);
        }
        Reset();
    }
}
