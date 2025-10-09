using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Patrol : MonoBehaviour
{
    public float pauseDuration = 1.5f;
    public Vector2[] patrolPoints;
    public float speed = 2f;

    private bool isPaused;
    private int currentPatrolIndex;
    private Rigidbody2D rb;
    private Vector2 target;
    private Animator anim;
    private bool isSettingPatrolPoint = false;
    private bool goingForward = true;
    private Transform spriteTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get from parent
        anim = GetComponentInChildren<Animator>();
        spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.mass = 100f;
            rb.drag = 5f;
        }

        if (patrolPoints.Length > 0)
        {
            currentPatrolIndex = 0;
            target = patrolPoints[currentPatrolIndex];
        }
    }

    private void OnEnable()
    {
        // When patrol is re-enabled, resume walking
        if (rb != null)
        {
            rb.isKinematic = false;
            StartCoroutine(StartMoving());
        }
    }

    IEnumerator StartMoving()
    {
        yield return new WaitForSeconds(0.1f);
        isPaused = false;
        if (anim != null) anim.Play("Walk");
    }

    void Update()
    {
        if (isPaused || patrolPoints.Length == 0 || rb == null)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = (target - (Vector2)spriteTransform.position).normalized;

        // Flip the sprite based on movement direction
        if (direction.x < 0 && spriteTransform.localScale.x > 0 || direction.x > 0 && spriteTransform.localScale.x < 0)
            spriteTransform.localScale = new Vector3(spriteTransform.localScale.x * -1, spriteTransform.localScale.y, spriteTransform.localScale.z);

        rb.velocity = direction * speed;

        if (Vector2.Distance(spriteTransform.position, target) < 0.2f && !isSettingPatrolPoint)
        {
            StartCoroutine(SetPatrolPoint());
        }
    }

    IEnumerator SetPatrolPoint()
    {
        isSettingPatrolPoint = true;
        isPaused = true;
        if (anim != null) anim.Play("Idle");
        yield return new WaitForSeconds(pauseDuration);

        if (goingForward)
        {
            currentPatrolIndex++;
            if (currentPatrolIndex >= patrolPoints.Length - 1)
            {
                currentPatrolIndex = patrolPoints.Length - 1;
                goingForward = false;
            }
        }
        else
        {
            currentPatrolIndex--;
            if (currentPatrolIndex <= 0)
            {
                currentPatrolIndex = 0;
                goingForward = true;
            }
        }

        target = patrolPoints[currentPatrolIndex];
        isPaused = false;
        isSettingPatrolPoint = false;
        if (anim != null) anim.Play("Walk");
    }

    private void OnDisable()
    {
        // Stop all coroutines when disabled
        StopAllCoroutines();
        isSettingPatrolPoint = false;
        isPaused = false;
    }

    void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.blue;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Gizmos.DrawWireSphere(patrolPoints[i], 0.3f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(patrolPoints[i] + Vector2.up * 0.5f, i.ToString());
#endif
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            int nextIndex = (i + 1) % patrolPoints.Length;
            Gizmos.DrawLine(patrolPoints[i], patrolPoints[nextIndex]);
        }

        if (Application.isPlaying && patrolPoints.Length > 0 && spriteTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(spriteTransform.position, target);
            Gizmos.DrawWireSphere(target, 0.2f);
        }
    }
}