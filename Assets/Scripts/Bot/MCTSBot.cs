using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCTSBot : TetrisBot
{
    int piecesPerRollout;

    MCTreeSearch treeSearch;
    MCTSNode currentNode;

    private List<PieceType> pieces;

    int rollouts = 0;

    public MCTSBot(PieceType[] initialPieces, float budget, int piecesPerRollout) : base(initialPieces)
    {
        this.piecesPerRollout = piecesPerRollout;
        pieces = new List<PieceType>(initialPieces);

        nextPieces = new List<PieceModel>();
        for(int i = 0; i < initialPieces.Length; i++)
        {
            nextPieces.Add(new PieceModel(initialPieces[i]));
        }
        CreateTree(nextPieces.ToArray(), budget);
    }

    private void CreateTree(PieceModel[] initialPieces, float budget)
    {
        treeSearch = new MCTreeSearch(actualTetrisState, budget * 0.5f, initialPieces);
        currentNode = treeSearch.rootNode;
    }

    public void AddNewPiece(PieceType newPieceType)
    {
        pieces.Add(newPieceType);
    }

    public IEnumerator ActCoroutine(PieceType nextPieceType, float budget)
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
            foreach(MCTSNode child in currentRollingNode.children)
            {
                float score = Rollout(child);
                child.score += score;
                child.n += 1;

                Backpropagation(child.parent, score);
            }

            MCTSNode bestChild = currentRollingNode.GetBestChild();
            
            if (bestChild.children.Count == 0)
            {
                if(bestChild.height < pieces.Count)
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
        actualTetrisState.DoAction(nextPiece, recommendedChild.action);

        Debug.Log("Rollouts: " + rollouts);

        TetrisBoardController.Instance.DoActionByBot(recommendedChild.action);
    }

    public MCTSNode GetRecommendedChild(MCTSNode rootNode)
    {
        MCTSNode bestChild = null;
        float bestScore = -Mathf.Infinity;

        //Debug.Log("Scores ");

        foreach (MCTSNode child in rootNode.children)
        {
            float score = 0;
            if (child.n != 0) 
            { 
                score = child.score / child.n; 
                Debug.Log("Score " + score);
                Debug.Log("X " + child.action.xCoord + " Rotation " + child.action.rotationIndex);
            }
            //Debug.Log("Score " + child.score);
            //Debug.Log("X " + child.action.xCoord + " Rotation " + child.action.rotationIndex);

            if (score >= bestScore)
            {
                bestScore = score;
                bestChild = child;
            }
        }

        Debug.Log("Chosen child " + bestChild.currentPiece.pieceType);
        Debug.Log("Score " + bestScore);
        Debug.Log("X " + bestChild.action.xCoord + " Rotation " + bestChild.action.rotationIndex);

        return bestChild;
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

        while (nPieces < nextPieces.Count/*nPieces < piecesPerRollout && !newState.IsTerminal(node.currentPiece)*/)
        {
            PieceModel piece;
            //if (nPieces < nextPieces.Count)
                piece = nextPieces[nPieces];
            /*else
                piece = new PieceModel((PieceType) Random.Range(0, 6));*/

            newState.DoAction(piece, newState.GetRandomAction(piece), true);
            nPieces++;
        }

        float score = newState.GetScore();

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
