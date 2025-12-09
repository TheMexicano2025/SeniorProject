using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script handles player interactions with cows
// you can milk them with a bottle or feed them corn to breed
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
        // can't interact with dead cows
        if (health != null && health.IsDead())
        {
            return false;
        }
        
        // can't interact with baby cows
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
        
        // milk the cow if holding a bottle
        if (equippedItem.toolType == ItemSO.ToolType.Bottle || equippedItem.itemName.ToLower().Contains("bottle"))
        {
            if (cow.TryMilk(player, equippedItem))
            {
                equipManager.RefreshEquippedItem();
            }
        }
        // feed corn to enter love mode
        else if (equippedItem.itemName.ToLower().Contains("corn"))
        {
            InvManager inventory = FindObjectOfType<InvManager>();
            if (inventory != null)
            {
                inventory.RemoveItem(equippedItem, 1);
                cow.EnterLoveMode();
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
