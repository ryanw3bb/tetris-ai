using System.Collections.Generic;

public class TetrisBag
{
    private List<Tetromino> pieces = new List<Tetromino>
    {
        Tetromino.I, 
        Tetromino.J,
        Tetromino.L, 
        Tetromino.O, 
        Tetromino.S, 
        Tetromino.T, 
        Tetromino.Z
    };
    private List<Tetromino> bag = new List<Tetromino>();

    public int GetPiece()
    {
        RefillBag();

        int piece = (int)bag[0];
        bag.RemoveAt(0);

        return piece;
    }

    public int PeekNextPiece()
    {
        RefillBag();

        return (int)bag[0];
    }

    private void RefillBag()
    {
        if (bag.IsNullOrEmpty())
        {
            bag = new List<Tetromino>(pieces);
            bag.Shuffle();
        }
    }
}