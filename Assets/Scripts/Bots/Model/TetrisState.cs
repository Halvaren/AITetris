using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modelization of the Tetris board
/// </summary>
public class TetrisState
{
    //Binary number are easy to store and easy to manage if you do it correctly
    int[] board = new int[24] //It's upside down
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

    int fullRow = 0b1111111111; //Filter to easily detect if a row is full

    int boardWidth;
    int boardHeight;

    int maxHeight = 20;

    //Number of lines cleared after using the last locked piece
    int clearedLines;

    bool terminalState = false; //If it's a gameOver, it's true
    bool BOOMTetris = false; //If a I piece has made a Tetris (deleted 4 rows), it is true

    public TetrisState()
    {
        boardWidth = TetrisBoardController.Instance.boardWidth;
        boardHeight = TetrisBoardController.Instance.boardHeight;
    }

    /// <summary>
    /// Locks a piece using binary numbers
    /// </summary>
    /// <param name="binaryTiles"></param>
    public void LockPiece(Vector2Int[] binaryTiles)
    {
        for (int i = 0; i < binaryTiles.Length; i++)
        {
            if (binaryTiles[i].y >= maxHeight) terminalState = true;
            board[binaryTiles[i].y] |= binaryTiles[i].x;
        }

        CheckLinesToClear();
    }

    /// <summary>
    /// Checks if there are lines to clear, and stores them in a bool list
    /// </summary>
    private void CheckLinesToClear()
    {
        List<bool> linesToClear = new List<bool>();
        int firstClearedLine = -1;
        int clearedLinesCount = 0;

        for (int y = 0; y < boardHeight; y++)
        {
            bool toClear = IsRowFull(y);

            linesToClear.Add(toClear);
            if(toClear)
            {
                clearedLinesCount++;
                if (firstClearedLine == -1) firstClearedLine = y;
            }
        }

        if (clearedLinesCount > 0)
        {
            clearedLines += linesToClear.Count;
            ClearLines(linesToClear, firstClearedLine);
        }
    }

