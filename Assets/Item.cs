using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void PickupItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning("WorldItem has no ItemSO assigned");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning("Item: inventoryManager is null!");
            return;
        }

        int leftOverItems = inventoryManager.AddItem(itemData, quantity);

        if (leftOverItems <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            quantity = leftOverItems;
        }
    }
}