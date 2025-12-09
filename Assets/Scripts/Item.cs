using UnityEngine;

// this script represents a physical item in the world
// when you press E near it it gets added to your inventory
public class Item : MonoBehaviour, Interactable
{
    public ItemSO itemData;
    public int quantity = 1;

    private InvManager inventoryManager;
    private const float itemRange = 2f;

    void Start()
    {
        inventoryManager = GameObject.Find("InventoryCanvas").GetComponent<InvManager>();
    }

    public string GetInteractionPrompt()
    {
        if (itemData != null)
        {
            return $"Press E to Pick Up {itemData.itemName}";
        }
        return "Press E to Pick Up Item";
    }

    public bool CanInteract(GameObject player)
    {
        return itemData != null && inventoryManager != null;
    }

    public void Interact(GameObject player)
    {
        PickupItem();
    }

    // add item to inventory and destroy this object
    private void PickupItem()
    {
        if (itemData == null || inventoryManager == null) return;

        int leftOverItems = inventoryManager.AddItem(itemData, quantity);

        // if all items fit in inventory destroy this
        if (leftOverItems <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            // if inventory full only some items picked up
            quantity = leftOverItems;
        }
    }
}
