using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Wander : MonoBehaviour
{
    [Header("Wander Area")]
    public float wanderWidth = 5;
    public float wanderHeight = 5;
    [HideInInspector] public Vector2 startingPosition;
    public float speed = 2;
    public Vector2 target;
    public float pauseDuration = 1;

    private Rigidbody2D rb;
    private bool isPaused;
    private Animator anim;
    private Transform spriteTransform;

    private void Awake()
    {
        // Get the rigidbody from the parent (this object)
        rb = GetComponent<Rigidbody2D>();

        // Get animator and sprite transform from children
        anim = GetComponentInChildren<Animator>();
        spriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
    }

    private void Start()
    {
        // Use the sprite's position as the starting position for wandering
        if (rb != null && spriteTransform != null)
        {
            startingPosition = spriteTransform.position;
            rb.freezeRotation = true;
            rb.mass = 100f;
            rb.drag = 5f;
        }
        else
        {
            Debug.LogError("Missing Rigidbody2D or SpriteRenderer on " + gameObject.name);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(PauseAndPickNewDestination());
    }

    private void Update()
    {
        if (isPaused || rb == null || spriteTransform == null)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        if (Vector2.Distance(spriteTransform.position, target) < 0.1f)
            StartCoroutine(PauseAndPickNewDestination());

        Move();
    }

    private void Move()
    {
        Vector2 direction = (target - (Vector2)spriteTransform.position).normalized;

        // Flip the sprite based on movement direction
        if (direction.x > 0 && spriteTransform.localScale.x < 0 || direction.x < 0 && spriteTransform.localScale.x > 0)
            spriteTransform.localScale = new Vector3(spriteTransform.localScale.x * -1, spriteTransform.localScale.y, spriteTransform.localScale.z);

        rb.velocity = direction * speed;
    }

    IEnumerator PauseAndPickNewDestination()
    {
        isPaused = true;
        if (anim != null) anim.Play("Idle");
        yield return new WaitForSeconds(pauseDuration);

        target = GetRandomTarget();
        isPaused = false;
        if (anim != null) anim.Play("Walk");
    }

    private Vector2 GetRandomTarget()
    {
        float halfWidth = wanderWidth / 2;
        float halfHeight = wanderHeight / 2;
        int edge = Random.Range(0, 4);

        return edge switch
        {
            0 => new Vector2(startingPosition.x - halfWidth, Random.Range(startingPosition.y - halfHeight, startingPosition.y + halfHeight)),
            1 => new Vector2(startingPosition.x + halfWidth, Random.Range(startingPosition.y - halfHeight, startingPosition.y + halfHeight)),
            2 => new Vector2(Random.Range(startingPosition.x - halfWidth, startingPosition.x + halfWidth), startingPosition.y - halfHeight),
            _ => new Vector2(Random.Range(startingPosition.x - halfWidth, startingPosition.x + halfWidth), startingPosition.y + halfHeight),
        };
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (!enabled) return;
        StartCoroutine(PauseAndPickNewDestination());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 centerPoint;
        if (Application.isPlaying)
        {
            centerPoint = startingPosition;
        }
        else if (spriteTransform != null)
        {
            centerPoint = spriteTransform.position;
        }
        else
        {
            // Fallback to finding sprite renderer position
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            centerPoint = sr != null ? sr.transform.position : transform.position;
        }

        Gizmos.DrawWireCube(centerPoint, new Vector3(wanderWidth, wanderHeight, 0));
    }
}