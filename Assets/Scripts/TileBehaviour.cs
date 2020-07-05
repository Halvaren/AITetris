using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A tile is one of the 4 parts that forms a Tetris piece. It has a relative position inside the piece depending on the type of the piece and the rotation
/// </summary>
public class TileBehaviour : MonoBehaviour
{
    public Vector2Int Coordinates { get; private set; } //Coordinates of the tile

    public PieceBehaviour Piece { get; private set; } //Pointer to the piece which forms
    public int tileIndex { get; private set; } //Index where it is stored in the array of tiles of its piece

    /// <summary>
    /// Initializes the tile with data of its piece
    /// </summary>
    /// <param name="myPB"></param>
    /// <param name="index"></param>
    public void InitializeTile(PieceBehaviour myPB, int index)
    {
        Piece = myPB;
        tileIndex = index;
    }

    /// <summary>
    /// Moves to a specific position
    /// </summary>
    /// <param name="newPos"></param>
    public void UpdatePosition(Vector2Int newPos)
    {
        Coordinates = newPos;
        transform.localPosition = new Vector3(newPos.x, newPos.y);
    }

    /// <summary>
    /// Changes its material
    /// </summary>
    /// <param name="material"></param>
    public void SetMaterial(Material material)
    {
        GetComponent<MeshRenderer>().material = material;
    }

    /// <summary>
    /// Moves a specific amount of movement
    /// </summary>
    /// <param name="movement"></param>
    public void MoveTile(Vector2Int movement)
    {
        Vector2Int endPos = Coordinates + movement;
        UpdatePosition(endPos);
    }

    /// <summary>
    /// Checks if the tile can move to that position
    /// </summary>
    /// <param name="endPos"></param>
    /// <returns></returns>
    public bool CanTileMove(Vector2Int endPos)
    {
        if (!TetrisBoardController.Instance.IsInBounds(endPos))
        {
            return false;
        }
        if (!TetrisBoardController.Instance.IsPosEmpty(endPos))
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Locks the tile. If it's over a specific position in Y, return false to indicate that is a gameOver
    /// </summary>
    /// <returns></returns>
    public bool SetTile()
    {
        if (Coordinates.y >= 20) return false;

        TetrisBoardController.Instance.OccupyPos(Coordinates, this); //Sets the position in the board
        return true;
    }

    /// <summary>
    /// Rotates the tile, which means moving it. It uses multiplication of matrices
    /// </summary>
    /// <param name="originPos"></param>
    /// <param name="clockwise"></param>
    public void RotateTile(Vector2Int originPos, bool clockwise)
    {
        Vector2Int relativePos = Coordinates - originPos;
        Vector2Int[] rotationMatrix = clockwise ? new Vector2Int[2] { new Vector2Int(0, -1), new Vector2Int(1, 0) }
                                                : new Vector2Int[2] { new Vector2Int(0, 1), new Vector2Int(-1, 0) };

        int newXPos = (rotationMatrix[0].x * relativePos.x) + (rotationMatrix[1].x * relativePos.y);
        int newYPos = (rotationMatrix[0].y * relativePos.x) + (rotationMatrix[1].y * relativePos.y);

        Vector2Int newPos = new Vector2Int(newXPos, newYPos);

        newPos += originPos;
        UpdatePosition(newPos);
    }
}
