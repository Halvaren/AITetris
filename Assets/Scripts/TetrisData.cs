using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisData
{
    #region Tile relative positions

    public static Vector2Int[] IRelativePositions = new Vector2Int[4]
    {
        Vector2Int.zero,
        Vector2Int.left,
        Vector2Int.right * 2,
        Vector2Int.right
    };

    public static Vector2Int[] JRelativePositions = new Vector2Int[4]
    {
        Vector2Int.zero,
        Vector2Int.left,
        new Vector2Int(-1, 1),
        Vector2Int.right
    };

    public static Vector2Int[] LRelativePositions = new Vector2Int[4]
    {
        Vector2Int.zero,
        Vector2Int.left,
        Vector2Int.one,
        Vector2Int.right
    };

    public static Vector2Int[] ORelativePositions = new Vector2Int[4]
    {
        Vector2Int.zero,
        Vector2Int.right,
        Vector2Int.one,
        Vector2Int.up
    };

    public static Vector2Int[] SRelativePositions = new Vector2Int[4]
    {
        Vector2Int.zero,
        Vector2Int.left,
        Vector2Int.one,
        Vector2Int.up
    };

    public static Vector2Int[] TRelativePositions = new Vector2Int[4]
    {
        Vector2Int.zero,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.right
    };

    public static Vector2Int[] ZRelativePositions = new Vector2Int[4]
    {
        Vector2Int.zero,
        Vector2Int.up,
        new Vector2Int(-1, 1),
        Vector2Int.right
    };

    #endregion

    #region Offset data

    public static Vector2Int[,] JLSTZ_OFFSET_DATA = new Vector2Int[5, 4]
    {
        { Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero },
        { Vector2Int.zero, Vector2Int.right, Vector2Int.zero, Vector2Int.left },
        { Vector2Int.zero, new Vector2Int(1, -1), Vector2Int.zero, new Vector2Int(-1, -1)},
        { Vector2Int.zero, Vector2Int.up * 2, Vector2Int.zero, Vector2Int.up * 2},
        { Vector2Int.zero, new Vector2Int(1, 2), Vector2Int.zero, new Vector2Int(-1, 2)}
    };

    public static Vector2Int[,] I_OFFSET_DATA = new Vector2Int[5, 4]
    {
        { Vector2Int.zero, Vector2Int.left, new Vector2Int(-1, 1), Vector2Int.right },
        { Vector2Int.left, Vector2Int.zero, Vector2Int.one, Vector2Int.up },
        { Vector2Int.right * 2, Vector2Int.zero, new Vector2Int(-2, 1), Vector2Int.up},
        { Vector2Int.left, Vector2Int.up, Vector2Int.right, Vector2Int.down},
        { Vector2Int.right * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.up * 2}
    };

    public static Vector2Int[,] O_OFFSET_DATA = new Vector2Int[1, 4]
    {
        { Vector2Int.zero, Vector2Int.down, new Vector2Int(-1, -1), Vector2Int.left }
    };

    #endregion
}
