using System.Collections.Generic;
using UnityEngine;

public class AIControllerLevel1 : PieceController
{
    public static AIControllerLevel1 Instance; // Singleton pour un accès facile
    private bool isSelected = false; // Booléen pour gérer la sélection de l'IA

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("AIController existe déjà ! Destruction de l'instance en double.");
            Destroy(gameObject);
        }
    }

    public void PlayAITurn()
    {
        if (GameManager.IsWhiteTurn) return; // L'IA joue avec les noirs

        List<PieceController> aiPieces = GetAIPieces();
        if (aiPieces.Count == 0)
        {
            Debug.Log("L'IA n'a plus de pièces !");
            return;
        }

        // Mélanger les pièces pour ne pas toujours jouer la même
        for (int i = 0; i < aiPieces.Count; i++)
        {
            int randomIndex = Random.Range(0, aiPieces.Count);
            PieceController temp = aiPieces[i];
            aiPieces[i] = aiPieces[randomIndex];
            aiPieces[randomIndex] = temp;
        }

        // Essayer une pièce au hasard après mélange
        foreach (PieceController piece in aiPieces)
        {
            List<Vector3> validMoves = GetValidMoves(piece);
            if (validMoves.Count > 0)
            {
                // Sélectionne un coup au hasard parmi les coups valides
                Vector3 moveTo = validMoves[Random.Range(0, validMoves.Count)];

                if (piece.IsValidMove(moveTo))
                {
                    MovePiece(piece, moveTo);

                    // **Désélectionner la pièce après le déplacement**
                    DeselectPiece();

                    // Fin du tour après le déplacement
                    GameManager.SwitchTurn();
                    return;
                }
            }
        }

        Debug.Log("L'IA ne peut pas bouger !");
    }

    private List<PieceController> GetAIPieces()
    {
        List<PieceController> pieces = new List<PieceController>();
        PieceController[] allPieces = FindObjectsOfType<PieceController>();

        foreach (PieceController piece in allPieces)
        {
            if (!piece.isPlayerWhite) // Sélectionne les pièces noires
            {
                pieces.Add(piece);
            }
        }

        return pieces;
    }

    private List<Vector3> GetValidMoves(PieceController piece)
    {
        List<Vector3> validMoves = new List<Vector3>();

        for (int x = -7; x <= 7; x++)
        {
            for (int z = -7; z <= 7; z++)
            {
                Vector3 testPosition = new Vector3(piece.transform.position.x + x, piece.transform.position.y, piece.transform.position.z + z);

                if (IsTileValid(testPosition) && piece.IsValidMove(testPosition))
                {
                    validMoves.Add(testPosition);
                }
            }
        }

        return validMoves;
    }

    private bool IsTileValid(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 5, Vector3.down, out hit))
        {
            return hit.collider.CompareTag("Tiles");
        }
        return false;
    }

    private void MovePiece(PieceController piece, Vector3 targetPosition)
    {
        piece.transform.position = targetPosition;
        isSelected = false; // Désélectionner la pièce après le déplacement
        Debug.Log("L'IA a déplacé " + piece.gameObject.name + " vers " + targetPosition);

        // Vérifier s'il y a une pièce ennemie à capturer
        Collider[] colliders = Physics.OverlapSphere(targetPosition, 0.3f);
        foreach (Collider collider in colliders)
        {
            PieceController otherPiece = collider.GetComponent<PieceController>();
            if (otherPiece != null && otherPiece.isPlayerWhite)
            {
                Debug.Log("L'IA capture : " + otherPiece.gameObject.name);
                Destroy(otherPiece.gameObject);
                break;
            }
        }
    }

    public void DeselectPiece()
    {
        isSelected = false;
        Debug.Log("L'IA a désélectionné sa pièce.");
    }
}
