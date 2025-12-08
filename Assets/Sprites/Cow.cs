using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cow : MonoBehaviour
{
    [Header("Lifecycle")]
    [Tooltip("Is this cow a baby?")]
    public bool isBaby = true;
    
    [Tooltip("Days until baby becomes adult")]
    public int daysToGrowUp = 3;
    
    [Tooltip("Current age in days")]
    public int currentAge = 0;
    
    [Tooltip("Adult cow prefab to spawn when grown")]
    public GameObject adultCowPrefab;
    
    [Header("Breeding")]
    [Tooltip("Can this cow breed?")]
    public bool canBreed = false;
    
    [Tooltip("Days cooldown after breeding")]
    public int breedingCooldownDays = 2;
    
    [Tooltip("Days until can breed again")]
    public int breedingCooldown = 0;
    
    [Tooltip("Baby cow prefab to spawn from breeding")]
    public GameObject babyCowPrefab;
    
    [Tooltip("Speed when moving to mate")]
    public float matingMoveSpeed = 2f;
    
    [Tooltip("How long the breeding animation lasts")]
    public float breedingDuration = 3f;
    
    [Tooltip("How far to push parents apart after breeding")]
    public float separationDistance = 3f;
    
    [Header("Milking")]
    [Tooltip("Can this cow be milked?")]
    public bool canBeMilked = false;
    
    [Tooltip("Days cooldown after milking")]
    public int milkCooldownDays = 1;
    
    [Tooltip("Days until can be milked again")]
    public int milkCooldown = 0;
    
    [Tooltip("Milk item to give when milked")]
    public ItemSO milkItem;
    
    [Tooltip("Should the empty bottle be consumed when milking?")]
    public bool consumeBottle = true;
    
    [Header("Death Drops")]
    [Tooltip("Item to drop on death")]
    public ItemSO meatDrop;
    
    [Tooltip("Amount of meat to drop")]
    public int meatDropAmount = 2;
    
    [Header("References")]
    public DayNightManager dayNightManager;
    public Health health;
    
    private bool hasSubscribedToDay = false;
    private bool isInLoveMode = false;
    private Cow targetMate = null;
    private bool isBreeding = false;
    private EasyPrimitiveAnimals.AnimalController animalController;
    private float originalMoveSpeed;

    private void Start()
    {
        if (dayNightManager == null)
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
        }
        
        if (health == null)
        {
            health = GetComponent<Health>();
        }
        
        animalController = GetComponent<EasyPrimitiveAnimals.AnimalController>();
        if (animalController != null)
        {
            originalMoveSpeed = animalController.movSpeed;
        }
        
        if (dayNightManager != null && !hasSubscribedToDay)
        {
            dayNightManager.onNewDay.AddListener(OnNewDay);
            hasSubscribedToDay = true;
        }
        
        if (health != null)
        {
            health.onDeath.AddListener(OnDeath);
        }
        
        UpdateStatus();
    }

    private void Update()
    {
        if (isInLoveMode && targetMate != null && !isBreeding)
        {
            MoveTowardsMate();
        }
    }

    private void OnNewDay()
    {
        currentAge++;
        
        if (isBaby && currentAge >= daysToGrowUp)
        {
            GrowUp();
        }
        
        if (breedingCooldown > 0)
        {
            breedingCooldown--;
        }
        
        if (milkCooldown > 0)
        {
            milkCooldown--;
        }
        
        UpdateStatus();
    }

    private void GrowUp()
    {
        if (adultCowPrefab == null)
        {
            Debug.LogWarning("No adult cow prefab assigned!");
            isBaby = false;
            UpdateStatus();
            return;
        }
        
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        
        GameObject adultCow = Instantiate(adultCowPrefab, position, rotation);
        Cow adultCowScript = adultCow.GetComponent<Cow>();
        
        if (adultCowScript != null)
        {
            adultCowScript.currentAge = currentAge;
        }
        
        Debug.Log($"{gameObject.name} has grown into an adult cow!");
        
        Destroy(gameObject);
    }

    private void UpdateStatus()
    {
        canBreed = !isBaby && breedingCooldown <= 0;
        canBeMilked = !isBaby && milkCooldown <= 0;
    }

    public void EnterLoveMode()
    {
        if (!canBreed || isBaby)
        {
            Debug.Log("This cow is not ready to breed!");
            return;
        }
        
        isInLoveMode = true;
        Debug.Log($"{gameObject.name} entered love mode!");
        
        FindNearbyMate();
    }

    private void FindNearbyMate()
    {
        Cow[] allCows = FindObjectsOfType<Cow>();
        
        foreach (Cow otherCow in allCows)
        {
            if (otherCow != this && otherCow.isInLoveMode && otherCow.canBreed && !otherCow.isBaby)
            {
                targetMate = otherCow;
                otherCow.targetMate = this;
                
                Debug.Log($"{gameObject.name} found mate: {otherCow.gameObject.name}");
                
                if (animalController != null)
                {
                    animalController.movSpeed = matingMoveSpeed;
                }
                
                return;
            }
        }
        
        Debug.Log($"{gameObject.name} is waiting for a mate...");
    }

    private void MoveTowardsMate()
    {
        if (targetMate == null)
        {
            FindNearbyMate();
            return;
        }
        
        Vector3 direction = (targetMate.transform.position - transform.position).normalized;
        direction.y = 0;
        
        transform.position += direction * matingMoveSpeed * Time.deltaTime;
        
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        
        float distance = Vector3.Distance(transform.position, targetMate.transform.position);
        if (distance < 2f && !isBreeding && targetMate != null && !targetMate.isBreeding)
        {
            if (!targetMate.isBreeding)
            {
                targetMate.isBreeding = true;
            }
            StartCoroutine(BreedWithMate());
        }
    }

    private IEnumerator BreedWithMate()
    {
        if (targetMate == null || isBreeding) yield break;
        
        isBreeding = true;
        
        if (animalController != null)
        {
            animalController.enabled = false;
        }
        
        if (targetMate.animalController != null)
        {
            targetMate.animalController.enabled = false;
        }
        
        Debug.Log($"{gameObject.name} breeding with {targetMate.gameObject.name}");
        
        yield return new WaitForSeconds(breedingDuration);
        
        if (babyCowPrefab != null && targetMate != null)
        {
            Vector3 midpoint = (transform.position + targetMate.transform.position) / 2f;
            Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            Vector3 spawnPos = midpoint + offset;
            spawnPos.y = transform.position.y;
            
            GameObject newBabyCow = Instantiate(babyCowPrefab, spawnPos, Quaternion.identity);
            Cow babyCowScript = newBabyCow.GetComponent<Cow>();
            
            if (babyCowScript != null)
            {
                babyCowScript.isBaby = true;
                babyCowScript.currentAge = 0;
            }
            
            Debug.Log("A baby cow was born!");
        }
        
        if (targetMate != null)
        {
            Vector3 directionAway = (transform.position - targetMate.transform.position).normalized;
            directionAway.y = 0;
            
            transform.position += directionAway * separationDistance;
            targetMate.transform.position -= directionAway * separationDistance;
            
            breedingCooldown = breedingCooldownDays;
            targetMate.breedingCooldown = breedingCooldownDays;
            
            targetMate.ForceExitLoveMode();
        }
        
        ForceExitLoveMode();
    }

    public void ForceExitLoveMode()
    {
        Debug.Log($"{gameObject.name} exiting love mode");
        
        isInLoveMode = false;
        isBreeding = false;
        targetMate = null;
        
        if (animalController != null)
        {
            animalController.movSpeed = originalMoveSpeed;
            animalController.enabled = true;
            Debug.Log($"{gameObject.name} AnimalController re-enabled");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no AnimalController!");
        }
        
        UpdateStatus();
    }

    public bool TryMilk(GameObject player, ItemSO bottleItem)
    {
        if (!canBeMilked)
        {
            if (isBaby)
            {
                Debug.Log("This cow is too young to milk!");
            }
            else
            {
                Debug.Log($"This cow was recently milked. Wait {milkCooldown} more day(s).");
            }
            return false;
        }
        
        InvManager inventory = FindObjectOfType<InvManager>();
        if (inventory != null && milkItem != null)
        {
            if (consumeBottle && bottleItem != null)
            {
                inventory.RemoveItem(bottleItem, 1);
            }
            
            inventory.AddItem(milkItem, 1);
            Debug.Log("Collected milk from cow!");
        }
        
        milkCooldown = milkCooldownDays;
        canBeMilked = false;
        
        return true;
    }

    private void OnDeath()
    {
        if (meatDrop != null)
        {
            InvManager inventory = FindObjectOfType<InvManager>();
            if (inventory != null)
            {
                inventory.AddItem(meatDrop, meatDropAmount);
                Debug.Log($"Cow dropped {meatDropAmount}x {meatDrop.itemName}");
            }
        }
    }

    private void OnDestroy()
    {
        if (dayNightManager != null && hasSubscribedToDay)
        {
            dayNightManager.onNewDay.RemoveListener(OnNewDay);
        }
        
        if (health != null)
        {
            health.onDeath.RemoveListener(OnDeath);
        }
    }

    public string GetCowInfo()
    {
        if (isBaby)
        {
            return $"Baby Cow (Age: {currentAge}/{daysToGrowUp} days)";
        }
        else
        {
            string info = "Adult Cow";
            
            if (isInLoveMode)
            {
                info += "\n💕 In Love Mode";
            }
            
            if (canBeMilked)
            {
                info += "\nReady to milk!";
            }
            else if (milkCooldown > 0)
            {
                info += $"\nMilk in {milkCooldown} day(s)";
            }
            
            if (canBreed)
            {
                info += "\nReady to breed!";
            }
            else if (breedingCooldown > 0)
            {
                info += $"\nBreed in {breedingCooldown} day(s)";
            }
            
            return info;
        }
    }
}