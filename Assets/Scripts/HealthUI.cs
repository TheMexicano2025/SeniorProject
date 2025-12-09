using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// this script displays player health as hearts or a health bar
public class HealthUI : MonoBehaviour
{
    [Header("Display Mode")]
    public HealthDisplayMode displayMode = HealthDisplayMode.Hearts;
    
    [Header("Heart Display")]
    public Transform heartsContainer;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;
    public float healthPerHeart = 20f; // each heart represents this much health
    
    private Image[] heartImages;
    
    [Header("Health Bar Display")]
    public Image healthBarFill;
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
        // find player health if not assigned
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
    }

    private void InitializeDisplay()
    {
        if (displayMode == HealthDisplayMode.Hearts || displayMode == HealthDisplayMode.Both)
        {
            CreateHearts();
        }
    }

    // create heart images based on max health
    private void CreateHearts()
    {
        if (heartsContainer == null || fullHeartSprite == null) return;
        
        // clear any existing hearts
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

    // update heart sprites based on current health
    private void UpdateHearts(float currentHealth)
    {
        if (heartImages == null || heartImages.Length == 0) return;
        
        int fullHearts = Mathf.FloorToInt(currentHealth / healthPerHeart);
        
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            
            if (i < fullHearts)
            {
                // this heart is full
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].color = Color.white;
            }
            else
            {
                float remainingHealth = currentHealth - (i * healthPerHeart);
                
                if (remainingHealth > 0)
                {
                    // this heart is partially full
                    heartImages[i].sprite = fullHeartSprite;
                    heartImages[i].fillAmount = remainingHealth / healthPerHeart;
                }
                else
                {
                    // this heart is empty
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

    // update health bar fill and text
    private void UpdateHealthBar(float currentHealth)
    {
        if (healthBarFill != null)
        {
            float healthPercent = currentHealth / playerHealth.GetMaxHealth();
            healthBarFill.fillAmount = healthPercent;
            
            // color shifts from red to green based on health
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
