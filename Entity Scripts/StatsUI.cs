using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    public GameObject[] statsSlots;
    public CanvasGroup statsCanvas;
    private bool statsOpen = false;


    private void Start()
    {
        UpdateAllStats();
    }

    private void Update()
    {
        UpdateAllStats();
        if (Input.GetButtonDown("ToggleStats"))
            if (statsOpen)
            {
                Time.timeScale = 1;
                statsCanvas.alpha = 0;
                statsCanvas.blocksRaycasts = false;
                statsOpen = false;
            }
            else
            {
                Time.timeScale = 0;
                statsCanvas.alpha = 1;
                statsCanvas.blocksRaycasts = true;
                statsOpen = true;
            }
    }

    public void UpdateHealth()
    {
        statsSlots[0].GetComponentInChildren<TMP_Text>().text = "Health: " + StatsManager.Instance.maxHealth;
    }

    public void UpdateDamage()
    {
        statsSlots[1].GetComponentInChildren<TMP_Text>().text = "Damage: " + StatsManager.Instance.damage;
    }

    public void UpdateSpeed()
    {
        statsSlots[2].GetComponentInChildren<TMP_Text>().text = "Speed: " + StatsManager.Instance.speed;
    }

    public void UpdateAllStats()
    {
        UpdateHealth();
        UpdateDamage();
        UpdateSpeed();
    }


}