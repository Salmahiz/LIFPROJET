using System.Collections.Generic;
using UnityEngine;

public class AIControllerLevel1 : PieceController
{
    public static AIControllerLevel1 Instance; // Singleton pour un accès facile

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
            List<Vector3> availableMoves = piece.GetAvailableMoves();

            if (availableMoves.Count > 0)
            {
                Vector3 moveTo = availableMoves[Random.Range(0, availableMoves.Count)];
                Collider[] colliders = Physics.OverlapSphere(moveTo, 0.3f);
                foreach (Collider col in colliders)
                {
                    PieceController enemy = col.GetComponent<PieceController>();
                    if (enemy != null && enemy.isPlayerWhite)
                    {
                        enemy.OnCaptured();
                        break;
                    }
                }
                // Téléportation immédiate (ou remplace par une animation si tu veux)
                piece.transform.position = moveTo;
                // réinitialise les couleurs et sélection
                piece.DeselectCase();
                // Tour suivant
                GameManager.SwitchTurn();
                Debug.Log($"L'IA déplace la pièce {piece.name} vers {moveTo}");
                return;
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

}
