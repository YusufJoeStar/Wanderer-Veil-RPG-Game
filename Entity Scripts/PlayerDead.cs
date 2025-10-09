using System.Collections;
using UnityEngine;

public class PlayerDead : MonoBehaviour // Changed class name from PlayerDeathAnimator to PlayerDead
{
    [Header("Death Animation Settings")]
    [Tooltip("The name of the death animation in the Animator Controller")]
    public string deathAnimationName = "Dead";

    [Tooltip("How long to wait before starting the death effect after animation begins")]
    public float deathEffectDelay = 0.5f;

    [Tooltip("If true, the death effect will start automatically after the animation")]
    public bool autoStartDeathEffect = true;

    private Animator animator;
    private DeathEffect deathEffect;
    private bool isPlayingDeathAnimation = false;

    private void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        deathEffect = GetComponent<DeathEffect>();

        // Validate components
        if (animator == null)
        {
            Debug.LogError("PlayerDead: No Animator component found on " + gameObject.name);
        }

        if (deathEffect == null && autoStartDeathEffect)
        {
            Debug.LogWarning("PlayerDead: No DeathEffect component found on " + gameObject.name +
                           ". Death effect will not play automatically.");
        }
    }

    /// <summary>
    /// Triggers the death animation. Called by PlayerHealth when the player dies.
    /// </summary>
    public void PlayDeathAnimation()
    {
        if (isPlayingDeathAnimation) return; // Prevent multiple calls

        isPlayingDeathAnimation = true;

        // DON'T disable controls immediately - let animation play first
        // DisablePlayerControls(); // Move this to later

        // Play the death animation - RESET ALL OTHER ANIMATION PARAMETERS FIRST
        if (animator != null)
        {
            // Reset all other animation bools that might interfere
            animator.SetBool("isIdle", false);
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isShooting", false);

            // Reset movement parameters
            animator.SetFloat("horizontal", 0);
            animator.SetFloat("vertical", 0);
            animator.SetFloat("aimX", 0);
            animator.SetFloat("aimY", 0);

            // NOW set the death parameter
            animator.SetBool("isDead", true);

            Debug.Log("Playing death animation: " + deathAnimationName);
        }

        // Start monitoring the animation and handle the sequence
        StartCoroutine(HandleDeathSequence());
    }

    /// <summary>
    /// Disables player movement and combat to prevent actions during death
    /// </summary>
    private void DisablePlayerControls()
    {
        // Disable movement
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Disable combat
        PlayerCombat playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.enabled = false;
        }

        // Disable bow
        PlayerBow playerBow = GetComponent<PlayerBow>();
        if (playerBow != null)
        {
            playerBow.enabled = false;
        }

        // Disable form switching
        SwitchForm switchForm = GetComponent<SwitchForm>();
        if (switchForm != null)
        {
            switchForm.enabled = false;
        }

        // Stop player movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // Changed from linearVelocity
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
    /// Handles the complete death sequence: animation first, then death effect
    /// </summary>
    private IEnumerator HandleDeathSequence()
    {
        // Wait a small amount to let the animation start
        yield return new WaitForSeconds(0.1f);

        // Wait for the death animation to complete OR use the delay timer
        float animationWaitTime = GetAnimationLength();

        if (animationWaitTime > 0)
        {
            // Wait for actual animation length
            yield return new WaitForSeconds(animationWaitTime);
        }
        else
        {
            // Fallback to manual delay
            yield return new WaitForSeconds(deathEffectDelay);
        }

        // NOW disable player controls (after animation completes)
        DisablePlayerControls();

        // Start the death effect
        if (autoStartDeathEffect && deathEffect != null)
        {
            deathEffect.StartDeathEffect();
        }
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
        Debug.Log("Death animation completed");

        // Start death effect immediately if not already started
        if (!autoStartDeathEffect && deathEffect != null)
        {
            deathEffect.StartDeathEffect();
        }
    }

    /// <summary>
    /// Call this from an Animation Event to disable controls at a specific time
    /// </summary>
    public void DisablePlayerControlsDelayed()
    {
        DisablePlayerControls();
    }

    /// <summary>
    /// Call this from an Animation Event at the perfect moment to start death effect
    /// </summary>
    public void TriggerDeathEffect()
    {
        if (deathEffect != null)
        {
            deathEffect.StartDeathEffect();
        }
    }

    /// <summary>
    /// Reset the death state (useful for respawning or restarting)
    /// </summary>
    public void ResetDeathState()
    {
        isPlayingDeathAnimation = false;

        if (animator != null)
        {
            animator.SetBool("isDead", false);
        }

        // Re-enable components if needed
        // Note: You might want to handle respawning logic elsewhere
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