using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBot
{
    protected TetrisState currentTetrisState;

    protected int boardWidth;
    protected int boardHeight;

    public TetrisBot()
    { 
        boardWidth = TetrisBoardController.Instance.boardWidth;
        boardHeight = TetrisBoardController.Instance.boardHeight;

        currentTetrisState = new TetrisState();
    }

    public virtual IEnumerator ActCoroutine(PieceType nextPieceType, float budget)
    {
        PieceModel nextPiece = new PieceModel(nextPieceType);

        float t0 = Time.time;

        List<PieceAction> possibleActions = currentTetrisState.GetActions(nextPiece);
        float bestScore = -float.MaxValue;
        PieceAction bestAction = null;

        int i = Random.Range(0, possibleActions.Count);
        int initialIndex = i;
        while(Time.time - t0 < budget)
        {
            TetrisState newState = currentTetrisState.CloneState();

            newState.DoAction(nextPiece, possibleActions[i]);
            nextPiece.ResetCoordinates();

            float score = newState.GetScore();

            if(score > bestScore)
            {
                bestScore = score;
                bestAction = possibleActions[i];
            }

            i++;

            if (i == possibleActions.Count) i = 0;
            if (i == initialIndex) break;

            yield return null;
        }

        currentTetrisState.DoAction(nextPiece, bestAction);

        TetrisBoardController.Instance.DoActionByBot(bestAction);
    }
}