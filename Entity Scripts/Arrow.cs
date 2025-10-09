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

    private bool hasHit = false; 
    private Vector3 hitPosition; 
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
        if (hasHit) return; 

       
        if ((enemyLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null) 
            {
                hasHit = true;

               
                enemyHealth.ChangeHealth(-damage);

               
                EnemyKnockBack enemyKnockback = other.GetComponent<EnemyKnockBack>();
                if (enemyKnockback != null)
                {
                    enemyKnockback.Knockback(transform, knockbackForce, knockbackTime, stunTime);
                }

                Debug.Log($"Arrow hit enemy: {other.gameObject.name}");
                Destroy(gameObject); 
                return;
            }
        }

        
        if ((obstacleLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            hitPosition = transform.position;
            AttachToTarget(other.transform);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return; 

        hitPosition = collision.contacts[0].point;

        if ((enemyLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null) 
            {
                hasHit = true;

                enemyHealth.ChangeHealth(-damage);

                EnemyKnockBack enemyKnockback = collision.gameObject.GetComponent<EnemyKnockBack>();
                if (enemyKnockback != null)
                {
                    enemyKnockback.Knockback(transform, knockbackForce, knockbackTime, stunTime);
                }

                Debug.Log($"Arrow hit enemy: {collision.gameObject.name}");
                Destroy(gameObject); 
                return;
            }
        }
        else if ((obstacleLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            AttachToTarget(collision.transform);
        }
    }

    private void AttachToTarget(Transform target)
    {
        hasHit = true;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        transform.position = hitPosition;

        if (buriedSprite != null)
            sr.sprite = buriedSprite;

        transform.SetParent(target);

        CancelInvoke(nameof(SelfDestruct));

        Debug.Log($"Arrow stuck to {target.name} at position {hitPosition}");
    }

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
