using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Most simple version of bot: it searches for the best action from the possible actions for the current piece in the current state. It doesn't look to the future of the game
/// </summary>
public class TetrisBot
{
    protected TetrisState currentTetrisState; //Current modelization of the actual board game

    protected int boardWidth;
    protected int boardHeight;

    public TetrisBot()
    { 
        boardWidth = TetrisBoardController.Instance.boardWidth;
        boardHeight = TetrisBoardController.Instance.boardHeight;

        currentTetrisState = new TetrisState();
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

        List<PieceAction> possibleActions = currentTetrisState.GetActions(nextPiece); //First of all, it gets the possible actions with the current piece in the current state
        float bestScore = -float.MaxValue;
        PieceAction bestAction = null;

        int i = Random.Range(0, possibleActions.Count); //The first possible action to test is chosen randomly
        int initialIndex = i;

        //In a while loop that goes until the time ends
        while(Time.time - t0 < budget)
        {
            TetrisState newState = currentTetrisState.CloneState(); //The TetrisState is cloned

            newState.DoAction(nextPiece, possibleActions[i]); //One of the possible actions is played
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

        TetrisBoardController.Instance.DoActionByBot(bestAction); //And also, it is said to the TetrisBoardController to play that action in the real board
    }
}