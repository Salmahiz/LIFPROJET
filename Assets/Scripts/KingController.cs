using UnityEngine;

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
            EndGame("Perdu ! Le roi ne peut plus se déplacer.");
        }

        
    }

    // Gère le clic souris pour déplacer le roi
    void HandleMouseClick()
    {   Debug.Log("HandleMouseClick appelé pour le roi");
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

                    // Vérifie si le mouvement est valide pour le roi
                    if (IsValidMove(hitPosition))
                    {
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

    // Vérifie si le mouvement est valide pour le roi
    public override bool IsValidMove(Vector3 hitPosition)
    {
        Vector3 currentPosition = transform.position;
        int currentX = Mathf.RoundToInt(currentPosition.x);
        int currentZ = Mathf.RoundToInt(currentPosition.z);
        int targetX = Mathf.RoundToInt(hitPosition.x);
        int targetZ = Mathf.RoundToInt(hitPosition.z);

        int dx = Mathf.Abs(targetX - currentX);
        int dz = Mathf.Abs(targetZ - currentZ);

        if (dx <= 1 && dz <= 1) // Le roi peut se déplacer d'une case
        {
            Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);

            foreach (Collider collider in colliders)
            {
                PieceController piece = collider.GetComponent<PieceController>();
                if (piece != null)
                {
                    if (piece.isPlayerWhite == this.isPlayerWhite)
                    {
                        return false; // Impossible de capturer une pièce alliée
                    }

                    if (piece is KingController)
                    {
                        Debug.Log("Le roi ne peut pas capturer l'autre roi !");
                        return false; // Interdiction de capturer le roi ennemi
                    }
                }
            }
            return true; // Déplacement valide
        }

        return false;
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

            DeselectPiece();
            GameManager.SwitchTurn();
        }
    }



    // Vérifie si le roi peut encore bouger
    private bool CanKingMove()
    {
        Vector3 currentPosition = transform.position;
        
        // Check les 8 possible movements du roi
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0) continue; // Skip position
                
                Vector3 testPos = currentPosition + new Vector3(x, 0, z);
                if (IsValidMove(testPos))
                {
                    return true;
                }
            }
        }
        return false;
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
                if (piece.IsValidMove(kingPosition))
                {
                    Debug.Log("Le roi est en échec par " + piece.gameObject.name);
                    return true;
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
    if (king == null || !king.IsInCheck()) return false;

    PieceController[] pieces = FindObjectsOfType<PieceController>();
    
    foreach (PieceController piece in pieces)
    {
        if (piece.isPlayerWhite != isWhite) continue;
        
        // Limiter aux cases autour de la pièce plutôt que tout l'échiquier
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 target = piece.transform.position + new Vector3(x, 0, z);
                if (!piece.IsValidMove(target)) continue;

                // Simulation du mouvement
                Vector3 originalPos = piece.transform.position;
                piece.transform.position = target;
                
                bool stillInCheck = king.IsInCheck();
                piece.transform.position = originalPos;
                
                if (!stillInCheck) return false;
            }
        }
    }
    return true;
}
}