using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Load the main gameplay scene (hooked to the Play button)
        SceneManager.LoadScene("GameWorld");
    }

    public void ExitGame()
    {
        // Quit the application (hooked to the Exit button)
        Application.Quit();
        Debug.Log("Game Closed");
    }
}