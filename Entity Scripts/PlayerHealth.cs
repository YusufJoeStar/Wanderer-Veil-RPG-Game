using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private HitEffect hitEffect;
    private DeathEffect deathEffect;
    private PlayerDeathAnimator deathAnimator;
    private bool isDead = false;

    void Start()
    {
        StatsManager.Instance.currentHealth = StatsManager.Instance.maxHealth;
        StatsManager.Instance.UpdateHealthUI();

        hitEffect = GetComponent<HitEffect>();
        deathEffect = GetComponent<DeathEffect>();
        deathAnimator = GetComponent<PlayerDeathAnimator>();
    }

    public void ChangeHealth(int amount)
    {
        if (isDead) return;

        StatsManager.Instance.currentHealth += amount;

        if (StatsManager.Instance.currentHealth > StatsManager.Instance.maxHealth)
            StatsManager.Instance.currentHealth = StatsManager.Instance.maxHealth;
        else if (StatsManager.Instance.currentHealth < 0)
            StatsManager.Instance.currentHealth = 0;

        StatsManager.Instance.UpdateHealthUI();

        if (amount < 0 && StatsManager.Instance.currentHealth > 0 && hitEffect != null)
        {
            hitEffect.PlayHitEffect();
        }

        if (StatsManager.Instance.currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        if (deathAnimator != null)
        {
            deathAnimator.PlayDeathAnimation();
        }
        else if (deathEffect != null)
        {
            deathEffect.StartDeathEffect();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetDeathState()
    {
        isDead = false;
        if (deathAnimator != null)
        {
            deathAnimator.ResetDeathState();
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}