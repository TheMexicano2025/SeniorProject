using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script lets the player eat food items to restore health
// it checks if you're looking at something interactable before eating
public class PlayerConsumption : MonoBehaviour
{
    [Header("References")]
    public Health playerHealth;
    public InvManager inventoryManager;
    public EquipManager equipManager;
    public PlayerInteraction playerInteraction;
    
    [Header("Consumption Settings")]
    public KeyCode consumeKey = KeyCode.E; // same key as interact but context matters

    private void Start()
    {
        // try to find all the components we need
        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
        }
        
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InvManager>();
        }
        
        if (equipManager == null)
        {
            equipManager = GetComponent<EquipManager>();
        }
        
        if (playerInteraction == null)
        {
            playerInteraction = GetComponent<PlayerInteraction>();
        }
    }

    private void Update()
    {
        // only try to consume if E is pressed
        if (Input.GetKeyDown(consumeKey))
        {
            // don't eat if we're looking at something we can interact with
            if (!IsLookingAtInteractable())
            {
                TryConsumeEquippedItem();
            }
        }
    }

    // check if we're looking at something interactable like a cow or door
    private bool IsLookingAtInteractable()
    {
        if (playerInteraction == null) return false;
        
        Camera mainCam = Camera.main;
        if (mainCam == null) return false;
        
        // shoot a ray from camera to see what we're looking at
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, playerInteraction.interactionRange, playerInteraction.interactableLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null && interactable.CanInteract(gameObject))
            {
                return true;
            }
        }
        
        return false;
    }

    // try to eat the item currently in your hand
    private void TryConsumeEquippedItem()
    {
        if (equipManager == null || playerHealth == null)
        {
            return;
        }
        
        ItemSO equippedItem = equipManager.GetEquippedItem();
        
        // can't eat if no item or item doesn't heal
        if (equippedItem == null || equippedItem.healAmount <= 0)
        {
            return;
        }
        
        // don't eat if already at full health
        if (playerHealth.GetCurrentHealth() >= playerHealth.GetMaxHealth())
        {
            return;
        }
        
        // heal the player and remove the item from inventory
        playerHealth.Heal(equippedItem.healAmount);
        
        if (inventoryManager != null)
        {
            inventoryManager.RemoveItem(equippedItem, 1);
        }
    }

    // other scripts can call this to make the player consume a specific item
    public void ConsumeItem(ItemSO item)
    {
        if (item == null || item.healAmount <= 0 || playerHealth == null)
        {
            return;
        }
        
        playerHealth.Heal(item.healAmount);
        
        if (inventoryManager != null)
        {
            inventoryManager.RemoveItem(item, 1);
        }
    }
}
