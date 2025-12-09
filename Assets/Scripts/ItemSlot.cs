using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemSO itemData;
    public int quantity;
    public bool isFull;
    public Sprite emptySprite; 

    public TMP_Text quantityText;
    public Image itemImage;

    public Image itemDescImage;
    public TMP_Text itemDescName;
    public TMP_Text itemDescText;

    public GameObject selectedShader;
    public bool thisItemSelected;

    public InvManager inventoryManager;

    private GameObject draggedIcon;
    private Canvas canvas;
    private Transform originalParent;

    private void Start()
    {
        if (inventoryManager == null)
        {
            GameObject invCanvas = GameObject.Find("InventoryCanvas");
            if (invCanvas != null)
            {
                inventoryManager = invCanvas.GetComponent<InvManager>();
            }
        }

        canvas = GetComponentInParent<Canvas>();
    }

    public int AddItem(ItemSO newData, int addedQty)
    {
        if (isFull) return addedQty;

        if (itemData == null)
        {
            itemData = newData;
        }

        if (itemData != newData) return addedQty;

        itemImage.sprite = itemData.itemSprite;
        itemImage.enabled = true;

        quantity += addedQty;

        int maxStack = itemData.maxStackSize;

        if (quantity >= maxStack)
        {
            quantityText.text = maxStack.ToString();
            quantityText.enabled = true;
            isFull = true;

            int extraItems = quantity - maxStack;
            quantity = maxStack;
            return extraItems;
        }
        quantityText.text = quantity.ToString();
        quantityText.enabled = true;
        
        return 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick();
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    public void OnLeftClick()
    {
        if (itemData == null) return;

        if (inventoryManager == null)
        {
            GameObject invCanvas = GameObject.Find("InventoryCanvas");
            if (invCanvas != null)
            {
                inventoryManager = invCanvas.GetComponent<InvManager>();
            }
        }

        if (inventoryManager == null) return;

        inventoryManager.DeselectAllSlots();
        
        if (selectedShader != null)
        {
            selectedShader.SetActive(true);
        }
        
        thisItemSelected = true;

        if (itemDescName != null)
        {
            itemDescName.text = itemData.itemName;
        }
        
        if (itemDescImage != null)
        {
            itemDescImage.sprite = itemData.itemSprite;
            
            if (itemDescImage.sprite == null)
            {
                itemDescImage.sprite = emptySprite;
            }
        }
        
        if (itemDescText != null)
        {
            itemDescText.text = itemData.itemDescription;
        }
    }

    public void OnRightClick()
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemData == null || quantity <= 0)
            return;

        draggedIcon = new GameObject("DraggedIcon");
        draggedIcon.transform.SetParent(canvas.transform);
        draggedIcon.transform.SetAsLastSibling();

        Image dragImage = draggedIcon.AddComponent<Image>();
        dragImage.sprite = itemData.itemSprite;
        dragImage.raycastTarget = false;

        RectTransform rectTransform = draggedIcon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(64, 64);

        CanvasGroup canvasGroup = draggedIcon.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        itemImage.color = new Color(1, 1, 1, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            draggedIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
        }

        itemImage.color = Color.white;

        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;

        if (dropTarget != null)
        {
            ItemSlot targetSlot = dropTarget.GetComponentInParent<ItemSlot>();

            if (targetSlot != null && targetSlot != this)
            {
                SwapItems(targetSlot);
            }
        }
    }

    private void SwapItems(ItemSlot targetSlot)
    {
        ItemSO tempData = targetSlot.itemData;
        int tempQuantity = targetSlot.quantity;
        bool tempIsFull = targetSlot.isFull;

        targetSlot.itemData = this.itemData;
        targetSlot.quantity = this.quantity;
        targetSlot.isFull = this.isFull;

        this.itemData = tempData;
        this.quantity = tempQuantity;
        this.isFull = tempIsFull;

        targetSlot.UpdateSlotUI();
        this.UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (itemData != null && quantity > 0)
        {
            itemImage.sprite = itemData.itemSprite;
            itemImage.enabled = true;
            quantityText.text = quantity.ToString();
            quantityText.enabled = true;
        }
        else
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
            quantityText.text = "";
            quantityText.enabled = false;
            itemData = null;
            quantity = 0;
            isFull = false;
        }
    }

    public bool RemoveItem(int removeQty)
    {
        if (itemData == null || quantity <= 0) return false;

        quantity -= removeQty;

        if (quantity <= 0)
        {
            itemData = null;
            quantity = 0;
            isFull = false;
            itemImage.sprite = null;
            itemImage.enabled = false;
            quantityText.text = "";
            quantityText.enabled = false;
            return true;
        }

        quantityText.text = quantity.ToString();
        return true;
    }
}
