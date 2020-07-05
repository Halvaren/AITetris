using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This bot uses the MonteCarlo Tree Search algorithm to find the best action for the current piece in the current state "looking to the near future"
/// </summary>
public class MCTSTetrisBot : TetrisBot
{
    protected MCTreeSearch treeSearch;
    protected MCTSNode currentNode;

    protected List<PieceType> pieces;

    protected int rollouts = 0;

    //Looking to the giving results, I see that it was "unfair" to give the same importance to all the scores got from a rollout (the score of playing the
    //current piece, the first next piece, the second next piece...), because those next pieces are important but not such important as the current piece is
    //So, with this value, the given score by playing the next pieces is reduced
    protected float rolloutScoreWeightReduction = 0.5f;  

    /// <summary>
    /// The MCTSTetrisBot is created giving it some initialPieces and a budget of time, and it only creates the tree
    /// </summary>
    /// <param name="initialPieces"></param>
    /// <param name="budget"></param>
    public MCTSTetrisBot(PieceType[] initialPieces, float budget)
    {
        pieces = new List<PieceType>(initialPieces);

        PieceModel[] nextPieces = new PieceModel[initialPieces.Length];
        for(int i = 0; i < initialPieces.Length; i++)
        {
            nextPieces[i] = new PieceModel(initialPieces[i]);
        }
        CreateTree(nextPieces, budget);
    }

    /// <summary>
    /// Creates the tree
    /// </summary>
    /// <param name="initialPieces"></param>
    /// <param name="budget"></param>
    protected void CreateTree(PieceModel[] initialPieces, float budget)
    {
        treeSearch = new MCTreeSearch(currentTetrisState, budget, initialPieces);
        currentNode = treeSearch.rootNode;
    }

    /// <summary>
    /// When a piece is lock in the real board, the bot receives the new next piece shown in the nextPiecesDisplayer
    /// </summary>
    /// <param name="newPieceType"></param>
    public void AddNewPiece(PieceType newPieceType)
    {
        pieces.Add(newPieceType);
    }

    /// <summary>
    /// Main method of the bot. While the time budget is not spent, the algorithm makes rollouts over the different nodes of the tree, choosing the best child
    /// of the current root node, and finally, chooses the best action for the current piece in the current state
    /// </summary>
    /// <param name="nextPieceType"></param>
    /// <param name="budget"></param>
    /// <returns></returns>
    public override IEnumerator ActCoroutine(PieceType nextPieceType, float budget)
    {
        rollouts = 0;

        PieceModel nextPiece = new PieceModel(nextPieceType);

        //Debug
        //yield return TetrisBoardController.Instance.StartCoroutine(TetrisBoardController.Instance.ShowPossibleActionsCoroutine(currentNode.state.GetActions(nextPiece)));

        float t0 = Time.time;

        MCTSNode currentRollingNode = currentNode;

        //if the currentRollingNode doesn't have children at the beginning of the method, that means that its child are the result of playing the nextPiece in
        //the state of the currentRollingNode
        if (currentRollingNode.children.Count == 0) 
        {
            currentRollingNode.ExtendNode(nextPiece);
        }

        //While there is still time
        while (Time.time - t0 < budget)
        {            
            //For each child, a rollout is made
            foreach(MCTSNode child in currentRollingNode.children)
            {
                //If the state is not terminal, the rollout is made, and the score stored and backpropagated
                if(!child.state.IsTerminal())
                {
                    float score = Rollout(child);
                    child.score += score;
                    child.n += 1;

                    Backpropagation(child.parent, score);
                }
                //If it's terminal, it store like a rollout was made in order to not choose this child when the best one is searched
                else
                    child.n += 1;
            }

            //After the rollouts, the best child is chosen
            MCTSNode bestChild = currentRollingNode.GetBestChild();
            
            //If the best child it doesn't have children
            if (bestChild.children.Count == 0)
            {
                //If their children piece is known
                if(bestChild.height < pieces.Count)
                {
                    //The node will be extended
                    bestChild.ExtendNode(new PieceModel(pieces[bestChild.height]));
                    RolloutOneRandomChild(bestChild);
                }
                //And then, it goes back to the currentRootNode
                currentRollingNode = currentNode;
            }
            //Otherwise, the algorithm moves to that best child
            else
            {
                currentRollingNode = bestChild;
            }

            yield return null;
        }

        //When the loop ends, the recommended child is chosen from the children of the current root node
        MCTSNode recommendedChild = GetRecommendedChild(currentNode);

        currentNode = recommendedChild; //The current root node is updated
        currentTetrisState.DoAction(nextPiece, recommendedChild.action); //Its action is played in the currentState

        //Debug.Log("Rollouts: " + rollouts);

        TetrisBoardController.Instance.DoActionByBot(recommendedChild.action); //And it is played in the real board
    }

    /// <summary>
    /// Returns the child with the best score
    /// </summary>
    /// <param name="rootNode"></param>
    /// <returns></returns>
    protected MCTSNode GetRecommendedChild(MCTSNode rootNode)
    {
        MCTSNode bestChild = null;
        float bestScore = -Mathf.Infinity;

        foreach (MCTSNode child in rootNode.children)
        {
            float score = 0;
            if (child.n != 0) 
            {
                score = child.state.IsTerminal() ? -float.MaxValue : child.score / child.n;
            }

            if (score >= bestScore)
            {
                bestScore = score;
                bestChild = child;
            }
        }

        return bestChild;
    }

    /// <summary>
    /// Increases the score of the previous nodes
    /// </summary>
    /// <param name="node"></param>
    /// <param name="score"></param>
    protected void Backpropagation(MCTSNode node, float score)
    {
        while(node != null)
        {
            node.n += 1;
            node.score += score;
            node = node.parent;

            if (node == currentNode) break; 
            //If node is pointing to the same of currentNode, the loop gets broken because the previous nodes to currentNode will never be visited again
        }
    }

    /// <summary>
    /// Executes random actions with the next pieces over the state of node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected virtual float Rollout(MCTSNode node)
    {
        TetrisState newState = node.state.CloneState();

        int nPieces = 0;

        float totalScore = node.state.GetScore();
        float weight = 1f;
        float totalWeight = weight;

        //node.height identifies the height of the node in the MCTreeSearch, but also identifies the index of the piece inside the history of all the pieces played
        //So, if that node.height plus the number of pieces played in the rollout are bigger than the number of known pieces, then the rollout must stop.
        //Also it stops if an action has caused a game over
        while ((node.height + nPieces) < pieces.Count && !newState.IsTerminal())
        {
            weight *= rolloutScoreWeightReduction;
            totalWeight += weight;

            PieceModel piece;
            piece = new PieceModel(pieces[node.height + nPieces]);

            newState.DoAction(piece, newState.GetRandomAction(piece));
            nPieces++;

            totalScore += newState.GetScore() * weight;
        }
        
        float score = totalScore / totalWeight;

        rollouts++;

        return score;
    }

    /// <summary>
    /// Makes a rollout on a random child of node
    /// </summary>
    /// <param name="node"></param>
    protected void RolloutOneRandomChild(MCTSNode node)
    {
        int oneChildIndex = Random.Range(0, node.children.Count);
        MCTSNode oneChild = node.children[oneChildIndex];

        float score = Rollout(oneChild);
        oneChild.score += score;
        oneChild.n += 1;

        Backpropagation(node, oneChild.score);
    }
}
