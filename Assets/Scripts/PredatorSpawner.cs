using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// this script spawns predators each night
// difficulty scales up with each night survived
public class PredatorSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject predatorPrefab;
    public Transform[] spawnPoints;
    public int minStartCoyotes = 2;
    public int maxStartCoyotes = 3;
    public int coyotesPerNight = 1; // extra coyotes per night
    public int maxDifficultyNight = 5; // difficulty caps at this night
    public float spawnDelay = 2f; // delay between each spawn
    
    [Header("Future Scaling (Not Yet Implemented)")]
    public float healthPerNight = 10f;
    public float damagePerNight = 2f;
    
    [Header("References")]
    public DayNightManager dayNightManager;
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
            
            // spawn predators when night starts
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

    // spawn predators based on current night number
    private void SpawnNightPredators()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        
        int nightDifficulty = Mathf.Min(currentNightCounter, maxDifficultyNight);
        
        int minCoyotes = minStartCoyotes + (nightDifficulty - 1) * coyotesPerNight;
        int maxCoyotes = maxStartCoyotes + (nightDifficulty - 1) * coyotesPerNight;
        
        int coyotesToSpawn = Random.Range(minCoyotes, maxCoyotes + 1);
        
        StartCoroutine(SpawnPredatorsOverTime(coyotesToSpawn, nightDifficulty));
    }

    // spawn predators one at a time with delay
    private IEnumerator SpawnPredatorsOverTime(int count, int nightDifficulty)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSinglePredator(nightDifficulty);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    // spawn one predator with scaled health and damage
    private void SpawnSinglePredator(int nightDifficulty)
    {
        if (predatorPrefab == null) return;
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = spawnPoint.position;
        
        // add random offset so they don't all spawn in exact same spot
        spawnPosition += new Vector3(
            Random.Range(-2f, 2f),
            0f,
            Random.Range(-2f, 2f)
        );
        
        GameObject coyote = Instantiate(predatorPrefab, spawnPosition, Quaternion.identity);
        
        // scale health based on night
        Health health = coyote.GetComponent<Health>();
        if (health != null && nightDifficulty > 1)
        {
            float bonusHealth = (nightDifficulty - 1) * healthPerNight;
            health.maxHealth += bonusHealth;
        }
        
        // scale damage based on night
        Predator predator = coyote.GetComponent<Predator>();
        if (predator != null && nightDifficulty > 1)
        {
            float bonusDamage = (nightDifficulty - 1) * damagePerNight;
            predator.attackDamage += bonusDamage;
        }
        
        activePredators.Add(coyote);
    }

    // remove dead predators from the list
    private void CleanupDeadPredators()
    {
        activePredators.RemoveAll(predator => predator == null);
    }

    // update UI showing how many coyotes are alive
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

    // draw spawn points in editor
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
