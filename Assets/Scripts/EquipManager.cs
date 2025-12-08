using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void updateEquip()
    {
        if (HBManager == null || inventoryManager == null || handPosition == null)
            return;
        
        int selectedSlot = HBManager.getSelect();

        if (selectedSlot != lastSlot)
        {
            lastSlot = selectedSlot;
            itemFromSlot(selectedSlot);
        }
        else if (selectedSlot >= 0 && selectedSlot < inventoryManager.itemSlot.Length)
        {
            ItemSlot currentSlot = inventoryManager.itemSlot[selectedSlot];
            
            if (currentEquippedItem != null && currentSlot.quantity <= 0)
            {
                itemFromSlot(selectedSlot);
            }
            else if (currentEquippedItem == null && currentSlot.quantity > 0)
            {
                itemFromSlot(selectedSlot);
            }
        }
    }

    private void itemFromSlot(int slotIndex)
    {
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

    private void createItem(ItemSO itemData)
    {
        GameObject equippedItem = GameObject.CreatePrimitive(PrimitiveType.Quad);
        equippedItem.name = "EquippedItem_" + itemData.itemName;

        equippedItem.transform.SetParent(handPosition);
        equippedItem.transform.localPosition = Vector3.zero;
        equippedItem.transform.localRotation = Quaternion.identity;
        equippedItem.transform.localScale = Vector3.one * defaultScale;

        Destroy(equippedItem.GetComponent<Collider>());

        Renderer renderer = equippedItem.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.mainTexture = itemData.itemSprite.texture;

        currentItem = equippedItem;
    }

    public ItemSO GetEquippedItem()
    {
        return currentEquippedItem;
    }

    public void RefreshEquippedItem()
    {
        itemFromSlot(lastSlot);
    }

}