    /// <summary>
    /// Clears the lines that the bool list indicates
    /// </summary>
    /// <param name="linesToBeCleared"></param>
    /// <param name="firstClearedLine"></param>
    private void ClearLines(List<bool> linesToBeCleared, int firstClearedLine)
    {
        int clearedLines = 0;
        for (int y = firstClearedLine; y < boardHeight; y++)
        {
            if (linesToBeCleared[y])
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
        else BOOMTetris = false;
    }

    /// <summary>
    /// Modifies piece position and rotation with the giving action, and moves the piece down until it locks
    /// </summary>
    /// <param name="piece"></param>
    /// <param name="action"></param>
    public void DoAction(PieceModel piece, PieceAction action)
    {
        piece.DoAction(action, this);

        while (piece.Move(Vector2Int.down, this)) { }

        LockPiece(piece.GetBinaryTiles());
    }

    #region Getters

    /// <summary>
    /// Returns the number of lines cleared by the last locked piece
    /// </summary>
    /// <returns></returns>
    public int GetClearedLines()
    {
        return clearedLines;
    }

    /// <summary>
    /// Returns the number of holes in the board
    /// </summary>
    /// <returns></returns>
    public int GetHoleCount()
    {
        int holeCount = 0;
        for(int x = 0; x < boardWidth; x++)
        {
            int binMask = 1 << x;

            for (int y = GetColHeight(x); y >= 0; y--)
            {
                if ((board[y] & binMask) == 0) holeCount++;
            }
        }

        return holeCount;
    }

    /// <summary>
    /// Returns the bumpiness value
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Returns the number of rows with at least one hole
    /// </summary>
    /// <returns></returns>
    public int GetRowsWithHoles()
    {
        int rowsCount = 0;
        bool[] checkedRows = new bool[boardHeight];

        for (int x = 0; x < boardWidth; x++)
        {
            int binMask = 1 << x;

            for (int y = GetColHeight(x); y >= 0; y--)
            {
                if ((board[y] & binMask) == 0)
                {
                    rowsCount++;
                    checkedRows[y] = true;
                }
            }
        }

        return rowsCount;
    }

    /// <summary>
    /// Returns the current height of the board (most high row where is at least one tile)
    /// </summary>
    /// <returns></returns>
    public int GetCurrentMaxHeight()
    {
        for(int y = maxHeight; y >= 0; y--)
        {
            if (!IsRowEmpty(y)) return y;
        }
        return maxHeight;
    }

    /// <summary>
    /// Returns the score of the state applying the 4 main factors
    /// </summary>
    /// <returns></returns>
    public float GetScore()
    {
        return -(TetrisBoardController.Instance.holesWeight * GetHoleCount())
            - (TetrisBoardController.Instance.bumpinessWeight * GetBumpiness())
            - (TetrisBoardController.Instance.rowHolesWeight * GetRowsWithHoles())
            + (TetrisBoardController.Instance.linesWeight * GetClearedLines());
    }

    /// <summary>
    /// Returns the score with the humanized factor applied
    /// </summary>
    /// <returns></returns>
    public float GetHumanizedScore()
    {
        return GetScore() - (GetOccupiedTilesInColumn(0) * GetHumanizedWeight());
    }

    /// <summary>
    /// Returns a variation of the humanizedWeight based on the current height of the board. Less height, more weight
    /// </summary>
    /// <returns></returns>
    private float GetHumanizedWeight()
    {
        float currentHeightWeight = (float) GetCurrentMaxHeight() / maxHeight;

        float result = (TetrisBoardController.Instance.humanizedWeight + (1 - currentHeightWeight)) / 2;

        return result;
    }

    /// <summary>
    /// Returns all possible actions
    /// </summary>
    /// <param name="pieceModel"></param>
    /// <returns></returns>
    public List<PieceAction> GetActions(PieceModel pieceModel)
    {
        List<PieceAction> actions = new List<PieceAction>();

        PieceType pieceType = pieceModel.pieceType;
        int rotations = 4;
        if (pieceType == PieceType.O) rotations = 1;
        else if (pieceType == PieceType.S || pieceType == PieceType.Z || pieceType == PieceType.I) rotations = 2;

        //For each possible rotation for the giving piece
        for (int i = 0; i < rotations; i++)
        {
            pieceModel.ResetCoordinates();
            for (int j = 0; j < i; j++) pieceModel.Rotate(); //It is rotated to an specific rotate index

            while (pieceModel.Move(Vector2Int.left, this)) { } //It is moved to the left limit of the board

            int horizontalPosition = 0;
            //For each available position between left and right limit
            while (horizontalPosition < boardWidth)
            {
                actions.Add(new PieceAction(i, pieceModel.GetOneTileCoords(0).x)); //That will be a new possible action

                while (!pieceModel.Move(Vector2Int.right, this)) { horizontalPosition++; if (horizontalPosition >= boardWidth) break; } //While the horizontal position is not available, the piece will be moved to the right
            }
        }

        pieceModel.ResetCoordinates();

        return actions;
    }
    
    /// <summary>
    /// Returns a random possible action
    /// </summary>
    /// <param name="pieceModel"></param>
    /// <returns></returns>
    public PieceAction GetRandomAction(PieceModel pieceModel)
    {
        int rotationIndex = UnityEngine.Random.Range(0, 3);
        int xCoord;

        for (int i = 0; i < rotationIndex; i++) pieceModel.Rotate();

        xCoord = UnityEngine.Random.Range(0, 10);
        int j = 0;
        while (!pieceModel.CanPieceMove(new Vector2Int(xCoord - pieceModel.GetOneTileCoords(0).x, 0), this) && j < 100)
        {
            xCoord = UnityEngine.Random.Range(0, 10);
            j++;
        }

        pieceModel.ResetCoordinates();

        return new PieceAction(rotationIndex, xCoord);
    }

    /// <summary>
    /// Number of tiles that are in the x column
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public int GetOccupiedTilesInColumn(int x)
    {
        int count = 0;

        int binMask = 1 << x;
        for (int y = boardHeight - 1; y >= 0; y--)
        {
            if ((board[y] & binMask) > 0) count++;
        }
        return count;
    }

    #endregion

    #region Simple checkers

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

    public bool IsTerminal()
    {
        return terminalState;
    }

    public bool IsTetris()
    {
        return BOOMTetris;
    }

    #endregion

    /// <summary>
    /// Returns a copy of the current state to test anything on it without modifying the actual state
    /// Here is important that the state is simple as possible in order to make cloning a cheap operation.
    /// Using binary number array, I made less operations than using a boolean matrix or something similar
    /// </summary>
    /// <returns></returns>
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

        /*for(int y = board.Length - 1; y >= 0; y--)
        {
            string number = Convert.ToString(board[y], 2);
            for (int i = 0; i < 10 - number.Length; i++) result += "0";
            result += number + "\n";
        }*/

        result += "Holes: " + GetHoleCount() + "\n";
        result += "Bumpiness: " + GetBumpiness() + "\n";
        result += "Lines: " + GetClearedLines() + "\n";
        result += "Rows with holes: " + GetRowsWithHoles() + "\n";
        result += "Tiles in first column: " + GetOccupiedTilesInColumn(0) + "\n";

        return result;
    }
}
