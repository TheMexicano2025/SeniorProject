using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SleepManager : MonoBehaviour, Interactable
{
    [Header("Sleep Settings")]
    public bool healOnSleep = true;
    public float wakeUpTime = 6f;
    public bool nightOnlyRestriction = true;
    public bool requireNoCoyotes = true;
    
    [Header("References")]
    public DayNightManager dayNightManager;
    public PredatorSpawner predatorSpawner;
    public FadeController fadeController;
    public TextMeshProUGUI dayPopupText;
    public CanvasGroup dayPopupCanvasGroup;
    public Health playerHealth;
    
    [Header("Popup Settings")]
    public float popupDuration = 3f;
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
        
        GameManager gameManager = GameManager.Instance;
        int currentDay = dayNightManager != null ? dayNightManager.GetCurrentDay() : 1;
        bool isNight = dayNightManager != null && dayNightManager.IsNight();
        bool shouldCheckVictory = gameManager != null && currentDay >= gameManager.GetDaysToSurvive() && isNight;
        
        if (fadeController != null)
        {
            fadeController.FadeOut();
            yield return new WaitForSeconds(fadeController.fadeDuration);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        if (shouldCheckVictory)
        {
            if (gameManager != null)
            {
                gameManager.TriggerVictory();
            }
            yield break;
        }
        
        if (healOnSleep && playerHealth != null)
        {
            playerHealth.Heal(playerHealth.maxHealth);
        }
        
        if (dayNightManager != null)
        {
            dayNightManager.SkipToTime(wakeUpTime);
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