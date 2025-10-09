using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int expReward = 3;
    public delegate void MonsterDefeated(int exp);
    public static event MonsterDefeated OnMonsterDefeated;

    public int currentHealth;
    public int maxHealth;
    private HitEffect hitEffect;
    private EnemyDeathAnimator deathAnimator; // Changed from DeathEffect to EnemyDeathAnimator
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        hitEffect = GetComponent<HitEffect>();
        deathAnimator = GetComponent<EnemyDeathAnimator>(); // Changed from DeathEffect to EnemyDeathAnimator
    }

    public void ChangeHealth(int amount)
    {
        // Don't take damage if already dead
        if (isDead) return;

        currentHealth += amount;

        // Play hit effect when taking damage (but NOT dying)
        if (amount < 0 && currentHealth > 0 && hitEffect != null)
        {
            hitEffect.PlayHitEffect();
        }

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true; // Set dead flag immediately

        // Award experience immediately
        OnMonsterDefeated?.Invoke(expReward);

        // Start death animation (this handles loot drops and cleanup)
        if (deathAnimator != null)
        {
            deathAnimator.PlayDeathAnimation();
        }
        else
        {
            // Fallback: destroy immediately if no death animator
            Destroy(gameObject);
        }
    }
}