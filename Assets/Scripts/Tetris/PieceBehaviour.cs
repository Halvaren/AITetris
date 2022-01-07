using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Types of a piece
/// </summary>
public enum PieceType
{
    I, J, L, O, S, T, Z
}

/// <summary>
/// A piece is a group of four tiles, it has a type that indicates which position combination these tiles are forming
/// A piece can move left, right and down, can rotate clockwise and counterclockwise, and can lock in the board
/// A piece is spawned by the PieceEmitter
/// </summary>
public class PieceBehaviour : MonoBehaviour
{
    public PieceType pieceType { get; private set; } //Type of the piece
    public TileBehaviour[] tiles; //Tiles that conform the piece
    
    private int rotationIndex; //Index that indicates in which rotation the piece is

    //Bool variables to notice if the piece has to be locked in the board, or the user has some time to avoid this lock
    private bool downLimitReached;
    private bool locked;
    private bool moved;

    private Coroutine lockedCoroutine = null;

    #region Global variables

    float lockedLimitTime; //The time between the piece touches some other piece or the bottom side of the board, and it locks
    Vector2Int spawnLocation; //Position inside the board where the piece will spawn

    #endregion

    private TetrisBoardController tbController;
    public TetrisBoardController TBController
    {
        get
        {
            if (tbController == null) tbController = TetrisBoardController.Instance;
            return tbController;
        }
    }

    void Awake()
    {
        InitializeGlobalVariables();

        rotationIndex = 0;
        downLimitReached = locked = false;
        moved = true;
    }

    /// <summary>
    /// Initializes the global variables
    /// </summary>
    void InitializeGlobalVariables()
    {
        lockedLimitTime = TBController.lockedLimitTime;
        spawnLocation = TBController.spawnPos;
    }

