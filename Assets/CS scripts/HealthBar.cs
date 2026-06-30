using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public RectTransform healthFill;     // the fill bar that shrinks as health drops
    public PlayerController player;       // the player whose health is shown (assign in Inspector)

    private void Update()
    {
        // Safety: bail out if no player is assigned
        if (player == null)
        {
            Debug.Log("Player is NULL in HealthBar!");
            return;
        }

        // Work out health as a 0–1 fraction
        float percent = player.currentHealth / player.maxHealth;
        Debug.Log("Health: " + player.currentHealth + " / " + player.maxHealth + ", Percent: " + percent);

        // Scale the fill bar's width to match the health fraction (398 = full width)
        healthFill.sizeDelta = new Vector2(398 * percent, 21.767f);
    }
}