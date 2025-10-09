using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
   private void OnEnable()
    {
        SkillSlot.OnAbilityPointSpent += HandleAbilityPointSpent;
    }

    private void OnDisable()
    {
        SkillSlot.OnAbilityPointSpent -= HandleAbilityPointSpent;
    }


    private void HandleAbilityPointSpent(SkillSlot slot)
    {
        string skillName = slot.skillSO.skillName;
        
        switch(skillName)
        {
            case "Max Health Boost":
                StatsManager.Instance.UpdateMaxHealth(2);
                break;

            default:
                Debug.LogWarning("Unknown skill: " + skillName);
                break;
        }
    }
}
