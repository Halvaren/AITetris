using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The random system of the modern Tetris uses a bag which contains the 7 type of pieces in the game, shuffled, and the game extracts this pieces in order.
/// When a bag is empty, there is another bag, also shuffled, and so on. In the actual random system, there are some restrictions, but they have been removed
/// in order to keep this aspect simple
/// </summary>
public class TetrisRandomGenerator : MonoBehaviour
{
    private List<int> currentBag; //Bag where the game is picking piece types
    private List<int> nextBag; //Auxiliar bag to avoid charging times when the current bag is empty

    private int nextPiece; //Points to the next piece from the current bag that is going to be spawned

    public Transform nextPiecesDisplayer; //GameObject where are going to be displayed the next pieces

    private System.Random rnd;
    public int randomSeed = 42;

    private static TetrisRandomGenerator instance;
    public static TetrisRandomGenerator Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;

        currentBag = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        nextBag = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        rnd = new System.Random(randomSeed); //The random seed is set in order to make a better testing and debugging

        nextPiece = 0;

        HideAllNextPieces();
    }

    /// <summary>
    /// Shuffles all the elements of a given list
    /// </summary>
    /// <param name="bag"></param>
    public void ShuffleBag(List<int> bag)
    {
        nextPiece = 0;

        for (int i = bag.Count - 1; i > 0; i--)
        {
            int randomIndex = rnd.Next(0, i);
            int tmp = bag[i];
            bag[i] = bag[randomIndex];
            bag[randomIndex] = tmp;
        }
    }

    /// <summary>
    /// Initializes both bags
    /// </summary>
    public void ShuffleBags()
    {
        nextPiece = 0;

        ShuffleBag(currentBag);
        ShuffleBag(nextBag);
    }

    /// <summary>
    /// Changes the elements of the current bag with the elements of the next bag and shuffles the next bag
    /// </summary>
    void ChangeBag()
    {
        nextPiece = 0;
        for (int i = 0; i < currentBag.Count; i++)
        {
            currentBag[i] = nextBag[i];
        }
        ShuffleBag(nextBag);
    }

    /// <summary>
    /// Returns the next piece of the current bag. It changes the bags in case the current bag is empty
    /// </summary>
    /// <returns></returns>
    public PieceType GetNextPiece()
    {
        PieceType result = (PieceType) currentBag[nextPiece++];

        if (nextPiece == 7)
            ChangeBag();

        ShowNextPieces();

        return result;
    }

    /// <summary>
    /// This method is used by the MCTSTetrisBot and HumanizedTetrisBot, to know the last piece shown in the nextPiecesDisplayer
    /// </summary>
    /// <returns></returns>
    public PieceType GetLastNextPiece()
    {
        PieceType result;

        if (nextPiece - 1 + nextPiecesDisplayer.childCount < currentBag.Count) result = (PieceType)currentBag[nextPiece - 1 + nextPiecesDisplayer.childCount];
        else result = (PieceType)nextBag[(nextPiece - 1 + nextPiecesDisplayer.childCount) - currentBag.Count];

        return result;
    }

    /// <summary>
    /// This method is used by the MCTSTetrisBot and HumanizedTetrisBot, to know the pieces shown in the nextPiecesDisplayer
    /// </summary>
    /// <returns></returns>
    public PieceType[] GetNextPieces()
    {
        PieceType[] result = new PieceType[nextPiecesDisplayer.childCount + 1];

        int i = 0;
        int j = 0;
        while (i < result.Length)
        {
            if((nextPiece - 1) + i < currentBag.Count)
            {
                result[i] = (PieceType) currentBag[(nextPiece - 1) + i];
                i++;
            }
            else
            {
                result[i] = (PieceType) nextBag[j];
                j++;
                i++;
            }
        }

        return result;
    }

    /// <summary>
    /// Displays the pieces in the nextPiecesDisplayer
    /// </summary>
    private void ShowNextPieces()
    {
        HideAllNextPieces();

        int i = 0;
        int j = 0;
        while (i < nextPiecesDisplayer.childCount)
        {
            if(nextPiece + i < currentBag.Count)
            {
                nextPiecesDisplayer.GetChild(i).GetChild((int)currentBag[nextPiece + i]).gameObject.SetActive(true);
                i++;
            }
            else
            {
                nextPiecesDisplayer.GetChild(i).GetChild((int)nextBag[j]).gameObject.SetActive(true);
                j++;
                i++;
            }
        }
    }

    /// <summary>
    /// Hide the pieces of the nextPiecesDisplayer
    /// </summary>
    private void HideAllNextPieces()
    {
        foreach (Transform nextPieceDisplayer in nextPiecesDisplayer)
        {
            foreach (Transform nextPiece in nextPieceDisplayer)
                nextPiece.gameObject.SetActive(false);
        }
    }
}
