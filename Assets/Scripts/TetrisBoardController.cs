using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBoardController : MonoBehaviour
{
    private TileBehaviour[,] board;

    public int boardWidth;
    public int boardHeight;

    public float gravityDropLimit = 0.5f;
    public float softDropLimit = 0.1f;
    public float horMovLimit = 0.05f;
    public float lockedLimitTime = 1f;

    private bool softDrop = false;
    private float dropCounter;
    private float horMovCounter;

    public PieceEmitter pieceEmitter;

    private PieceBehaviour currentPiece;

    public Vector2Int spawnPos;

    public UIController UIController;

    public int linesToLevelUp = 10;
    public int[] possibleScores;

    private int level;
    private int linesCleared;
    private int score;

    private bool startedGame = false;

    private static TetrisBoardController instance;
    public static TetrisBoardController Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(InitializeGame());
    }

    IEnumerator InitializeGame()
    {
        board = new TileBehaviour[boardWidth, boardHeight];
        level = 1;
        linesCleared = score = 0;
        UpdateUI();

        StartCoroutine(TetrisRandomGenerator.Instance.FillBags());

        while (!TetrisRandomGenerator.Instance.GetBagsPrepared())
            yield return null;

        currentPiece = pieceEmitter.EmitPiece();
        startedGame = true;
    }

    void Update()
    {
        if (startedGame)
        {
            Rotation();
            HoritzontalMovement();
            VerticalMovement();
        }
    }

    void Rotation()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentPiece.RotatePiece(true, true);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            currentPiece.RotatePiece(false, true);
        }
    }

    void HoritzontalMovement()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horMovCounter += Time.deltaTime;

            if (horMovCounter > horMovLimit)
            {
                horMovCounter = 0.0f;
                currentPiece.MovePiece(Vector2Int.left);
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            horMovCounter += Time.deltaTime;

            if (horMovCounter > horMovLimit)
            {
                horMovCounter = 0.0f;
                currentPiece.MovePiece(Vector2Int.right);
            }
        }

        if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            horMovCounter = 0.0f;
    }

    void VerticalMovement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentPiece.DropPiece(true);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                softDrop = true;
                dropCounter = softDropLimit;
            }
            else if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                softDrop = false;
                dropCounter = 0.0f;
            }

            dropCounter += Time.deltaTime;
            if ((softDrop && dropCounter >= softDropLimit) || (!softDrop && dropCounter > gravityDropLimit))
            {
                dropCounter = 0.0f;
                currentPiece.DropPiece();
            }
        }
    }

    public bool IsInBounds(Vector2Int position)
    {
        if (position.x < 0 || position.x >= board.GetLength(0) || position.y < 0)
            return false;

        return true;
    }

    public bool IsPosEmpty(Vector2Int position)
    {
        if (position.y >= 20)
        {
            return true;
        }

        if (board[position.x, position.y] != null) return false;
        return true;
    }

    public void OccupyPos(Vector2Int coords, TileBehaviour tile)
    {
        board[coords.x, coords.y] = tile;
    }

    public void CheckLinesToClear()
    {
        List<int> linesToClear = new List<int>();

        for (int i = 0; i < boardHeight; i++)
        {
            bool toClear = true;
            for (int j = 0; j < boardWidth; j++)
            {
                toClear &= board[j, i];
                if (!toClear) break;
            }

            if (toClear) linesToClear.Add(i);
        }

        if (linesToClear.Count > 0)
        {
            UpdateScoreLines(linesToClear.Count);
            ClearLines(linesToClear);
        }

        currentPiece = pieceEmitter.EmitPiece();
    }

    void ClearLines(List<int> lineIndices)
    {
        int clearedLines = 0;
        for (int i = lineIndices[0]; i < boardHeight; i++)
        {
            if (lineIndices.Contains(i))
            {
                clearedLines++;
            }
            else
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    if (board[j, i] != null)
                    {
                        board[j, i].MoveTile(Vector2Int.down * clearedLines);
                    }

                    if (board[j, i - clearedLines] != null)
                    {
                        PieceBehaviour pB = board[j, i - clearedLines].Piece;
                        pB.tiles[board[j, i - clearedLines].tileIndex] = null;

                        Destroy(board[j, i - clearedLines].gameObject);
                        if (!pB.AnyTilesLeft()) Destroy(pB.gameObject);
                    }

                    board[j, i - clearedLines] = board[j, i];
                    board[j, i] = null;
                }
            }
        }
    }

    void UpdateScoreLines(int linesCleared)
    {
        score += possibleScores[linesCleared - 1] * level;

        this.linesCleared += linesCleared;
        if (this.linesCleared > level * linesToLevelUp)
        {
            level++;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        UIController.UpdateScoreText(score);
        UIController.UpdateLinesText(linesCleared);
        UIController.UpdateLevelText(level);
    }

    public void GameOver(TileBehaviour[] lastPieceTiles)
    {
        StartCoroutine(GameOverCoroutine(lastPieceTiles));
    }

    IEnumerator GameOverCoroutine(TileBehaviour[] lastPieceTiles)
    {
        startedGame = false;
        currentPiece = null;

        WaitForSeconds timeBetweenLineRemoving = new WaitForSeconds(0.02f);

        PieceBehaviour pB = lastPieceTiles[0].Piece;
        foreach (TileBehaviour tile in lastPieceTiles)
        {
            Destroy(tile.gameObject);
        }
        Destroy(pB.gameObject);

        yield return timeBetweenLineRemoving;

        for (int i = boardHeight - 1; i >= 0; i--)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                if (board[j, i] != null)
                {
                    pB = board[j, i].Piece;
                    pB.tiles[board[j, i].tileIndex] = null;

                    Destroy(board[j, i].gameObject);

                    if (!pB.AnyTilesLeft()) Destroy(pB.gameObject);
                }
            }
            yield return timeBetweenLineRemoving;
        }

        StopAllCoroutines();
        StartCoroutine(InitializeGame());
    }
}