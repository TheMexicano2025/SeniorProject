using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvManager : MonoBehaviour
{
    public GameObject InvMenu;
    private bool menuActiviated;

    public ItemSlot[] itemSlot;

    void Update()
    {
        if (Input.GetButtonDown("Inventory") && menuActiviated)
        {
            Time.timeScale = 1;
            InvMenu.SetActive(false);
            menuActiviated = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Input.GetButtonDown("Inventory") && !menuActiviated)
        {
            Time.timeScale = 0;
            InvMenu.SetActive(true);
            menuActiviated = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public int AddItem(ItemSO itemData, int quantity)
    {
        if (itemData == null) return quantity;

        int remainingQuantity = quantity;

        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].itemData == itemData && itemSlot[i].quantity > 0 && !itemSlot[i].isFull)
            {
                remainingQuantity = itemSlot[i].AddItem(itemData, remainingQuantity);
                if (remainingQuantity <= 0) return 0;
            }
        }

        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].quantity == 0)
            {
                remainingQuantity = itemSlot[i].AddItem(itemData, remainingQuantity);
                if (remainingQuantity <= 0) return 0;
            }
        }

        return remainingQuantity;
    }

    public bool RemoveItem(ItemSO itemData, int quantity)
    {
        if (itemData == null) return false;

        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].itemData == itemData && itemSlot[i].quantity > 0)
            {
                return itemSlot[i].RemoveItem(quantity);
            }
        }

        return false;
    }

    public void DeselectAllSlots()
    {
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i].selectedShader.SetActive(false);
            itemSlot[i].thisItemSelected = false;
        }
    }

    public ItemSlot[] GetAllSlots()
    {
        return itemSlot;
    }
}
