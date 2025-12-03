using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Currency Settings")]
    [SerializeField] private int startingMoney = 100;
    private int currentMoney;

    [Header("UI")]
    [SerializeField] private TMP_Text moneyText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        currentMoney = startingMoney;
        UpdateMoneyUI();
    }

    public int GetMoney()
    {
        return currentMoney;
    }

    public bool CanAfford(int amount)
    {
        return currentMoney >= amount;
    }

    public bool AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot add negative money. Use RemoveMoney instead.");
            return false;
        }

        currentMoney += amount;
        UpdateMoneyUI();
        Debug.Log($"Added ${amount}. Current money: ${currentMoney}");
        return true;
    }

    public bool RemoveMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot remove negative money. Use AddMoney instead.");
            return false;
        }

        if (!CanAfford(amount))
        {
            Debug.LogWarning($"Not enough money! Need ${amount}, have ${currentMoney}");
            return false;
        }

        currentMoney -= amount;
        UpdateMoneyUI();
        Debug.Log($"Removed ${amount}. Current money: ${currentMoney}");
        return true;
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"${currentMoney}";
        }
    }
}

