using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HotbarSelect : MonoBehaviour
{
    [Header("HotbarSlots")]
    [Tooltip("Drag the 6 HotbarSlots GO here")]
    public GameObject[] HBSlots = new GameObject[6];

    [Header("References")]
    [Tooltip("Drag the InvManager here")]
    public InvManager inventoryManager;

    private int currentItem = 0;

    void Start()
    {
        SelectSlot(0);
    }

    void Update()
    {
        HBInput();
        UpdateDisplay();
    }

    private void HBInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectSlot(5);
    }

    private void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= HBSlots.Length)
            return;

        Debug.Log($"SelectSlot called with index: {slotIndex}");

        for (int i = 0; i < HBSlots.Length; i++)
        {
            if (HBSlots[i] == null)
            {
                Debug.LogWarning($"HBSlots[{i}] is null!");
                continue;
            }

            Transform highlightTransform = HBSlots[i].transform.Find("selectHigh");
            if (highlightTransform != null)
            {
                bool shouldBeActive = (i == slotIndex);
                highlightTransform.gameObject.SetActive(shouldBeActive);
                Debug.Log($"Slot {i} highlight set to: {shouldBeActive}");
            }
            else
            {
                Debug.LogWarning($"Could not find 'selectHigh' in {HBSlots[i].name}");
            }
        }
        currentItem = slotIndex;

        if (inventoryManager != null && 
            slotIndex < inventoryManager.itemSlot.Length &&
            inventoryManager.itemSlot[slotIndex] != null &&
            inventoryManager.itemSlot[slotIndex].itemData != null)
        {
            inventoryManager.itemSlot[slotIndex].OnLeftClick();
        }
    }

    private void UpdateDisplay()
    {
        if (inventoryManager == null)
            return;

        if (inventoryManager.itemSlot == null)
            return;

        for (int i = 0; i < HBSlots.Length; i++)
        {
            if (HBSlots[i] == null)
                continue;

            if (i >= inventoryManager.itemSlot.Length)
                continue;

            ItemSlot invSlot = inventoryManager.itemSlot[i];
            
            if (invSlot == null)
                continue;
            
            Transform imageTransform = HBSlots[i].transform.Find("hbImage");
            if (imageTransform == null)
                continue;

            Image iconImage = imageTransform.GetComponent<Image>();
            if (iconImage == null)
                continue;

            if(invSlot.itemData != null && invSlot.quantity > 0)
            {
                iconImage.sprite = invSlot.itemData.itemSprite;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
        }
    }

    public int getSelect()
    {
        return currentItem;
    }
}
