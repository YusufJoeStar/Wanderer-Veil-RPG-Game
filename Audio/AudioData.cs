using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "Audio/Audio Data")]
public class AudioData : ScriptableObject
{
    [Header("Music")]
    public AudioClip gameMusic;
    public AudioClip combatMusic;
    public AudioClip shopMusic;
    public AudioClip mainMenuMusic;

    [Header("Combat - Player")]
    public AudioClip swordSwing;
    public AudioClip heavySwordSwing;
    public AudioClip arrowShoot;
    public AudioClip arrowHitEnemy;
    public AudioClip arrowHitWall;

    [Header("Combat - Damage")]
    public AudioClip playerHit;
    public AudioClip playerDeath;
    public AudioClip enemyHit;
    public AudioClip enemyDeath;
    public AudioClip knockbackImpact;

    [Header("Movement")]
    public AudioClip[] footsteps;
    public AudioClip land;
    public AudioClip jump;

    [Header("Items & Inventory")]
    public AudioClip itemPickup;
    public AudioClip goldPickup;
    public AudioClip itemUse;
    public AudioClip itemDrop;
    public AudioClip inventoryFull;
    public AudioClip potionDrink;

    [Header("Shop")]
    public AudioClip shopOpen;
    public AudioClip shopClose;
    public AudioClip purchaseSuccess;
    public AudioClip purchaseFail;
    public AudioClip sellItem;
    public AudioClip buttonClick;
    public AudioClip itemHover;

    [Header("Teleportation")]
    public AudioClip teleportActivate;
    public AudioClip teleportWhoosh;
    public AudioClip teleportArrive;

    [Header("UI")]
    public AudioClip levelUp;
    public AudioClip formSwitch;
    public AudioClip menuOpen;
    public AudioClip menuClose;

    // Dictionary for quick lookup
    private Dictionary<string, AudioClip> sfxDictionary;

    public AudioClip GetSFX(string soundName)
    {
        // Build dictionary on first access
        if (sfxDictionary == null)
        {
            BuildDictionary();
        }

        if (sfxDictionary.ContainsKey(soundName))
        {
            return sfxDictionary[soundName];
        }

        Debug.LogWarning($"AudioData: Sound '{soundName}' not found!");
        return null;
    }

    private void BuildDictionary()
    {
        sfxDictionary = new Dictionary<string, AudioClip>
        {
            // Combat - Player
            { "SwordSwing", swordSwing },
            { "HeavySwordSwing", heavySwordSwing },
            { "ArrowShoot", arrowShoot },
            { "ArrowHitEnemy", arrowHitEnemy },
            { "ArrowHitWall", arrowHitWall },

            // Combat - Damage
            { "PlayerHit", playerHit },
            { "PlayerDeath", playerDeath },
            { "EnemyHit", enemyHit },
            { "EnemyDeath", enemyDeath },
            { "KnockbackImpact", knockbackImpact },

            // Movement
            { "Land", land },
            { "Jump", jump },

            // Items
            { "ItemPickup", itemPickup },
            { "GoldPickup", goldPickup },
            { "ItemUse", itemUse },
            { "ItemDrop", itemDrop },
            { "InventoryFull", inventoryFull },
            { "PotionDrink", potionDrink },

            // Shop
            { "ShopOpen", shopOpen },
            { "ShopClose", shopClose },
            { "PurchaseSuccess", purchaseSuccess },
            { "PurchaseFail", purchaseFail },
            { "SellItem", sellItem },
            { "ButtonClick", buttonClick },
            { "ItemHover", itemHover },

            // Teleportation
            { "TeleportActivate", teleportActivate },
            { "TeleportWhoosh", teleportWhoosh },
            { "TeleportArrive", teleportArrive },

            // UI
            { "LevelUp", levelUp },
            { "FormSwitch", formSwitch },
            { "MenuOpen", menuOpen },
            { "MenuClose", menuClose }
        };
    }
}