using UnityEngine;
using UnityEngine.UI;
using TMPro;

// this helper script auto-fixes all item slot references in the inventory
// run this from the context menu if slots aren't showing items correctly
public class FixItemSlotReferences : MonoBehaviour
{
    [Header("Global Description UI (Assign These)")]
    public Image invDescItemImage;
    public TMP_Text invDescItemName;
    public TMP_Text invDescText;

    [Header("Inventory Manager")]
    public InvManager inventoryManager;

    [Header("Optional - Empty Sprite")]
    public Sprite emptySprite;

    [ContextMenu("Fix All Slots")]
    public void FixAllSlots()
    {
        ItemSlot[] allSlots = GetComponentsInChildren<ItemSlot>();
        int fixedCount = 0;

        foreach (ItemSlot slot in allSlots)
        {
            // find and assign ItemImage
            Transform itemImageTransform = slot.transform.Find("ItemImage");
            if (itemImageTransform != null)
            {
                Image itemImage = itemImageTransform.GetComponent<Image>();
                if (itemImage != null)
                {
                    slot.itemImage = itemImage;
                }
            }

            // find and assign QuantityText
            Transform quantityTextTransform = slot.transform.Find("QuantityText");
            if (quantityTextTransform != null)
            {
                TMP_Text quantityText = quantityTextTransform.GetComponent<TextMeshProUGUI>();
                if (quantityText != null)
                {
                    slot.quantityText = quantityText;
                }
            }

            // find and assign SelectedPanel
            Transform selectedPanelTransform = slot.transform.Find("SelectedPanel");
            if (selectedPanelTransform != null)
            {
                slot.selectedShader = selectedPanelTransform.gameObject;
            }

            // assign global description UI
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
    }
}
