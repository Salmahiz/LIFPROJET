using UnityEngine;

public class QueenController : PieceController
{
    public float moveSpeed = 5f; 
    //private bool isSelected = false; 
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

                    if (IsValidMove(hitPosition))
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

    public override bool IsValidMove(Vector3 hitPosition)
    {
        Vector3 currentPosition = transform.position;
        int currentX = Mathf.RoundToInt(currentPosition.x);
        int currentZ = Mathf.RoundToInt(currentPosition.z);
        int targetX = Mathf.RoundToInt(hitPosition.x);
        int targetZ = Mathf.RoundToInt(hitPosition.z);

        int dx = Mathf.Abs(targetX - currentX);
        int dz = Mathf.Abs(targetZ - currentZ);

        // Vérifier si le mouvement est en ligne droite (tour) ou en diagonale (fou)
        if (dx == dz || currentX == targetX || currentZ == targetZ)
        {
            if (IsPathClear(currentPosition, hitPosition))
            {
                if (IsDestinationFree(hitPosition))
                {
                    return true;
                }
            }
            Debug.Log("La case de destination ou une case sur le chemin est occupée.");
            return false;
        }

        //Debug.Log("Déplacement impossible.");
        return false;
    }

    bool IsPathClear(Vector3 start, Vector3 end)
    {
        int startX = Mathf.RoundToInt(start.x);
        int startZ = Mathf.RoundToInt(start.z);
        int endX = Mathf.RoundToInt(end.x);
        int endZ = Mathf.RoundToInt(end.z);

        int dx = endX - startX;
        int dz = endZ - startZ;

        int stepX = (dx == 0) ? 0 : (dx > 0 ? 1 : -1);
        int stepZ = (dz == 0) ? 0 : (dz > 0 ? 1 : -1);

        int x = startX + stepX;
        int z = startZ + stepZ;

        while (x != endX || z != endZ)
        {
            Vector3 checkPosition = new Vector3(x, start.y, z);
            Collider[] colliders = Physics.OverlapSphere(checkPosition, 0.3f);

            foreach (Collider collider in colliders)
            {
                if (collider.GetComponent<PieceController>() != null)
                {
                    return false; // Il y a une pièce qui bloque le chemin
                }
            }

            x += stepX;
            z += stepZ;
        }

        return true;
    }

    bool IsDestinationFree(Vector3 targetPosition)
    {
        Collider[] finalColliders = Physics.OverlapSphere(targetPosition, 0.3f);
        foreach (Collider collider in finalColliders)
        {
            PieceController piece = collider.GetComponent<PieceController>();
            if (piece != null)
            {
                if (piece.isPlayerWhite == this.isPlayerWhite)
                {
                    return false; // Impossible d'aller sur une pièce alliée
                }
            }
        }

        return true;
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

            DeselectPiece();
            GameManager.SwitchTurn();
        }
    }


}
