using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PawnController : PieceController
{
    public float moveSpeed = 5f; // Vitesse de déplacement
    //private bool isSelected = false; // État de sélection
    private Vector3 targetPosition; // Position de destination
    private bool isMoving = false; // Si le pion est en train de se déplacer
    private bool isFirstMove = true; // Premier déplacement du pion
    private PieceController pieceToCapture = null; // Stocke la pièce ennemie à capturer


    private void Update()
    {
        // Si le pion est sélectionné, on permet de le déplacer
        if (isSelected && !isMoving)
        {
            HandleMouseClick();
        }

        // Si le pion est en mouvement, on le déplace
        if (isMoving)
        {
            MovePawn();
        }
    }

    // Gère le clic souris pour déplacer le pion
    void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0)) // Si clic gauche
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Lancer un raycast depuis la position de la souris
            if (Physics.Raycast(ray, out hit))
            {
                // Vérifie si on clique sur une case ("Tiles")
                if (hit.collider.CompareTag("Tiles"))
                {
                    Vector3 hitPosition = hit.collider.transform.position;

                    // Vérifie si la case est une case valide (1 case ou 2 cases au premier mouvement)
                    if (IsValidMove(hitPosition))
                    {
                        targetPosition = hitPosition; // Met à jour la position cible
                        isMoving = true; // Active le mouvement

                        // Désactive le premier mouvement après le déplacement
                        if (isFirstMove)
                            isFirstMove = false;
                    }
                    else
                    {
                        Debug.Log("Déplacement invalide.");
                    }
                }
            }
        }
    }

    public override bool IsValidMove(Vector3 hitPosition)
    {
        Vector3 currentPosition = transform.position;

        int currentX = Mathf.RoundToInt(currentPosition.x);
        int currentZ = Mathf.RoundToInt(currentPosition.z);
        int targetX = Mathf.RoundToInt(hitPosition.x);
        int targetZ = Mathf.RoundToInt(hitPosition.z);

        int distanceX = Mathf.Abs(targetX - currentX);
        int distanceZ = targetZ - currentZ; // Direction avant ou arrière en fonction du joueur

        // Vérifie le déplacement standard de 1 case
        if (distanceX == 0 && distanceZ == (isPlayerWhite ? 1 : -1))
        {
            // Vérifie si la case est libre et le chemin est dégagé
            if (IsPathClear(currentPosition, hitPosition))
            {
                isFirstMove = false;
                return true;
            }
            else
            {
                Debug.Log("La case de destination est occupée.");
            }
        }

        // Vérifie le premier mouvement (2 cases)
        if (isFirstMove && distanceX == 0 && distanceZ == (isPlayerWhite ? 2 : -2))
        {
            if (IsPathClear(currentPosition, hitPosition))
            {
                isFirstMove = false;
                return true;
            }
            else
            {
                Debug.Log("Le chemin pour le premier déplacement est bloqué.");
            }
        }

        // Vérification pour une capture (mouvement diagonal de 1 case)
        if (distanceX == 1 && distanceZ == (isPlayerWhite ? 1 : -1))
        {
            Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
            foreach (Collider collider in colliders)
            {
                PieceController piece = collider.GetComponent<PieceController>();
                if (piece != null && piece.isPlayerWhite != this.isPlayerWhite) 
                {
                    pieceToCapture = piece;
                    return true; // Permet la capture
                }
            }
        }

        //Debug.Log("Déplacement impossible");
        return false;
    } 

    // Vérifie si le chemin entre la position actuelle et la position cible est dégagé
    bool IsPathClear(Vector3 currentPosition, Vector3 targetPosition)
    {
        if (!isFirstMove)
        {
            // Vérifie si le déplacement est strictement vertical (Z change mais X reste le même)
            if (currentPosition.x == targetPosition.x)
            {
                // Détermine la direction du mouvement (avant pour blanc, arrière pour noir)
                int direction = isPlayerWhite ? 1 : -1;

                // La case devant la position actuelle est une case à la même position X et Y, mais une position Z décalée de 1
                Vector3 checkPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + direction);

                // Vérifie si la case de devant est occupée
                Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.1f);
                foreach (Collider collider in colliders)
                {
                    PieceController piece = collider.GetComponent<PieceController>();
                    if (piece != null)
                    {
                        // Si une pièce est trouvée, on retourne false (le chemin est bloqué)
                        return false;
                    }
                }
            }

            // Si la case devant est libre, retourne true
            return true;
        }

        if (currentPosition.x == targetPosition.x)
        {
            int direction = isPlayerWhite ? 1 : -1;

            // On vérifie chaque case entre la position actuelle et la position cible
            for (float z = currentPosition.z + direction; z != targetPosition.z + direction; z += direction)
            {
                Vector3 checkPosition = new Vector3(currentPosition.x, currentPosition.y, z);

                Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.1f);
                bool isOccupied = false;
                foreach (Collider collider in colliders)
                {
                    PieceController piece = collider.GetComponent<PieceController>();

                    // Affichage pour débogage
                    Debug.Log("Vérification pièce : " + piece);
                    if (piece != null)
                    {
                        isOccupied = true;
                        Debug.Log("Case occupée par une pièce : " + piece.gameObject.name);
                        break; // On sort dès qu'une pièce est détectée
                    }
                }

                // Si la case est occupée, le chemin est bloqué
                if (isOccupied)
                {
                    Debug.Log("Chemin bloqué à : " + checkPosition);
                    return false;
                }
            }
        }
        return true; // Le chemin est libre
    }





    // Déplace le pion vers la position cible
    void MovePawn()
    {
        Vector3 oldPosition = transform.position; // Sauvegarde la position actuelle

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            Debug.Log(gameObject.name + " a atteint sa destination.");

            if (pieceToCapture != null)
            {
                Debug.Log("Destruction de : " + pieceToCapture.gameObject.name);
                Destroy(pieceToCapture.gameObject); // Supprime le pion ennemi capturé
                pieceToCapture = null;
            }

            //ClearOldPosition(oldPosition); // Nettoie l'ancienne position après capture

            DeselectPiece();
            GameManager.SwitchTurn();
        }
        if ((isPlayerWhite && Mathf.RoundToInt(targetPosition.z) == 7) ||
            (!isPlayerWhite && Mathf.RoundToInt(targetPosition.z) == 0))
        {
            //PromotePawn(); // affiche choix de piece pour changer
        }

    }
    

}


