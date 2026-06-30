using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float chaseRange = 15f;    // distance at which the enemy starts chasing
    [SerializeField] private float attackRange = 2.5f;  // distance at which the enemy starts attacking
    [SerializeField] private float rotationSpeed = 5f;  // how fast the enemy turns to face the player
    [SerializeField] private float moveSpeed = 3f;      // chase movement speed

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 2f; // delay between attacks
    [SerializeField] private float attackDamage = 15f;  // damage dealt to the player per hit

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Cinders")]
    [SerializeField] private int cinderReward = 50;     // currency dropped on death

    private Transform player;          // player position to chase/attack
    private CharacterController cc;     // handles movement and collision
    private Animator anim;             // drives enemy animations
    private Vector3 velocity;          // current movement vector
    private float attackTimer;         // counts down between attacks
    private bool dead;                 // stops behaviour once dead

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

        // Distance and direction to the player
        float dist = Vector3.Distance(transform.position, player.position);
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;   // stay upright, ignore vertical

        // Turn to face the player
        if (dir.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);

        // In attack range and off cooldown: attack
        if (dist < attackRange && attackTimer <= 0)
        {
            anim.SetFloat("Speed", 0);
            anim.SetTrigger("Attack");
            DamagePlayer();
            attackTimer = attackCooldown;
        }
        // In chase range: move toward the player
        else if (dist < chaseRange)
        {
            velocity.x = dir.x * moveSpeed;
            velocity.z = dir.z * moveSpeed;
            anim.SetFloat("Speed", 1);
        }
        // Out of range: stand still
        else
        {
            velocity.x = 0;
            velocity.z = 0;
            anim.SetFloat("Speed", 0);
        }

        // Apply gravity to keep the enemy grounded
        if (cc.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += Physics.gravity.y * Time.deltaTime;

        cc.Move(velocity * Time.deltaTime);   // move the enemy
        attackTimer -= Time.deltaTime;          // tick down attack cooldown
    }

    private void DamagePlayer()
    {
        // Reduce the player's health by this enemy's attack damage
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.currentHealth -= attackDamage;
        }
    }

    public void TakeDamage(float damage)
    {
        // Called by the player's attacks
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        dead = true;
        anim.SetTrigger("Death");   // play death animation

        // Award cinders to the player
        if (CinderManager.instance != null)
        {
            CinderManager.instance.AddCinders(cinderReward);
        }

        // Grant a flask charge on kill
        if (Flask.instance != null)
        {
            Flask.instance.AddFlask(1);
            Debug.Log("Flask gained!");
        }

        Destroy(gameObject, 4f);   // remove enemy after death animation
    }
}