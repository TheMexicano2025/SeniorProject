using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour, Interactable
{
    [Header("Shop Settings")]
    [SerializeField] private string shopName = "Merchant";
    
    [Header("Shop Inventory")]
    [Tooltip("Items available for purchase")]
    [SerializeField] private ItemSO[] itemsForSale;

    private ShopUI shopUI;

    private void Start()
    {
        shopUI = FindObjectOfType<ShopUI>(true);
    }

    public string GetInteractionPrompt()
    {
        return $"Press E to trade with {shopName}";
    }

    public bool CanInteract(GameObject player)
    {
        return true;
    }

    public void Interact(GameObject player)
    {
        if (shopUI != null)
        {
            shopUI.OpenShop(itemsForSale, shopName);
        }
        else
        {
            Debug.LogWarning("ShopUI not found in scene!");
        }
    }
}

