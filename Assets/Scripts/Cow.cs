using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script controls cow behavior including growing up breeding and milking
// cows age each day and can be fed corn to breed or milked with a bottle
public class Cow : MonoBehaviour
{
    [Header("Lifecycle")]
    public bool isBaby = true;
    public int daysToGrowUp = 3;
    public int currentAge = 0;
    public GameObject adultCowPrefab;
    
    [Header("Breeding")]
    public bool canBreed = false;
    public int breedingCooldownDays = 2;
    public int breedingCooldown = 0;
    public GameObject babyCowPrefab;
    public float matingMoveSpeed = 2f;
    public float breedingDuration = 3f;
    public float separationDistance = 3f; // how far apart to push cows after breeding
    
    [Header("Milking")]
    public bool canBeMilked = false;
    public int milkCooldownDays = 1;
    public int milkCooldown = 0;
    public ItemSO milkItem;
    public bool consumeBottle = true; // does milking use up the bottle
    
    [Header("Death Drops")]
    public ItemSO meatDrop;
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
        
        // subscribe to new day events to age the cow
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
        // if in love mode move toward mate
        if (isInLoveMode && targetMate != null && !isBreeding)
        {
            MoveTowardsMate();
        }
    }

    // called every new day to age the cow and reset cooldowns
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

    // turn baby cow into adult cow
    private void GrowUp()
    {
        if (adultCowPrefab == null)
        {
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
        
        Destroy(gameObject);
    }

    // update what the cow can do based on age and cooldowns
    private void UpdateStatus()
    {
        canBreed = !isBaby && breedingCooldown <= 0;
        canBeMilked = !isBaby && milkCooldown <= 0;
    }

    // called when player feeds corn to the cow
    public void EnterLoveMode()
    {
        if (!canBreed || isBaby) return;
        
        isInLoveMode = true;
        FindNearbyMate();
    }

    // look for another cow in love mode to breed with
    private void FindNearbyMate()
    {
        Cow[] allCows = FindObjectsOfType<Cow>();
        
        foreach (Cow otherCow in allCows)
        {
            if (otherCow != this && otherCow.isInLoveMode && otherCow.canBreed && !otherCow.isBaby)
            {
                targetMate = otherCow;
                otherCow.targetMate = this;
                
                if (animalController != null)
                {
                    animalController.movSpeed = matingMoveSpeed;
                }
                
                return;
            }
        }
    }

    // move toward the mate until close enough to breed
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

    // breed with mate and spawn a baby cow
    private IEnumerator BreedWithMate()
    {
        if (targetMate == null || isBreeding) yield break;
        
        isBreeding = true;
        
        // disable wandering AI during breeding
        if (animalController != null)
        {
            animalController.enabled = false;
        }
        
        if (targetMate.animalController != null)
        {
            targetMate.animalController.enabled = false;
        }
        
        yield return new WaitForSeconds(breedingDuration);
        
        // spawn baby cow between the two parents
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
        }
        
        // push cows apart and set breeding cooldown
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

    // exit love mode and return to normal behavior
    public void ForceExitLoveMode()
    {
        isInLoveMode = false;
        isBreeding = false;
        targetMate = null;
        
        if (animalController != null)
        {
            animalController.movSpeed = originalMoveSpeed;
            animalController.enabled = true;
        }
        
        UpdateStatus();
    }

    // milk the cow when interacted with bottle
    public bool TryMilk(GameObject player, ItemSO bottleItem)
    {
        if (!canBeMilked)
        {
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
        }
        
        milkCooldown = milkCooldownDays;
        canBeMilked = false;
        
        return true;
    }

    // drop meat when cow dies
    private void OnDeath()
    {
        if (meatDrop != null)
        {
            InvManager inventory = FindObjectOfType<InvManager>();
            if (inventory != null)
            {
                inventory.AddItem(meatDrop, meatDropAmount);
            }
        }
    }

    private void OnDestroy()
    {
        // clean up event listeners
        if (dayNightManager != null && hasSubscribedToDay)
        {
            dayNightManager.onNewDay.RemoveListener(OnNewDay);
        }
        
        if (health != null)
        {
            health.onDeath.RemoveListener(OnDeath);
        }
    }

    // get cow status info for debugging
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
