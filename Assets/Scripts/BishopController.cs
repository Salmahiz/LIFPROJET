using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BishopController : PieceController
{
    public float moveSpeed = 5f; // Vitesse de déplacement
    private Vector3 targetPosition; // Position de destination
    private bool isMoving = false; // Si le fou est en train de se déplacer
    private PieceController pieceToCapture = null; // Pièce à capturer

    private void Update()
    {
        // Si le fou est sélectionné, on permet de le déplacer
        if (isSelected && !isMoving)
        {
            HandleMouseClick();
        }

        // Si le fou est en mouvement, on le déplace
        if (isMoving)
        {
            MoveBishop();
        }
    }

    // Gère le clic souris pour déplacer le fou
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

                    Vector3 matchedMove = GetAvailableMoves().FirstOrDefault(pos => Vector3.Distance(pos, hitPosition) < 0.1f);

                    bool moveFound = GetAvailableMoves().Any(pos => Vector3.Distance(pos, hitPosition) < 0.1f);

                    if (moveFound)
                    {
                        targetPosition = matchedMove;
                        isMoving = true;
                    }
                    else
                    {
                        //Debug.Log("Déplacement invalide.");
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




    void MoveBishop()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            //Debug.Log(gameObject.name + " a atteint sa destination.");

            Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.3f);

            // Vérifie s'il y a une pièce ennemie à capturer (après avoir atteint la destination)
            foreach (Collider collider in colliders)
            {
                PieceController piece = collider.GetComponent<PieceController>();
                if (piece != null && piece.isPlayerWhite != this.isPlayerWhite)
                {
                    pieceToCapture = piece;
                    //Debug.Log("Pièce ennemie capturée : " + piece.gameObject.name);
                    Destroy(piece.gameObject);
                    break;
                }
            }

            DeselectCase();
            GameManager.SwitchTurn();
        }
    }



}
