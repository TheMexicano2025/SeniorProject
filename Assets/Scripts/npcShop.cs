using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script makes an NPC open a shop when you interact with them
public class ShopNPC : MonoBehaviour, Interactable
{
    [Header("Shop Settings")]
    [SerializeField] private string shopName = "Merchant";
    
    [Header("Shop Inventory")]
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
    }
}
