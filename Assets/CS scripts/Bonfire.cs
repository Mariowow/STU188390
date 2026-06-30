using UnityEngine;

public class Bonfire : MonoBehaviour
{
    [SerializeField] private float interactionRange = 5f;   // how close the player must be to interact
    [SerializeField] private GameObject levelUpUI;          // the level-up menu panel this bonfire opens
    private PlayerController player;                         // reference to the player
    private bool playerNearby = false;                       // is the player currently in range?
    private bool menuOpen = false;                            // is the level-up menu currently open?

    private void Start()
    {
        // Find the player in the scene and grab its controller
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

        // Make sure the menu starts hidden
        if (levelUpUI != null)
            levelUpUI.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;   // safety: do nothing if no player found

        // Measure distance between bonfire and player
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // Player just entered range
        if (distance < interactionRange && !playerNearby)
        {
            playerNearby = true;
            Debug.Log("PRESS U TO LEVEL UP!");
        }
        // Player just left range — close the menu if it was open
        else if (distance >= interactionRange && playerNearby)
        {
            playerNearby = false;
            CloseMenu();
        }

        // While in range, U toggles the menu open/closed
        if (playerNearby && Input.GetKeyDown(KeyCode.U))
        {
            if (menuOpen)
                CloseMenu();
            else
                OpenMenu();
        }
    }

    private void OpenMenu()
    {
        menuOpen = true;

        // Show the level-up panel
        if (levelUpUI != null)
            levelUpUI.SetActive(true);

        // Free the cursor so the player can click menu buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Level Up Menu Opened!");
    }

    private void CloseMenu()
    {
        menuOpen = false;

        // Hide the level-up panel
        if (levelUpUI != null)
            levelUpUI.SetActive(false);

        // Re-lock the cursor for normal gameplay/camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Level Up Menu Closed!");
    }
}