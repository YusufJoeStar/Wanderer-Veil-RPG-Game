using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Loot : MonoBehaviour
{
    public ItemSO itemSO;
    public SpriteRenderer sr;
    public Animator anim;
    public int quantity;
    public bool canBePickedUp = true;

    public static event Action<ItemSO, int> OnItemLooted;

    public void OnValidate()
    {
        if (itemSO == null)
            return;

        UpdateAppearance();
    }

    public void Initialize(ItemSO itemSO, int quantity)
    {
        this.itemSO = itemSO;
        this.quantity = quantity;

        // Fix pickup delay - don't disable pickup for dropped items
        // Only disable for manually placed items that need the trigger exit mechanic
        canBePickedUp = true;

        UpdateAppearance();

        // Set proper sorting layer for visibility
        if (sr != null)
        {
            sr.sortingLayerName = "UI"; // Change this to your highest sorting layer
            sr.sortingOrder = 100; // High value to appear above tilemaps
        }
    }

    private void UpdateAppearance()
    {
        if (sr != null)
            sr.sprite = itemSO.icon;
        this.name = itemSO.itemName;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canBePickedUp == true)
        {
            // Play pickup animation if available
            if (anim != null)
            {
                anim.Play("New Animation");
            }

            // Trigger the loot event
            OnItemLooted?.Invoke(itemSO, quantity);

            // Destroy immediately - no delay needed for dropped loot
            Destroy(gameObject, anim != null ? 0.5f : 0f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Only use this mechanic for manually placed loot in scenes
        // Dropped loot should be pickupable immediately
        if (collision.CompareTag("Player") && !wasDroppedByEnemy)
        {
            canBePickedUp = true;
        }
    }

    // Track if this was dropped by an enemy vs manually placed
    private bool wasDroppedByEnemy = false;

    /// <summary>
    /// Call this for enemy-dropped loot to bypass the exit trigger mechanic
    /// </summary>
    public void SetAsEnemyDrop()
    {
        wasDroppedByEnemy = true;
        canBePickedUp = true;
    }
}