using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static bool IsWhiteTurn = true; // Tour des Blancs commence
    public static bool IsWhiteInCheck = false;
    public static bool IsBlackInCheck = false;
    public Camera mainCamera; // La caméra principale

    //permet de s'assurer qu'une piece soit deselectionnée avant de selectionner une autre
    public static PieceController selectedPiece = null;
    public float rotationSpeed = 2f; // Vitesse de rotation
    public float positionSpeed = 2f; // Vitesse de déplacement
    public static bool IsPvPMode = false; // Par défaut, IA activée
    public static int level = 0; // par defaut, le level de l'ia est 1


    



    private Vector3 whitePosition = new Vector3(-0.5f, 8, -5.5f);
    private Vector3 blackPosition = new Vector3(-0.5f, 8, 4.5f);
    private Vector3 whiteRotation = new Vector3(60, 0, 0);
    private Vector3 blackRotation = new Vector3(60, 180, 0);
    public static bool isGameOver = false;

    public static void DeclareCheckmate(bool whiteWon)
    {
        isGameOver = true;
        Debug.Log(whiteWon ? "Échec et mat ! Les blancs gagnent." : "Échec et mat ! Les noirs gagnent.");
        // Affichage UI ici
    }

    public static void SwitchTurn()
    {
        IsWhiteTurn = !IsWhiteTurn;

        if (!IsWhiteTurn && !IsPvPMode && level == 1) // Si c'est le tour des Noirs et qu'on n'est pas en PvP, l'IA de niveau 1 joue
        {
            if (AIControllerLevel1.Instance != null)
            {
                AIControllerLevel1.Instance.PlayAITurn();
            }
            else
            {
                Debug.LogError("AIControllerLevel1.Instance est NULL !");
            }
        }else if (!IsWhiteTurn && !IsPvPMode && level == 3) // Si c'est le tour des Noirs et qu'on n'est pas en PvP, l'IA de niveau 3 joue
        {
            if (AIControllerLevel3.Instance != null)
            {
                AIControllerLevel3.Instance.PlayAITurn();
            }
            else
            {
                Debug.LogError("AIControllerLevel3.Instance est NULL !");
            }
        }

        Debug.Log(IsWhiteTurn ? "Tour des Blancs." : (IsPvPMode ? "Tour des Noirs." : "Tour de l'IA."));

        GameManager instance = FindFirstObjectByType<GameManager>();

        if (instance != null)
        {
            instance.RotateAndMoveCamera();
        }
        else
        {
            Debug.LogError("GameManager introuvable dans la scène !");
        }

        if (IsInCheck(IsWhiteTurn))
        {
            Debug.Log(IsWhiteTurn ? "Les Blancs sont en échec !" : "Les Noirs sont en échec !");
        }
        
        

        
    }

    //logique pour deselectionner une piece si une autre est selectionnée
    public static void SelectPiece(PieceController piece)
    {
        if (selectedPiece != null)
        {
            // Si une pièce est déjà sélectionnée, désélectionne-la
            selectedPiece.DeselectPiece();
        }

        // Sélectionner la nouvelle pièce
        selectedPiece = piece;
    }

    void RotateAndMoveCamera()
    {
        Vector3 targetPosition = IsWhiteTurn ? whitePosition : blackPosition;
        Vector3 targetRotation = IsWhiteTurn ? whiteRotation : blackRotation;
        StartCoroutine(MoveAndRotateCamera(targetPosition, targetRotation));
    }

    IEnumerator MoveAndRotateCamera(Vector3 targetPosition, Vector3 targetRotation)
    {
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        Quaternion targetQuat = Quaternion.Euler(targetRotation);
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * positionSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            mainCamera.transform.rotation = Quaternion.Lerp(startRotation, targetQuat, elapsedTime);
            yield return null;
        }

        // Pour s'assurer que la caméra atteint bien la position et la rotation finale
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetQuat;
    }

    
    public static KingController GetKing(bool isWhite)
    {
        foreach (PieceController piece in FindObjectsOfType<PieceController>())
        {
            if (piece is KingController && piece.isPlayerWhite == isWhite)
            {
                return piece as KingController;
            }
        }
        return null;
    }

    public static bool IsInCheck(bool isWhite)
    {
        KingController king = GetKing(isWhite);
        if (king == null) return false;

        bool isInCheck = king.IsInCheck();
        king.SetInCheckVisual(isInCheck);  //visuel

        return isInCheck;
    }
}
