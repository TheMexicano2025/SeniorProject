using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowInteraction : MonoBehaviour, Interactable
{
    private Cow cow;
    private Health health;

    private void Start()
    {
        cow = GetComponent<Cow>();
        health = GetComponent<Health>();
    }

    public bool CanInteract(GameObject player)
    {
        if (health != null && health.IsDead())
        {
            return false;
        }
        
        if (cow == null || cow.isBaby) return false;
        
        EquipManager equipManager = player.GetComponent<EquipManager>();
        if (equipManager == null) return false;
        
        ItemSO equippedItem = equipManager.GetEquippedItem();
        
        if (equippedItem == null) return false;
        
        bool hasBottle = equippedItem.toolType == ItemSO.ToolType.Bottle || 
                        equippedItem.itemName.ToLower().Contains("bottle");
        
        bool hasCorn = equippedItem.itemName.ToLower().Contains("corn");
        
        return (hasBottle && cow.canBeMilked) || (hasCorn && cow.canBreed);
    }

    public void Interact(GameObject player)
    {
        if (cow == null) return;
        
        EquipManager equipManager = player.GetComponent<EquipManager>();
        if (equipManager == null) return;
        
        ItemSO equippedItem = equipManager.GetEquippedItem();
        
        if (equippedItem == null) return;
        
        if (equippedItem.toolType == ItemSO.ToolType.Bottle || equippedItem.itemName.ToLower().Contains("bottle"))
        {
            if (cow.TryMilk(player, equippedItem))
            {
                equipManager.RefreshEquippedItem();
                Debug.Log("Successfully milked the cow!");
            }
        }
        else if (equippedItem.itemName.ToLower().Contains("corn"))
        {
            InvManager inventory = FindObjectOfType<InvManager>();
            if (inventory != null)
            {
                inventory.RemoveItem(equippedItem, 1);
                cow.EnterLoveMode();
                Debug.Log("Fed corn to cow! It's looking for a mate...");
            }
        }
    }

    public string GetInteractionPrompt()
    {
        if (health != null && health.IsDead())
        {
            return "";
        }
        
        if (cow == null || cow.isBaby) return "";
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return "";
        
        EquipManager equipManager = player.GetComponent<EquipManager>();
        if (equipManager == null) return "";
        
        ItemSO equippedItem = equipManager.GetEquippedItem();
        
        if (equippedItem == null) return "";
        
        bool hasBottle = equippedItem.toolType == ItemSO.ToolType.Bottle || 
                        equippedItem.itemName.ToLower().Contains("bottle");
        
        bool hasCorn = equippedItem.itemName.ToLower().Contains("corn");
        
        if (hasBottle && cow.canBeMilked)
        {
            return "Milk Cow [E]";
        }
        
        if (hasCorn && cow.canBreed)
        {
            return "Feed Corn [E]";
        }
        
        return "";
    }
}
