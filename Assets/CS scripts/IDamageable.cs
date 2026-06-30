using UnityEngine;

/// <summary>
/// Any entity that can receive damage should implement this interface.
/// The player's attack system calls TakeDamage on anything it hits.
/// </summary>
public interface IDamageable
{
    /// <param name="damage">Raw damage amount before enemy defenses</param>
    /// <param name="sourcePosition">World position of attacker (for knockback direction)</param>
    void TakeDamage(float damage, Vector3 sourcePosition);
}