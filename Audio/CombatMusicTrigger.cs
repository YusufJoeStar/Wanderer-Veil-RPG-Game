using UnityEngine;

public class CombatMusicTrigger : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 10f;
    public LayerMask playerLayer;
    public Transform detectionPoint;

    private bool playerInRange = false;
    private bool hasDied = false;

    private void Start()
    {
        if (detectionPoint == null)
            detectionPoint = transform;
    }

    private void Update()
    {
        if (hasDied) return;

        // Check if player is in range
        bool wasInRange = playerInRange;
        playerInRange = Physics2D.OverlapCircle(detectionPoint.position, detectionRadius, playerLayer) != null;

        // Player entered range
        if (playerInRange && !wasInRange)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.EnterCombat();
            }
        }
        // Player left range
        else if (!playerInRange && wasInRange)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ExitCombat();
            }
        }
    }

    // Call this when enemy dies
    public void OnEnemyDeath()
    {
        if (hasDied) return; // Prevent multiple calls

        hasDied = true;

        Debug.Log($"Enemy {gameObject.name} died, exiting combat");

        // Exit combat if player was in range
        if (playerInRange && AudioManager.Instance != null)
        {
            AudioManager.Instance.ExitCombat();
            playerInRange = false; // Make sure we don't exit again
        }
    }

    private void OnDestroy()
    {
        // Make sure to exit combat when enemy is destroyed
        if (!hasDied && playerInRange && AudioManager.Instance != null)
        {
            Debug.Log($"Enemy {gameObject.name} destroyed, exiting combat");
            AudioManager.Instance.ExitCombat();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (detectionPoint == null) detectionPoint = transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(detectionPoint.position, detectionRadius);
    }
}