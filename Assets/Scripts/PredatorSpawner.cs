using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PredatorSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [Tooltip("The coyote prefab to spawn")]
    public GameObject predatorPrefab;
    
    [Tooltip("Spawn points around the farm (set these manually)")]
    public Transform[] spawnPoints;
    
    [Tooltip("Minimum coyotes to spawn on first night")]
    public int minStartCoyotes = 2;
    
    [Tooltip("Maximum coyotes to spawn on first night")]
    public int maxStartCoyotes = 3;
    
    [Tooltip("How many additional coyotes per night")]
    public int coyotesPerNight = 1;
    
    [Tooltip("Maximum night difficulty (stops scaling after this)")]
    public int maxDifficultyNight = 5;
    
    [Tooltip("Delay between spawning each coyote (seconds)")]
    public float spawnDelay = 2f;
    
    [Header("Future Scaling (Not Yet Implemented)")]
    [Tooltip("Health bonus per night")]
    public float healthPerNight = 10f;
    
    [Tooltip("Damage bonus per night")]
    public float damagePerNight = 2f;
    
    [Header("References")]
    [Tooltip("Reference to DayNightManager")]
    public DayNightManager dayNightManager;
    
    [Tooltip("UI Text showing remaining coyotes")]
    public TextMeshProUGUI coyoteCounterText;
    
    [Header("Debug")]
    public bool showSpawnGizmos = true;
    
    private List<GameObject> activePredators = new List<GameObject>();
    private bool hasSpawnedTonight = false;
    private int currentNightCounter = 0;

    private void Start()
    {
        if (dayNightManager == null)
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
        }
        
        UpdateCoyoteCounter();
    }

    private void Update()
    {
        CleanupDeadPredators();
        UpdateCoyoteCounter();
        
        if (dayNightManager != null)
        {
            bool isNight = dayNightManager.IsNight();
            
            if (isNight && !hasSpawnedTonight)
            {
                currentNightCounter = dayNightManager.GetNightsSurvived() + 1;
                SpawnNightPredators();
                hasSpawnedTonight = true;
            }
            else if (!isNight && hasSpawnedTonight)
            {
                hasSpawnedTonight = false;
            }
        }
    }

    private void SpawnNightPredators()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("PredatorSpawner: No spawn points assigned!");
            return;
        }
        
        int nightDifficulty = Mathf.Min(currentNightCounter, maxDifficultyNight);
        
        int minCoyotes = minStartCoyotes + (nightDifficulty - 1) * coyotesPerNight;
        int maxCoyotes = maxStartCoyotes + (nightDifficulty - 1) * coyotesPerNight;
        
        int coyotesToSpawn = Random.Range(minCoyotes, maxCoyotes + 1);
        
        Debug.Log($"Night {currentNightCounter}: Spawning {coyotesToSpawn} coyotes (Difficulty: {nightDifficulty})");
        
        StartCoroutine(SpawnPredatorsOverTime(coyotesToSpawn, nightDifficulty));
    }

    private System.Collections.IEnumerator SpawnPredatorsOverTime(int count, int nightDifficulty)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSinglePredator(nightDifficulty);
            yield return new UnityEngine.WaitForSeconds(spawnDelay);
        }
        
        Debug.Log($"Finished spawning {count} coyotes!");
    }

    private void SpawnSinglePredator(int nightDifficulty)
    {
        if (predatorPrefab == null)
        {
            Debug.LogError("PredatorSpawner: No predator prefab assigned!");
            return;
        }
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = spawnPoint.position;
        
        spawnPosition += new Vector3(
            Random.Range(-2f, 2f),
            0f,
            Random.Range(-2f, 2f)
        );
        
        GameObject coyote = Instantiate(predatorPrefab, spawnPosition, Quaternion.identity);
        
        Health health = coyote.GetComponent<Health>();
        if (health != null && nightDifficulty > 1)
        {
            float bonusHealth = (nightDifficulty - 1) * healthPerNight;
            health.maxHealth += bonusHealth;
        }
        
        Predator predator = coyote.GetComponent<Predator>();
        if (predator != null && nightDifficulty > 1)
        {
            float bonusDamage = (nightDifficulty - 1) * damagePerNight;
            predator.attackDamage += bonusDamage;
        }
        
        activePredators.Add(coyote);
        
        Debug.Log($"Spawned coyote at {spawnPosition} (Night {nightDifficulty})");
    }

    private void CleanupDeadPredators()
    {
        activePredators.RemoveAll(predator => predator == null);
    }

    private void UpdateCoyoteCounter()
    {
        if (coyoteCounterText != null)
        {
            int aliveCount = activePredators.Count;
            coyoteCounterText.text = $"Coyotes: {aliveCount}";
            
            if (aliveCount == 0)
            {
                coyoteCounterText.gameObject.SetActive(false);
            }
            else
            {
                coyoteCounterText.gameObject.SetActive(true);
            }
        }
    }

    public int GetActivePredatorCount()
    {
        CleanupDeadPredators();
        return activePredators.Count;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showSpawnGizmos || spawnPoints == null) return;
        
        Gizmos.color = Color.red;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 3f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 5f);
            }
        }
    }
}

