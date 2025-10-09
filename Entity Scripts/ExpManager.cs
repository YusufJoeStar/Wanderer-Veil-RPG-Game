using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ExpManager : MonoBehaviour
{
    public int level = 1;
    public int currentExp;
    public int exptoLevel = 10;
    public float expGrowthMultiplier = 1.2f;
    public Slider expSlider;
    public TMP_Text currentLevelText;

    public static event Action<int> OnLevelUp;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GainExperience(2);
        }
    }

    private void OnEnable()
    {
        EnemyHealth.OnMonsterDefeated += GainExperience;
    }

    private void OnDisable()
    {
        EnemyHealth.OnMonsterDefeated -= GainExperience;
    }

    public void GainExperience(int amount)
    {
        currentExp += amount;
        CheckForLevelUp();
        UpdateUI();
    }

    private void CheckForLevelUp()
    {
        while (currentExp >= exptoLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        currentExp -= exptoLevel;
        exptoLevel = Mathf.RoundToInt(exptoLevel * expGrowthMultiplier);
        OnLevelUp?.Invoke(1);
    }

    private void UpdateUI()
    {
        expSlider.maxValue = exptoLevel;
        expSlider.value = currentExp;
        currentLevelText.text = "Level: " + level;
    }
}