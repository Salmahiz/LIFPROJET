using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class KnightController : PieceController
{
    public float moveSpeed = 5f; // Vitesse de déplacement
    private Vector3 targetPosition; // Position de destination
    private bool isMoving = false; // Si le cavalier est en train de se déplacer
    private PieceController pieceToCapture = null; // Pièce à capturer

    private void Update()
    {
        // Si le cavalier est sélectionné, on permet de le déplacer
        if (isSelected && !isMoving)
        {
            HandleMouseClick();
        }

        // Si le cavalier est en mouvement, on le déplace
        if (isMoving)
        {
            MoveKnight();
        }
    }

    // Gère le clic souris pour déplacer le cavalier
    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0)) // Si clic gauche
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Lancer un raycast depuis la position de la souris
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Vérifie si on clique sur une case ("Tiles")
                if (hit.collider.CompareTag("Tiles"))
                {
                    Vector3 hitPosition = hit.collider.transform.position;

                    List<Vector3> possibleMoves = GetAvailableMoves();
                    Vector3 matchedMove = possibleMoves.FirstOrDefault(pos => Vector3.Distance(pos, hitPosition) < 0.1f);

                    bool moveFound = possibleMoves.Any(pos => Vector3.Distance(pos, hitPosition) < 0.1f);

                    // Vérifie si le mouvement est valide pour le cavalier
                    if (moveFound)
                    {
                        targetPosition = matchedMove; // Met à jour la position cible
                        isMoving = true; // Active le mouvement
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

        Vector2Int[] offsets = new Vector2Int[]
        {
            new Vector2Int(2, 1), new Vector2Int(2, -1),
            new Vector2Int(-2, 1), new Vector2Int(-2, -1),
            new Vector2Int(1, 2), new Vector2Int(-1, 2),
            new Vector2Int(1, -2), new Vector2Int(-1, -2)
        };

        foreach (var offset in offsets)
        {
            int newX = x + offset.x;
            int newZ = z + offset.y;

            if (!board.IsInBoard(newX, newZ)) continue;

            Vector3 newPos = BoardToWorldPosition(newX, newZ);

            if (!IsOccupiedByAlly(newPos))
            {
                moves.Add(newPos);
            }
        }

        return moves;
    }


    // Déplace le cavalier vers la position cible
    void MoveKnight()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            Debug.Log(gameObject.name + " a atteint sa destination.");

            // Vérifie s'il y a une pièce ennemie à capturer
            Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.3f);
            foreach (Collider collider in colliders)
            {
                PieceController piece = collider.GetComponent<PieceController>();
                if (piece != null && piece.isPlayerWhite != this.isPlayerWhite)
                {
                    pieceToCapture = piece;
                    Debug.Log("Pièce ennemie détectée pour capture : " + piece.gameObject.name);
                    Destroy(piece.gameObject); // Capture la pièce
                    break;
                }
            }

            DeselectCase();
            GameManager.SwitchTurn();
        }
    }
}
