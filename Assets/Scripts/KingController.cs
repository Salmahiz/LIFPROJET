using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class KingController : PieceController
{
    public float moveSpeed = 5f; // Vitesse de déplacement
    //private bool isSelected = false; // État de sélection
    private Vector3 targetPosition; // Position de destination
    private bool isMoving = false; // Si le roi est en train de se déplacer

    private void Update()
    {
        // Si le roi est sélectionné, on permet de le déplacer
        if (isSelected && !isMoving)
        {
            HandleMouseClick();
        }

        // Si le roi est en mouvement, on le déplace
        if (isMoving)
        {   
            MoveKing();
        }
        

        // Vérifie si le roi peut encore bouger, sinon termine la partie
        if (isPlayerWhite == GameManager.IsWhiteTurn && IsCheckmate(isPlayerWhite))
        {
            //EndGame("Perdu ! Le roi ne peut plus se déplacer.");
            GameManager.FindFirstObjectByType<GameManager>()?.DeclareCheckmate(!isPlayerWhite);

        }

        
    }

    // Gère le clic souris pour déplacer le roi
    void HandleMouseClick()
    {   //Debug.Log("HandleMouseClick appelé pour le roi");
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


                    // Vérifie si le mouvement est valide pour le roi
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
        (int x, int z) = GetBoardIndex();
        ChessBoardGenerator board = GameObject.FindObjectOfType<ChessBoardGenerator>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0) continue;

                int newX = x + dx;
                int newZ = z + dz;

                if (!board.IsInBoard(newX, newZ)) continue;

                Vector3 newPos = BoardToWorldPosition(newX, newZ);

                Collider[] colliders = Physics.OverlapSphere(newPos, 0.3f);

                bool isBlocked = colliders.Any(c =>
                {
                    PieceController pc = c.GetComponent<PieceController>();
                    return pc != null && (
                        pc.isPlayerWhite == this.isPlayerWhite || // allié
                        pc is KingController                    // autre roi
                    );
                });

                if (!isBlocked)
                {
                    moves.Add(newPos);
                }
            }
        }

        return moves;
    }


    // Déplace le roi vers la position cible
    void MoveKing()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            Debug.Log(gameObject.name + " a atteint sa destination.");

            // Vérifier si une pièce ennemie est présente pour la capturer
            Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.3f);
            foreach (Collider collider in colliders)
            {
                PieceController piece = collider.GetComponent<PieceController>();
                if (piece != null && piece.isPlayerWhite != this.isPlayerWhite)
                {
                    if (piece is KingController)
                    {
                        Debug.Log("Le roi ne peut pas capturer l'autre roi !");
                        return; // Annule l'attaque
                    }

                    Debug.Log("Le roi capture : " + piece.gameObject.name);
                    Destroy(piece.gameObject);
                    break;
                }
            }

            DeselectCase();
            GameManager.SwitchTurn();
        }
    }


    // Méthode pour terminer la partie en cas de défaite
    void EndGame(string message)
    {
        Debug.Log(message);
        // Ajouter ici la logique pour afficher un message de fin de partie et arrêter le jeu
        Time.timeScale = 0f; // Stop le jeu
        //afficher un message et une option "Rejouer"
    }



    //override de la fonction dans pieceController
    public override void OnCaptured()
    {
        base.OnCaptured(); // detruit la piece quand meme
        EndGame("Perdu ! Le roi a été capturé !");
    }

    public bool IsInCheck()
    {
        Vector3 kingPosition = transform.position;

        // Trouve toutes les pièces ennemies
        PieceController[] allPieces = FindObjectsOfType<PieceController>();

        foreach (PieceController piece in allPieces)
        {
            if (piece == this) continue; // Ignore lui-même
            if (piece.isPlayerWhite != this.isPlayerWhite)
            {
                // Si une pièce ennemie peut se déplacer vers le roi, alors le roi est en échec
                if (piece.GetAvailableMoves().Any(p => Vector3Int.RoundToInt(p) == Vector3Int.RoundToInt(kingPosition)))
                {
                    Debug.Log("Le roi est en échec par " + piece.gameObject.name);
                    return true;
                }
                else
                {
                    //Debug.Log(piece.name + " ne menace pas le roi.");
                }
            }
        }

        return false;
    }
    

    public void SetInCheckVisual(bool inCheck)
    {
        if (inCheck)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            UpdateSelectionVisual(); // remet la couleur d'origine
        }
    }


    public static bool IsCheckmate(bool isWhite)
    {
        KingController king = GameManager.GetKing(isWhite);
        if (king == null) return true; // Roi déjà capturé = checkmate direct
        if (!king.IsInCheck()) Debug.Log("Le roi n'est PAS en échec."); return false;


        PieceController[] pieces = FindObjectsOfType<PieceController>();

        foreach (PieceController piece in pieces)
        {
            if (piece.isPlayerWhite != isWhite) continue;

            Vector3 originalPosition = piece.transform.position;

            foreach (Vector3 move in piece.GetAvailableMoves())
            {
                // Sauvegarde des éventuelles pièces à capturer
                Collider[] beforeMove = Physics.OverlapSphere(move, 0.3f);
                PieceController capturedPiece = beforeMove
                    .Select(c => c.GetComponent<PieceController>())
                    .FirstOrDefault(p => p != null && p.isPlayerWhite != piece.isPlayerWhite);

                // Appliquer le mouvement
                piece.transform.position = move;
                if (capturedPiece != null)
                    capturedPiece.gameObject.SetActive(false); // Simule la capture

                bool stillInCheck = king.IsInCheck();

                // Annuler le mouvement
                piece.transform.position = originalPosition;
                if (capturedPiece != null)
                    capturedPiece.gameObject.SetActive(true);

                if (!stillInCheck)
                    return false; // Un coup sauve le roi → pas mat
            }
        }

        return true; // Aucun coup ne sauve → mat
    }

}