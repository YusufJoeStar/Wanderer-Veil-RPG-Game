using System.Collections;
using UnityEngine;

public class EnemyDeathAnimator : MonoBehaviour
{
    [Header("Death Animation Settings")]
    [Tooltip("The name of the death animation in the Animator Controller")]
    public string deathAnimationName = "EnemyDeath";

    [Tooltip("How long to wait before destroying the enemy after animation begins")]
    public float destroyDelay = 2f;

    [Tooltip("If true, the enemy will be destroyed automatically after the animation")]
    public bool autoDestroy = true;

    private Animator animator;
    private EnemyLootDrop lootDrop;
    private bool isPlayingDeathAnimation = false;

    private void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        lootDrop = GetComponent<EnemyLootDrop>();

        // Validate components
        if (animator == null)
        {
            Debug.LogWarning("EnemyDeathAnimator: No Animator component found on " + gameObject.name);
        }
    }

    /// <summary>
    /// Triggers the death animation. Called by EnemyHealth when the enemy dies.
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (isPlayingDeathAnimation) return; // Prevent multiple calls

        isPlayingDeathAnimation = true;

        // Disable enemy controls immediately to prevent further actions
        DisableEnemyControls();

        // Drop loot FIRST, before any visual effects
        if (lootDrop != null)
        {
            lootDrop.DropLoot();
        }

        // Play the death animation
        if (animator != null)
        {
            // Reset all other animation bools that might interfere
            animator.SetBool("isIdle", false);
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", false);

            // Reset movement parameters
            animator.SetFloat("horizontal", 0);
            animator.SetFloat("vertical", 0);

            // Set the death parameter
            animator.SetBool("isDead", true);

            Debug.Log("Playing enemy death animation: " + deathAnimationName);
        }

        // Start the death sequence
        if (autoDestroy)
        {
            StartCoroutine(HandleDeathSequence());
        }
    }

    /// <summary>
    /// Disables enemy movement and combat to prevent actions during death
    /// </summary>
    private void DisableEnemyControls()
    {
        // Disable enemy movement
        EnemyMove enemyMovement = GetComponent<EnemyMove>();
        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        // Disable enemy combat
        EnemyCombat enemyCombat = GetComponent<EnemyCombat>();
        if (enemyCombat != null)
        {
            enemyCombat.enabled = false;
        }

        // Disable knockback
        EnemyKnockBack enemyKnockback = GetComponent<EnemyKnockBack>();
        if (enemyKnockback != null)
        {
            enemyKnockback.enabled = false;
        }

        // Stop enemy movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Disable collider to prevent further interactions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    /// <summary>
    /// Handles the complete death sequence: animation first, then destruction
    /// </summary>
    private IEnumerator HandleDeathSequence()
    {
        // Wait for the death animation to complete OR use the destroy delay
        float animationWaitTime = GetAnimationLength();

        if (animationWaitTime > 0)
        {
            // Wait for actual animation length
            yield return new WaitForSeconds(animationWaitTime);
        }
        else
        {
            // Fallback to manual delay
            yield return new WaitForSeconds(destroyDelay);
        }

        // Destroy the enemy
        Destroy(gameObject);
    }

    /// <summary>
    /// Gets the length of the death animation
    /// </summary>
    private float GetAnimationLength()
    {
        if (animator == null) return 0f;

        // Try to get the animation clip length
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        if (ac != null)
        {
            foreach (AnimationClip clip in ac.animationClips)
            {
                if (clip.name == deathAnimationName)
                {
                    return clip.length;
                }
            }
        }

        return 0f; // Couldn't find the animation
    }

    /// <summary>
    /// Call this from an Animation Event if you want precise timing control
    /// </summary>
    public void OnDeathAnimationComplete()
    {
        Debug.Log("Enemy death animation completed");

        if (!autoDestroy)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Call this from an Animation Event at a specific time to drop loot
    /// (Alternative to dropping loot immediately)
    /// </summary>
    public void TriggerLootDrop()
    {
        if (lootDrop != null)
        {
            lootDrop.DropLoot();
        }
    }

    /// <summary>
    /// Call this from an Animation Event to destroy the enemy at the perfect moment
    /// </summary>
    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Check if the death animation is currently playing
    /// </summary>
    public bool IsPlayingDeathAnimation()
    {
        return isPlayingDeathAnimation;
    }

    /// <summary>
    /// Get the current animation state info for the death animation
    /// </summary>
    public bool IsDeathAnimationFinished()
    {
        if (animator == null) return true;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(deathAnimationName) && stateInfo.normalizedTime >= 1.0f;
    }
}