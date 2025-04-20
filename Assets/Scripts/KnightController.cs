using UnityEngine;

public class KnightController : PieceController
{
    public float moveSpeed = 5f; // Vitesse de déplacement
    //private bool isSelected = false; // État de sélection
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

                    // Vérifie si le mouvement est valide pour le cavalier
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

    // Vérifie si le mouvement est valide pour le cavalier
    public override bool IsValidMove(Vector3 hitPosition)
    {
        Vector3 currentPosition = transform.position;

        // Convertir en entiers pour éviter les imprécisions
        int currentX = Mathf.RoundToInt(currentPosition.x);
        int currentZ = Mathf.RoundToInt(currentPosition.z);
        int targetX = Mathf.RoundToInt(hitPosition.x);
        int targetZ = Mathf.RoundToInt(hitPosition.z);

        // Le cavalier se déplace en "L" : 2 cases dans une direction, 1 dans l'autre
        int dx = Mathf.Abs(targetX - currentX);
        int dz = Mathf.Abs(targetZ - currentZ);

        if ((dx == 2 && dz == 1) || (dx == 1 && dz == 2))
        {
            // Vérifie si la case cible est occupée par une pièce alliée
            Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f); // Utilise un rayon pour vérifier la collision

            foreach (Collider collider in colliders)
            {
                PieceController piece = collider.GetComponent<PieceController>();
                if (piece != null && piece.isPlayerWhite == this.isPlayerWhite)
                {
                    return false; // Si une pièce alliée est présente sur la cible, le déplacement est invalide
                }
            }

            return true; // Le mouvement est valide
        }

        return false; // Le mouvement ne correspond pas à un "L"
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

            DeselectPiece();
            GameManager.SwitchTurn();
        }
    }
}
