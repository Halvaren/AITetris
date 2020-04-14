﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCTreeSearch
{
    public MCTSNode rootNode;
    public static int nNodes = 0;
    public static int currentHeight = 0;

    public MCTreeSearch(TetrisState state, float budget, PieceModel[] initialPieces)
    {
        TetrisBoardController.Instance.StartCoroutine(CreateTree(state, budget, initialPieces));
    }

    private IEnumerator CreateTree(TetrisState state, float budget, PieceModel[] initialPieces)
    {
        float t0 = Time.time;

        rootNode = new MCTSNode(nNodes, null, state, null, null, initialPieces[0]);

        Queue<MCTSNode> queue = new Queue<MCTSNode>();
        queue.Enqueue(rootNode);

        while (Time.time - t0 < budget)
        {
            MCTSNode currentNode = queue.Dequeue();

            if (currentNode.height >= initialPieces.Length) break;

            currentNode.ExtendNode(initialPieces[currentNode.height], currentNode.height + 1 >= initialPieces.Length ? null : initialPieces[currentNode.height + 1]);

            if (currentNode.children.Count > 0)
                foreach (MCTSNode child in currentNode.children)
                    queue.Enqueue(child);

            yield return null;
        }

        //PrintTree(rootNode);
    }

    private void PrintTree(MCTSNode rootNode)
    {
        Debug.Log("Number of nodes: " + nNodes);
        Debug.Log("Current height: " + currentHeight);

        Queue<MCTSNode> queue = new Queue<MCTSNode>();
        queue.Enqueue(rootNode);

        while (queue.Count > 0)
        {
            MCTSNode currentNode = queue.Dequeue();

            Debug.Log(currentNode.ToString());

            if (currentNode.children.Count > 0)
                foreach (MCTSNode child in currentNode.children)
                    queue.Enqueue(child);
        }
    }

    public void AddPiece(MCTSNode initialNode, PieceModel newPiece)
    {
        Queue<MCTSNode> queue = new Queue<MCTSNode>();
        queue.Enqueue(initialNode);

        while (queue.Count > 0)
        {
            MCTSNode currentNode = queue.Dequeue();
            //if (currentNode.height == currentHeight - 1) currentNode.nextPiece = newPiece;

            if (currentNode.children.Count > 0)
                foreach (MCTSNode child in currentNode.children)
                {
                    queue.Enqueue(child);
                }

            else
            {
                currentNode.ExtendNode(newPiece, null);
            }
        }

        //PrintTree(rootNode);
    }
}