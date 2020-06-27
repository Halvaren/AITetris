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

    int maxHeight = 20;

    int clearedLines;

    bool terminalState = false;
    bool BOOMTetris = false;

    public TetrisState()
    {
        boardWidth = TetrisBoardController.Instance.boardWidth;
        boardHeight = TetrisBoardController.Instance.boardHeight;
    }

    public void LockPiece(Vector2Int[] binaryTiles)
    {
        for (int i = 0; i < binaryTiles.Length; i++)
        {
            if (binaryTiles[i].y >= maxHeight) terminalState = true;
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

        if (clearedLines == 4) BOOMTetris = true;
    }

    public void DoAction(PieceModel piece, PieceAction action)
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

            if (IsColFull(x)) continue;
            for (int y = boardHeight - 1; y >= 0; y--)
            {
                if (IsRowFull(y)) continue;
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

    public int GetRowsWithHoles()
    {
        int rowsCount = 0;
        bool[] checkedRows = new bool[boardHeight];

        for (int x = 0; x < boardWidth; x++)
        {
            int binMask = 1 << x;
            bool hitTile = false;

            if (IsColFull(x)) continue;
            for (int y = boardHeight - 1; y >= 0; y--)
            {
                if (IsRowFull(y)) continue;

                if ((board[y] & binMask) > 0) hitTile = true;
                if (hitTile && (board[y] & binMask) == 0 && !checkedRows[y]) { rowsCount++; checkedRows[y] = true; }
            }
        }

        return rowsCount;
    }

    public int GetCurrentHeight()
    {
        for(int y = board.Length - maxHeight; y < board.Length; y++)
        {
            if (!IsRowEmpty(y)) return maxHeight - y;
        }
        return maxHeight;
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

    private bool IsRowEmpty(int y)
    {
        return board[y] == 0;
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

    public int GetOccupiedTilesInColumn(int x)
    {
        int count = 0;

        int binMask = 1 << x;
        for(int y = boardHeight - 1; y >= 0; y--)
        {
            if ((board[y] & binMask) > 0) count++;
        }
        return count;
    }

    public bool IsTerminal()
    {
        return terminalState;
    }

    public bool IsTetris()
    {
        return BOOMTetris;
    }

    public float GetScore()
    {
        return -(TetrisBoardController.Instance.holesWeight * GetHoleCount())
            - (TetrisBoardController.Instance.bumpinessWeight * GetBumpiness())
            - (TetrisBoardController.Instance.rowHolesWeight * GetRowsWithHoles())
            + (TetrisBoardController.Instance.linesWeight * GetClearedLines());
    }

    public float GetHumanizedScore()
    {
        return GetScore() - (GetOccupiedTilesInColumn(0) * GetHumanizedWeight());
    }

    private float GetHumanizedWeight()
    {
        return 2 * GetCurrentHeight() / maxHeight;
    }

    public List<PieceAction> GetActions(PieceModel pieceModel)
    {
        List<PieceAction> actions = new List<PieceAction>();

        PieceType pieceType = pieceModel.pieceType;
        int rotations = 4;
        if (pieceType == PieceType.O) rotations = 1;
        else if (pieceType == PieceType.S || pieceType == PieceType.Z || pieceType == PieceType.I) rotations = 2;

        for (int i = 0; i < rotations; i++)
        {
            pieceModel.ResetCoordinates();
            for (int j = 0; j < i; j++) pieceModel.Rotate(); //It is rotated to an specific rotate index

            while (pieceModel.Move(Vector2Int.left, this)) { } //It is moved to the left limit of the board

            int horizontalPosition = 0;
            //For each available position between left and right limit
            while(horizontalPosition < boardWidth)
            {
                actions.Add(new PieceAction(i, pieceModel.GetOneTileCoords(0).x)); //That will be a new possible action

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

        for(int y = board.Length - 1; y >= 0; y--)
        {
            string number = Convert.ToString(board[y], 2);
            for (int i = 0; i < 10 - number.Length; i++) result += "0";
            result += number + "\n";
        }

        result += "Holes: " + GetHoleCount() + "\n";
        result += "Bumpiness: " + GetBumpiness() + "\n";
        result += "Lines: " + GetClearedLines() + "\n";

        return result;
    }
}
