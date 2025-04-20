using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Start()
    {
        SceneManager.UnloadSceneAsync("AI Menu");
    }

    public void PlayAgainstPlayer()
    {
        GameManager.IsPvPMode = true;
        SceneManager.LoadScene("ChessBoard");
        SceneManager.UnloadSceneAsync("Main Menu");
    }

    public void PlayAgainstAI()
    {
        SceneManager.UnloadSceneAsync("Main Menu");
        SceneManager.LoadScene("AI Menu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }

    public void level1()
    {
        GameManager.IsPvPMode = false;
        GameManager.level = 1;
        SceneManager.LoadScene("ChessBoard");
        SceneManager.UnloadSceneAsync("AI Menu");
    }

    public void level2()
    {
        GameManager.IsPvPMode = false;
        SceneManager.LoadScene("ChessBoard");
        SceneManager.UnloadSceneAsync("AI Menu");
    }

    public void level3()
    {
        GameManager.IsPvPMode = false;
        GameManager.level = 3;
        SceneManager.LoadScene("ChessBoard");
        SceneManager.UnloadSceneAsync("AI Menu");
    }

    public void QuitSubMenu()
    {
        SceneManager.UnloadSceneAsync("AI Menu");
        SceneManager.LoadScene("Main Menu");
    }
}
