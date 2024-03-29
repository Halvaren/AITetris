﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The MCTreeSearch is used by the MCTSTetrisBot in order to "look to the future". Having knowledge about what pieces are coming after the current one, 
/// each node of the tree search represents a possible situation after playing these pieces in different combinations, making possible to plan the next 
/// immediate action thinking about what is better for the near future.
/// </summary>
public class MCTreeSearch
{
    private MCTSTetrisBot bot;
    public MCTSNode rootNode;
    public static int nNodes = 0;
    public static int currentHeight = 0;

    private TetrisBoardController tbController;
    public TetrisBoardController TBController
    {
        get
        {
            if (tbController == null) tbController = TetrisBoardController.Instance;
            return tbController;
        }
    }

    public MCTreeSearch(MCTSTetrisBot bot, TetrisState state, float budget, PieceModel[] initialPieces)
    {
        this.bot = bot;
        TBController.StartCoroutine(CreateTree(state, budget, initialPieces));
    }

    /// <summary>
    /// With a giving budget of time and the initial pieces of the game, the tree is created
    /// </summary>
    /// <param name="state"></param>
    /// <param name="budget"></param>
    /// <param name="initialPieces"></param>
    /// <returns></returns>
    private IEnumerator CreateTree(TetrisState state, float budget, PieceModel[] initialPieces)
    {
        rootNode = new MCTSNode(nNodes, null, state, null, null);

        Queue<MCTSNode> queue = new Queue<MCTSNode>();
        queue.Enqueue(rootNode); //Tree is created following the same strategy as BFS: creating first all the nodes for a determined height, and then start with next level

        while (bot.t0 < budget)
        {
            if(!TBController.pausedGame)
            {
                bot.t0 += Time.deltaTime;

                MCTSNode currentNode = queue.Dequeue();

                if (currentNode.height >= initialPieces.Length) break; //As the next pieces after initial pieces are unknown, it's not possible to create nodes for any other pieces than the initial ones

                currentNode.ExtendNode(initialPieces[currentNode.height]);

                if (currentNode.children.Count > 0)
                    foreach (MCTSNode child in currentNode.children)
                        queue.Enqueue(child);
            }

            yield return null;
        }

        //PrintTree(rootNode);
    }

    /// <summary>
    /// Debug method to print the tree in a log file
    /// </summary>
    /// <param name="rootNode"></param>
    private void PrintTree(MCTSNode rootNode)
    {
        LogWriter.InitializeMCTreeSearchLog();

        LogWriter.WriteMCTreeSearch("Number of nodes: " + nNodes);
        LogWriter.WriteMCTreeSearch("Current height: " + currentHeight);

        Queue<MCTSNode> queue = new Queue<MCTSNode>();
        queue.Enqueue(rootNode);

        while (queue.Count > 0)
        {
            MCTSNode currentNode = queue.Dequeue();

            LogWriter.WriteMCTreeSearch(currentNode.ToString());

            if (currentNode.children.Count > 0)
                foreach (MCTSNode child in currentNode.children)
                    queue.Enqueue(child);
        }
    }
}
