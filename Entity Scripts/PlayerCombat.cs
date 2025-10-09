using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator anim;
    public int cooldown = 1;
    public int heavyAttackCooldown = 2; // Longer cooldown for heavy attack
    public Transform attackPoint;

    public LayerMask enemyLayer;

    private float timer;
    private float heavyTimer;
    private PlayerMovement playerMovement; // Reference to get facing direction

    [Header("Heavy Attack Settings")]
    public float heavyAttackDamageMultiplier = 2f;
    public float heavyAttackRangeMultiplier = 1.5f;
    public float heavyAttackKnockbackMultiplier = 1.8f;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (heavyTimer > 0)
        {
            heavyTimer -= Time.deltaTime;
        }
    }

    public void Attack()
    {
        if (timer <= 0 && heavyTimer <= 0)
        {
            anim.SetBool("isAttacking", true);
            timer = cooldown;
            // Note: Don't deal damage here if using animation events
        }
    }

    public void HeavyAttack()
    {
        Debug.Log("HeavyAttack() called - timer: " + timer + ", heavyTimer: " + heavyTimer);
        if (timer <= 0 && heavyTimer <= 0)
        {
            Debug.Log("Setting isHeavyAttacking to true");
            anim.SetBool("isHeavyAttacking", true);
            heavyTimer = heavyAttackCooldown;
            // Note: Don't deal damage here if using animation events
        }
        else
        {
            Debug.Log("Heavy attack blocked by cooldown");
        }
    }

    // Call this from animation event at the right frame for normal attack
    public void DealDamage()
    {
        DealDamageInternal(1f, 1f, 1f); // Normal multipliers
    }

    // Call this from animation event at the right frame for heavy attack
    public void DealHeavyDamage()
    {
        DealDamageInternal(heavyAttackDamageMultiplier, heavyAttackRangeMultiplier, heavyAttackKnockbackMultiplier);
    }

    private void DealDamageInternal(float damageMultiplier, float rangeMultiplier, float knockbackMultiplier)
    {
        float attackRange = StatsManager.Instance.weaponRange * rangeMultiplier;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // Filter enemies to only those in front of player
        foreach (Collider2D enemy in enemies)
        {
            if (IsEnemyInFront(enemy.transform))
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                EnemyKnockBack enemyKnockback = enemy.GetComponent<EnemyKnockBack>();

                if (enemyKnockback != null)
                {
                    enemyKnockback.Knockback(transform,
                        StatsManager.Instance.knockbackForce * knockbackMultiplier,
                        StatsManager.Instance.knockbackTime,
                        StatsManager.Instance.stunTime);
                }

                if (enemyHealth != null)
                {
                    int damage = Mathf.RoundToInt(StatsManager.Instance.damage * damageMultiplier);
                    enemyHealth.ChangeHealth(-damage);
                    Debug.Log("Hit enemy: " + enemy.name + " for " + damage + " damage");
                }
                break; // Only hit the first valid enemy
            }
        }
    }

    private bool IsEnemyInFront(Transform enemy)
    {
        // Get direction to enemy
        Vector2 directionToEnemy = (enemy.position - transform.position).normalized;

        // Get player's facing direction
        int facingDir = playerMovement != null ? playerMovement.facingDirection :
                       (transform.localScale.x > 0 ? 1 : -1);

        // Check if enemy is in the same direction as player is facing
        return (facingDir > 0 && directionToEnemy.x > 0.1f) ||
               (facingDir < 0 && directionToEnemy.x < -0.1f);
    }

    public void FinishAttacking()
    {
        anim.SetBool("isAttacking", false);
    }

    public void FinishHeavyAttacking()
    {
        anim.SetBool("isHeavyAttacking", false);
    }
}