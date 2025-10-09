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
    private EnemyDeathAnimator deathAnimator; 
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        hitEffect = GetComponent<HitEffect>();
        deathAnimator = GetComponent<EnemyDeathAnimator>(); 
    }

    public void ChangeHealth(int amount)
    {
        if (isDead) return;

        currentHealth += amount;

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
        isDead = true; 

        
        OnMonsterDefeated?.Invoke(expReward);

        if (deathAnimator != null)
        {
            deathAnimator.PlayDeathAnimation();
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
