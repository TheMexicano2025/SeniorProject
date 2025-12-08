using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FixItemSlotReferences : MonoBehaviour
{
    [Header("Global Description UI (Assign These)")]
    [Tooltip("Drag the /InvDesc/Item Image GameObject here")]
    public Image invDescItemImage;
    
    [Tooltip("Drag the /InvDesc/Description/ItemNameText GameObject here")]
    public TMP_Text invDescItemName;
    
    [Tooltip("Drag the /InvDesc/Description/DescriptionText GameObject here")]
    public TMP_Text invDescText;

    [Header("Inventory Manager")]
    [Tooltip("Drag the InventoryCanvas GameObject here")]
    public InvManager inventoryManager;

    [Header("Optional - Empty Sprite")]
    public Sprite emptySprite;

    [ContextMenu("Fix All Slots")]
    public void FixAllSlots()
    {
        ItemSlot[] allSlots = GetComponentsInChildren<ItemSlot>();
        
        Debug.Log($"Found {allSlots.Length} ItemSlot components to fix");

        int fixedCount = 0;

        foreach (ItemSlot slot in allSlots)
        {
            Transform itemImageTransform = slot.transform.Find("ItemImage");
            if (itemImageTransform != null)
            {
                Image itemImage = itemImageTransform.GetComponent<Image>();
                if (itemImage != null)
                {
                    slot.itemImage = itemImage;
                }
            }

            Transform quantityTextTransform = slot.transform.Find("QuantityText");
            if (quantityTextTransform != null)
            {
                TMP_Text quantityText = quantityTextTransform.GetComponent<TextMeshProUGUI>();
                if (quantityText != null)
                {
                    slot.quantityText = quantityText;
                }
            }

            Transform selectedPanelTransform = slot.transform.Find("SelectedPanel");
            if (selectedPanelTransform != null)
            {
                slot.selectedShader = selectedPanelTransform.gameObject;
            }

            if (invDescItemImage != null)
            {
                slot.itemDescImage = invDescItemImage;
            }

            if (invDescItemName != null)
            {
                slot.itemDescName = invDescItemName;
            }

            if (invDescText != null)
            {
                slot.itemDescText = invDescText;
            }

            if (inventoryManager != null)
            {
                slot.inventoryManager = inventoryManager;
            }

            if (emptySprite != null)
            {
                slot.emptySprite = emptySprite;
            }

            fixedCount++;
        }

        Debug.Log($"Successfully fixed {fixedCount} ItemSlot references!");
    }
}
