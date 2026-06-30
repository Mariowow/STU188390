using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public RectTransform staminaFill;     // the fill bar that shrinks as stamina drops
    public PlayerController player;         // the player whose stamina is shown (assign in Inspector)

    private void Update()
    {
        if (player == null) return;   // safety: need a player assigned

        // Stamina as a 0–1 fraction (divides by fixed 100)
        float percent = player.currentStamina / 100f;

        // Scale the fill bar's width to match (398 = full width)
        staminaFill.sizeDelta = new Vector2(398 * percent, 21.767f);
    }
}