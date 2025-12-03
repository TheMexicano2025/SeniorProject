using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SleepManager : MonoBehaviour, Interactable
{
    [Header("Sleep Settings")]
    [Tooltip("Should player be healed on sleep?")]
    public bool healOnSleep = true;
    
    [Tooltip("Time to skip to when sleeping (6 AM recommended)")]
    public float wakeUpTime = 6f;
    
    [Tooltip("Can only sleep at night?")]
    public bool nightOnlyRestriction = true;
    
    [Tooltip("Must kill all coyotes before sleeping?")]
    public bool requireNoCoyotes = true;
    
    [Header("References")]
    [Tooltip("The DayNightManager")]
    public DayNightManager dayNightManager;
    
    [Tooltip("The PredatorSpawner")]
    public PredatorSpawner predatorSpawner;
    
    [Tooltip("The FadeController")]
    public FadeController fadeController;
    
    [Tooltip("Day popup text (Day X)")]
    public TextMeshProUGUI dayPopupText;
    
    [Tooltip("Day popup canvas group (for fading)")]
    public CanvasGroup dayPopupCanvasGroup;
    
    [Tooltip("Player Health component")]
    public Health playerHealth;
    
    [Header("Popup Settings")]
    [Tooltip("How long the day popup stays visible")]
    public float popupDuration = 3f;
    
    [Tooltip("Fade speed for popup")]
    public float popupFadeSpeed = 2f;
    
    private bool isSleeping = false;

    private void Start()
    {
        if (dayNightManager == null)
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
        }
        
        if (predatorSpawner == null)
        {
            predatorSpawner = FindObjectOfType<PredatorSpawner>();
        }
        
        if (fadeController == null)
        {
            fadeController = FindObjectOfType<FadeController>();
        }
        
        if (playerHealth == null)
        {
            playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Health>();
        }
        
        if (dayPopupCanvasGroup != null)
        {
            dayPopupCanvasGroup.alpha = 0f;
            dayPopupCanvasGroup.gameObject.SetActive(false);
        }
    }

    public bool CanInteract(GameObject player)
    {
        if (isSleeping) return false;
        
        if (nightOnlyRestriction && dayNightManager != null && !dayNightManager.IsNight())
        {
            return false;
        }
        
        if (requireNoCoyotes && predatorSpawner != null && predatorSpawner.GetActivePredatorCount() > 0)
        {
            return false;
        }
        
        return true;
    }

    public void Interact(GameObject player)
    {
        if (!CanInteract(player)) return;
        
        StartCoroutine(SleepSequence());
    }

    public string GetInteractionPrompt()
    {
        if (isSleeping)
        {
            return "";
        }
        
        if (nightOnlyRestriction && dayNightManager != null && !dayNightManager.IsNight())
        {
            return "Can only sleep at night";
        }
        
        if (requireNoCoyotes && predatorSpawner != null && predatorSpawner.GetActivePredatorCount() > 0)
        {
            int coyoteCount = predatorSpawner.GetActivePredatorCount();
            return $"Defeat all coyotes first ({coyoteCount} remaining)";
        }
        
        return "Press E to Sleep";
    }

    private IEnumerator SleepSequence()
    {
        isSleeping = true;
        
        Debug.Log("Going to sleep...");
        
        if (fadeController != null)
        {
            fadeController.FadeOut();
            yield return new WaitForSeconds(fadeController.fadeDuration);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (healOnSleep && playerHealth != null)
        {
            playerHealth.Heal(playerHealth.maxHealth);
            Debug.Log("Player healed on sleep!");
        }
        
        if (dayNightManager != null)
        {
            dayNightManager.SkipToTime(wakeUpTime);
            Debug.Log($"Skipped to {wakeUpTime}:00 (morning)");
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (fadeController != null)
        {
            fadeController.FadeIn();
            yield return new WaitForSeconds(fadeController.fadeDuration);
        }
        
        if (dayNightManager != null && dayPopupText != null)
        {
            ShowDayPopup();
        }
        
        isSleeping = false;
        
        Debug.Log("Woke up! Good morning!");
    }

    private void ShowDayPopup()
    {
        int currentDay = dayNightManager.GetCurrentDay();
        
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            int totalDays = gameManager.GetDaysToSurvive();
            dayPopupText.text = $"Day {currentDay} / {totalDays}";
        }
        else
        {
            dayPopupText.text = $"Day {currentDay}";
        }
        
        StartCoroutine(FadePopup());
    }

    private IEnumerator FadePopup()
    {
        if (dayPopupCanvasGroup == null) yield break;
        
        dayPopupCanvasGroup.gameObject.SetActive(true);
        
        float elapsed = 0f;
        while (elapsed < 1f / popupFadeSpeed)
        {
            elapsed += Time.deltaTime;
            dayPopupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed * popupFadeSpeed);
            yield return null;
        }
        dayPopupCanvasGroup.alpha = 1f;
        
        yield return new WaitForSeconds(popupDuration);
        
        elapsed = 0f;
        while (elapsed < 1f / popupFadeSpeed)
        {
            elapsed += Time.deltaTime;
            dayPopupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed * popupFadeSpeed);
            yield return null;
        }
        dayPopupCanvasGroup.alpha = 0f;
        
        dayPopupCanvasGroup.gameObject.SetActive(false);
    }
}
