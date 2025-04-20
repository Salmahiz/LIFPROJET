using UnityEngine;

using System.Collections.Generic; //pour pouvoir utiliser List dans GetAllPotentialMoves
//public GameObject moveHighlightPrefab; // Prefab pour highlight les mouvements possibles // a faire

public class RookController : PieceController // Hérite de PieceController
{
    public float moveSpeed = 5f; // Vitesse de déplacement
    //private bool isSelected = false; // État de sélection
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

    // Vérifie si le déplacement est valide
    public override bool IsValidMove(Vector3 hitPosition)
    {
        Vector3 currentPosition = transform.position;

        // Convertir en entiers pour éviter les imprécisions
        int currentX = Mathf.RoundToInt(currentPosition.x);
        int currentZ = Mathf.RoundToInt(currentPosition.z);
        int targetX = Mathf.RoundToInt(hitPosition.x);
        int targetZ = Mathf.RoundToInt(hitPosition.z);

        // Calcul des distances
        int distanceX = Mathf.Abs(targetX - currentX);
        int distanceZ = Mathf.Abs(targetZ - currentZ);

        // La tour peut se déplacer uniquement verticalement ou horizontalement
        if (distanceX == 0 || distanceZ == 0)
        {
            // Vérifie si la trajectoire est dégagée et valide pour le mouvement
            if (IsPathClear(currentPosition, hitPosition))
            {
                // Vérifie si la case de destination est libre ou occupée par une pièce ennemie
                if (IsDestinationFree(hitPosition))
                {
                    return true;
                }
            }
            Debug.Log("La case de destination est occupée par une pièce.");
            return false;
        }

        return false; // Mouvement invalide
    }

    // Vérifie si la case de destination est libre
    bool IsDestinationFree(Vector3 targetPosition)
    {
        // Vérifie si la case de destination est occupée par une pièce de la même couleur
        Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.1f);
        foreach (Collider collider in colliders)
        {
            PieceController piece = collider.GetComponent<PieceController>();
            if (piece != null && piece.isPlayerWhite == this.isPlayerWhite)
            {
                return false; // Il y a une pièce alliée sur la case
            }
        }
        return true; // La case est libre
    }

    // Vérifie si le chemin entre la position actuelle et la position cible est dégagé
   bool IsPathClear(Vector3 currentPosition, Vector3 targetPosition)
    {
        int currentX = Mathf.RoundToInt(currentPosition.x);
        int currentZ = Mathf.RoundToInt(currentPosition.z);
        int targetX = Mathf.RoundToInt(targetPosition.x);
        int targetZ = Mathf.RoundToInt(targetPosition.z);

        // Vérifie que le mouvement est horizontal ou vertical (pas diagonal)
        if (currentX != targetX && currentZ != targetZ)
        {
            return false; // Si le déplacement n'est pas horizontal ou vertical, c'est invalide
        }

        // Si le mouvement est horizontal (X change)
        if (currentZ == targetZ)
        {
            // Détermine la direction du mouvement (X)
            int dx = (targetX > currentX) ? 1 : -1;
            int x = currentX + dx;

            // Parcourt chaque case entre la position actuelle et la position cible sur l'axe X
            while (x != targetX)
            {
                Vector3 checkPosition = new Vector3(x, currentPosition.y, currentZ);
                Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.3f); // Rayon de 0.3 pour vérifier les collisions

                foreach (Collider collider in colliders)
                {
                    if (collider.TryGetComponent(out PieceController piece))
                    {
                        return false; // Si une pièce est présente sur le chemin, retournez false
                    }
                }

                x += dx; // Avance à la prochaine case sur l'axe X
            }
        }
        // Si le mouvement est vertical (Z change)
        else if (currentX == targetX)
        {
            // Détermine la direction du mouvement (Z)
            int dz = (targetZ > currentZ) ? 1 : -1;
            int z = currentZ + dz;

            // Parcourt chaque case entre la position actuelle et la position cible sur l'axe Z
            while (z != targetZ)
            {
                Vector3 checkPosition = new Vector3(currentX, currentPosition.y, z);
                Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.3f); // Rayon de 0.3 pour vérifier les collisions

                foreach (Collider collider in colliders)
                {
                    if (collider.TryGetComponent(out PieceController piece))
                    {
                        return false; // Si une pièce est présente sur le chemin, retournez false
                    }
                }

                z += dz; // Avance à la prochaine case sur l'axe Z
            }
        }

        // Vérifie la dernière case (cible) pour une capture éventuelle
        Collider[] finalColliders = Physics.OverlapSphere(targetPosition, 0.3f);
        foreach (Collider collider in finalColliders)
        {
            PieceController piece = collider.GetComponent<PieceController>();
            if (piece != null && piece.isPlayerWhite == this.isPlayerWhite)
            {
                return false; // Impossible d'atterrir sur une pièce alliée
            }
        }

        return true; // Le chemin est dégagé
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
            DeselectPiece();
            GameManager.SwitchTurn(); // Passer au joueur suivant
        }
    }


}
