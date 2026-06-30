using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float chaseRange = 25f;    // distance at which boss starts chasing
    [SerializeField] private float attackRange = 4f;    // distance at which boss starts attacking
    [SerializeField] private float rotationSpeed = 3f;  // how fast boss turns to face player
    [SerializeField] private float moveSpeed = 2.5f;    // base chase speed

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 2f;        // delay between attacks
    [SerializeField] private float attackDamage = 30f;         // damage of normal combo hits
    [SerializeField] private float slamAttackDamage = 60f;     // damage of the heavy combo finisher
    [SerializeField] private float specialAttackCooldown = 8f; // reserved for special attack timing

    [Header("Health")]
    [SerializeField] private float maxHealth = 300f;
    public float currentHealth = 300f;

    [Header("Phases")]
    [SerializeField] private float phase2Threshold = 150f;  // HP at which phase 2 begins (50%)
    [SerializeField] private float phase3Threshold = 75f;   // HP at which phase 3 begins (25%)

    [Header("Cinders")]
    [SerializeField] private int cinderReward = 500;   // currency awarded on death

    private Transform player;            // player position to chase/attack
    private CharacterController cc;       // handles boss movement and collision
    private Animator anim;               // drives boss animations
    private Vector3 velocity;            // current movement vector
    private float attackTimer;           // counts down between attacks
    private float specialAttackTimer;    // counts down for special attacks
    private bool dead;                   // stops all behaviour once dead
    private int currentPhase = 1;        // current difficulty phase (1-3)
    private int comboCount = 0;          // tracks position in the attack combo

    private void Start()
    {
        // Cache components and find the player
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (dead || player == null) return;   // do nothing if dead or no player

        UpdatePhase();   // check if boss should enter a harder phase

        // Work out distance and direction to the player
        float dist = Vector3.Distance(transform.position, player.position);
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;   // keep boss upright (ignore vertical)

        // Rotate to face the player
        if (dir.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);

        // Tick down attack timers
        attackTimer -= Time.deltaTime;
        specialAttackTimer -= Time.deltaTime;

        // In attack range and ready: attack
        if (dist < attackRange && attackTimer <= 0)
        {
            anim.SetFloat("Speed", 0);
            PerformComboAttack();
        }
        // In chase range: move toward player (faster in phase 3)
        else if (dist < chaseRange)
        {
            float speed = currentPhase >= 3 ? moveSpeed * 1.5f : moveSpeed;
            velocity.x = dir.x * speed;
            velocity.z = dir.z * speed;
            anim.SetFloat("Speed", 1);
        }
        // Out of range: stop and reset combo
        else
        {
            velocity.x = 0;
            velocity.z = 0;
            anim.SetFloat("Speed", 0);
            comboCount = 0;
        }

        // Apply gravity so the boss stays grounded
        if (cc.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += Physics.gravity.y * Time.deltaTime;

        cc.Move(velocity * Time.deltaTime);   // perform the movement
    }

    private void PerformComboAttack()
    {
        // Higher phases attack faster by shortening the cooldown
        float cooldownMultiplier = 1f;
        if (currentPhase >= 3)
            cooldownMultiplier = 0.5f;   // phase 3: 50% faster
        else if (currentPhase >= 2)
            cooldownMultiplier = 0.7f;   // phase 2: 30% faster

        // Step through the 3-hit combo: Attack1 -> Attack2 -> Slam -> repeat
        if (comboCount == 0)
        {
            anim.SetTrigger("Attack1");
            DamagePlayer(attackDamage);
            comboCount = 1;
        }
        else if (comboCount == 1)
        {
            anim.SetTrigger("Attack2");
            DamagePlayer(attackDamage);
            comboCount = 2;
        }
        else if (comboCount == 2)
        {
            anim.SetTrigger("SlamAttack");
            DamagePlayer(slamAttackDamage);   // finisher hits harder
            comboCount = 0;
        }

        // Reset the attack timer (scaled by current phase)
        attackTimer = attackCooldown * cooldownMultiplier;
    }

    private void UpdatePhase()
    {
        // Enter phase 3 once HP drops below the lower threshold
        if (currentHealth <= phase3Threshold && currentPhase != 3)
        {
            currentPhase = 3;
            Debug.Log("BOSS PHASE 3: ENRAGED!");
        }
        // Enter phase 2 once HP drops below the upper threshold
        else if (currentHealth <= phase2Threshold && currentPhase != 2)
        {
            currentPhase = 2;
            Debug.Log("BOSS PHASE 2: STRONGER!");
        }
    }

    private void DamagePlayer(float damage)
    {
        // Apply damage to the player's health
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
            playerController.currentHealth -= damage;
    }

    public void TakeDamage(float damage)
    {
        // Called by the player's attacks to hurt the boss
        currentHealth -= damage;
        Debug.Log("Boss Health: " + currentHealth + " / " + maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        dead = true;
        anim.SetTrigger("Death");   // play death animation

        // Award cinders to the player on defeat
        if (CinderManager.instance != null)
        {
            CinderManager.instance.AddCinders(cinderReward);
            Debug.Log("BOSS DEFEATED!");
        }

        Destroy(gameObject, 4f);   // remove boss after death animation
    }
}