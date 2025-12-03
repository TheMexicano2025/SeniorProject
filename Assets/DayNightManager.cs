using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class DayNightManager : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("How many real seconds = 1 in-game hour")]
    public float secondsPerHour = 60f;
    
    [Tooltip("Hour when night begins (0-24)")]
    public float nightStartHour = 18f;
    
    [Tooltip("Hour when day begins (0-24)")]
    public float dayStartHour = 6f;

    [Header("Current State")]
    [SerializeField] private float currentTime = 6f;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int nightsSurvived = 0;
    [SerializeField] private bool isNight = false;

    [Header("Difficulty Scaling")]
    [Tooltip("Health multiplier per night (1.0 = no change, 1.2 = +20% per night)")]
    public float healthScalePerNight = 1.15f;
    
    [Tooltip("Damage multiplier per night (1.0 = no change, 1.1 = +10% per night)")]
    public float damageScalePerNight = 1.1f;

    [Header("References")]
    public LightingManager lightingManager;
    public TMP_Text timeText;
    public TMP_Text dayText;
    public TMP_Text nightCounterText;

    [Header("Events")]
    public UnityEvent onDayStart;
    public UnityEvent onNightStart;
    public UnityEvent onNewDay;

    private bool wasNight = false;

    private void Start()
    {
        if (lightingManager == null)
        {
            lightingManager = GetComponent<LightingManager>();
            if (lightingManager == null)
            {
                Debug.LogWarning("DayNightManager: LightingManager not found!");
            }
        }

        UpdateTimeOfDay();
        UpdateUI();
    }

    private void Update()
    {
        AdvanceTime();
        UpdateTimeOfDay();
        CheckDayNightTransition();
        UpdateUI();
    }

    private void AdvanceTime()
    {
        float previousTime = currentTime;
        
        currentTime += (Time.deltaTime / secondsPerHour);

        if (currentTime >= 24f)
        {
            currentTime = 0f;
            currentDay++;
            onNewDay?.Invoke();
            Debug.Log($"Day {currentDay} has begun!");
        }
    }

    private void UpdateTimeOfDay()
    {
        if (lightingManager != null)
        {
            typeof(LightingManager)
                .GetField("TimeOfDay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(lightingManager, currentTime);
        }
    }

    private void CheckDayNightTransition()
    {
        isNight = currentTime >= nightStartHour || currentTime < dayStartHour;

        if (isNight && !wasNight)
        {
            onNightStart?.Invoke();
            Debug.Log($"Night {nightsSurvived + 1} has fallen on Day {currentDay}. Predators may appear!");
        }
        else if (!isNight && wasNight)
        {
            nightsSurvived++;
            onDayStart?.Invoke();
            Debug.Log($"Dawn has broken on Day {currentDay}. You survived {nightsSurvived} night(s)!");
        }

        wasNight = isNight;
    }

    private void UpdateUI()
    {
        if (timeText != null)
        {
            int hours = Mathf.FloorToInt(currentTime);
            int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
            string period = hours >= 12 ? "PM" : "AM";
            int displayHours = hours > 12 ? hours - 12 : (hours == 0 ? 12 : hours);
            
            timeText.text = $"{displayHours:00}:{minutes:00} {period}";
        }

        if (dayText != null)
        {
            dayText.text = $"Day {currentDay}";
        }

        if (nightCounterText != null)
        {
            nightCounterText.text = $"Nights Survived: {nightsSurvived}";
        }
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }

    public int GetNightsSurvived()
    {
        return nightsSurvived;
    }

    public bool IsNight()
    {
        return isNight;
    }

    public float GetHealthMultiplier()
    {
        return Mathf.Pow(healthScalePerNight, nightsSurvived);
    }

    public float GetDamageMultiplier()
    {
        return Mathf.Pow(damageScalePerNight, nightsSurvived);
    }

    public void SetTime(float newTime)
    {
        currentTime = Mathf.Clamp(newTime, 0f, 24f);
    }

    public void SkipToTime(float targetTime)
    {
        if (targetTime < currentTime)
        {
            currentDay++;
            onNewDay?.Invoke();
        }
        currentTime = Mathf.Clamp(targetTime, 0f, 24f);
    }
}
