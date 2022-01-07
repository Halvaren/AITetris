using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HumanizedTetrisBot is a improvement on the basic bot trying to give it a more human behaviour. 
/// Tetris players use to leave one column of the board empty always they can in order to make Tetris (delete 4 rows with only one piece)
/// when they get a I piece. For that, HumanizedTetrisBot uses another weight in the evaluation of an action which makes worse an action that 
/// places tiles in the first column
/// </summary>
public class HumanizedTetrisBot : TetrisBot
{
    public float t0;
    PieceAction bestAction;

    /// <summary>
    /// Main loop of the bot. It's based on the same method of TetrisBot, but with the add-on that checks if a Tetris can be played
    /// </summary>
    /// <param name="nextPieceType"></param>
    /// <param name="budget"></param>
    /// <returns></returns>
    public override IEnumerator ActCoroutine(PieceType nextPieceType, float budget)
    {
        t0 = 0.0f;
        
        PieceModel nextPiece = new PieceModel(nextPieceType);

        List<PieceAction> possibleActions = currentTetrisState.GetActions(nextPiece);
        bestAction = null;

        yield return null;

        t0 += Time.deltaTime;

        //If the currentPiece is a I piece, the Tetris is checked since there is no possibility to make a Tetris with another type of piece
        if (nextPieceType == PieceType.I) CheckTetris(nextPiece, currentTetrisState, possibleActions, budget);

        //If there is no possibility of Tetris, the same algorithm than TetrisBot is played
        if (bestAction == null)
        {
            float bestScore = -float.MaxValue;

            int i = Random.Range(0, possibleActions.Count);
            int initialIndex = i;
            while (t0 < budget)
            {
                if (!TBController.pausedGame)
                {
                    t0 += Time.deltaTime;

                    TetrisState newState = currentTetrisState.CloneState();

                    newState.DoAction(nextPiece, possibleActions[i]);
                    nextPiece.ResetCoordinates();

                    float score = newState.GetHumanizedScore();

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestAction = possibleActions[i];
                    }

                    i++;

                    if (i == possibleActions.Count) i = 0;
                    if (i == initialIndex) break;
                }

                yield return null;
            }
        }
        else Debug.Log("BOOM! Tetris for bot");

        //If there is no bestAction at this point, a random action is going to be played
        if (bestAction == null) bestAction = currentTetrisState.GetRandomAction(nextPiece);
        currentTetrisState.DoAction(nextPiece, bestAction);

        TBController.DoActionByBot(bestAction);
    }

    /// <summary>
    /// Uses also the budget in order to make a fair calculation: if the budget is consumed checking the Tetris, then it stops
    /// </summary>
    /// <param name="nextPiece"></param>
    /// <param name="currentTetrisState"></param>
    /// <param name="possibleActions"></param>
    /// <param name="budget"></param>
    /// <returns></returns>
    protected IEnumerator CheckTetris(PieceModel nextPiece, TetrisState currentTetrisState, List<PieceAction> possibleActions, float budget)
    {
        int i = 0;
        while(t0 < budget && i < possibleActions.Count)
        {
            if (!TBController.pausedGame)
            {
                t0 += Time.deltaTime;

                PieceAction action = possibleActions[i];
                if (action.rotationIndex == 0) //The I piece is horizontal, not vertical, so it can't be a Tetris
                {
                    i++;
                    continue;
                }

                TetrisState newState = currentTetrisState.CloneState();

                newState.DoAction(nextPiece, action);
                nextPiece.ResetCoordinates();

                if (newState.IsTetris()) bestAction = action;

                i++;
            }

            yield return null;
        }
    }
}