    /// <summary>
    /// Initializes the piece type, the material of the tiles, the tiles' positions and moves the piece to its spawn position
    /// </summary>
    /// <param name="newType"></param>
    /// <param name="material"></param>
    public void SpawnPiece(PieceType newType, Material material)
    {
        pieceType = newType;
        Vector2Int[] tileRelativePositions = null;

        switch (pieceType)
        {
            case PieceType.I:
                tileRelativePositions = TetrisData.IRelativePositions;
                break;
            case PieceType.J:
                tileRelativePositions = TetrisData.JRelativePositions;
                break;
            case PieceType.L:
                tileRelativePositions = TetrisData.LRelativePositions;
                break;
            case PieceType.O:
                tileRelativePositions = TetrisData.ORelativePositions;
                break;
            case PieceType.S:
                tileRelativePositions = TetrisData.SRelativePositions;
                break;
            case PieceType.T:
                tileRelativePositions = TetrisData.TRelativePositions;
                break;
            case PieceType.Z:
                tileRelativePositions = TetrisData.ZRelativePositions;
                break;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].UpdatePosition(spawnLocation + tileRelativePositions[i]);
            tiles[i].InitializeTile(this, i);
            tiles[i].SetMaterial(material);
        }
    }

    /// <summary>
    /// It checks if the piece can move a specific amount of movement by checking if all of the tiles can move
    /// </summary>
    /// <param name="movement"></param>
    /// <returns></returns>
    public bool CanPieceMove(Vector2Int movement)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (!tiles[i].CanTileMove(movement + tiles[i].Coordinates))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns the coordinates of the tiles
    /// </summary>
    /// <returns></returns>
    public Vector2Int[] GetTileCoords()
    {
        Vector2Int[] tileCoords = new Vector2Int[4];

        for (int i = 0; i < tiles.Length; i++)
        {
            tileCoords[i] = tiles[i].Coordinates;
        }

        Vector2Int[] result = tileCoords.ToArray();

        return result;
    }

    /// <summary>
    /// Checks if there aren't no more tiles conforming this piece, because they have been deleted when lines have been cleared. It is useful to delete the piece
    /// GameObject when there aren't no more tiles
    /// </summary>
    /// <returns></returns>
    public bool AnyTilesLeft()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null) return true;
        }

        return false;
    }

    /// <summary>
    /// Moves the piece a specific amount of movement, only if it can
    /// </summary>
    /// <param name="movement"></param>
    /// <param name="debug">If it is true, that means the method has been called in a testing context, to test something visually, so it doesn't have to lock the piece if it's moved down</param>
    /// <returns></returns>
    public bool MovePiece(Vector2Int movement, bool debug = false)
    {
        bool canMove = CanPieceMove(movement);

        if (!canMove)
        {
            if (!debug && movement == Vector2Int.down) SetPiece();
            return false;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].MoveTile(movement);
        }

        moved = true;

        return true;
    }

    /// <summary>
    /// Moves the piece to a specific coordinates
    /// </summary>
    /// <param name="tileCoords"></param>
    public void MovePiece(Vector2Int[] tileCoords)
    {
        for(int i = 0; i < tileCoords.Length; i++)
        {
            tiles[i].UpdatePosition(tileCoords[i]);
        }
    }

    /// <summary>
    /// Rotates the piece in a specific direction
    /// </summary>
    /// <param name="clockwise"></param>
    /// <param name="shouldOffset">Indicates if the piece has also to move to make a proper rotation</param>
    public void RotatePiece(bool clockwise, bool shouldOffset)
    {
        int oldRotationIndex = rotationIndex;
        rotationIndex += clockwise ? 1 : -1;
        rotationIndex = Mod(rotationIndex, 4);

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].RotateTile(tiles[0].Coordinates, clockwise);
        }

        if (!shouldOffset) return;

        bool canOffset = Offset(oldRotationIndex, rotationIndex);

        //If the piece should offset but it can't, then the opposite rotation has to be done
        if (!canOffset) RotatePiece(!clockwise, false);
        else moved = true;
    }

    int Mod(int x, int m) { return (x % m + m) % m; }

    /// <summary>
    /// Checks if a piece can offset, offsetting it if it's the case, and returning a bool with the result. It uses the offset data in the TetrisData class
    /// </summary>
    /// <param name="oldRotationIndex"></param>
    /// <param name="newRotationIndex"></param>
    /// <returns></returns>
    bool Offset(int oldRotationIndex, int newRotationIndex)
    {
        Vector2Int offsetValue1, offsetValue2, endOffset = Vector2Int.zero;
        Vector2Int[,] currentOffsetData;

        if (pieceType == PieceType.O) currentOffsetData = TetrisData.O_OFFSET_DATA;
        else if (pieceType == PieceType.I) currentOffsetData = TetrisData.I_OFFSET_DATA;
        else currentOffsetData = TetrisData.JLSTZ_OFFSET_DATA;

        bool movePossible = false;

        for (int testIndex = 0; testIndex < 5; testIndex++)
        {
            offsetValue1 = currentOffsetData[testIndex, oldRotationIndex];
            offsetValue2 = currentOffsetData[testIndex, newRotationIndex];
            endOffset = offsetValue1 - offsetValue2;
            if (CanPieceMove(endOffset))
            {
                movePossible = true;
                break;
            }
        }

        if (movePossible) MovePiece(endOffset);

        return movePossible;
    }

    /// <summary>
    /// Locks all the tiles of a piece, testing if it causes a game over or not. In that second case, it calls the TetrisBoardController to check if there are lines to clear
    /// </summary>
    public void SetPiece()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (!tiles[i].SetTile())
            {
                TBController.GameOver(tiles);
                enabled = false;
                return;
            }
        }
        TetrisBoardController.Instance.CheckLinesToClear();
        enabled = false;
    }

    /// <summary>
    /// The normal behaviour is that the game is moving the piece down when some time has passed, 
    /// but it can be a hard drop when it has to move directly to the bottom
    /// </summary>
    /// <param name="hardDrop"></param>
    /// <param name="test">If that's true, that means it doesn't have to lock the piece when is hard dropping</param>
    public void DropPiece(bool hardDrop = false, bool test = false)
    {
        if (hardDrop)
        {
            while (MovePiece(Vector2Int.down, test)) { }
        }
        else
        {
            downLimitReached = !MovePiece(Vector2Int.down);

            if (downLimitReached) //If the down limit has been reached
            {
                if (!moved || locked) //And the piece hasn't move between drops or the locked bool indicates that the piece has to lock, it does
                {
                    SetPiece();
                    return;
                }
                else //In any other case, then the locked coroutine will begin
                {
                    if (lockedCoroutine == null)
                    {
                        moved = false;
                        lockedCoroutine = StartCoroutine(GettingLockedCoroutine());
                    }
                }
            }
        }
    }

    /// <summary>
    /// Coroutine that starts a timer and changes locked value to true after some time to make the piece locked after this time
    /// </summary>
    /// <returns></returns>
    IEnumerator GettingLockedCoroutine()
    {
        yield return new WaitForSeconds(lockedLimitTime);

        locked = true;
    }
}
