using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script displays the currently equipped item in the player's hand
// it syncs with the hotbar to show what you have selected
public class EquipManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag the HandPosition here")]
    public Transform handPosition;

    [Tooltip("Drag the HotbarManager here")]
    public HotbarSelect HBManager;

    [Tooltip("Drag the InvManager here")]
    public InvManager inventoryManager;

    [Header("Equipped Item")]
    [Tooltip("Default size for the item equipped")]
    public float defaultScale = 0.3f;

    private GameObject currentItem;
    private int lastSlot = -1;
    private ItemSO currentEquippedItem;

    void Update()
    {
        updateEquip();
    }

    // check if the equipped item needs to update
    private void updateEquip()
    {
        if (HBManager == null || inventoryManager == null || handPosition == null)
            return;
        
        int selectedSlot = HBManager.getSelect();

        // hotbar slot changed
        if (selectedSlot != lastSlot)
        {
            lastSlot = selectedSlot;
            itemFromSlot(selectedSlot);
        }
        else if (selectedSlot >= 0 && selectedSlot < inventoryManager.itemSlot.Length)
        {
            ItemSlot currentSlot = inventoryManager.itemSlot[selectedSlot];
            
            // item was consumed or removed
            if (currentEquippedItem != null && currentSlot.quantity <= 0)
            {
                itemFromSlot(selectedSlot);
            }
            // item was added to empty slot
            else if (currentEquippedItem == null && currentSlot.quantity > 0)
            {
                itemFromSlot(selectedSlot);
            }
        }
    }

    // update the item in hand based on hotbar slot
    private void itemFromSlot(int slotIndex)
    {
        // destroy old item
        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
        }

        currentEquippedItem = null;

        if (slotIndex < 0 || slotIndex >= inventoryManager.itemSlot.Length)
            return;
        
        ItemSlot slot = inventoryManager.itemSlot[slotIndex];
        if (slot.itemData != null && slot.quantity > 0)
        {
            currentEquippedItem = slot.itemData;
            createItem(slot.itemData);
        }
    }

    // create a visual representation of the item in hand
    private void createItem(ItemSO itemData)
    {
        GameObject equippedItem = GameObject.CreatePrimitive(PrimitiveType.Quad);
        equippedItem.name = "EquippedItem_" + itemData.itemName;

        equippedItem.transform.SetParent(handPosition);
        equippedItem.transform.localPosition = Vector3.zero;
        equippedItem.transform.localRotation = Quaternion.identity;
        equippedItem.transform.localScale = Vector3.one * defaultScale;

        Destroy(equippedItem.GetComponent<Collider>());

        // apply the item sprite as a texture
        Renderer renderer = equippedItem.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.mainTexture = itemData.itemSprite.texture;

        currentItem = equippedItem;
    }

    public ItemSO GetEquippedItem()
    {
        return currentEquippedItem;
    }

    // refresh the hand item after using or consuming
    public void RefreshEquippedItem()
    {
        itemFromSlot(lastSlot);
    }
}
