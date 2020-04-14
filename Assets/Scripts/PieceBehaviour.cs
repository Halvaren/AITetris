using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PieceType
{
    I, J, L, O, S, T, Z
}

public class PieceBehaviour : MonoBehaviour
{
    public PieceType pieceType { get; private set; }
    public TileBehaviour[] tiles;

    private int rotationIndex;

    private bool downLimitReached;
    private bool locked;
    private bool moved;

    private Coroutine lockedCoroutine = null;

    #region Global variables

    //Have to be got from the board script
    float lockedLimitTime;
    Vector2Int spawnLocation;

    #endregion

    void Awake()
    {
        rotationIndex = 0;
        downLimitReached = locked = false;
        moved = true;
    }

    public void SpawnPiece(PieceType newType, Material material)
    {
        pieceType = newType;
        Vector2Int[] tileRelativePositions = null;
        
        //Get data from other script

        for(int i = 0; i < tiles.Length; i++)
        {
            tiles[i].UpdatePosition(spawnLocation + tileRelativePositions[i]);
            tiles[i].InitializeTile(this, i);
            tiles[i].SetMaterial(material);
        }
    }

    public bool CanPieceMove(Vector2Int movement)
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            if(!tiles[i].CanTileMove(movement + tiles[i].Coordinates))
            {
                return false;
            }
        }
        return true;
    }

    public Vector2Int[] GetTileCoords()
    {
        List<Vector2Int> tileCoords = new List<Vector2Int>();

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
                continue;
            tileCoords.Add(tiles[i].Coordinates);
        }

        tileCoords = tileCoords.OrderBy(x => x.x).ThenByDescending(x => x.y).ToList();
        Vector2Int[] result = tileCoords.ToArray();

        return result;
    }

    public bool AnyTilesLeft()
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null) return true;
        }

        return false;
    }

    public bool MovePiece(Vector2Int movement)
    {
        bool canMove = CanPieceMove(movement);

        if(!canMove)
        {
            if (movement == Vector2Int.down) SetPiece();
            return false;
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].MoveTile(movement);
        }

        moved = true;

        return true;
    }

    public void RotatePiece(bool clockwise, bool shouldOffset)
    {
        int oldRotationIndex = rotationIndex;
        rotationIndex += clockwise ? 1 : -1;
        rotationIndex = Mod(rotationIndex, 4);

        for(int i = 0; i < tiles.Length; i++)
        {
            tiles[i].RotateTile(tiles[0].Coordinates, clockwise);
        }

        if (!shouldOffset) return;

        bool canOffset = Offset(oldRotationIndex, rotationIndex);

        if (!canOffset) RotatePiece(!clockwise, false);
        else moved = true;
    }

    int Mod(int x, int m) { return (x % m + m) % m; }

    bool Offset(int oldRotationIndex, int newRotationIndex)
    {
        Vector2Int offsetVal1, offsetVal2, endOffset = Vector2Int.zero;
        Vector2Int[,] currentOffsetData = null;

        //Get offset data from other script

        bool movePossible = false;

        for(int testIndex = 0; testIndex < 5; testIndex++)
        {
            offsetVal1 = currentOffsetData[testIndex, oldRotationIndex];
            offsetVal2 = currentOffsetData[testIndex, newRotationIndex];
            endOffset = offsetVal1 - offsetVal2;
            if(CanPieceMove(endOffset))
            {
                movePossible = true;
                break;
            }
        }

        if (movePossible) MovePiece(endOffset);

        return movePossible;
    }

    public void SetPiece()
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            if(!tiles[i].SetTile())
            {
                //Game over
                return;
            }
        }
        //Check lines to clear
        this.enabled = false;
    }

    public void DropPiece(bool hardDrop = false)
    {
        if(hardDrop)
        {
            while (MovePiece(Vector2Int.down)) { }
        }
        else
        {
            downLimitReached = !MovePiece(Vector2Int.down);

            if (downLimitReached)
            {
                if (!moved || locked)
                {
                    SetPiece();
                    return;
                }
                else
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

    IEnumerator GettingLockedCoroutine()
    {
        yield return new WaitForSeconds(lockedLimitTime);

        locked = true;
    }
}
