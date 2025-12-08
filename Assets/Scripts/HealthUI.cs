using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("Display Mode")]
    [Tooltip("Choose how to display health")]
    public HealthDisplayMode displayMode = HealthDisplayMode.Hearts;
    
    [Header("Heart Display")]
    [Tooltip("Container for heart icons")]
    public Transform heartsContainer;
    
    [Tooltip("Heart sprite (full)")]
    public Sprite fullHeartSprite;
    
    [Tooltip("Heart sprite (empty)")]
    public Sprite emptyHeartSprite;
    
    [Tooltip("How much health per heart")]
    public float healthPerHeart = 20f;
    
    private Image[] heartImages;
    
    [Header("Health Bar Display")]
    [Tooltip("Health bar fill image")]
    public Image healthBarFill;
    
    [Tooltip("Health text display")]
    public TMP_Text healthText;
    
    [Header("References")]
    public Health playerHealth;
    
    public enum HealthDisplayMode
    {
        Hearts,
        HealthBar,
        Both
    }

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerCombat>()?.GetComponent<Health>();
        }
        
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.AddListener(UpdateHealthDisplay);
            InitializeDisplay();
            UpdateHealthDisplay(playerHealth.GetCurrentHealth());
        }
        else
        {
            Debug.LogError("HealthUI: No player Health component found!");
        }
    }

    private void InitializeDisplay()
    {
        if (displayMode == HealthDisplayMode.Hearts || displayMode == HealthDisplayMode.Both)
        {
            CreateHearts();
        }
    }

    private void CreateHearts()
    {
        if (heartsContainer == null || fullHeartSprite == null)
        {
            Debug.LogWarning("HealthUI: Hearts container or sprite not assigned!");
            return;
        }
        
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }
        
        int heartCount = Mathf.CeilToInt(playerHealth.GetMaxHealth() / healthPerHeart);
        heartImages = new Image[heartCount];
        
        for (int i = 0; i < heartCount; i++)
        {
            GameObject heartObj = new GameObject($"Heart_{i}");
            heartObj.transform.SetParent(heartsContainer);
            heartObj.transform.localScale = Vector3.one;
            
            Image heartImage = heartObj.AddComponent<Image>();
            heartImage.sprite = fullHeartSprite;
            heartImage.preserveAspect = true;
            
            heartImages[i] = heartImage;
        }
    }

    private void UpdateHealthDisplay(float currentHealth)
    {
        if (displayMode == HealthDisplayMode.Hearts || displayMode == HealthDisplayMode.Both)
        {
            UpdateHearts(currentHealth);
        }
        
        if (displayMode == HealthDisplayMode.HealthBar || displayMode == HealthDisplayMode.Both)
        {
            UpdateHealthBar(currentHealth);
        }
    }

    private void UpdateHearts(float currentHealth)
    {
        if (heartImages == null || heartImages.Length == 0) return;
        
        int fullHearts = Mathf.FloorToInt(currentHealth / healthPerHeart);
        
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            
            if (i < fullHearts)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].color = Color.white;
            }
            else
            {
                float remainingHealth = currentHealth - (i * healthPerHeart);
                
                if (remainingHealth > 0)
                {
                    heartImages[i].sprite = fullHeartSprite;
                    heartImages[i].fillAmount = remainingHealth / healthPerHeart;
                }
                else
                {
                    if (emptyHeartSprite != null)
                    {
                        heartImages[i].sprite = emptyHeartSprite;
                        heartImages[i].color = Color.white;
                    }
                    else
                    {
                        heartImages[i].sprite = fullHeartSprite;
                        heartImages[i].color = new Color(1f, 1f, 1f, 0.3f);
                    }
                }
            }
        }
    }

    private void UpdateHealthBar(float currentHealth)
    {
        if (healthBarFill != null)
        {
            float healthPercent = currentHealth / playerHealth.GetMaxHealth();
            healthBarFill.fillAmount = healthPercent;
            
            healthBarFill.color = Color.Lerp(Color.red, Color.green, healthPercent);
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)} / {playerHealth.GetMaxHealth()}";
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged.RemoveListener(UpdateHealthDisplay);
        }
    }
}

