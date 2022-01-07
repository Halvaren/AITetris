using Assets.Scripts;
using Assets.Scripts.GeneticAlgorithm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayMode
{
    Player, ShowOff, Training, Testing
}

/// <summary>
/// There are different versions of bots
/// </summary>
public enum BotVersion
{
    TetrisBot, MCTSBot, HumanizedBot
}

/// <summary>
/// Main script of the game
/// </summary>
public class TetrisBoardController : MonoBehaviour
{
    #region Variables 

    #region Board variables

    private TileBehaviour[,] board; //Matrix of tiles that models the board
    //The left column of the board corresponds to 0 position in X axis
    //The bottom row of the board corresponds to 0 position in Y axis

    public int boardWidth;
    public int boardHeight;

    public Vector2Int spawnPos; //Position where the piece will be spawned

    #endregion

    #region Movement variables

    public float gravityDropLimit = 0.5f; //Time between natural drops (drop of the piece without user interaction)
    public float softDropLimit = 0.1f; //Time between soft drops (drop of the piece with the user moving it down)
    public float horMovLimit = 0.05f; //Time between horizontal movements
    public float lockedLimitTime = 1f; //Time until a piece is locked when it has touched another locked piece or the bottom of the board

    private bool softDrop = false;
    private float dropCounter;
    private float horMovCounter;

    #endregion

    #region Score and other data variables

    public int linesToLevelUp = 10; //Amount of lines that have to be cleared in order to level up
    public int[] possibleScores; //Scores associated with the amount of cleared with one piece

    private int level; //Level of the game
    private int linesCleared; //Total amount of lines cleared during the game
    private int score; //Score made during the game
    private int pieces; //Pieces locked during the game

    #endregion

    #region Bot variables

    public PlayMode playMode; //Indicates if it is the user who plays or, if it is the bot, it is training, testing its training or just showing off its behavior
    public BotVersion botVersion; //Indicates which of the three BotVersions is being used

    [Tooltip("Type 0 to play the bot with the current weights")] 
    public int generation; //Generation from where the weights are extracted when it's not training

    private TetrisBot bot; //The bot that is going to be used

    public float initialActionTime; //Time that has the bot for the first action. For MCTSBot and HumanizedBot it is useful because they need more time in the first turn to create the MCTreeSearch
    public float nextActionsTime = 1; //Time that has the bot for the rest of the actions of the game

    #region Bot weight variables

    //Weights use to calculate the "score" of each action made by a bot, in order to choose the best one
    public float holesWeight; //It will be multiplied by the number of holes. A hole is a tile position where the bot cannot access
    public float bumpinessWeight; //It will be multiplied by the bumpiness value. This value is calculated measuring the difference of heights between columns
    public float linesWeight; //It will be multiplied by the number of lines cleared by one piece
    public float linesHolesWeight; //It will be multiplied by the number of rows that have at least one hole.
    public float humanizedWeight; //It will be multiplied by the number of tiles in the first column of the board. It is called humanizedWeight because is only used by the HumanizedBot

    #endregion

    #region Genetic algorithm variables

    private GeneticAlgorithm geneticAlgorithm; //Algorithm that searches the best weight combination for the selected bot version
    public int populationSize; //Size of the population of the genetic algorithm
    public float mutationRate; //Probability of mutation per gene

    [Tooltip("Type 0 to keep the game over as the only limit")] 
    public int pieceLimitTraining = 200; //Number of pieces that represents the limit per game when training is on

    #endregion

    #region Testing variables

    public float initialCalculationTime = 1.0f;
    public float decreasingCalculationTimeFactor = 0.05f;

    #endregion

    #endregion

    #region Other variables

    private PieceEmitter pieceEmitter; //It points to the PieceEmitter
    public PieceEmitter PieceEmitter
    {
        get
        {
            if (pieceEmitter == null) pieceEmitter = PieceEmitter.Instance;
            return pieceEmitter;
        }
    }

    private UIController uiController;
    public UIController UIController //It is points to the controller of the UI
    {
        get
        {
            if (uiController == null) uiController = UIController.Instance;
            return uiController;
        }
    }

    private OtherSettingsController osController;
    public OtherSettingsController OSController
    {
        get
        {
            if (osController == null) osController = OtherSettingsController.Instance;
            return osController;
        }
    }

    private TetrisRandomGenerator trg; //Points to the Tetris Random Generator
    public TetrisRandomGenerator TRG
    {
        get
        {
            if (trg == null) trg = TetrisRandomGenerator.Instance;
            return trg;
        }
    }

