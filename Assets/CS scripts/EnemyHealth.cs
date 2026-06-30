using System;
using UnityEngine;

/// <summary>
/// Starter enemy health component — implements IDamageable.
/// Attach this to any enemy GameObject.
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float defense = 10f;   // flat amount subtracted from incoming damage
    [SerializeField] private float poise = 30f;   // damage needed to stagger the enemy

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3f;   // strength of the push-back on hit

    private float _currentHealth;            // current HP
    private float _poiseDamageAccum;         // running total of damage toward a stagger
    private CharacterController _cc;          // used to apply knockback movement

    public event Action<float, float> OnHealthChanged;   // fired on damage: (current, max) — for UI
    public event Action OnDeath;            // fired when the enemy dies

    private void Awake()
    {
        // Initialise health and cache the controller
        _currentHealth = maxHealth;
        _cc = GetComponent<CharacterController>();
    }

    public void TakeDamage(float damage, Vector3 sourcePosition)
    {
        if (_currentHealth <= 0f) return;   // ignore hits if already dead

        // Apply defense, but always deal at least 1 damage
        float finalDamage = Mathf.Max(1f, damage - defense);
        _currentHealth -= finalDamage;
        _poiseDamageAccum += finalDamage;

        // Push the enemy away from the damage source
        Vector3 knockDir = (transform.position - sourcePosition).normalized;
        knockDir.y = 0.2f;   // slight upward lift
        StartCoroutine(ApplyKnockback(knockDir));

        // If enough damage has built up, stagger the enemy
        bool staggered = _poiseDamageAccum >= poise;
        if (staggered)
        {
            _poiseDamageAccum = 0f;
            GetComponent<Animator>()?.SetTrigger("Stagger");
        }

        // Notify listeners (e.g. health bars) and log
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {finalDamage} damage. HP: {_currentHealth}/{maxHealth}");

        if (_currentHealth <= 0f)
            Die();
    }

    private System.Collections.IEnumerator ApplyKnockback(Vector3 dir)
    {
        if (_cc == null) yield break;   // no controller, no knockback

        // Push the enemy over a short window, easing off toward the end
        float elapsed = 0f;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _cc.Move(dir * knockbackForce * (1f - elapsed / duration) * Time.deltaTime);
            yield return null;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        OnDeath?.Invoke();                              // tell listeners the enemy died
        GetComponent<Animator>()?.SetTrigger("Death");  // play death animation
        enabled = false;   // stop taking hits; EnemyAI handles actual removal
    }
}