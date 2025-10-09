using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private Transform player;
    private int facingDirection = -1;
    private Animator anim;
    private Vector2 startPosition;
    private EnemyState enemyState;
    private float attackCooldownTimer;
    private float attackDurationTimer;
    private bool isAttackInProgress;

    public float moveSpeed = 4f;
    public float attackRange = 2;
    public float attackCooldown = 2;
    public float attackDuration = 1f;
    public float playerDetectRange = 5;
    public Transform detectionPoint;
    public LayerMask playerLayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ChangeState(EnemyState.Idle);
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        startPosition = transform.position;
    }

    private void Update()
    {
        if (enemyState != EnemyState.Knockback)
        {
            if (attackCooldownTimer > 0)
            {
                attackCooldownTimer -= Time.deltaTime;
            }

            if (isAttackInProgress)
            {
                attackDurationTimer -= Time.deltaTime;
                rb.velocity = Vector2.zero; // Changed from linearVelocity

                if (attackDurationTimer <= 0)
                {
                    isAttackInProgress = false;
                }
            }

            if (!isAttackInProgress)
            {
                CheckForPlayer();
                CheckIfChasing();
            }

            if (enemyState == EnemyState.Attacking && !isAttackInProgress)
            {
                rb.velocity = Vector2.zero; // Changed from linearVelocity
            }
        }
    }

    private void CheckIfChasing()
    {
        if (enemyState == EnemyState.Chasing)
        {
            if (player.position.x > transform.position.x && facingDirection == -1 ||
                player.position.x < transform.position.x && facingDirection == 1)
            {
                Flip();
            }
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed; // Changed from linearVelocity
        }
        else if (enemyState == EnemyState.Idle)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);
            if (hits.Length == 0)
            {
                Vector2 direction = (startPosition - (Vector2)transform.position).normalized;

                if (Vector2.Distance(transform.position, startPosition) > 0.1f)
                {
                    if (startPosition.x > transform.position.x && facingDirection == -1 ||
                        startPosition.x < transform.position.x && facingDirection == 1)
                    {
                        Flip();
                    }

                    ChangeState(EnemyState.Returning);
                    rb.velocity = direction * moveSpeed; // Changed from linearVelocity
                }
                else
                {
                    rb.velocity = Vector2.zero; // Changed from linearVelocity
                }
            }
            else
            {
                rb.velocity = Vector2.zero; // Changed from linearVelocity
            }
        }
        else if (enemyState == EnemyState.Returning)
        {
            Vector2 direction = (startPosition - (Vector2)transform.position).normalized;

            if (Vector2.Distance(transform.position, startPosition) > 0.1f)
            {
                if (startPosition.x > transform.position.x && facingDirection == -1 ||
                    startPosition.x < transform.position.x && facingDirection == 1)
                {
                    Flip();
                }
                rb.velocity = direction * moveSpeed; // Changed from linearVelocity
            }
            else
            {
                rb.velocity = Vector2.zero; // Changed from linearVelocity
                ChangeState(EnemyState.Idle);
            }
        }
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    private void CheckForPlayer()
    {
        if (isAttackInProgress) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);
        if (hits.Length > 0)
        {
            Transform detectedPlayer = hits[0].transform;
            float distanceToPlayer = Vector2.Distance(transform.position, detectedPlayer.position);

            if (distanceToPlayer <= attackRange && attackCooldownTimer <= 0)
            {
                attackCooldownTimer = attackCooldown;
                attackDurationTimer = attackDuration;
                isAttackInProgress = true;
                ChangeState(EnemyState.Attacking);
            }
            else if (distanceToPlayer > attackRange && distanceToPlayer <= playerDetectRange)
            {
                ChangeState(EnemyState.Chasing);
            }
            else if (distanceToPlayer <= attackRange && attackCooldownTimer > 0)
            {
                rb.velocity = Vector2.zero; // Changed from linearVelocity
                if (enemyState != EnemyState.Attacking)
                {
                    ChangeState(EnemyState.Idle);
                }
            }
        }
        else
        {
            ChangeState(EnemyState.Idle);
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if (enemyState == EnemyState.Idle)
            anim.SetBool("isIdle", false);
        else if (enemyState == EnemyState.Chasing)
            anim.SetBool("isChasing", false);
        else if (enemyState == EnemyState.Attacking)
            anim.SetBool("isAttacking", false);
        else if (enemyState == EnemyState.Returning)
            anim.SetBool("isChasing", false);

        enemyState = newState;

        if (enemyState == EnemyState.Idle)
            anim.SetBool("isIdle", true);
        else if (enemyState == EnemyState.Chasing)
            anim.SetBool("isChasing", true);
        else if (enemyState == EnemyState.Attacking)
            anim.SetBool("isAttacking", true);
        else if (enemyState == EnemyState.Returning)
            anim.SetBool("isChasing", true);
    }
}

public enum EnemyState
{
    Idle,
    Chasing,
    Returning,
    Attacking,
    Knockback
}