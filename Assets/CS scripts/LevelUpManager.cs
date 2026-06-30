using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager instance;     // singleton: one global level-up manager

    [SerializeField] private GameObject levelUpScreen;          // the level-up menu panel
    [SerializeField] private Button increaseHealthButton;       // button to buy +health
    [SerializeField] private Button increaseStaminaButton;      // button to buy +stamina
    [SerializeField] private Button increaseStrengthButton;     // button to buy +strength
    [SerializeField] private TextMeshProUGUI cindersDisplay;    // shows current cinder total

    private PlayerController player;     // the player whose stats are upgraded
    private int healthCost = 50;        // cinder cost per health upgrade
    private int staminaCost = 50;       // cinder cost per stamina upgrade
    private int strengthCost = 50;      // cinder cost per strength upgrade

    private void Awake()
    {
        // Singleton setup: keep the first instance, destroy duplicates
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Find the player and hide the menu at startup
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        levelUpScreen.SetActive(false);

        // Hook each button up to its upgrade method
        increaseHealthButton.onClick.AddListener(IncreaseHealth);
        increaseStaminaButton.onClick.AddListener(IncreaseStamina);
        increaseStrengthButton.onClick.AddListener(IncreaseStrength);
    }

    public void ShowLevelUpScreen()
    {
        // Open the menu and refresh the cinder count
        levelUpScreen.SetActive(true);
        UpdateCindersDisplay();
    }

    public void CloseLevelUpScreen()
    {
        // Hide the menu
        levelUpScreen.SetActive(false);
    }

    private void IncreaseHealth()
    {
        // Spend cinders to raise max health (and refill to full)
        if (CinderManager.instance.currentCinders >= healthCost)
        {
            CinderManager.instance.currentCinders -= healthCost;
            player.maxHealth += 20;
            player.currentHealth = player.maxHealth;
            UpdateCindersDisplay();
            Debug.Log("Health increased! Max: " + player.maxHealth);
        }
        else
        {
            Debug.Log("Not enough cinders!");
        }
    }

    private void IncreaseStamina()
    {
        // Spend cinders to raise max stamina (and refill to full)
        if (CinderManager.instance.currentCinders >= staminaCost)
        {
            CinderManager.instance.currentCinders -= staminaCost;
            player.maxStamina += 20;
            player.currentStamina = player.maxStamina;
            UpdateCindersDisplay();
            Debug.Log("Stamina increased! Max: " + player.maxStamina);
        }
        else
        {
            Debug.Log("Not enough cinders!");
        }
    }

    private void IncreaseStrength()
    {
        // Spend cinders on strength (currently logs only — no stat applied yet)
        if (CinderManager.instance.currentCinders >= strengthCost)
        {
            CinderManager.instance.currentCinders -= strengthCost;
            Debug.Log("Strength increased!");
            UpdateCindersDisplay();
        }
        else
        {
            Debug.Log("Not enough cinders!");
        }
    }

    private void UpdateCindersDisplay()
    {
        // Refresh the on-screen cinder total
        if (cindersDisplay != null)
            cindersDisplay.text = "Cinders: " + CinderManager.instance.currentCinders;
    }
}