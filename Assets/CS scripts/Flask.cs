using UnityEngine;

public class Flask : MonoBehaviour
{
    public static Flask instance;                  // singleton: one global flask manager
    public int maxFlasks = 5;                      // maximum charges the player can hold
    public int currentFlasks = 5;                  // current charges available
    [SerializeField] private float healAmount = 50f;   // HP restored per flask

    private PlayerController player;                // reference to the player

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
        // Find the player to heal
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    private void Update()
    {
        // Press E to drink a flask
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseFlask();
        }
    }

    public void UseFlask()
    {
        // Only heal if a charge is available and the player isn't already full
        if (currentFlasks > 0 && player != null && player.currentHealth < player.maxHealth)
        {
            currentFlasks--;
            player.currentHealth += healAmount;

            // Don't overheal past max
            if (player.currentHealth > player.maxHealth)
                player.currentHealth = player.maxHealth;

            Debug.Log("Flask used! Flasks remaining: " + currentFlasks);
        }
    }

    public void AddFlask(int amount)
    {
        // Add charges (e.g. from a kill), capped at the maximum
        currentFlasks += amount;
        if (currentFlasks > maxFlasks)
            currentFlasks = maxFlasks;
    }
}