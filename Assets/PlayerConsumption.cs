using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConsumption : MonoBehaviour
{
    [Header("References")]
    public Health playerHealth;
    public InvManager inventoryManager;
    public EquipManager equipManager;
    public PlayerInteraction playerInteraction;
    
    [Header("Consumption Settings")]
    public KeyCode consumeKey = KeyCode.E;

    private void Start()
    {
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
        if (Input.GetKeyDown(consumeKey))
        {
            if (!IsLookingAtInteractable())
            {
                TryConsumeEquippedItem();
            }
        }
    }

    private bool IsLookingAtInteractable()
    {
        if (playerInteraction == null) return false;
        
        Camera mainCam = Camera.main;
        if (mainCam == null) return false;
        
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

    private void TryConsumeEquippedItem()
    {
        if (equipManager == null || playerHealth == null)
        {
            return;
        }
        
        ItemSO equippedItem = equipManager.GetEquippedItem();
        
        if (equippedItem == null)
        {
            return;
        }
        
        if (equippedItem.healAmount <= 0)
        {
            return;
        }
        
        if (playerHealth.GetCurrentHealth() >= playerHealth.GetMaxHealth())
        {
            Debug.Log("Already at full health!");
            return;
        }
        
        playerHealth.Heal(equippedItem.healAmount);
        
        if (inventoryManager != null)
        {
            inventoryManager.RemoveItem(equippedItem, 1);
        }
        
        Debug.Log($"Consumed {equippedItem.itemName} and restored {equippedItem.healAmount} health!");
    }

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
        
        Debug.Log($"Consumed {item.itemName} and restored {item.healAmount} health!");
    }
}
