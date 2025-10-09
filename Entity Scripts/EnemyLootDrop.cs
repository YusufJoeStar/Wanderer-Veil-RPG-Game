using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootDrop
{
    [Header("Loot Item")]
    public ItemSO item;

    [Header("Drop Settings")]
    [Range(0f, 100f)]
    public float dropChance = 50f; // Percentage chance to drop this item

    [Header("Quantity")]
    public int minQuantity = 1;
    public int maxQuantity = 1;

    [Header("Special Drop (Gold)")]
    public bool isGuaranteedDrop = false; // If true, this item always drops regardless of chance
}

public class EnemyLootDrop : MonoBehaviour
{
    [Header("Loot Configuration")]
    [Tooltip("List of possible items this enemy can drop")]
    public List<LootDrop> possibleLoot = new List<LootDrop>();

    [Header("Drop Settings")]
    [Tooltip("Maximum number of different items that can drop at once")]
    [Range(1, 5)]
    public int maxItemsToDrop = 2;

    [Tooltip("Spread radius for dropped items")]
    public float dropSpreadRadius = 1f;

    [Header("References")]
    [Tooltip("The loot prefab to spawn (should have Loot component)")]
    public GameObject lootPrefab;

    private void Start()
    {
        // Validate loot prefab
        if (lootPrefab == null)
        {
            Debug.LogWarning($"EnemyLootDrop on {gameObject.name}: No loot prefab assigned!");
        }
        else if (lootPrefab.GetComponent<Loot>() == null)
        {
            Debug.LogError($"EnemyLootDrop on {gameObject.name}: Loot prefab must have a Loot component!");
        }
    }

    /// <summary>
    /// Call this method when the enemy dies to drop loot
    /// </summary>
    public void DropLoot()
    {
        if (possibleLoot == null || possibleLoot.Count == 0)
        {
            Debug.Log($"No loot configured for {gameObject.name}");
            return;
        }

        if (lootPrefab == null)
        {
            Debug.LogWarning($"Cannot drop loot for {gameObject.name}: No loot prefab assigned!");
            return;
        }

        List<LootDrop> itemsToDrop = DetermineLootToDrop();

        foreach (LootDrop lootDrop in itemsToDrop)
        {
            SpawnLootItem(lootDrop);
        }
    }

    /// <summary>
    /// Determines which items should drop based on drop chances
    /// </summary>
    private List<LootDrop> DetermineLootToDrop()
    {
        List<LootDrop> selectedLoot = new List<LootDrop>();

        // First, add all guaranteed drops
        foreach (LootDrop lootDrop in possibleLoot)
        {
            if (lootDrop.isGuaranteedDrop && lootDrop.item != null)
            {
                selectedLoot.Add(lootDrop);
            }
        }

        // Then, roll for chance-based drops
        List<LootDrop> chanceBasedLoot = new List<LootDrop>();
        foreach (LootDrop lootDrop in possibleLoot)
        {
            if (!lootDrop.isGuaranteedDrop && lootDrop.item != null)
            {
                float roll = Random.Range(0f, 100f);
                if (roll <= lootDrop.dropChance)
                {
                    chanceBasedLoot.Add(lootDrop);
                }
            }
        }

        // Shuffle the chance-based loot and add up to the limit
        for (int i = chanceBasedLoot.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            LootDrop temp = chanceBasedLoot[i];
            chanceBasedLoot[i] = chanceBasedLoot[randomIndex];
            chanceBasedLoot[randomIndex] = temp;
        }

        // Add chance-based loot up to the maximum limit
        int remainingSlots = maxItemsToDrop - selectedLoot.Count;
        for (int i = 0; i < Mathf.Min(remainingSlots, chanceBasedLoot.Count); i++)
        {
            selectedLoot.Add(chanceBasedLoot[i]);
        }

        return selectedLoot;
    }

    /// <summary>
    /// Spawns a loot item at a random position near the enemy
    /// </summary>
    private void SpawnLootItem(LootDrop lootDrop)
    {
        // Calculate random position within spread radius
        Vector2 randomOffset = Random.insideUnitCircle * dropSpreadRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        // Spawn the loot object
        GameObject lootObject = Instantiate(lootPrefab, spawnPosition, Quaternion.identity);

        // Fix sorting layer issue - set high sorting order
        SpriteRenderer lootRenderer = lootObject.GetComponent<SpriteRenderer>();
        if (lootRenderer != null)
        {
            lootRenderer.sortingLayerName = "UI"; // Or use your highest sorting layer
            lootRenderer.sortingOrder = 100; // High value to appear above tilemaps
        }

        // Get the Loot component and initialize it
        Loot lootComponent = lootObject.GetComponent<Loot>();
        if (lootComponent != null)
        {
            // Calculate random quantity within the specified range
            int quantity = Random.Range(lootDrop.minQuantity, lootDrop.maxQuantity + 1);

            // Initialize the loot item - fix pickup delay by setting canBePickedUp to true
            lootComponent.Initialize(lootDrop.item, quantity);
            lootComponent.SetAsEnemyDrop(); // Mark as enemy drop for immediate pickup

            Debug.Log($"Dropped {quantity}x {lootDrop.item.itemName} from {gameObject.name}");
        }
        else
        {
            Debug.LogError($"Loot prefab is missing Loot component!");
            Destroy(lootObject);
        }
    }

    /// <summary>
    /// Helper method to add a new loot drop via code (useful for dynamic loot assignment)
    /// </summary>
    public void AddLootDrop(ItemSO item, float dropChance, int minQty = 1, int maxQty = 1, bool guaranteed = false)
    {
        LootDrop newLoot = new LootDrop
        {
            item = item,
            dropChance = dropChance,
            minQuantity = minQty,
            maxQuantity = maxQty,
            isGuaranteedDrop = guaranteed
        };

        possibleLoot.Add(newLoot);
    }

    /// <summary>
    /// Preview method for testing in the editor
    /// </summary>
    [ContextMenu("Test Drop Loot")]
    private void TestDropLoot()
    {
        if (Application.isPlaying)
        {
            DropLoot();
        }
        else
        {
            Debug.Log("Test Drop Loot only works in Play Mode");
        }
    }
}