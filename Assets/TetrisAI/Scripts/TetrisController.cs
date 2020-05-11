using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisController : MonoBehaviour
{
    public GameObject[] Tetrominoes;

    private void Start()
    {
        NewTetronimo();
    }

    public void NewTetronimo()
    {
        Instantiate(Tetrominoes[Random.Range(0, Tetrominoes.Length)], transform.position, Quaternion.identity);
    }
}
