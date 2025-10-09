using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int facingDirection = 1;
    public Rigidbody2D rb;
    public Animator anim;

    private bool isKnockedBack;
    public bool isShooting;
    public PlayerCombat player_combat;

    private void Update()
    {
        // Normal attack
        if (Input.GetButtonDown("Slash")
      && !DialogueManager.IsDialogueActive
      && !EventSystem.current.IsPointerOverGameObject())
        {
            player_combat.Attack();
        }

        // Heavy attack with right mouse button
        if (Input.GetMouseButtonDown(1) // Right mouse button
            && !DialogueManager.IsDialogueActive
            && !EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Right mouse button pressed - calling HeavyAttack()");
            player_combat.HeavyAttack();
        }
    }

    public void Stop()
    {
        rb.velocity = Vector2.zero;
    }

    public void ForceIdle()
    {
        // Stop motion
        rb.velocity = Vector2.zero;

        // Reset animator movement parameters
        if (anim != null)
        {
            anim.SetFloat("horizontal", 0f);
            anim.SetFloat("vertical", 0f);
        }
    }

    void FixedUpdate()
    {
        if (DialogueManager.IsDialogueActive)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        if (isShooting == true)
        {
            rb.velocity = Vector2.zero;
        }
        else if (isKnockedBack == false)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Only flip based on movement input, not mouse position
            // The PlayerBow script will handle facing direction when shooting
            PlayerBow bow = GetComponent<PlayerBow>();
            bool isInArcherMode = (bow != null && bow.enabled);

            // Only auto-flip from movement when NOT in archer mode
            // The PlayerBow script handles turning when in archer mode
            if (!isInArcherMode)
            {
                if (horizontal > 0 && facingDirection < 0)
                {
                    Flip();
                }
                else if (horizontal < 0 && facingDirection > 0)
                {
                    Flip();
                }
            }

            // Update animator parameters
            if (anim != null)
            {
                anim.SetFloat("horizontal", Mathf.Abs(horizontal));
                anim.SetFloat("vertical", Mathf.Abs(vertical));
            }

            // Handle movement
            Vector2 movement = new Vector2(horizontal, vertical);

            // Normalize the vector to maintain consistent speed in all directions
            if (movement.magnitude > 1f)
            {
                movement = movement.normalized;
            }

            rb.velocity = movement * StatsManager.Instance.speed;
        }
    }

    public void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    // Method to manually set facing direction (used by PlayerBow)
    public void SetFacingDirection(int direction)
    {
        if (direction != facingDirection)
        {
            Flip();
        }
    }

    public void Knockback(Transform enemy, float force, float stunTime)
    {
        isKnockedBack = true;
        Vector2 direction = (transform.position - enemy.position).normalized;

        rb.AddForce(direction * force, ForceMode2D.Impulse);

        StartCoroutine(KnockbackCounterImproved(stunTime));
    }

    IEnumerator KnockbackCounterImproved(float stunTime)
    {
        float originalDrag = rb.drag;
        rb.drag = 8f;

        yield return new WaitForSeconds(stunTime);

        // Gradually stop instead of instant stop
        float stopTime = 0.2f;
        float timer = 0;
        Vector2 startVelocity = rb.velocity;

        while (timer < stopTime)
        {
            timer += Time.deltaTime;
            float progress = timer / stopTime;
            rb.velocity = Vector2.Lerp(startVelocity, Vector2.zero, progress);
            yield return null;
        }

        rb.velocity = Vector2.zero;
        rb.drag = originalDrag;
        isKnockedBack = false;
    }

    IEnumerator KnockbackCounter(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }
}