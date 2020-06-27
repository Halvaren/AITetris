using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class TetrisRandomGenerator : MonoBehaviour
{
    private List<int> currentBag;
    private List<int> nextBag;

    private int nextPiece;

    private int lastIPosition;
    private PieceType lastPieceInPreviousBag;
    public int maxPiecesBetweenIs;

    public Transform nextPiecesDisplayer;

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
        Random.InitState(randomSeed);

        nextPiece = 0;

        lastIPosition = -1;

        HideAllNextPieces();
    }

    public void ShuffleBag(List<int> bag)
    {
        nextPiece = 0;

        for (int i = bag.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i);
            int tmp = bag[i];
            bag[i] = bag[randomIndex];
            bag[randomIndex] = tmp;
        }
    }

    public void ShuffleBags()
    {
        nextPiece = 0;

        ShuffleBag(currentBag);
        ShuffleBag(nextBag);
    }

    IEnumerator FillBag(ArrayList bag)
    {
        bag.Clear();

        bool IPieceInBag = false;
        while(bag.Count < 7)
        {
            PieceType newPiece;
            if(lastIPosition != -1 && !IPieceInBag && ((7 - lastIPosition) + bag.Count) >= maxPiecesBetweenIs)
            {
                newPiece = PieceType.I;
            }
            else
            {
                newPiece = (PieceType) Random.Range(0, 7); //, 1);
            }
            //bag.Add(newPiece);


            if (!bag.Contains(newPiece) && (bag.Count > 0 || newPiece != lastPieceInPreviousBag))
            {
                if(newPiece == PieceType.I)
                {
                    IPieceInBag = true;
                    lastIPosition = currentBag.Count;
                }
                bag.Add(newPiece);

                if (bag.Count == 7) lastPieceInPreviousBag = newPiece;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    void ChangeBag()
    {
        nextPiece = 0;
        for (int i = 0; i < currentBag.Count; i++)
        {
            currentBag[i] = nextBag[i];
        }
        ShuffleBag(nextBag);
    }

    public PieceType GetNextPiece()
    {
        PieceType result = (PieceType) currentBag[nextPiece++];

        if (nextPiece == 7)
            ChangeBag();

        ShowNextPieces();

        return result;
    }

    public PieceType GetLastNextPiece()
    {
        PieceType result;

        if (nextPiece - 1 + nextPiecesDisplayer.childCount < currentBag.Count) result = (PieceType)currentBag[nextPiece - 1 + nextPiecesDisplayer.childCount];
        else result = (PieceType)nextBag[(nextPiece - 1 + nextPiecesDisplayer.childCount) - currentBag.Count];

        return result;
    }

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

    private void HideAllNextPieces()
    {
        foreach (Transform nextPieceDisplayer in nextPiecesDisplayer)
        {
            foreach (Transform nextPiece in nextPieceDisplayer)
                nextPiece.gameObject.SetActive(false);
        }
    }

    public bool GetBagsPrepared()
    {
        return currentBag.Count == 7 && nextBag.Count == 7;
    }
}
