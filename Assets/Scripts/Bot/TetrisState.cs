using System;
using System.Collections.Generic;
using UnityEngine;

public class TetrisState
{
    int[] board = new int[24] //Está bocaabajo
    {
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000,
        0b0000000000
    };

    int fullRow = 0b1111111111;

    int boardWidth;
    int boardHeight;

    int clearedLines;

    public TetrisState()
    {
        boardWidth = TetrisBoardController.Instance.boardWidth;
        boardHeight = TetrisBoardController.Instance.boardHeight;
    }

    public void LockPiece(Vector2Int[] binaryTiles)
    {
        for (int i = 0; i < binaryTiles.Length; i++)
        {
            board[binaryTiles[i].y] |= binaryTiles[i].x;
        }

        CheckLinesToClear();
    }

    private void CheckLinesToClear()
    {
        List<int> linesToClear = new List<int>();

        for (int y = 0; y < boardHeight; y++)
        {
            if (IsRowFull(y)) linesToClear.Add(y);
        }

        if (linesToClear.Count > 0)
        {
            clearedLines += linesToClear.Count;
            ClearLines(linesToClear);
        }
    }

    private void ClearLines(List<int> lineIndices)
    {
        int clearedLines = 0;
        for (int y = lineIndices[0]; y < boardHeight; y++)
        {
            if (lineIndices.Contains(y))
            {
                clearedLines++;
            }
            else
            {
                board[y - clearedLines] = board[y];
            }
            board[y] = 0;
        }
    }

    public void DoAction(PieceModel piece, PieceAction action, bool aux = false)
    {
        piece.DoAction(action, this);

        while (piece.Move(Vector2Int.down, this)) { }

        LockPiece(piece.GetBinaryTiles(piece.GetCoordinates()));
    }

    public int GetClearedLines()
    {
        return clearedLines;
    }

    public int GetHoleCount()
    {
        int holeCount = 0;
        for(int x = 0; x < boardWidth; x++)
        {
            int binMask = 1 << x;
            bool hitTile = false;

            for(int y = boardHeight - 1; y >= 0; y--)
            {
                if (IsColFull(x)) continue;
                if ((board[y] & binMask) > 0) hitTile = true;
                if (hitTile && (board[y] & binMask) == 0) holeCount++;
            }
        }

        return holeCount;
    }

    public int GetBumpiness()
    {
        int bumpiness = 0;
        int prevHeight = -1;

        for(int x = 0; x < boardWidth; x++)
        {
            int height = GetColHeight(x);
            if (prevHeight != -1) bumpiness += Mathf.Abs(prevHeight - height);
            prevHeight = height;
        }

        return bumpiness;
    }

    private bool IsColFull(int x)
    {
        int binMask = 1 << x;
        for(int y = boardHeight - 1; y >= 0; y--)
        {
            if ((board[y] & binMask) == 0) return false;
        }
        return true;
    }

    private bool IsRowFull(int y)
    {
        return board[y] == fullRow;
    }

    private int GetColHeight(int x)
    {
        int binMask = 1 << x;
        for(int y = boardHeight - 1; y >= 0; y--)
        {
            if ((board[y] & binMask) > 0) return y;
        }
        return 0;
    }

    public bool IsInBounds(Vector2Int binaryTile)
    {
        if (binaryTile.x <= 0 || binaryTile.x >= (1 << 10) || binaryTile.y < 0) return false;
        return true;
    }

    public bool IsPosEmpty(Vector2Int binaryTile) //Recibe las coordenadas de cada tile y si en las comprobaciones da un número mayor a 0, es que hay colisión
    {
        if ((binaryTile.x & board[binaryTile.y]) > 0) return false;

        return true;
    }

    public bool IsTerminal(PieceModel pieceModel)
    {
        return !pieceModel.CanPieceMove(Vector2Int.zero, this);
    }

    public List<PieceAction> GetActions(PieceModel pieceModel)
    {
        List<PieceAction> actions = new List<PieceAction>();
        Vector2Int[] originalCoordinates = pieceModel.GetCoordinates();

        /*for(int i = 0; i < 2; i++)
        {
            pieceModel.SetCoordinates(originalCoordinates); //The piece is placed in the spawn position
            for (int j = 0; j < i; j++) pieceModel.Rotate(); //It is rotated to an specific rotate index

            while (pieceModel.Move(Vector2Int.left, this)) { } //It is moved to the left limit of the board

            while (pieceModel.Move(Vector2Int.down, this)) { } //The piece is moved down until it collides with something

            actions.Add(new PieceAction(i, pieceModel.GetOneTileCoords(0).x)); //That will be a new possible action
        }*/

        for (int i = 0; i < 4; i++)
        {
            pieceModel.SetCoordinates(originalCoordinates); //The piece is placed in the spawn position
            for (int j = 0; j < i; j++) pieceModel.Rotate(); //It is rotated to an specific rotate index

            while (pieceModel.Move(Vector2Int.left, this)) { } //It is moved to the left limit of the board

            int horizontalPosition = 0;
            //For each available position between left and right limit
            while(horizontalPosition < boardWidth)
            {
                Vector2Int[] coordinatesFromTop = pieceModel.GetCoordinates();

                while (pieceModel.Move(Vector2Int.down, this)) { } //The piece is moved down until it collides with something

                actions.Add(new PieceAction(i, pieceModel.GetOneTileCoords(0).x)); //That will be a new possible action

                pieceModel.SetCoordinates(coordinatesFromTop); //Coordinates of the piece are reset to be again at the top of the board

                while (!pieceModel.Move(Vector2Int.right, this)) { horizontalPosition++; if (horizontalPosition >= boardWidth) break; } //While the horizontal position is not available, the piece will be moved to the right
            }
        }

        pieceModel.ResetCoordinates();

        return actions;
    }

    public PieceAction GetRandomAction(PieceModel pieceModel)
    {
        int rotationIndex = UnityEngine.Random.Range(0, 3);
        int xCoord;

        for (int i = 0; i < rotationIndex; i++) pieceModel.Rotate();

        xCoord = UnityEngine.Random.Range(0, 10);
        int j = 0;
        while(!pieceModel.CanPieceMove(new Vector2Int(xCoord - pieceModel.GetOneTileCoords(0).x, 0), this) && j < 100)
        {
            xCoord = UnityEngine.Random.Range(0, 10);
            j++;
        }
        
        pieceModel.ResetCoordinates();

        return new PieceAction(rotationIndex, xCoord);
    }

    public TetrisState CloneState()
    {
        TetrisState newState = new TetrisState();

        for(int i = 0; i < board.Length; i++)
        {
            newState.board[i] = board[i];
        }

        return newState;
    }

    public override string ToString()
    {
        string result = "";

        for(int y = 0; y < board.Length; y++)
        {
            result += Convert.ToString(board[y], 2) + "\n";
        }

        return result;
    }
}
