using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceEmitter : MonoBehaviour
{
    public GameObject piecePrefab;
    public Material[] pieceMaterials;

    private static PieceEmitter instance;
    public static PieceEmitter Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Gets the next piece from the TetrisRandomGenerator, instantiates it, initializes it and returns it to the TetrisBoardController
    /// </summary>
    public PieceBehaviour EmitPiece()
    {
        PieceType pieceType = TetrisRandomGenerator.Instance.GetNextPiece();

        PieceBehaviour piece = Instantiate(piecePrefab, transform.position, Quaternion.identity, transform).GetComponent<PieceBehaviour>();
        piece.SpawnPiece(pieceType, pieceMaterials[(int) pieceType]);

        return piece;
    }
}