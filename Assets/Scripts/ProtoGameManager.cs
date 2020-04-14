using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoGameManager : MonoBehaviour
{
    PieceBehaviour currentPiece;
    public GameObject piecePrefab;
    public Material material;

    public Vector2Int spawnPosition = new Vector2Int(4, 21);

    private bool softDrop = false;
    private float dropCounter;
    public float gravityDropLimit = 0.5f;
    public float softDropLimit = 0.1f;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G) && currentPiece == null)
        {
            PieceType pieceType = (PieceType)Random.Range(0, 7);
            currentPiece = Instantiate(piecePrefab).GetComponent<PieceBehaviour>();
            currentPiece.spawnLocation = spawnPosition;

            currentPiece.SpawnPiece(pieceType, material);
        }
        
        if(currentPiece != null)
        {
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentPiece.RotatePiece(true, true);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                currentPiece.RotatePiece(false, true);
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentPiece.MovePiece(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentPiece.MovePiece(Vector2Int.right);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentPiece.DropPiece(true);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    softDrop = true;
                    dropCounter = softDropLimit;
                }
                else if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    softDrop = false;
                    dropCounter = 0.0f;
                }

                dropCounter += Time.deltaTime;
                if ((softDrop && dropCounter >= softDropLimit) || (!softDrop && dropCounter > gravityDropLimit))
                {
                    dropCounter = 0.0f;
                    currentPiece.DropPiece();
                }
            }
        }
    }
}
