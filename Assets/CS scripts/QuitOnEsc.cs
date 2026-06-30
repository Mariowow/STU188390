using UnityEngine;

public class QuitOnEsc : MonoBehaviour
{
    private void Update()
    {
        // Press Esc to quit the game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();           // closes the built application
            Debug.Log("Game Closed");     // confirmation (Quit does nothing in the editor)
        }
    }
}