using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchForm : MonoBehaviour
{
    public PlayerCombat combat;
    public PlayerBow bow;

    private Animator animator;
    private bool isArcherForm = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        SetCombatForm();
    }

    void Update()
    {
        if (Input.GetButtonDown("SwitchForm"))
        {
            if (isArcherForm)
                SetCombatForm();
            else
                SetArcherForm();
        }
    }

    private void SetCombatForm()
    {
        isArcherForm = false;

        bow.enabled = false;

        if (animator != null)
        {
            animator.SetLayerWeight(0, 1f);
            animator.SetLayerWeight(1, 0f);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isShooting", false);
            animator.SetFloat("aimX", 0f);
            animator.SetFloat("aimY", 0f);
        }

        combat.enabled = true;

        Debug.Log("Switched to Combat Form");
    }

    private void SetArcherForm()
    {
        isArcherForm = true;

        combat.enabled = false;

        if (animator != null)
        {
            animator.SetLayerWeight(0, 0f);
            animator.SetLayerWeight(1, 1f);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isShooting", false);


        }

        bow.enabled = true;

        Debug.Log("Switched to Archer Form");
    }

    public bool IsArcherForm()
    {
        return isArcherForm;
    }

}
