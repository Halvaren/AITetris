using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public Vector2Int Coordinates { get; private set; }

    public PieceBehaviour Piece { get; private set; }
    public int tileIndex { get; private set; }

    public void InitializeTile(PieceBehaviour myPB, int index)
    {
        Piece = myPB;
        tileIndex = index;
    }

    public void UpdatePosition(Vector2Int newPos)
    {
        Coordinates = newPos;
        transform.localPosition = new Vector3(newPos.x, newPos.y);
    }

    public void SetMaterial(Material material)
    {
        GetComponent<MeshRenderer>().material = material;
    }

    public void MoveTile(Vector2Int movement)
    {
        Vector2Int endPos = Coordinates + movement;
        UpdatePosition(endPos);
    }

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

    public bool SetTile()
    {
        if (Coordinates.y >= 20) return false;

        TetrisBoardController.Instance.OccupyPos(Coordinates, this);
        return true;
    }

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
