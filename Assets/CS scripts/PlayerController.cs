using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;        // normal movement speed
    [SerializeField] private float runSpeed = 7f;         // sprint speed
    [SerializeField] private float rotationSpeed = 12f;   // how fast the player turns to face movement
    [SerializeField] private float gravity = -20f;        // downward force

    [Header("Health")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    [SerializeField] private float staminaRegenPerSec = 20f;   // stamina recovered per second
    [SerializeField] private float staminaRegenDelay = 1.2f;   // wait time before regen starts
    [SerializeField] private float dodgeCost = 0f;             // stamina cost of a dodge (currently free)
    [SerializeField] private float attackCost = 15f;           // stamina cost of a light attack
    [SerializeField] private float runStaminaDrain = 10f;      // stamina drained per second while sprinting

    public float currentStamina = 100f;
    private float staminaRegenTimer = 0f;     // tracks the delay before regen kicks in

    [Header("Combat")]
    private bool isAttacking = false;                          // blocks new attacks mid-swing
    [SerializeField] private float attackAnimationDuration = 0.8f;  // length of the attack lockout
    [SerializeField] private float lightAttackDamage = 20f;
    [SerializeField] private float heavyAttackDamage = 40f;
    [SerializeField] private float attackRange = 3f;           // radius of the attack hit-check

    [Header("Dodge")]
    [SerializeField] private float rollSpeed = 12f;            // how fast the dodge moves the player
    [SerializeField] private float rollDuration = 0.7f;       // how long the dodge lasts
    private bool isRolling = false;                            // blocks input during a roll

    [Header("Audio")]
    [SerializeField] private AudioClip attackSwooshSFX;        // plays on a missed swing
    [SerializeField] private AudioClip hitSFX;                 // plays on a connecting hit
    [SerializeField] private float hitSoundDelay = 0.2f;      // delay so the hit lands after the swoosh

    private CharacterController _cc;     // handles movement and collision
    private Animator _anim;             // drives player animations
    private Camera _cam;                // used for camera-relative movement
    private Vector3 _velocity;          // current movement vector
    private AudioSource audioSource;    // plays combat sounds

    private void Start()
    {
        // Cache components, set starting stats, and lock the cursor for camera control
        _cc = GetComponent<CharacterController>();
        _anim = GetComponentInChildren<Animator>();
        _cam = Camera.main;
        audioSource = GetComponent<AudioSource>();
        currentStamina = maxStamina;
        currentHealth = maxHealth;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (currentHealth <= 0) return;   // stop all control once dead

        HandleMovement();
        HandleStamina();
        HandleGravity();

        _cc.Move(_velocity * Time.deltaTime);   // apply the final movement vector
    }

    private void HandleMovement()
    {
        if (isRolling) return;   // dodge controls movement instead during a roll

        // Read input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Get camera-relative forward/right, flattened to the ground plane
        Vector3 forward = _cam.transform.forward;
        Vector3 right = _cam.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward = forward.normalized;
        right = right.normalized;

        // Combine input into a single movement direction
        Vector3 moveDir = (forward * v + right * h).normalized;
        bool sprinting = Input.GetKey(KeyCode.LeftShift);

        if (moveDir.magnitude > 0.1f)
        {
            // Rotate to face movement, then move at walk or run speed
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDir),
                rotationSpeed * Time.deltaTime
            );
            float speed = sprinting ? runSpeed : walkSpeed;
            _velocity.x = moveDir.x * speed;
            _velocity.z = moveDir.z * speed;
            _anim.SetFloat("Speed", sprinting ? 1f : 0.5f);   // drive walk/run animation
        }
        else
        {
            // No input: stop and go to idle
            _velocity.x = 0f;
            _velocity.z = 0f;
            _anim.SetFloat("Speed", 0f);
        }
    }

    private void HandleGravity()
    {
        // Keep grounded, then apply gravity each frame
        if (_cc.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        _velocity.y += gravity * Time.deltaTime;
    }

    private void HandleStamina()
    {
        bool sprinting = Input.GetKey(KeyCode.LeftShift);

        // Space: dodge roll (free, can't roll mid-roll)
        if (Input.GetKeyDown(KeyCode.Space) && !isRolling)
        {
            _anim.SetTrigger("Roll");
            StartCoroutine(PerformRoll());
        }

        // Sprinting drains stamina and resets the regen delay
        if (sprinting)
        {
            currentStamina -= runStaminaDrain * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;
            staminaRegenTimer = 0;
        }

        // Left-click: light attack (if enough stamina and not busy)
        if (Input.GetMouseButtonDown(0) && !isAttacking && !isRolling)
        {
            if (currentStamina >= attackCost)
            {
                currentStamina -= attackCost;
                _anim.SetTrigger("Attack1");

                bool hitEnemy = DealDamage(lightAttackDamage);

                // Swoosh only plays on a miss
                if (!hitEnemy && audioSource != null && attackSwooshSFX != null)
                    audioSource.PlayOneShot(attackSwooshSFX);

                StartCoroutine(AttackCooldown());
            }
        }

        // Right-click: heavy attack (costs double stamina, more damage)
        if (Input.GetMouseButtonDown(1) && !isAttacking && !isRolling)
        {
            if (currentStamina >= (attackCost * 2f))
            {
                currentStamina -= attackCost * 2f;
                _anim.SetTrigger("HeavyAttack");

                bool hitEnemy = DealDamage(heavyAttackDamage);

                if (!hitEnemy && audioSource != null && attackSwooshSFX != null)
                    audioSource.PlayOneShot(attackSwooshSFX);

                StartCoroutine(AttackCooldown());
            }
        }

        // Regenerate stamina once the delay has passed and not sprinting
        if (currentStamina < maxStamina && !sprinting)
        {
            staminaRegenTimer += Time.deltaTime;
            if (staminaRegenTimer >= staminaRegenDelay)
            {
                currentStamina = Mathf.Min(currentStamina + staminaRegenPerSec * Time.deltaTime, maxStamina);
            }
        }
        else if (!sprinting)
        {
            staminaRegenTimer = 0f;
        }
    }

    private IEnumerator PerformRoll()
    {
        isRolling = true;

        // Roll in the current input direction, or forward if standing still
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 forward = _cam.transform.forward;
        Vector3 right = _cam.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward = forward.normalized;
        right = right.normalized;

        Vector3 rollDir = (forward * v + right * h).normalized;
        if (rollDir.magnitude < 0.1f)
            rollDir = transform.forward;

        // Face the roll direction
        if (rollDir.magnitude > 0.1f)
            transform.rotation = Quaternion.LookRotation(rollDir);

        // Drive movement for the roll's duration
        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            _velocity.x = rollDir.x * rollSpeed;
            _velocity.z = rollDir.z * rollSpeed;
            yield return null;
        }

        isRolling = false;
    }

    private bool DealDamage(float damage)
    {
        // Find everything within attack range
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var hit in hits)
        {
            Debug.Log("Overlapped: " + hit.name);

            // Check for a boss on the hit object or its parents/children
            BossAI boss = hit.GetComponentInParent<BossAI>();
            if (boss == null) boss = hit.GetComponentInChildren<BossAI>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                if (audioSource != null && hitSFX != null)
                    StartCoroutine(PlayHitSoundDelayed(hitSoundDelay));
                return true;   // hit registered
            }

            // Otherwise check for a regular enemy
            EnemyAI enemy = hit.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                if (audioSource != null && hitSFX != null)
                    StartCoroutine(PlayHitSoundDelayed(hitSoundDelay));
                return true;
            }
        }

        return false;   // nothing hit (miss)
    }

    private IEnumerator PlayHitSoundDelayed(float delay)
    {
        // Delay the hit sound so it lands after the swing
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(hitSFX);
    }

    private IEnumerator AttackCooldown()
    {
        // Lock out attacks for the animation's duration
        isAttacking = true;
        yield return new WaitForSeconds(attackAnimationDuration);
        isAttacking = false;
    }
}