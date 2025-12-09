using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is a scriptable object that holds all the data for an item
// you can create new items in the project window with right click > inventory > item
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO: ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite itemSprite;

    [TextArea]
    public string itemDescription;

    public ItemType itemType;
    public ToolType toolType;
    public SeedType seedType;

    public int itemStat;
    public int maxStackSize = 99;

    public GameObject ItemPrefab;

    [Header("Seed Settings")]
    [Tooltip("What item this seed produces when harvested")]
    public ItemSO harvestItem;
    
    [Tooltip("The seed item to return when harvesting (usually the same seed)")]
    public ItemSO seedItem;
    
    [Tooltip("How many crops to give when harvested")]
    public int harvestYield = 1;
    
    [Tooltip("How many seeds to give when harvested")]
    public int seedYield = 1;

    [Header("Economy Settings")]
    [Tooltip("How much it costs to buy this item from the shop")]
    public int buyPrice = 0;
    
    [Tooltip("How much the shop will pay you for this item")]
    public int sellPrice = 0;
    
    [Tooltip("Can this item be bought from the shop?")]
    public bool canBuy = false;
    
    [Tooltip("Can this item be sold to the shop?")]
    public bool canSell = true;

    [Header("Combat Properties")]
    [Tooltip("Damage dealt when attacking (for weapons)")]
    public float attackDamage = 0f;

    [Tooltip("Health restored when consumed (for food)")]
    public float healAmount = 0f;

    public enum ItemType
    {
        None,
        Sellable,
        Healing,
        Equipment
    }

    public enum ToolType
    {
        None,
        Hoe,
        Sword,
        WaterCan,
        Bottle
    }

    public enum SeedType
    {
        None,
        CornSeed
    }
}
