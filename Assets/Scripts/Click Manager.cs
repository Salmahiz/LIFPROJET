using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public LayerMask clickableLayers; // Masque pour spécifier les couches cliquables (pièces)

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Détection d’un clic gauche
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Effectuer un raycast avec un filtre sur les couches cliquables
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayers))
            {
                Debug.Log("Raycast hit detected: " + hit.collider.gameObject.name);

                /*
                // Vérifier si l'objet cliqué a un composant de mouvement spécifique (pion, tour, etc.)
                if (hit.collider.TryGetComponent(out PawnController pawn))
                {
                    Debug.Log("Pion cliqué, appel du script de mouvement.");
                    pawn.OnPieceClicked(); // Appeler la méthode du pion
                }
                else if (hit.collider.TryGetComponent(out RookController rook))
                {
                    Debug.Log("Tour cliquée, appel du script de mouvement.");
                    rook.OnPieceClicked(); // Appeler la méthode de la tour
                }
                else if (hit.collider.TryGetComponent(out BishopController bishop))
                {
                    Debug.Log("Fou cliqué, appel du script de mouvement.");
                    bishop.OnPieceClicked(); // Appeler la méthode du fou
                }
                else if (hit.collider.TryGetComponent(out KnightController knight))
                {
                    Debug.Log("Cavalier cliqué, appel du script de mouvement.");
                    knight.OnPieceClicked(); // Appeler la méthode du cavalier
                }
                else if (hit.collider.TryGetComponent(out KingController king))
                {
                    Debug.Log("Roi cliqué, appel du script de mouvement.");
                    king.OnPieceClicked();
                }
                else if (hit.collider.TryGetComponent(out QueenController queen))
                {
                    Debug.Log("Reine cliquée, appel du script de mouvement.");
                    queen.OnPieceClicked();
                }
                else
                {
                    Debug.Log("Aucune pièce ou pièce inconnue détectée.");
                }*/

                // Vérifier si l'objet cliqué a un composant de mouvement spécifique (pion, tour, etc.) au lieu de faire un else if pour chacun
                PieceController clickedPiece = hit.collider.GetComponent<PieceController>();

                if (clickedPiece != null)
                {
                    // Si une pièce est déjà sélectionnée, désélectionner la précédente
                    if (GameManager.selectedPiece != null && GameManager.selectedPiece != clickedPiece)
                    {
                        GameManager.selectedPiece.DeselectPiece();
                    }

                    // Appeler la méthode OnPieceClicked() de la nouvelle pièce
                    clickedPiece.OnPieceClicked();
                    GameManager.selectedPiece = clickedPiece;  // Met à jour la pièce sélectionnée
                }
                else
                {
                    Debug.Log("Aucune pièce détectée ou pièce inconnue.");
                }
            
            }
            else
            {
                Debug.Log("Aucun objet cliquable détecté !");
            }
        }
        //else if (GameManager.isGameOver) return; // Ignore les clics après fin partie
    }
}
