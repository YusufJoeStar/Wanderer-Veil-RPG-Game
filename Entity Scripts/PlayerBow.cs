using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBow : MonoBehaviour
{
    [Header("Shooting Settings")]
    public Transform launchPoint;
    public GameObject arrowPrefab;
    public PlayerMovement playerMovement;
    public float shootCooldown = 0.5f;
    public Animator anim;

    [Header("Aiming Settings")]
    public bool useMouseAiming = true;
    public float aimSmoothSpeed = 8f;
    public float aimDeadzone = 0.1f;
    [Range(4, 32)]
    public int aimDirections = 16; // Number of discrete aim directions

    private Vector2 aimDirection = Vector2.right;
    private Vector2 targetAimDirection = Vector2.right;
    private Vector2 lastValidInput = Vector2.right;
    private float shootTimer;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        shootTimer -= Time.deltaTime;
        HandleAiming();

        if (Input.GetButtonDown("Shoot") && shootTimer <= 0)
        {
            playerMovement.isShooting = true;
            anim.SetBool("isShooting", true);
        }
    }

    private void OnEnable()
    {
        if (playerMovement != null)
        {
            // Preserve the current facing direction when switching to archer mode
            aimDirection = playerMovement.facingDirection > 0 ? Vector2.right : Vector2.left;
            targetAimDirection = aimDirection;
            lastValidInput = aimDirection;

            // Make sure the animator gets the correct initial values
            anim.SetFloat("aimX", aimDirection.x);
            anim.SetFloat("aimY", aimDirection.y);

            Debug.Log($"Archer mode enabled, facing direction: {playerMovement.facingDirection}, aim direction: {aimDirection}");
        }
    }

    private void HandleAiming()
    {
        if (useMouseAiming && mainCamera != null)
        {
            HandleMouseAiming();
        }
        else
        {
            HandleKeyboardAiming();
        }

        // Smooth the aim direction with optional snapping
        if (aimDirections > 0)
        {
            // Snap to discrete directions for more predictable aiming
            targetAimDirection = SnapToDirection(targetAimDirection, aimDirections);
        }

        aimDirection = Vector2.Lerp(aimDirection, targetAimDirection, aimSmoothSpeed * Time.deltaTime);
        aimDirection = aimDirection.normalized;

        // Update animator with current aim direction
        anim.SetFloat("aimX", aimDirection.x);
        anim.SetFloat("aimY", aimDirection.y);
    }

    private void HandleMouseAiming()
    {
        // Get mouse position in world space
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Calculate direction from player to mouse
        Vector2 mouseDirection = (mousePosition - transform.position).normalized;

        // Check if mouse moved significantly
        if (mouseDirection.magnitude > 0.1f)
        {
            targetAimDirection = mouseDirection;
            lastValidInput = targetAimDirection;

            // Handle player facing direction based on mouse position
            if (playerMovement != null && Mathf.Abs(mouseDirection.x) > aimDeadzone)
            {
                if (mouseDirection.x > 0 && playerMovement.facingDirection < 0)
                {
                    playerMovement.transform.localScale = new Vector3(
                        Mathf.Abs(playerMovement.transform.localScale.x),
                        playerMovement.transform.localScale.y,
                        playerMovement.transform.localScale.z
                    );
                    playerMovement.facingDirection = 1;
                }
                else if (mouseDirection.x < 0 && playerMovement.facingDirection > 0)
                {
                    playerMovement.transform.localScale = new Vector3(
                        -Mathf.Abs(playerMovement.transform.localScale.x),
                        playerMovement.transform.localScale.y,
                        playerMovement.transform.localScale.z
                    );
                    playerMovement.facingDirection = -1;
                }
            }
        }
        else
        {
            // Fallback to last valid input if mouse is too close
            targetAimDirection = lastValidInput;
        }
    }

    private void HandleKeyboardAiming()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(horizontal) > aimDeadzone || Mathf.Abs(vertical) > aimDeadzone)
        {
            targetAimDirection = new Vector2(horizontal, vertical).normalized;
            lastValidInput = targetAimDirection;

            // Handle player facing direction based on keyboard input
            if (playerMovement != null && Mathf.Abs(horizontal) > aimDeadzone)
            {
                if (horizontal > 0 && playerMovement.facingDirection < 0)
                {
                    playerMovement.transform.localScale = new Vector3(
                        Mathf.Abs(playerMovement.transform.localScale.x),
                        playerMovement.transform.localScale.y,
                        playerMovement.transform.localScale.z
                    );
                    playerMovement.facingDirection = 1;
                }
                else if (horizontal < 0 && playerMovement.facingDirection > 0)
                {
                    playerMovement.transform.localScale = new Vector3(
                        -Mathf.Abs(playerMovement.transform.localScale.x),
                        playerMovement.transform.localScale.y,
                        playerMovement.transform.localScale.z
                    );
                    playerMovement.facingDirection = -1;
                }
            }
        }
        else
        {
            targetAimDirection = lastValidInput;
        }
    }

    // Snap direction to one of N discrete directions
    private Vector2 SnapToDirection(Vector2 direction, int numDirections)
    {
        if (numDirections <= 0) return direction;

        float angle = Mathf.Atan2(direction.y, direction.x);
        float angleStep = (2f * Mathf.PI) / numDirections;
        float snappedAngle = Mathf.Round(angle / angleStep) * angleStep;

        return new Vector2(Mathf.Cos(snappedAngle), Mathf.Sin(snappedAngle));
    }

    // This method is called by the animation event
    public void Shoot()
    {
        if (shootTimer <= 0)
        {
            // Use current aim direction for accurate shooting
            Vector2 shootDirection = aimDirection;

            // Ensure we have a valid direction
            if (shootDirection.magnitude < 0.1f)
            {
                shootDirection = playerMovement.facingDirection > 0 ? Vector2.right : Vector2.left;
            }

            // Create the arrow
            Arrow arrow = Instantiate(arrowPrefab, launchPoint.position, Quaternion.identity).GetComponent<Arrow>();
            if (arrow != null)
            {
                arrow.direction = shootDirection;

                // Copy stats from StatsManager if available
                if (StatsManager.Instance != null)
                {
                    arrow.damage = StatsManager.Instance.damage;
                    arrow.knockbackForce = StatsManager.Instance.knockbackForce;
                    arrow.knockbackTime = StatsManager.Instance.knockbackTime;
                    arrow.stunTime = StatsManager.Instance.stunTime;
                }
            }

            shootTimer = shootCooldown;
            anim.SetBool("isShooting", false);
            playerMovement.isShooting = false;

            Debug.Log($"Shot arrow in direction: {shootDirection}");
        }
    }

    // Get the current direction index (useful for animation systems)
    public int GetCurrentDirectionIndex()
    {
        if (aimDirections <= 0) return 0;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x);
        if (angle < 0) angle += 2f * Mathf.PI;

        float angleStep = (2f * Mathf.PI) / aimDirections;
        return Mathf.RoundToInt(angle / angleStep) % aimDirections;
    }

    // Get angle in degrees
    public float GetCurrentAngle()
    {
        return Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
    }

    // Public methods for external control
    public void SetUseMouseAiming(bool useMouseAiming)
    {
        this.useMouseAiming = useMouseAiming;
    }

    public Vector2 GetCurrentAimDirection()
    {
        return aimDirection;
    }

    public bool CanShoot()
    {
        return shootTimer <= 0;
    }

    // Gizmo for debugging aim direction
    private void OnDrawGizmos()
    {
        if (enabled && Application.isPlaying)
        {
            // Draw current aim direction
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, aimDirection * 2f);

            // Draw all possible aim directions
            if (aimDirections > 0)
            {
                Gizmos.color = Color.yellow;
                float angleStep = 360f / aimDirections;
                for (int i = 0; i < aimDirections; i++)
                {
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Gizmos.DrawRay(transform.position, dir * 1.5f);
                }
            }

            if (launchPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(launchPoint.position, 0.1f);
            }
        }
    }
}