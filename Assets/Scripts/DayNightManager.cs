using UnityEngine;
using UnityEngine.Events;
using TMPro;

// this script manages the day night cycle
// it tracks time days and freezes time when predators are alive at night
public class DayNightManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float secondsPerHour = 60f;
    public float nightStartHour = 18f;
    public float dayStartHour = 6f;
    public float freezeTimeHour = 2f; // time freezes after this hour if predators alive

    [Header("Current State")]
    [SerializeField] private float currentTime = 6f;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int nightsSurvived = 0;
    [SerializeField] private bool isNight = false;
    [SerializeField] private bool timeFrozen = false;

    [Header("Difficulty Scaling")]
    public float healthScalePerNight = 1.15f;
    public float damageScalePerNight = 1.1f;

    [Header("References")]
    public LightingManager lightingManager;
    public TMP_Text timeText;
    public TMP_Text dayText;
    public TMP_Text nightCounterText;

    [Header("Events")]
    public UnityEvent onNewDay; // other scripts listen to this for daily updates

    private bool wasNight = false;

    private void Start()
    {
        if (lightingManager == null)
        {
            lightingManager = GetComponent<LightingManager>();
        }

        UpdateTimeOfDay();
        UpdateUI();
    }

    private void Update()
    {
        CheckTimeFreezeCondition();
        
        if (!timeFrozen)
        {
            AdvanceTime();
        }
        
        UpdateTimeOfDay();
        CheckDayNightTransition();
        UpdateUI();
    }
    
    // freeze time at night if predators are still alive
    private void CheckTimeFreezeCondition()
    {
        if (!isNight)
        {
            timeFrozen = false;
            return;
        }
        
        if (currentTime >= freezeTimeHour && currentTime < dayStartHour)
        {
            GameObject[] predators = GameObject.FindGameObjectsWithTag("Pig");
            
            if (predators.Length > 0)
            {
                timeFrozen = true;
            }
            else
            {
                timeFrozen = false;
            }
        }
        else
        {
            timeFrozen = false;
        }
    }

    // move time forward
    private void AdvanceTime()
    {
        currentTime += (Time.deltaTime / secondsPerHour);

        // wrap around to next day at midnight
        if (currentTime >= 24f)
        {
            currentTime = 0f;
            currentDay++;
            onNewDay?.Invoke();
        }
    }

    // sync time with the lighting manager
    private void UpdateTimeOfDay()
    {
        if (lightingManager != null)
        {
            typeof(LightingManager)
                .GetField("TimeOfDay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(lightingManager, currentTime);
        }
    }

    // check when night starts and ends
    private void CheckDayNightTransition()
    {
        isNight = currentTime >= nightStartHour || currentTime < dayStartHour;

        if (isNight && !wasNight)
        {
            // night just started
        }
        else if (!isNight && wasNight)
        {
            nightsSurvived++;
        }

        wasNight = isNight;
    }

    // update all the UI text elements
    private void UpdateUI()
    {
        if (timeText != null)
        {
            timeText.text = GetFormattedTime();
            timeText.color = timeFrozen ? Color.yellow : Color.white;
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
    
    // convert 24 hour time to 12 hour AM PM format
    private string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        string period = hours >= 12 ? "PM" : "AM";
        int displayHours = hours > 12 ? hours - 12 : (hours == 0 ? 12 : hours);
        
        return $"{displayHours:00}:{minutes:00} {period}";
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
    
    public bool IsTimeFrozen()
    {
        return timeFrozen;
    }

    // enemy health scales up each night
    public float GetHealthMultiplier()
    {
        return Mathf.Pow(healthScalePerNight, nightsSurvived);
    }

    // enemy damage scales up each night
    public float GetDamageMultiplier()
    {
        return Mathf.Pow(damageScalePerNight, nightsSurvived);
    }

    public void SetTime(float newTime)
    {
        currentTime = Mathf.Clamp(newTime, 0f, 24f);
    }

    // jump to a specific time and handle day rollover
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
