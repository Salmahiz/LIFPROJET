using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class QueenController : PieceController
{
    public float moveSpeed = 5f; 
    private Vector3 targetPosition;
    private bool isMoving = false; 
    private PieceController pieceToCapture = null;

    private void Update()
    {
        if (isSelected && !isMoving)
        {
            HandleMouseClick();
        }

        if (isMoving)
        {
            MoveQueen();
        }
    }

    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Tiles"))
                {
                    Vector3 hitPosition = hit.collider.transform.position;

                    if (GetAvailableMoves().Any(pos => Vector3.Distance(pos, hitPosition) < 0.1f))
                    {
                        targetPosition = hitPosition; 
                        isMoving = true; 
                    }
                    else
                    {
                        Debug.Log("Déplacement invalide.");
                    }
                }
            }
        }
    }

    public override List<Vector3> GetAvailableMoves()
    {
        List<Vector3> moves = new List<Vector3>();
        ChessBoardGenerator board = GameObject.FindObjectOfType<ChessBoardGenerator>();
        (int x, int z) = GetBoardIndex();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1),
            new Vector2Int(1, -1), new Vector2Int(-1, -1)
        };

        foreach (var dir in directions)
        {
            int step = 1;
            while (true)
            {
                int newX = x + dir.x * step;
                int newZ = z + dir.y * step;

                if (!board.IsInBoard(newX, newZ)) break;

                Vector3 newPos = BoardToWorldPosition(newX, newZ);

                if (IsOccupiedByAlly(newPos)) break;

                moves.Add(newPos);

                // Si une pièce ennemie est présente, on peut capturer mais on arrête là
                Collider[] colliders = Physics.OverlapSphere(newPos, 0.3f);
                if (colliders.Any(c =>
                    c.GetComponent<PieceController>() is PieceController pc &&
                    pc.isPlayerWhite != this.isPlayerWhite))
                {
                    break;
                }

                step++;
            }
        }

        return moves;
    }

    void MoveQueen()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            Debug.Log(gameObject.name + " a atteint sa destination.");

            Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.3f);
            foreach (Collider collider in colliders)
            {
                PieceController piece = collider.GetComponent<PieceController>();
                if (piece != null && piece.isPlayerWhite != this.isPlayerWhite)
                {
                    pieceToCapture = piece;
                    Debug.Log("Pièce ennemie capturée : " + piece.gameObject.name);
                    Destroy(piece.gameObject);
                    break;
                }
            }

            DeselectCase();
            GameManager.SwitchTurn();
        }
    }


}
