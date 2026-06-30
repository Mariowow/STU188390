using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public Canvas gameOverCanvas;          // the "YOU DIED" screen
    public PlayerController player;         // the player to watch (ASSIGN IN INSPECTOR)
    private float respawnTimer = 3f;        // seconds to wait before respawning
    private bool isDead = false;            // has the player already died this run?

    private void Start()
    {
        // Hide the game over screen at the start
        if (gameOverCanvas != null)
            gameOverCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;   // safety: need a player to monitor

        // Player health hit zero and hasn't been marked dead yet
        if (player.currentHealth <= 0 && !isDead)
        {
            Die();
        }

        // Once dead, count down to respawn
        if (isDead)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                Respawn();
            }
        }
    }

    private void Die()
    {
        isDead = true;

        // Show the game over screen
        if (gameOverCanvas != null)
            gameOverCanvas.gameObject.SetActive(true);

        // Play the player's death animation
        if (player != null)
        {
            player.GetComponent<Animator>().SetTrigger("Death");
        }

        Debug.Log("PLAYER DIED!");
    }

    private void Respawn()
    {
        // Reload the level to restart
        SceneManager.LoadScene("SampleScene");
    }
}