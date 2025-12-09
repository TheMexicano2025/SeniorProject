using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// this script manages the player's money
// other scripts use this to check if player can afford items
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
        // singleton pattern so there's only one currency manager
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
        if (amount < 0) return false;

        currentMoney += amount;
        UpdateMoneyUI();
        return true;
    }

    public bool RemoveMoney(int amount)
    {
        if (amount < 0) return false;
        if (!CanAfford(amount)) return false;

        currentMoney -= amount;
        UpdateMoneyUI();
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
