using UnityEngine;
using System.Linq;


using System.Collections.Generic; //pour pouvoir utiliser List dans GetAllPotentialMoves
//public GameObject moveHighlightPrefab; // Prefab pour highlight les mouvements possibles // a faire

public class RookController : PieceController // Hérite de PieceController
{
    public float moveSpeed = 5f; // Vitesse de déplacement
    private Vector3 targetPosition; // Position de destination
    private bool isMoving = false; // Si la tour est en train de se déplacer
    private PieceController pieceToCapture = null; // Pièce à capturer


    private void Update()
    {
        // Si la tour est sélectionnée, on permet de la déplacer
        if (isSelected && !isMoving)
        {
            HandleMouseClick();
        }

        // Si la tour est en mouvement, on la déplace
        if (isMoving)
        {
            MoveRook();
        }
    }

    // Gère le clic souris pour déplacer la tour
    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0)) // Si clic gauche
        {
            //Debug.Log("Clic détecté sur une pièce !");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Lancer un raycast depuis la position de la souris
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Vérifie si on clique sur une case ("Tiles")
                if (hit.collider.CompareTag("Tiles"))
                {
                    Vector3 hitPosition = hit.collider.transform.position;

                    // Vérifie si la case est une case valide pour la tour
                    if (GetAvailableMoves().Contains(hitPosition))
                    {
                        //Debug.Log("Position cliquée valide : " + hitPosition);
                        targetPosition = hitPosition; // Met à jour la position cible
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
        //Debug.Log(gameObject.name + " calcule ses mouvements...");//regarde si les pieces calculent vraiment les mouvements disponibles

        List<Vector3> moves = new List<Vector3>();
        ChessBoardGenerator board = GameObject.FindObjectOfType<ChessBoardGenerator>();
        (int x, int z) = GetBoardIndex();

        // 4 directions : haut, bas, gauche, droite
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        foreach (var dir in directions)
        {
            int step = 1;
            while (true)
            {
                int newX = x + dir.x * step;
                int newZ = z + dir.y * step;
                if (!board.IsInBoard(newX, newZ)) break;

                Vector3 newPos = BoardToWorldPosition(newX, newZ);//s'assure qu'il prends les bonnes coordonnées

                if (IsOccupiedByAlly(newPos)) break;

                moves.Add(newPos);
                // Si une pièce ennemie est sur la case, on ajoute et on s’arrête
                Collider[] colliders = Physics.OverlapSphere(newPos, 0.3f);
                if (colliders.Any(c => c.GetComponent<PieceController>() is PieceController pc && pc.isPlayerWhite != this.isPlayerWhite))
                {
                    break;
                }

                step++;
            }
        }
        //Debug.Log("Mouvements possibles pour la tour : " + moves.Count);
        return moves;
    }


    // Déplace la tour vers la position cible
    void MoveRook()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Si la tour atteint la position cible, arrêter le mouvement
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            Debug.Log(gameObject.name + " a atteint sa destination.");

            // Si la case d'arrivée est occupée par une pièce ennemie, la capturer
            Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.3f);
            foreach (Collider collider in colliders)
            {
                PieceController piece = collider.GetComponent<PieceController>();
                if (piece != null && piece.isPlayerWhite != this.isPlayerWhite)
                {
                    // Capture la pièce ennemie
                    pieceToCapture = piece;
                    Debug.Log("Pièce ennemie détectée pour capture : " + piece.gameObject.name);
                    Destroy(piece.gameObject);
                    break;
                }
            }
            DeselectCase();
            GameManager.SwitchTurn(); // Passer au joueur suivant
        }
    }


}
