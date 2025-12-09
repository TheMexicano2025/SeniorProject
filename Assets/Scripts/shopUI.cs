using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// this script controls the shop UI
// it shows items you can buy from NPC and items you can sell
public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TMP_Text shopTitleText;
    [SerializeField] private Transform buyContent;
    [SerializeField] private Transform sellContent;
    [SerializeField] private GameObject shopItemButtonPrefab;
    
    [Header("Tabs")]
    [SerializeField] private GameObject buyTab;
    [SerializeField] private GameObject sellTab;

    private ItemSO[] shopInventory;
    private InvManager playerInventory;

    private void Start()
    {
        playerInventory = FindObjectOfType<InvManager>();
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // press escape to close shop
        if (shopPanel != null && shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    public void OpenShop(ItemSO[] items, string shopName)
    {
        shopInventory = items;
        
        if (shopTitleText != null)
        {
            shopTitleText.text = shopName;
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        // show cursor when shop is open
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowBuyTab();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // hide cursor when shop closes
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowBuyTab()
    {
        if (buyTab != null) buyTab.SetActive(true);
        if (sellTab != null) sellTab.SetActive(false);
        
        PopulateBuyItems();
    }

    public void ShowSellTab()
    {
        if (buyTab != null) buyTab.SetActive(false);
        if (sellTab != null) sellTab.SetActive(true);
        
        PopulateSellItems();
    }

    // fill buy tab with items from shop inventory
    private void PopulateBuyItems()
    {
        ClearContent(buyContent);

        if (shopInventory == null) return;

        foreach (ItemSO item in shopInventory)
        {
            if (item != null && item.canBuy)
            {
                CreateShopButton(item, buyContent, true);
            }
        }
    }

    // fill sell tab with items from player inventory
    private void PopulateSellItems()
    {
        ClearContent(sellContent);

        if (playerInventory == null) return;

        HashSet<ItemSO> uniqueItems = new HashSet<ItemSO>();

        foreach (var slot in playerInventory.GetAllSlots())
        {
            if (slot.itemData != null && slot.quantity > 0 && slot.itemData.canSell && !uniqueItems.Contains(slot.itemData))
            {
                uniqueItems.Add(slot.itemData);
                CreateShopButton(slot.itemData, sellContent, false);
            }
        }
    }

    // create a button for buying or selling an item
    private void CreateShopButton(ItemSO item, Transform parent, bool isBuying)
    {
        if (shopItemButtonPrefab == null) return;

        GameObject buttonObj = Instantiate(shopItemButtonPrefab, parent);
        
        TMP_Text nameText = buttonObj.transform.Find("ItemName")?.GetComponent<TMP_Text>();
        TMP_Text priceText = buttonObj.transform.Find("PriceText")?.GetComponent<TMP_Text>();
        Image iconImage = buttonObj.transform.Find("ItemIcon")?.GetComponent<Image>();
        Button button = buttonObj.GetComponent<Button>();

        if (nameText != null) nameText.text = item.itemName;
        if (iconImage != null) iconImage.sprite = item.itemSprite;

        int price = isBuying ? item.buyPrice : item.sellPrice;
        if (priceText != null) priceText.text = $"${price}";

        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                if (isBuying)
                {
                    BuyItem(item);
                }
                else
                {
                    SellItem(item);
                }
            });
        }
    }

    private void BuyItem(ItemSO item)
    {
        if (CurrencyManager.Instance == null) return;
        if (!CurrencyManager.Instance.CanAfford(item.buyPrice)) return;

        if (CurrencyManager.Instance.RemoveMoney(item.buyPrice))
        {
            playerInventory.AddItem(item, 1);
        }
    }

    private void SellItem(ItemSO item)
    {
        if (CurrencyManager.Instance == null || playerInventory == null) return;

        if (playerInventory.RemoveItem(item, 1))
        {
            CurrencyManager.Instance.AddMoney(item.sellPrice);
            PopulateSellItems();
        }
    }

    // clear all items from a content panel
    private void ClearContent(Transform content)
    {
        if (content == null) return;

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }
}
