using System.Collections;
using UnityEngine;

public class EnemyDeathAnimator : MonoBehaviour
{
    [Header("Death Animation Settings")]
    [Tooltip("The name of the death animation in the Animator Controller")]
    public string deathAnimationName = "EnemyDeath";

    [Tooltip("How long to wait before destroying the enemy after animation begins")]
    public float destroyDelay = 2f;

    [Tooltip("If true, the enemy will be destroyed automatically after the animation")]
    public bool autoDestroy = true;

    private Animator animator;
    private EnemyLootDrop lootDrop;
    private bool isPlayingDeathAnimation = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        lootDrop = GetComponent<EnemyLootDrop>();

        if (animator == null)
        {
            Debug.LogWarning("EnemyDeathAnimator: No Animator component found on " + gameObject.name);
        }
    }


    public void PlayDeathAnimation()
    {
        if (isPlayingDeathAnimation) return;

        isPlayingDeathAnimation = true;

        DisableEnemyControls();

        if (lootDrop != null)
        {
            lootDrop.DropLoot();
        }

        if (animator != null)
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isChasing", false);
            animator.SetBool("isAttacking", false);

           
            animator.SetFloat("horizontal", 0);
            animator.SetFloat("vertical", 0);

          
            animator.SetBool("isDead", true);

            Debug.Log("Playing enemy death animation: " + deathAnimationName);
        }

 
        if (autoDestroy)
        {
            StartCoroutine(HandleDeathSequence());
        }
    }


    private void DisableEnemyControls()
    {
        EnemyMove enemyMovement = GetComponent<EnemyMove>();
        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        EnemyCombat enemyCombat = GetComponent<EnemyCombat>();
        if (enemyCombat != null)
        {
            enemyCombat.enabled = false;
        }

        EnemyKnockBack enemyKnockback = GetComponent<EnemyKnockBack>();
        if (enemyKnockback != null)
        {
            enemyKnockback.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

 
    private IEnumerator HandleDeathSequence()
    {
        float animationWaitTime = GetAnimationLength();

        if (animationWaitTime > 0)
        {
            yield return new WaitForSeconds(animationWaitTime);
        }
        else
        {
            yield return new WaitForSeconds(destroyDelay);
        }

        Destroy(gameObject);
    }


    private float GetAnimationLength()
    {
        if (animator == null) return 0f;

        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        if (ac != null)
        {
            foreach (AnimationClip clip in ac.animationClips)
            {
                if (clip.name == deathAnimationName)
                {
                    return clip.length;
                }
            }
        }

        return 0f; 
    }


    public void OnDeathAnimationComplete()
    {
        Debug.Log("Enemy death animation completed");

        if (!autoDestroy)
        {
            Destroy(gameObject);
        }
    }


    public void TriggerLootDrop()
    {
        if (lootDrop != null)
        {
            lootDrop.DropLoot();
        }
    }

 
    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

 
    public bool IsPlayingDeathAnimation()
    {
        return isPlayingDeathAnimation;
    }


    public bool IsDeathAnimationFinished()
    {
        if (animator == null) return true;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(deathAnimationName) && stateInfo.normalizedTime >= 1.0f;
    }

}
