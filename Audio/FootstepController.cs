using System.Collections;
using UnityEngine;

public class FootstepController : MonoBehaviour
{
    [Header("Footstep Settings")]
    public float footstepInterval = 0.4f; // Time between footsteps
    public float movementThreshold = 0.1f; // Minimum speed to play footsteps

    private Rigidbody2D rb;
    private float footstepTimer;
    private bool isMoving;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (rb == null) return;

        // Check if player is moving
        isMoving = rb.velocity.magnitude > movementThreshold;

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                PlayFootstep();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // Reset timer when stopped
            footstepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFootstep();
        }
    }
}