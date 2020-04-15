using System.Collections;
using UnityEngine;

public class TetrisRandomGenerator : MonoBehaviour
{
    private ArrayList currentBag;
    private ArrayList nextBag;

    private int nextPiece;

    private int lastIPosition;
    private PieceType lastPieceInPreviousBag;
    public int maxPiecesBetweenIs;

    public Transform nextPiecesDisplayer;

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

        currentBag = new ArrayList();
        nextBag = new ArrayList();

        nextPiece = 0;

        lastIPosition = -1;

        HideAllNextPieces();
    }

    public IEnumerator FillBags()
    {
        yield return StartCoroutine(FillBag(currentBag));
        StartCoroutine(FillBag(nextBag));
    }

    IEnumerator FillBag(ArrayList bag)
    {
        bag.Clear();

        bool IPieceInBag = false;
        while(bag.Count < 7)
        {
            PieceType newPiece;
            /*if(lastIPosition != -1 && !IPieceInBag && ((7 - lastIPosition) + bag.Count) >= maxPiecesBetweenIs)
            {
                newPiece = PieceType.I;
            }
            else
            {*/
            newPiece = Random.value > 0.5f ? PieceType.I : PieceType.O;//(PieceType) Random.Range(0, 1);
            //}
            bag.Add(newPiece);


            /*if (!bag.Contains(newPiece) && (bag.Count > 0 || newPiece != lastPieceInPreviousBag))
            {
                if(newPiece == PieceType.I)
                {
                    //IPieceInBag = true;
                    lastIPosition = currentBag.Count;
                }
                bag.Add(newPiece);

                if (bag.Count == 7) lastPieceInPreviousBag = newPiece;
            }*/

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
        StartCoroutine(FillBag(nextBag));
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

        if (nextPiece + nextPiecesDisplayer.childCount < currentBag.Count) result = (PieceType)currentBag[nextPiece + nextPiecesDisplayer.childCount];
        else result = (PieceType)nextBag[(nextPiece + nextPiecesDisplayer.childCount) - currentBag.Count];

        return result;
    }

    public PieceType[] GetNextPieces()
    {
        PieceType[] result = new PieceType[nextPiecesDisplayer.childCount + 1];

        int i = 0;
        int j = 0;
        while (i < result.Length)
        {
            if(nextPiece + i < currentBag.Count)
            {
                result[i] = (PieceType) currentBag[nextPiece + i];
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
