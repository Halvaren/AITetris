using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanizedTetrisBot : TetrisBot
{
    float t0;

    public HumanizedTetrisBot()
    {
        
    }

    public override IEnumerator ActCoroutine(PieceType nextPieceType, float budget)
    {
        PieceModel nextPiece = new PieceModel(nextPieceType);

        t0 = Time.time;

        List<PieceAction> possibleActions = currentTetrisState.GetActions(nextPiece);
        PieceAction bestAction = null;

        if(nextPieceType == PieceType.I) bestAction = CheckTetris(nextPiece, currentTetrisState, possibleActions, budget);

        if (bestAction == null)
        {
            float bestScore = -float.MaxValue;

            int i = Random.Range(0, possibleActions.Count);
            int initialIndex = i;
            while (Time.time - t0 < budget)
            {
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

                yield return null;
            }
        }
        else Debug.Log("BOOM! Tetris for bot");

        currentTetrisState.DoAction(nextPiece, bestAction);

        TetrisBoardController.Instance.DoActionByBot(bestAction);
        {
            /*rollouts = 0;

            PieceModel nextPiece = new PieceModel(nextPieceType);

            //yield return TetrisBoardController.Instance.StartCoroutine(TetrisBoardController.Instance.ShowPossibleActionsCoroutine(currentNode.state.GetActions(nextPiece)));

            float t0 = Time.time;

            MCTSNode currentRollingNode = currentNode;
            if (currentRollingNode.children.Count == 0)
            {
                currentRollingNode.ExtendNode(nextPiece);
            }

            MCTSNode recommendedChild = null;

            while (Time.time - t0 < budget)
            {
                MCTSNode bestChild = null;
                foreach (MCTSNode child in currentRollingNode.children)
                {
                    if (child.state.IsTetris())
                    {
                        Debug.Log("Current piece: " + child.currentPiece.pieceType);
                        Debug.Log("BOOM!! Tetris for Bot");
                        recommendedChild = child;
                        break;
                    }

                    if (!child.state.IsTerminal())
                    {
                        float score = Rollout(child);
                        child.score += score;
                        child.n += 1;

                        Backpropagation(child.parent, score);
                    }
                    else
                        child.n += 1;
                }

                if (recommendedChild == null) bestChild = currentRollingNode.GetBestChild();
                else break;

                if (bestChild.children.Count == 0)
                {
                    if (bestChild.height < pieces.Count)
                    {
                        bestChild.ExtendNode(new PieceModel(pieces[bestChild.height]));
                        RolloutOneRandomChild(bestChild);
                    }
                    currentRollingNode = currentNode;
                }
                else
                {
                    currentRollingNode = bestChild;
                }

                yield return null;
            }

            if (recommendedChild == null) recommendedChild = GetRecommendedChild(currentNode);

            currentNode = recommendedChild;
            currentTetrisState.DoAction(nextPiece, recommendedChild.action);

            TetrisBoardController.Instance.DoActionByBot(recommendedChild.action);*/
        }
    }

    protected PieceAction CheckTetris(PieceModel nextPiece, TetrisState currentTetrisState, List<PieceAction> possibleActions, float budget)
    {
        int i = 0;
        while(Time.time - t0 < budget && i < possibleActions.Count)
        {
            PieceAction action = possibleActions[i];

            TetrisState newState = currentTetrisState.CloneState();

            newState.DoAction(nextPiece, action);
            nextPiece.ResetCoordinates();

            if (newState.IsTetris()) return action;

            i++;
        }
        return null;
    }
}
