using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanizedTetrisBot : MCTSTetrisBot
{
    public HumanizedTetrisBot(PieceType[] initialPieces, float budget) : base(initialPieces, budget)
    {
        pieces = new List<PieceType>(initialPieces);

        nextPieces = new List<PieceModel>();
        for (int i = 0; i < initialPieces.Length; i++)
        {
            nextPieces.Add(new PieceModel(initialPieces[i]));
        }
        CreateTree(nextPieces.ToArray(), budget);
    }

    public override IEnumerator ActCoroutine(PieceType nextPieceType, float budget)
    {
        //Debug.Log(nextPieceType);
        rollouts = 0;

        PieceModel nextPiece = new PieceModel(nextPieceType);

        //yield return TetrisBoardController.Instance.StartCoroutine(TetrisBoardController.Instance.ShowPossibleActionsCoroutine(currentNode.state.GetActions(nextPiece)));

        float t0 = Time.time;

        MCTSNode currentRollingNode = currentNode;
        if (currentRollingNode.children.Count == 0)
        {
            currentRollingNode.ExtendNode(nextPiece);
        }

        while (Time.time - t0 < budget)
        {
            MCTSNode bestChild = null;
            foreach (MCTSNode child in currentRollingNode.children)
            {
                if (child.state.IsTetris())
                {
                    Debug.Log("BOOM!! Tetris for Bot");
                    bestChild = child;
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

            if(bestChild == null) bestChild = currentRollingNode.GetBestChild();

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

        MCTSNode recommendedChild = GetRecommendedChild(currentNode);

        currentNode = recommendedChild;
        currentTetrisState.DoAction(nextPiece, recommendedChild.action);

        //Debug.Log("Rollouts: " + rollouts);

        TetrisBoardController.Instance.DoActionByBot(recommendedChild.action);
    }

    protected override float Rollout(MCTSNode node)
    {
        TetrisState newState = node.state.CloneState();

        int nPieces = 0;

        float totalScore = node.state.GetHumanizedScore();
        float weight = 1f;
        float totalWeight = weight;

        while (nPieces < nextPieces.Count && !newState.IsTerminal())
        {
            weight *= rolloutScoreWeightReduction;
            totalWeight += weight;

            PieceModel piece;
            piece = nextPieces[nPieces];

            newState.DoAction(piece, newState.GetRandomAction(piece));
            nPieces++;

            totalScore += newState.GetHumanizedScore() * weight;
        }

        float score = totalScore / totalWeight;

        rollouts++;

        return score;
    }
}
