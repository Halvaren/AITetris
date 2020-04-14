using System;
using System.Collections.Generic;

public class TetrisBot
{
    protected List<PieceModel> nextPieces;
    protected TetrisState actualTetrisState;

    protected int boardWidth;
    protected int boardHeight;

    public TetrisBot(PieceType[] initialPieces)
    { 
        boardWidth = TetrisBoardController.Instance.boardWidth;
        boardHeight = TetrisBoardController.Instance.boardHeight;

        actualTetrisState = new TetrisState();
    }
}