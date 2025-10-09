using System.Collections;
using UnityEngine;

public class EnemyKnockBack : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 1f; // Global multiplier for knockback strength
    [SerializeField] private float maxKnockbackForce = 20f; // Cap for knockback force
    [SerializeField] private float minKnockbackForce = 2f; // Minimum knockback force

    [Header("Physics Settings")]
    [SerializeField] private float knockbackDrag = 10f; // Drag during knockback
    [SerializeField] private float recoveryTime = 0.1f; // Time to fully recover after knockback
    [SerializeField] private AnimationCurve knockbackCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)); // Smooth deceleration curve

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    // Cached components for performance
    private Rigidbody2D rb;
    private EnemyMove enemyMovement;

    // State management
    private bool isKnockedBack;
    private Coroutine knockbackCoroutine;

    // Physics caching
    private float originalDrag;
    private bool hasOriginalDrag;

    private void Start()
    {
        CacheComponents();
        ValidateComponents();
    }

    private void CacheComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyMovement = GetComponent<EnemyMove>();
    }

    private void ValidateComponents()
    {
        if (rb == null)
        {
            Debug.LogError($"EnemyKnockBack: No Rigidbody2D found on {gameObject.name}");
            enabled = false;
            return;
        }

        if (enemyMovement == null)
        {
            Debug.LogWarning($"EnemyKnockBack: No EnemyMove found on {gameObject.name}. State changes will be ignored.");
        }

        // Cache original physics values
        if (!hasOriginalDrag)
        {
            originalDrag = rb.drag;
            hasOriginalDrag = true;
        }
    }

    public void Knockback(Transform attackerTransform, float knockbackForce, float knockbackTime, float stunTime)
    {
        // Validate input
        if (attackerTransform == null)
        {
            if (showDebugInfo) Debug.LogWarning("EnemyKnockBack: Null attacker transform");
            return;
        }

        // Don't interrupt existing knockback unless this one is stronger
        if (isKnockedBack && knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        // Apply knockback multiplier and clamp values
        float finalForce = Mathf.Clamp(knockbackForce * knockbackMultiplier, minKnockbackForce, maxKnockbackForce);
        float finalStunTime = Mathf.Max(0.1f, stunTime); // Minimum stun time

        // Change enemy state
        if (enemyMovement != null)
        {
            enemyMovement.ChangeState(EnemyState.Knockback);
        }

        // Start knockback
        knockbackCoroutine = StartCoroutine(PerformKnockback(attackerTransform, finalForce, finalStunTime));

        if (showDebugInfo)
        {
            Debug.Log($"Knockback applied: Force={finalForce}, Duration={finalStunTime}");
        }
    }

    private IEnumerator PerformKnockback(Transform attackerTransform, float force, float duration)
    {
        isKnockedBack = true;

        // Calculate knockback direction
        Vector2 direction = CalculateKnockbackDirection(attackerTransform.position);

        // Apply initial knockback impulse
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        // Apply knockback physics
        rb.drag = knockbackDrag;

        // Main knockback phase with smooth deceleration
        yield return StartCoroutine(SmoothKnockbackPhase(direction * force, duration));

        // Recovery phase
        yield return StartCoroutine(RecoveryPhase());

        // Restore state
        RestoreNormalState();

        isKnockedBack = false;
        knockbackCoroutine = null;
    }

    private Vector2 CalculateKnockbackDirection(Vector3 attackerPosition)
    {
        Vector2 direction = (transform.position - attackerPosition).normalized;

        // Ensure we have a valid direction (avoid zero vector)
        if (direction.magnitude < 0.1f)
        {
            direction = Vector2.right; // Default fallback direction
        }

        return direction;
    }

    private IEnumerator SmoothKnockbackPhase(Vector2 initialVelocity, float duration)
    {
        float elapsedTime = 0f;
        float mainPhaseDuration = duration * 0.7f; // 70% of total time for main phase

        while (elapsedTime < mainPhaseDuration)
        {
            float progress = elapsedTime / mainPhaseDuration;
            float curveValue = knockbackCurve.Evaluate(progress);

            // Apply curve-based deceleration
            Vector2 currentVelocity = Vector2.Lerp(initialVelocity, Vector2.zero, 1f - curveValue);
            rb.velocity = currentVelocity;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator RecoveryPhase()
    {
        float elapsedTime = 0f;
        Vector2 startVelocity = rb.velocity;

        while (elapsedTime < recoveryTime)
        {
            float progress = elapsedTime / recoveryTime;

            // Smoothly reduce velocity to zero
            rb.velocity = Vector2.Lerp(startVelocity, Vector2.zero, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure complete stop
        rb.velocity = Vector2.zero;
    }

    private void RestoreNormalState()
    {
        // Restore original physics
        if (hasOriginalDrag)
        {
            rb.drag = originalDrag;
        }

        // Change enemy state back to idle
        if (enemyMovement != null)
        {
            enemyMovement.ChangeState(EnemyState.Idle);
        }

        if (showDebugInfo)
        {
            Debug.Log("Knockback recovery complete");
        }
    }

    // Public utility methods
    public bool IsKnockedBack() => isKnockedBack;

    public void CancelKnockback()
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            RestoreNormalState();
            isKnockedBack = false;
            knockbackCoroutine = null;
        }
    }

    // For debugging purposes
    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo || !Application.isPlaying) return;

        // Draw current velocity direction
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            Gizmos.color = isKnockedBack ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, rb.velocity.normalized * 2f);
        }
    }

    // Editor validation
    private void OnValidate()
    {
        // Ensure reasonable values
        knockbackMultiplier = Mathf.Max(0f, knockbackMultiplier);
        maxKnockbackForce = Mathf.Max(minKnockbackForce, maxKnockbackForce);
        minKnockbackForce = Mathf.Max(0.1f, minKnockbackForce);
        knockbackDrag = Mathf.Max(0f, knockbackDrag);
        recoveryTime = Mathf.Max(0.05f, recoveryTime);

        // Create default curve if none exists
        if (knockbackCurve == null || knockbackCurve.keys.Length == 0)
        {
            knockbackCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        }
    }
}