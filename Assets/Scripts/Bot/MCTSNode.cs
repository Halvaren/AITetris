using System.Collections.Generic;
using UnityEngine;

public class MCTSNode
{
    public int id;
    public MCTSNode parent;
    public List<MCTSNode> children;
    public TetrisState state;
    public PieceAction action;
    public PieceModel currentPiece;
    public PieceModel nextPiece;

    public float score;
    public int n;
    public float C = Mathf.Sqrt(2);

    public int height;

    public MCTSNode(int id, MCTSNode parent, TetrisState state, PieceAction action, PieceModel currentPiece, PieceModel nextPiece)
    {
        this.id = id;
        this.parent = parent;
        this.state = state;
        this.action = action;
        this.currentPiece = currentPiece;
        this.nextPiece = nextPiece;

        children = new List<MCTSNode>();

        if (parent != null) height = parent.height + 1;
        else height = 0;

        MCTreeSearch.nNodes++;
        if(height > MCTreeSearch.currentHeight) MCTreeSearch.currentHeight = height;
    }

    public MCTSNode GetBestChild()
    {
        MCTSNode bestChild = null;
        float bestUCB = -Mathf.Infinity;

        foreach(MCTSNode child in children)
        {
            float childUCB = 0;
            if(child.n == 0)
            {
                childUCB = Mathf.Infinity; //Posibilidad de cambiar con un epsilon, más aleatoriedad bla bla bla
            }
            else
            {
                childUCB = child.score / child.n + C * Mathf.Sqrt(Mathf.Log(n) / child.n); //Añadir epsilon
            }

            if(childUCB >= bestUCB)
            {
                bestChild = child;
                bestUCB = childUCB;
            }
        }

        return bestChild;
    }

    public void ExtendNode(PieceModel newCurrentPiece, PieceModel newNextPiece)
    {
        List<PieceAction> actions = state.GetActions(newCurrentPiece);

        foreach(PieceAction action in actions)
        {
            TetrisState newState = state.CloneState();
            newState.DoAction(newCurrentPiece, action);

            newCurrentPiece.ResetCoordinates();

            MCTSNode newNode = new MCTSNode(MCTreeSearch.nNodes, this, newState, action, newCurrentPiece, newNextPiece);
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
        result += "Next piece: " + (nextPiece != null ? nextPiece.pieceType.ToString() : "null") + "\n";
        result += "---------------------------------------------------------\n";
        result += "Score: " + score + "\n";
        result += "N: " + n + "\n";

        return result;
    }
}
