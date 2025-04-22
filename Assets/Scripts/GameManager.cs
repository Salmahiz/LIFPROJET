using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static bool IsWhiteTurn = true; // Tour des Blancs commence
    public Camera mainCamera; // La caméra principale

    //permet de s'assurer qu'une piece soit deselectionnée avant de selectionner une autre
    public static PieceController selectedPiece = null;
    public float positionSpeed = 2f; // Vitesse de déplacement
    public static bool IsPvPMode = false; // Par défaut, IA activée
    public static int level = 0; // par defaut, le level de l'ia est 1


    private Vector3 whitePosition = new Vector3(-0.5f, 8, -5.5f);
    private Vector3 blackPosition = new Vector3(-0.5f, 8, 4.5f);
    private Vector3 whiteRotation = new Vector3(60, 0, 0);
    private Vector3 blackRotation = new Vector3(60, 180, 0);
    public static bool isGameOver = false;

    public void DeclareCheckmate(bool whiteWon)
    {
        isGameOver = true;
        Debug.Log(whiteWon ? "Échec et mat ! Les blancs gagnent." : "Échec et mat ! Les noirs gagnent.");
        // Affichage UI ici
        // Affiche un petit délai pour lire le message dans la console
        GameManager instance = FindFirstObjectByType<GameManager>();
        if (instance != null)
        {
            instance.StartCoroutine(instance.ReturnToMenuAfterDelay(2f));
        }
    }
    void Update()
    {   //quand on clique sur M on retourne sur le menu
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Test manuel : retour au menu");
            SceneManager.LoadScene("Main Menu");
        }
        //if(roi est en checkmate){
        //  SceneManager.LoadScene("Main Menu");
        //}
    }


    public static void SwitchTurn()
    {
        IsWhiteTurn = !IsWhiteTurn;
        // Nouvelle vérification d'échec et mat juste après changement de tour
        //Debug.Log("Test d’échec et mat...");
        if (KingController.IsCheckmate(IsWhiteTurn))
        {
            Debug.Log("ÉCHEC ET MAT détecté !");
            GameManager gameMgr = FindFirstObjectByType<GameManager>();
            if (gameMgr != null)
            {
                gameMgr.DeclareCheckmate(!IsWhiteTurn);
            }

            // l’autre joueur gagne
            return;
        }


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
            selectedPiece.ResetColors();
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
        king.SetInCheckVisual(isInCheck);  //le roi devient rouge

        return isInCheck;
    }

    private bool SceneExists(string sceneName)
{
    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(i);
        string name = System.IO.Path.GetFileNameWithoutExtension(path);
        if (name == sceneName)
        {
            return true;
        }
    }
    return false;
}

public IEnumerator ReturnToMenuAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    
    string menuScene = "Main Menu";
    if (SceneExists(menuScene))
    {
        SceneManager.LoadScene(menuScene);
    }
    else
    {
        Debug.LogError($"La scène '{menuScene}' n'existe pas dans les paramètres de build !");
    }
}
public void SetPlayerNameAndLoadScene(string username)
{
    Debug.Log("Nom reçu depuis JavaScript : " + username);
    GameData.playerName = username;
    UnityEngine.SceneManagement.SceneManager.LoadScene("AI Menu");
}


}
