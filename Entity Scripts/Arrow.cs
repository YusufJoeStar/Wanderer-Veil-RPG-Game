using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public Rigidbody2D rb;
    public Vector2 direction = Vector2.right;
    public float lifeSpawn = 2f;
    public float speed;
    public int damage;
    public float knockbackForce;
    public float knockbackTime;
    public float stunTime;
    public LayerMask enemyLayer;
    public LayerMask obstacleLayer;
    public SpriteRenderer sr;
    public Sprite buriedSprite;

    private bool hasHit = false; // Prevents multiple collisions
    private Vector3 hitPosition; // Store hit position for precise sticking

    void Start()
    {
        rb.velocity = direction * speed;
        RotateArrow();
        Invoke(nameof(SelfDestruct), lifeSpawn);
    }

    private void RotateArrow()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void SelfDestruct()
    {
        if (!hasHit)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return; // Already hit something, ignore

        // Check if it's an enemy by layer
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            // Make sure it's actually an enemy with the required components
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null) // Only hit if it has EnemyHealth component
            {
                hasHit = true;

                // Deal damage
                enemyHealth.ChangeHealth(-damage);

                // Apply knockback
                EnemyKnockBack enemyKnockback = other.GetComponent<EnemyKnockBack>();
                if (enemyKnockback != null)
                {
                    enemyKnockback.Knockback(transform, knockbackForce, knockbackTime, stunTime);
                }

                Debug.Log($"Arrow hit enemy: {other.gameObject.name}");
                Destroy(gameObject); // Immediately destroy after hitting an enemy
                return;
            }
        }

        // Check if it's an obstacle
        if ((obstacleLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            hitPosition = transform.position;
            AttachToTarget(other.transform);
        }
    }

    // Keep collision detection as backup for solid colliders
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return; // Already hit something, ignore

        // Store the exact hit position
        hitPosition = collision.contacts[0].point;

        // Enemy hit
        if ((enemyLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            // Make sure it's actually an enemy with the required components
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null) // Only hit if it has EnemyHealth component
            {
                hasHit = true;

                // Deal damage
                enemyHealth.ChangeHealth(-damage);

                // Apply knockback
                EnemyKnockBack enemyKnockback = collision.gameObject.GetComponent<EnemyKnockBack>();
                if (enemyKnockback != null)
                {
                    enemyKnockback.Knockback(transform, knockbackForce, knockbackTime, stunTime);
                }

                Debug.Log($"Arrow hit enemy: {collision.gameObject.name}");
                Destroy(gameObject); // Immediately destroy after hitting an enemy
                return;
            }
        }
        // Obstacle hit
        else if ((obstacleLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            AttachToTarget(collision.transform);
        }
    }

    private void AttachToTarget(Transform target)
    {
        hasHit = true;

        // Immediately stop all physics
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;

        // Disable collider to prevent further physics interactions
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Set position to exact hit point for precise sticking
        transform.position = hitPosition;

        // Change sprite to buried version
        if (buriedSprite != null)
            sr.sprite = buriedSprite;

        // Parent to target for movement with it
        transform.SetParent(target);

        // Cancel self-destruct
        CancelInvoke(nameof(SelfDestruct));

        Debug.Log($"Arrow stuck to {target.name} at position {hitPosition}");
    }

    // Safety check to ensure arrow stops if it's still moving after being marked as hit
    private void FixedUpdate()
    {
        if (hasHit && !rb.isKinematic)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
    }
}