    private static TetrisBoardController instance;
    public static TetrisBoardController Instance
    {
        get
        {
            return instance;
        }
    }

    private bool startedGame = false; //It shows if the game has been started or not
    public bool pausedGame = false;
    private PieceBehaviour currentPiece; //It points to the current piece that is playing

    #endregion

    #region Debug variables

    public bool debugMode = false;

    #endregion

    #endregion

    #region Methods

    #region Initialization methods

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UpdateUIOtherSettings();
    }

    public void InitializeGame()
    {
        if (playMode == PlayMode.Training) //The training has to initialize only once
        {
            InitializeTraining();
        }
        else //If there is no training, the weights are loaded from a file
        {
            if(generation != 0 && playMode != PlayMode.Player)
            {
                LoadWeightsFromFile();
            }
        }

        StartGame();
    }

    /// <summary>
    /// Initializes the game: creates the board, initializes the score variables, calls to shuffle the bags, emits the first piece and starts the bot, if it's the case
    /// If the training is activated, plays it too
    /// </summary>
    void StartGame()
    {
        board = new TileBehaviour[boardWidth, boardHeight];
        level = 1;
        linesCleared = score = pieces = 0;
        UpdateUIGameStats();

        TRG.ShuffleBags();

        currentPiece = PieceEmitter.EmitPiece();
        startedGame = true;

        if (playMode != PlayMode.Player)
        {
            if(playMode != PlayMode.Training)
            {
                if (playMode == PlayMode.Testing) nextActionsTime = initialCalculationTime;
                PlayBot();
            }
            else
            {
                geneticAlgorithm.PlayNextGame();
            }
        }
    }

    #endregion

    #region Handling user interaction methods

    void Update()
    {
        //It only works when the game has started and the player mode is on
        if (startedGame && !pausedGame && playMode == PlayMode.Player)
        {
            Rotation();
            HoritzontalMovement();
            VerticalMovement();
        }

        //Debug

        if(debugMode)
        {
            /*if(startedGame && firstAction && !waitForUser)
            {
                waitForUser = true;
                StartCoroutine(bot.ActCoroutine(currentPiece.pieceType, nextActionsTime * 0.9f));
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                DebugMethod();
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log(debugText);
                waitForUser = false;
                DoActionByBot(nextPieceAction, true);
            }*/
        }
    }

    /// <summary>
    /// Rotates a piece using the user interaction
    /// </summary>
    void Rotation()
    {
        //Clockwise
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentPiece.RotatePiece(true, true);
        }
        //Counterclockwise
        else if (Input.GetKeyDown(KeyCode.C))
        {
            currentPiece.RotatePiece(false, true);
        }
    }

    /// <summary>
    /// Moves a piece horizontally using the user interaction
    /// </summary>
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

    /// <summary>
    /// Moves a piece down using the user interaction
    /// </summary>
    void VerticalMovement()
    {
        //This is a hard drop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentPiece.DropPiece(true);
        }
        //And this is a soft drop
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

    #endregion

    #region Piece handling methods

    /// <summary>
    /// Checks if a position is in the bounds of the borad or not
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsInBounds(Vector2Int position)
    {
        if (position.x < 0 || position.x >= board.GetLength(0) || position.y < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a position is empty or not
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsPosEmpty(Vector2Int position)
    {
        if (position.y >= 20)
        {
            return true;
        }

        if (board[position.x, position.y] != null) return false;
        return true;
    }

    /// <summary>
    /// Sets a tile in a position of the board
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="tile"></param>
    public void OccupyPos(Vector2Int coords, TileBehaviour tile)
    {
        board[coords.x, coords.y] = tile;
    }

    /// <summary>
    /// Checks if there are lines to clear and if there are, calls to clear that lines
    /// </summary>
    public void CheckLinesToClear()
    {
        //This list will stored the same bool values as rows the board have, and the rows that have to be cleared will be true in this list
        List<bool> linesToClear = new List<bool>();
        int firstClearedLine = -1; //First line that has to be cleared starting from bottom
        int clearedLinesCount = 0; //Amount of lines that have to be cleared

        for (int i = 0; i < boardHeight; i++)
        {
            bool toClear = true;
            //If there is any position without a tile, then the row isn't completed
            for (int j = 0; j < boardWidth; j++)
            {
                toClear &= board[j, i];
                if (!toClear) break;
            }

            linesToClear.Add(toClear);
            if(toClear)
            {
                clearedLinesCount++;
                if (firstClearedLine == -1) firstClearedLine = i;
            }
        }

        if (clearedLinesCount > 0)
        {
            UpdateScoreLines(clearedLinesCount);
            ClearLines(linesToClear, firstClearedLine);
        }

        //Next piece is spawned
        currentPiece = PieceEmitter.EmitPiece();
        pieces++;

        UpdateUIGameStats();

        if(playMode == PlayMode.Training && pieceLimitTraining != 0 && pieces > pieceLimitTraining) //In case training is activated and there is a limit of pieces per game, it checks that this limit has been reached
        {
            GameOver(); //In that case, the game is over
            return;
        }

        //Since this is a method called when a piece is locked in the board, here a new next piece is sent to the bot (if that bot is based in MCTS)
        if (botVersion == BotVersion.MCTSBot) ((MCTSTetrisBot)bot).AddNewPiece(TRG.GetLastNextPiece());
    }

    /// <summary>
    /// Going through the linesToBeClear list, starting from the first one that has to be cleared, some tiles will be deleted and some tiles will be moved properly
    /// </summary>
    /// <param name="lineIndices"></param>
    void ClearLines(List<bool> linesToBeCleared, int firstClearedLine)
    {
        int clearedLines = 0;

        for (int i = firstClearedLine; i < linesToBeCleared.Count; i++)
        {
            if (linesToBeCleared[i])
            {
                clearedLines++;
            }
            else
            {
                for (int j = 0; j < boardWidth; j++)
                {   
                    //Moving top tiles to down
                    if (board[j, i] != null)
                    {
                        board[j, i].MoveTile(Vector2Int.down * clearedLines);
                    }

                    //Managing tiles that were in the cleared line
                    if (board[j, i - clearedLines] != null)
                    {
                        PieceBehaviour pB = board[j, i - clearedLines].Piece;
                        pB.tiles[board[j, i - clearedLines].tileIndex] = null;

                        Destroy(board[j, i - clearedLines].gameObject);
                        if (!pB.AnyTilesLeft()) Destroy(pB.gameObject); //If all the tiles of a piece are deleted, then the piece GameObject can be deleted as well
                    }

                    board[j, i - clearedLines] = board[j, i];
                    board[j, i] = null;
                }
            }
        }
    }

    #endregion

    #region Update data methods

    /// <summary>
    /// It updates the score, the level and cleared lines values
    /// </summary>
    /// <param name="linesCleared"></param>
    void UpdateScoreLines(int linesCleared)
    {
        score += possibleScores[linesCleared - 1] * level;

        this.linesCleared += linesCleared;
        if (this.linesCleared > level * linesToLevelUp)
        {
            level++;
            if(playMode == PlayMode.Testing)
            {
                nextActionsTime -= decreasingCalculationTimeFactor;
            }
        }
    }

    /// <summary>
    /// Updates the UI with the proper data
    /// </summary>
    void UpdateUIGameStats()
    {
        UIController.UpdateScoreText(score);
        UIController.UpdatePiecesText(pieces);
        UIController.UpdateLinesText(linesCleared);
        UIController.UpdateLevelText(level);
    }

    /// <summary>
    /// Updates the weight values in the UI
    /// </summary>
    void UpdateUIWeights()
    {
        UIController.UpdateHolesWeightText(holesWeight);
        UIController.UpdateBumpinessWeightText(bumpinessWeight);
        UIController.UpdateLinesWeightText(linesWeight);
        UIController.UpdateRowsHolesWeightText(linesHolesWeight);
        UIController.UpdateHumanizedText(humanizedWeight);
    }

    /// <summary>
    /// Updates the generation value in the UI
    /// </summary>
    /// <param name="generation"></param>
    public void UpdateUIGeneration(int generation)
    {
        UIController.UpdateGenerationText(generation);
    }

    public void UpdateUIOtherSettings()
    {
        OSController.SetInitialActionTime(initialActionTime);
        OSController.SetNextActionsTime(nextActionsTime);
        OSController.SetPopulationSize(populationSize);
        OSController.SetMutationRate(mutationRate);
        OSController.SetPieceLimit(pieceLimitTraining);
        OSController.SetInitialCalculationTime(initialCalculationTime);
        OSController.SetDecreasingCalculationFactor(decreasingCalculationTimeFactor);
    }

    #endregion

    #region GameOver methods

    public void GameOver(bool stoppedTesting = false)
    {
        GameOver(currentPiece.tiles, stoppedTesting);
    }

    /// <summary>
    /// Starts the game over coroutine
    /// </summary>
    /// <param name="lastPieceTiles"></param>
    public void GameOver(TileBehaviour[] lastPieceTiles, bool stoppedTesting = false)
    {
        StopAllCoroutines();

        StartCoroutine(GameOverCoroutine(lastPieceTiles, stoppedTesting));
    }

    /// <summary>
    /// First of all, it deletes the tiles of the piece that has caused the game over (it hasn't been locked in the board because there is the possiblity that its tiles occupy positions that are already occupied)
    /// Then, it deletes the tiles of each row of the board passing a short time between them
    /// In case, the game is on bot mode and the weightEvolution bool is true, it will log the needed data in a txt and update the values of the weights
    /// And finally, it starts the game again
    /// </summary>
    /// <param name="lastPieceTiles"></param>
    /// <returns></returns>
    IEnumerator GameOverCoroutine(TileBehaviour[] lastPieceTiles, bool stoppedTesting = false)
    {
        startedGame = false;
        pausedGame = false;
        currentPiece = null;

        WaitForSeconds timeBetweenLineRemoving = new WaitForSeconds(0.02f);

        PieceBehaviour pB;
        if (lastPieceTiles != null)
        {
            pB = lastPieceTiles[0].Piece;
            foreach (TileBehaviour tile in lastPieceTiles)
            {
                Destroy(tile.gameObject);
            }
            Destroy(pB.gameObject);

            yield return timeBetweenLineRemoving;
        }

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

        StopAllCoroutines(); //????????

        if (playMode == PlayMode.Training) 
        { 
            geneticAlgorithm.SetDataFromLastGame(score, pieces, linesCleared, level);

            InitializeGame();
        }
        else
        {
            if (playMode == PlayMode.Testing) WriteTestingData(stoppedTesting);

            UIController.ResetButtons();
            UIController.EnableBotSettings(true);
            UIController.EnableWeightFields(true);
        }
    }

    #endregion

    #region Bot methods

    /// <summary>
    /// Starts the BotCoroutine
    /// </summary>
    public void PlayBot()
    {
        StartCoroutine(BotCoroutine());
    }

    /// <summary>
    /// A coroutine that lasts during all the game
    /// </summary>
    /// <returns></returns>
    IEnumerator BotCoroutine()
    {
        //Firstly, it creates the bot depending on the version
        switch (botVersion)
        {
            case BotVersion.TetrisBot:
                bot = new TetrisBot();
                break;
            case BotVersion.MCTSBot:
                bot = new MCTSTetrisBot(TetrisRandomGenerator.Instance.GetNextPieces(), initialActionTime * 0.9f); //It doesn't give the whole time to avoid that the bot uses more time than the actual time
                break;
            case BotVersion.HumanizedBot:
                bot = new HumanizedTetrisBot(/*TetrisRandomGenerator.Instance.GetNextPieces(), initialActionTime * 0.9f*/);
                break;
        }

        float timer = 0.0f;

        while(timer < initialActionTime)
        {
            if(!pausedGame)
                timer += Time.deltaTime;

            yield return null;
        }

        //And starts the main loop
        while (startedGame)
        {
            yield return StartCoroutine(bot.ActCoroutine(currentPiece.pieceType, nextActionsTime * 0.9f));
        }
    }

    /// <summary>
    /// It executes the action chosen by the bot
    /// </summary>
    /// <param name="action"></param>
    public void DoActionByBot(PieceAction action, bool debug = false)
    {
        /*if (debugMode && !debug) nextPieceAction = action;
        else
        {*/
            for (int i = 0; i < action.rotationIndex; i++)
            {
                currentPiece.RotatePiece(true, true);
            }
            currentPiece.MovePiece(new Vector2Int(action.xCoord - currentPiece.tiles[0].Coordinates.x, 0));

            currentPiece.DropPiece(true);
        //}
    }

    #endregion

    #region Genetic Algorithm methods

    /// <summary>
    /// Creates the genetic algorithm object, based on the selected bot version
    /// </summary>
    private void InitializeTraining()
    {
        int dnaSize = botVersion == BotVersion.HumanizedBot ? 5 : 4;
        geneticAlgorithm = new GeneticAlgorithm(populationSize, dnaSize, mutationRate, botVersion);
    }

    /// <summary>
    /// Giving a generation, loads that generation from a file and sets its weights to the weights of the bot
    /// </summary>
    private void LoadWeightsFromFile()
    {
        TetrisGeneration tetrisGeneration = LogWriter.GetGeneration(botVersion, generation - 1);

        if(tetrisGeneration != null)
        {
            holesWeight = tetrisGeneration.bestWeights[0];
            bumpinessWeight = tetrisGeneration.bestWeights[1];
            linesWeight = tetrisGeneration.bestWeights[2];
            linesHolesWeight = tetrisGeneration.bestWeights[3];
            if(botVersion == BotVersion.HumanizedBot) humanizedWeight = tetrisGeneration.bestWeights[4];

            UpdateUIWeights();
            UpdateUIGeneration(generation);
        }
    }

    /// <summary>
    /// Sets weights (from a generation being trained by the genetic algorithm)
    /// </summary>
    /// <param name="weights"></param>
    public void SetWeights(float[] weights)
    {
        holesWeight = weights[0];
        bumpinessWeight = weights[1];
        linesWeight = weights[2];
        linesHolesWeight = weights[3];
        if(weights.Length == 5) humanizedWeight = weights[4];

        UpdateUIWeights();
    }

    #endregion

    public void WriteTestingData(bool stoppedTesting = false)
    {
        TestingData testingData = new TestingData();
        testingData.lines = linesCleared;
        testingData.level = level;
        testingData.score = score;
        testingData.pieces = pieces;

        testingData.lastCalculationTime = nextActionsTime;

        testingData.stopped = stoppedTesting;

        LogWriter.WriteTesting(botVersion, testingData);
    }

    #region Debug methods

    /// <summary>
    /// Coroutine use for debugging. It shows all the possible actions passed by parameter for the current piece
    /// </summary>
    /// <param name="actions"></param>
    /// <returns></returns>
    public IEnumerator ShowPossibleActionsCoroutine(List<PieceAction> actions)
    {
        Vector2Int[] originalCoordinates = currentPiece.GetTileCoords();

        currentPiece.MovePiece(originalCoordinates);
        foreach (PieceAction action in actions)
        {
            yield return ShowPossibleActionCoroutine(action);

            currentPiece.MovePiece(originalCoordinates);
        }
    }

    public IEnumerator ShowPossibleActionCoroutine(PieceAction action)
    {
        for (int i = 0; i < action.rotationIndex; i++)
        {
            currentPiece.RotatePiece(true, true);
        }
        Vector2Int direction = new Vector2Int(action.xCoord - currentPiece.tiles[0].Coordinates.x, 0);
        currentPiece.MovePiece(direction);

        yield return new WaitForSeconds(0.1f);

        currentPiece.DropPiece(true, true);

        yield return new WaitForSeconds(0.3f);
    }

    /// <summary>
    /// Auxiliar debug method
    /// </summary>
    void DebugMethod()
    {
        TetrisState state = new TetrisState();

        Vector2Int[] pieces = new Vector2Int[6];
        Vector2Int[] pieces2 = new Vector2Int[6];

        pieces[0].x = 0b1111111011;
        pieces[0].y = 0;
        pieces[1].x = 0b1011110011;
        pieces[1].y = 1;
        pieces[2].x = 0b0010010010;
        pieces[2].y = 2;
        pieces[3].x = 0b0010010011;
        pieces[3].y = 3;
        pieces[4].x = 0b0000010011;
        pieces[4].y = 4;
        pieces[5].x = 0b0000010011;
        pieces[5].y = 5;

        pieces2[0].x = 0b1111111110;
        pieces2[0].y = 0;
        pieces2[1].x = 0b1111111110;
        pieces2[1].y = 1;
        pieces2[2].x = 0b1111111110;
        pieces2[2].y = 2;
        pieces2[3].x = 0b1111111110;
        pieces2[3].y = 3;
        pieces2[4].x = 0b1111111110;
        pieces2[4].y = 4;
        pieces2[5].x = 0b1111111110;
        pieces2[5].y = 5;

        state.LockPiece(pieces);
        Debug.Log(state.GetHoleCount());
        Debug.Log(state.GetBumpiness());

        state = new TetrisState();

        state.LockPiece(pieces2);
        Debug.Log(state.GetHoleCount());
        Debug.Log(state.GetBumpiness());
    }

    #endregion

    #endregion
}
