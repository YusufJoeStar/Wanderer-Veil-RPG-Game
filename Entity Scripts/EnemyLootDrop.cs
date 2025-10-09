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
    public float dropChance = 50f;

    [Header("Quantity")]
    public int minQuantity = 1;
    public int maxQuantity = 1;

    [Header("Special Drop (Gold)")]
    public bool isGuaranteedDrop = false;
}

public class EnemyLootDrop : MonoBehaviour
{
    [Header("Loot Configuration")]
    public List<LootDrop> possibleLoot = new List<LootDrop>();

    [Header("Drop Settings")]
    [Range(1, 5)]
    public int maxItemsToDrop = 2;
    public float dropSpreadRadius = 1f;

    [Header("References")]
    public GameObject lootPrefab;

    private void Start()
    {
        if (lootPrefab == null)
        {
            Debug.LogWarning($"EnemyLootDrop on {gameObject.name}: No loot prefab assigned!");
        }
        else if (lootPrefab.GetComponent<Loot>() == null)
        {
            Debug.LogError($"EnemyLootDrop on {gameObject.name}: Loot prefab must have a Loot component!");
        }
    }

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

    private List<LootDrop> DetermineLootToDrop()
    {
        List<LootDrop> selectedLoot = new List<LootDrop>();

        foreach (LootDrop lootDrop in possibleLoot)
        {
            if (lootDrop.isGuaranteedDrop && lootDrop.item != null)
            {
                selectedLoot.Add(lootDrop);
            }
        }

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

        for (int i = chanceBasedLoot.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            LootDrop temp = chanceBasedLoot[i];
            chanceBasedLoot[i] = chanceBasedLoot[randomIndex];
            chanceBasedLoot[randomIndex] = temp;
        }

        int remainingSlots = maxItemsToDrop - selectedLoot.Count;
        for (int i = 0; i < Mathf.Min(remainingSlots, chanceBasedLoot.Count); i++)
        {
            selectedLoot.Add(chanceBasedLoot[i]);
        }

        return selectedLoot;
    }

    private void SpawnLootItem(LootDrop lootDrop)
    {
        Vector2 randomOffset = Random.insideUnitCircle * dropSpreadRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        GameObject lootObject = Instantiate(lootPrefab, spawnPosition, Quaternion.identity);

        SpriteRenderer lootRenderer = lootObject.GetComponent<SpriteRenderer>();
        if (lootRenderer != null)
        {
            lootRenderer.sortingLayerName = "UI";
            lootRenderer.sortingOrder = 100;
        }

        Loot lootComponent = lootObject.GetComponent<Loot>();
        if (lootComponent != null)
        {
            int quantity = Random.Range(lootDrop.minQuantity, lootDrop.maxQuantity + 1);
            lootComponent.Initialize(lootDrop.item, quantity);
            lootComponent.SetAsEnemyDrop();
            Debug.Log($"Dropped {quantity}x {lootDrop.item.itemName} from {gameObject.name}");
        }
        else
        {
            Debug.LogError($"Loot prefab is missing Loot component!");
            Destroy(lootObject);
        }
    }

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
