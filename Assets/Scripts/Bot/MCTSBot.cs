using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCTSBot : TetrisBot
{
    int piecesPerRollout;

    MCTreeSearch treeSearch;
    MCTSNode currentNode;

    public float holesWeight;
    public float bumpinessWeight;
    public float linesWeight;

    int rollouts = 0;

    public MCTSBot(PieceType[] initialPieces, float budget, int piecesPerRollout) : base(initialPieces)
    {
        this.piecesPerRollout = piecesPerRollout;

        nextPieces = new List<PieceModel>();
        for(int i = 0; i < initialPieces.Length; i++)
        {
            nextPieces.Add(new PieceModel(initialPieces[i]));
        }
        CreateTree(nextPieces.ToArray(), budget);
    }

    public void SetWeights(float holesWeight, float bumpinessWeight, float linesWeight)
    {
        this.holesWeight = holesWeight;
        this.bumpinessWeight = bumpinessWeight;
        this.linesWeight = linesWeight;
    }

    private void CreateTree(PieceModel[] initialPieces, float budget)
    {
        treeSearch = new MCTreeSearch(actualTetrisState, budget * 0.5f, initialPieces);
        currentNode = treeSearch.rootNode;
    }

    public void AddNewPiece(PieceType newPieceType)
    {
        nextPieces.RemoveAt(0);
        PieceModel newPiece = new PieceModel(newPieceType);

        nextPieces.Add(newPiece);
        treeSearch.AddPiece(currentNode, newPiece);
    }

    public void Act(PieceType nextPieceType, float budget)
    {
        TetrisBoardController.Instance.StartCoroutine(ActCoroutine(nextPieceType, budget));
    }

    private IEnumerator ActCoroutine(PieceType nextPieceType, float budget)
    {
        rollouts = 0;
        float t0 = Time.time;

        PieceModel nextPiece = new PieceModel(nextPieceType);

        MCTSNode currentRollingNode = currentNode;
        if (currentRollingNode.children.Count == 0)
        {
            currentRollingNode.ExtendNode(nextPiece, null);
        }

        while (Time.time - t0 < budget)
        {
            float t1 = Time.time;
            
            foreach(MCTSNode child in currentRollingNode.children)
            {
                float score = Rollout(child);
                child.score += score;
                child.n += 1;

                Backpropagation(child.parent, score);
                currentRollingNode = currentNode;
            }

            MCTSNode bestChild = currentRollingNode.GetBestChild();
            
            if (bestChild.children.Count == 0)
            {
                if (bestChild.nextPiece != null)
                {
                    bestChild.ExtendNode(bestChild.nextPiece, null);
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
        actualTetrisState.DoAction(nextPiece, recommendedChild.action);

        Debug.Log("Rollouts: " + rollouts);

        TetrisBoardController.Instance.DoActionByBot(recommendedChild.action);
    }

    public MCTSNode GetRecommendedChild(MCTSNode rootNode)
    {
        MCTSNode bestChild = null;
        float bestScore = -Mathf.Infinity;

        foreach (MCTSNode child in rootNode.children)
        {
            float score = 0;
            if (child.n != 0) { score = child.score / child.n; /*Debug.Log(score);*/ }

            if(score >= bestScore)
            {
                bestScore = score;
                bestChild = child;
            }
        }

        return bestChild;
    }

    private float GetScore(TetrisState state)
    {
        return -(holesWeight * state.GetHoleCount()) - (bumpinessWeight * state.GetBumpiness()) + (linesWeight * TetrisBoardController.Instance.possibleScores[state.GetClearedLines()]);
    }

    public void Backpropagation(MCTSNode node, float score)
    {
        while(node != null)
        {
            node.n += 1;
            node.score += score;
            node = node.parent;
        }
    }

    private float Rollout(MCTSNode node)
    {
        TetrisState newState = node.state.CloneState();

        int nPieces = 0;
        while(nPieces < piecesPerRollout && !newState.IsTerminal(node.currentPiece))
        {
            PieceModel piece;
            if (nPieces < nextPieces.Count)
                piece = nextPieces[nPieces];
            else
                piece = new PieceModel((PieceType) Random.Range(0, 6));

            newState.DoAction(piece, newState.GetRandomAction(piece));
            nPieces++;
        }

        float score = GetScore(newState);

        /*Debug.Log(newState.ToString());
        Debug.Log("Score: " + score);*/

        rollouts++;

        return score;
    }

    private void RolloutOneRandomChild(MCTSNode node)
    {
        int oneChildIndex = Random.Range(0, node.children.Count);
        MCTSNode oneChild = node.children[oneChildIndex];

        float score = Rollout(oneChild);
        oneChild.score += score;
        oneChild.n += 1;

        Backpropagation(node, oneChild.score);
    }
}
