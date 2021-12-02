using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Most simple version of bot: it searches for the best action from the possible actions for the current piece in the current state. It doesn't look to the future of the game
/// </summary>
public class TetrisBot
{
    protected TetrisState currentTetrisState; //Current modelization of the actual board game
    protected TetrisState emptyTetrisState; //Empty modelization of the board game useful for the calculation of the possible actions with each piece

    protected int boardWidth;
    protected int boardHeight;

    protected Dictionary<PieceType, List<PieceAction>> pieceActionDictionary;

    private TetrisBoardController tbController;
    public TetrisBoardController TBController
    {
        get
        {
            if (tbController == null) tbController = TetrisBoardController.Instance;
            return tbController;
        }
    }

    public TetrisBot()
    { 
        boardWidth = TBController.boardWidth;
        boardHeight = TBController.boardHeight;

        currentTetrisState = new TetrisState();
        emptyTetrisState = new TetrisState();

        pieceActionDictionary = new Dictionary<PieceType, List<PieceAction>>();
    }

    /// <summary>
    /// Main bot method that searches for the best action to do with the current piece in the current state.
    /// It has a budget of time which is the max time it has to find that best action
    /// </summary>
    /// <param name="nextPieceType"></param>
    /// <param name="budget"></param>
    /// <returns></returns>
    public virtual IEnumerator ActCoroutine(PieceType nextPieceType, float budget)
    {
        PieceModel nextPiece = new PieceModel(nextPieceType);

        float t0 = Time.time;

        List<PieceAction> possibleActions; //First of all, it gets the possible actions with the current piece in the current state
        if (!pieceActionDictionary.ContainsKey(nextPieceType))
        {
            possibleActions = emptyTetrisState.GetActions(nextPiece);
            pieceActionDictionary.Add(nextPieceType, possibleActions);
        }
        else possibleActions = pieceActionDictionary[nextPieceType];

        float bestScore = -float.MaxValue;
        PieceAction bestAction;

        int i = Random.Range(0, possibleActions.Count); //The first possible action to test is chosen randomly
        int initialIndex = i;

        bestAction = possibleActions[i];

        Debug.Log("possible actions " + possibleActions.Count);
        //In a while loop that goes until the time ends
        while (Time.time - t0 < budget && i < possibleActions.Count)
        {
            Debug.Log("action " + i);
            TetrisState newState = currentTetrisState.CloneState(); //The TetrisState is cloned

            newState.DoAction(nextPiece, possibleActions[i]); //One of the possible actions is played in the cloned state
            nextPiece.ResetCoordinates();

            float score = newState.GetScore(); //And its score is got

            if(score > bestScore)
            {
                bestScore = score;
                bestAction = possibleActions[i];
            }

            i++;

            if (i == possibleActions.Count) i = 0;
            if (i == initialIndex) break; //If all the possible actions have been tested, the while loop ends

            yield return null;
        }

        currentTetrisState.DoAction(nextPiece, bestAction); //The bestAction is played in the real state

        TBController.DoActionByBot(bestAction); //And also, it is said to the TetrisBoardController to play that action in the real board
    }
}