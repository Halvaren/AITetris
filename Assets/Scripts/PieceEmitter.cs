using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceEmitter : MonoBehaviour
{
    public GameObject piecePrefab;
    public Material[] pieceMaterials;

    public PieceBehaviour EmitPiece()
    {
        PieceType pieceType = TetrisRandomGenerator.Instance.GetNextPiece();

        PieceBehaviour piece = Instantiate(piecePrefab, transform.position, Quaternion.identity, transform).GetComponent<PieceBehaviour>();
        piece.SpawnPiece(pieceType, pieceMaterials[(int) pieceType]);

        return piece;
    }
}