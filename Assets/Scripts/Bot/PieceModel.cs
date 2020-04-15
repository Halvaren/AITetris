using UnityEngine;

public class PieceModel
{
    public PieceType pieceType;

    private Vector2Int[] binaryTiles;
    private Vector2Int[] originalTileCoordinates;
    private Vector2Int[] tileCoordinates;

    Vector2Int[] rotationClockwiseMatrix = new Vector2Int[2] { new Vector2Int(0, -1), new Vector2Int(1, 0) };
    Vector2Int[] rotationCounterclockwiseMatrix = new Vector2Int[2] { new Vector2Int(0, 1), new Vector2Int(-1, 0) };

    public PieceModel(PieceType pieceType)
    {
        this.pieceType = pieceType;

        Vector2Int spawnPosition = TetrisBoardController.Instance.spawnPos;
        originalTileCoordinates = new Vector2Int[4];

        Vector2Int[] tileRelativePositions = null;
        tileCoordinates = new Vector2Int[4];

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

        for (int i = 0; i < tileRelativePositions.Length; i++) tileCoordinates[i] = spawnPosition + tileRelativePositions[i];

        for (int i = 0; i < originalTileCoordinates.Length; i++) originalTileCoordinates[i] = tileCoordinates[i];

        binaryTiles = new Vector2Int[4];
    }

    public Vector2Int[] GetCoordinates()
    {
        return tileCoordinates;
    }

    public Vector2Int GetOneTileCoords(int tileIndex)
    {
        return tileCoordinates[tileIndex];
    }

    public void ResetCoordinates()
    {
        for (int i = 0; i < originalTileCoordinates.Length; i++) tileCoordinates[i] = originalTileCoordinates[i];
    }

    public void SetCoordinates(Vector2Int[] coords)
    {
        tileCoordinates = coords;
    }

    public void SetCoordinates(TileBehaviour[] tiles)
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            tileCoordinates[i] = tiles[i].Coordinates;
        }
    }

    public Vector2Int[] GetBinaryTiles(Vector2Int[] coords)
    {
        for(int i = 0; i < coords.Length; i++)
        {
            binaryTiles[i] = GetBinaryTile(coords[i]);
        }

        return binaryTiles;
    }

    public Vector2Int GetBinaryTile(Vector2Int coords)
    {
        Vector2Int binaryTile = new Vector2Int();
        binaryTile.x = 1 << coords.x;
        binaryTile.y = coords.y;

        return binaryTile;
    }

    public bool CanPieceMove(Vector2Int movement, TetrisState tetrisState)
    {
        for(int i = 0; i < tileCoordinates.Length; i++)
        {
            Vector2Int binaryTile = GetBinaryTile(tileCoordinates[i] + movement);

            if(!tetrisState.IsInBounds(binaryTile))
            {
                return false;
            }
            if (!tetrisState.IsPosEmpty(binaryTile))
            {
                return false;
            }
        }
        return true;
    }

    public bool Move(Vector2Int movement, TetrisState tetrisState)
    {
        bool canMove = CanPieceMove(movement, tetrisState);

        if (canMove)
        {
            for (int i = 0; i < tileCoordinates.Length; i++)
            {
                tileCoordinates[i] += movement;
            }
        }

        return canMove;
    }

    public void Rotate(bool clockwise = true)
    {
        Vector2Int originPos = tileCoordinates[0];
        Vector2Int[] rotationMatrix = clockwise ? rotationClockwiseMatrix : rotationCounterclockwiseMatrix;

        for (int i = 0; i < tileCoordinates.Length; i++)
        {
            Vector2Int relativePos = tileCoordinates[i] - originPos;

            int newXPos = (rotationMatrix[0].x * relativePos.x) + (rotationMatrix[1].x * relativePos.y);
            int newYPos = (rotationMatrix[0].y * relativePos.x) + (rotationMatrix[1].y * relativePos.y);

            Vector2Int newPos = new Vector2Int(newXPos, newYPos);

            newPos += originPos;
            tileCoordinates[i] = newPos;
        }
    }

    public void DoAction(PieceAction action, TetrisState tetrisState)
    {
        Move(new Vector2Int(action.xCoord - tileCoordinates[0].x, 0), tetrisState);
        for(int i = 0; i < action.rotationIndex; i++)
        {
            Rotate();
        }
    }

    public PieceModel ClonePiece()
    {
        PieceModel newPiece = new PieceModel(pieceType);

        newPiece.originalTileCoordinates = new Vector2Int[4];
        for(int i = 0; i < originalTileCoordinates.Length; i++)
        {
            newPiece.originalTileCoordinates[i] = originalTileCoordinates[i];
        }

        newPiece.tileCoordinates = new Vector2Int[4];
        for (int i = 0; i < tileCoordinates.Length; i++)
        {
            newPiece.tileCoordinates[i] = tileCoordinates[i];
        }

        newPiece.binaryTiles = new Vector2Int[4];
        for (int i = 0; i < binaryTiles.Length; i++)
        {
            newPiece.binaryTiles[i] = binaryTiles[i];
        }

        return newPiece;
    }
}
