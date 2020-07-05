using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class node for MCTreeSearch creation. It has a piece, an action (that the piece would play) and a state (where the piece action has just been played)
/// It also has a score (sum of the scores obtained doing rollouts), some children and one parent
/// </summary>
public class MCTSNode
{
    public int id;
    public MCTSNode parent;
    public List<MCTSNode> children;
    public TetrisState state;
    public PieceAction action;
    public PieceModel currentPiece;

    public float score;
    public int n;
    public float C = Mathf.Sqrt(2);

    public int height; //Identifies in which height of the tree search is

    public MCTSNode(int id, MCTSNode parent, TetrisState state, PieceAction action, PieceModel currentPiece)
    {
        this.id = id;
        this.parent = parent;
        this.state = state;
        this.action = action;
        this.currentPiece = currentPiece;

        children = new List<MCTSNode>();

        if (parent != null) height = parent.height + 1;
        else height = 0;

        MCTreeSearch.nNodes++;
        if(height > MCTreeSearch.currentHeight) MCTreeSearch.currentHeight = height;
    }

    /// <summary>
    /// Returns the best of the childs acording to the UCT formula, that tries to balance between exploration and explotation
    /// </summary>
    /// <returns></returns>
    public MCTSNode GetBestChild()
    {
        MCTSNode bestChild = null;
        float bestUCB = -Mathf.Infinity;

        foreach(MCTSNode child in children)
        {
            float childUCB = 0;
            if(child.n == 0)
            {
                childUCB = Mathf.Infinity;
            }
            else
            {
                childUCB = child.score / child.n + C * Mathf.Sqrt(Mathf.Log(n) / child.n);
            }

            if(childUCB >= bestUCB)
            {
                bestChild = child;
                bestUCB = childUCB;
            }
        }

        return bestChild;
    }

    /// <summary>
    /// Creates the children of this node with a given new piece
    /// </summary>
    /// <param name="newCurrentPiece"></param>
    public void ExtendNode(PieceModel newCurrentPiece)
    {
        List<PieceAction> actions = state.GetActions(newCurrentPiece);

        foreach(PieceAction action in actions) //Each child is related with one of the possible actions for the newCurrentPiece played in the state of this node
        {
            TetrisState newState = state.CloneState();
            newState.DoAction(newCurrentPiece, action);

            newCurrentPiece.ResetCoordinates();

            MCTSNode newNode = new MCTSNode(MCTreeSearch.nNodes, this, newState, action, newCurrentPiece);
            children.Add(newNode);
        }
    }

    public override string ToString()
    {
        string result = "";

        result += "ID: " + id + "\n";
        result += "Parent ID: " + ((parent == null) ? "null" : parent.id.ToString()) + "\n";
        result += "Height: " + height + "\n";
        result += "Children: " + children.Count + "\n";
        result += "---------------------------------------------------------\n";
        result += "State: \n";
        result += state.ToString();
        result += "Current piece: " + (currentPiece != null ? currentPiece.pieceType.ToString() : "null") + "\n";
        result += "---------------------------------------------------------\n";
        result += "Score: " + score + "\n";
        result += "N: " + n + "\n";

        return result;
    }
}
