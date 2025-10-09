using System.Collections;
using UnityEngine;

public class EnemyKnockBack : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackMultiplier = 1f;
    [SerializeField] private float maxKnockbackForce = 20f;
    [SerializeField] private float minKnockbackForce = 2f;

    [Header("Physics Settings")]
    [SerializeField] private float knockbackDrag = 10f;
    [SerializeField] private float recoveryTime = 0.1f;
    [SerializeField] private AnimationCurve knockbackCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    private Rigidbody2D rb;
    private EnemyMove enemyMovement;
    private bool isKnockedBack;
    private Coroutine knockbackCoroutine;
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

        if (!hasOriginalDrag)
        {
            originalDrag = rb.drag;
            hasOriginalDrag = true;
        }
    }

    public void Knockback(Transform attackerTransform, float knockbackForce, float knockbackTime, float stunTime)
    {
        if (attackerTransform == null)
        {
            if (showDebugInfo) Debug.LogWarning("EnemyKnockBack: Null attacker transform");
            return;
        }

        if (isKnockedBack && knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        float finalForce = Mathf.Clamp(knockbackForce * knockbackMultiplier, minKnockbackForce, maxKnockbackForce);
        float finalStunTime = Mathf.Max(0.1f, stunTime);

        if (enemyMovement != null)
        {
            enemyMovement.ChangeState(EnemyState.Knockback);
        }

        knockbackCoroutine = StartCoroutine(PerformKnockback(attackerTransform, finalForce, finalStunTime));

        if (showDebugInfo)
        {
            Debug.Log($"Knockback applied: Force={finalForce}, Duration={finalStunTime}");
        }
    }

    private IEnumerator PerformKnockback(Transform attackerTransform, float force, float duration)
    {
        isKnockedBack = true;
        Vector2 direction = CalculateKnockbackDirection(attackerTransform.position);
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        rb.drag = knockbackDrag;

        yield return StartCoroutine(SmoothKnockbackPhase(direction * force, duration));
        yield return StartCoroutine(RecoveryPhase());
        RestoreNormalState();

        isKnockedBack = false;
        knockbackCoroutine = null;
    }

    private Vector2 CalculateKnockbackDirection(Vector3 attackerPosition)
    {
        Vector2 direction = (transform.position - attackerPosition).normalized;
        if (direction.magnitude < 0.1f)
        {
            direction = Vector2.right;
        }
        return direction;
    }

    private IEnumerator SmoothKnockbackPhase(Vector2 initialVelocity, float duration)
    {
        float elapsedTime = 0f;
        float mainPhaseDuration = duration * 0.7f;

        while (elapsedTime < mainPhaseDuration)
        {
            float progress = elapsedTime / mainPhaseDuration;
            float curveValue = knockbackCurve.Evaluate(progress);
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
            rb.velocity = Vector2.Lerp(startVelocity, Vector2.zero, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
    }

    private void RestoreNormalState()
    {
        if (hasOriginalDrag)
        {
            rb.drag = originalDrag;
        }

        if (enemyMovement != null)
        {
            enemyMovement.ChangeState(EnemyState.Idle);
        }

        if (showDebugInfo)
        {
            Debug.Log("Knockback recovery complete");
        }
    }

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

    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo || !Application.isPlaying) return;
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            Gizmos.color = isKnockedBack ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, rb.velocity.normalized * 2f);
        }
    }

    private void OnValidate()
    {
        knockbackMultiplier = Mathf.Max(0f, knockbackMultiplier);
        maxKnockbackForce = Mathf.Max(minKnockbackForce, maxKnockbackForce);
        minKnockbackForce = Mathf.Max(0.1f, minKnockbackForce);
        knockbackDrag = Mathf.Max(0f, knockbackDrag);
        recoveryTime = Mathf.Max(0.05f, recoveryTime);

        if (knockbackCurve == null || knockbackCurve.keys.Length == 0)
        {
            knockbackCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        }
    }
}
