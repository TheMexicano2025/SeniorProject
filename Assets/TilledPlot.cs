using UnityEngine;

public class TilledPlot : MonoBehaviour, Interactable
{
    [Header("Plot State")]
    public bool isPlanted = false;
    public ItemSO plantedCrop;
    public int growthStage = 0;
    public int maxGrowthStage = 3;
    public GameObject plantVisual;

    [Header("Growth Settings")]
    [Tooltip("Hours between each growth stage")]
    public float hoursPerStage = 2f;
    
    [Header("Growth Stage Prefabs")]
    [HideInInspector] public GameObject stage0Prefab;
    [HideInInspector] public GameObject stage1Prefab;
    [HideInInspector] public GameObject stage2Prefab;
    [HideInInspector] public GameObject stage3Prefab;
    
    private float plantedAtHour = 0f;
    private DayNightManager dayNightManager;

    private void Start()
    {
        dayNightManager = FindObjectOfType<DayNightManager>();
    }

    private void Update()
    {
        if (isPlanted && growthStage < maxGrowthStage && dayNightManager != null)
        {
            float currentTime = dayNightManager.GetCurrentTime();
            float hoursPassed = currentTime - plantedAtHour;
            
            if (hoursPassed < 0) hoursPassed += 24f;

            int targetStage = Mathf.FloorToInt(hoursPassed / hoursPerStage);
            targetStage = Mathf.Clamp(targetStage, 0, maxGrowthStage);

            if (targetStage > growthStage)
            {
                growthStage = targetStage;
                UpdatePlantVisual();
            }
        }
    }

    public string GetInteractionPrompt()
    {
        if (!isPlanted) return "Press E to Plant Seeds";
        if (growthStage >= maxGrowthStage) return "Press E to Harvest";
        return $"Growing... Stage {growthStage}/{maxGrowthStage}";
    }

    public bool CanInteract(GameObject player)
    {
        if (isPlanted && growthStage >= maxGrowthStage) return true;
        if (isPlanted) return false;

        EquipManager equipManager = player.GetComponent<EquipManager>();
        if (equipManager == null) return false;

        ItemSO equippedItem = equipManager.GetEquippedItem();
        return equippedItem != null && equippedItem.seedType != ItemSO.SeedType.None;
    }

    public void Interact(GameObject player)
    {
        if (isPlanted && growthStage >= maxGrowthStage)
        {
            Harvest(player);
            return;
        }

        EquipManager equipManager = player.GetComponent<EquipManager>();
        if (equipManager == null) return;

        ItemSO equippedItem = equipManager.GetEquippedItem();
        if (equippedItem != null && equippedItem.seedType != ItemSO.SeedType.None)
        {
            PlantSeed(equippedItem, player);
        }
    }

    private void PlantSeed(ItemSO seed, GameObject player)
    {
        if (dayNightManager != null)
        {
            plantedAtHour = dayNightManager.GetCurrentTime();
        }

        isPlanted = true;
        plantedCrop = seed;
        growthStage = 0;
        
        CreatePlantVisual();

        InvManager inventoryManager = FindObjectOfType<InvManager>();
        if (inventoryManager != null)
        {
            inventoryManager.RemoveItem(seed, 1);
        }

        Debug.Log($"Planted {seed.itemName}");
    }

    private void CreatePlantVisual()
    {
        GameObject prefabToUse = GetPrefabForStage(growthStage);
        
        if (prefabToUse != null)
        {
            plantVisual = Instantiate(prefabToUse, transform);
            
            plantVisual.transform.localPosition = Vector3.up * 0.3f;
            plantVisual.transform.localRotation = Quaternion.identity;
            plantVisual.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);
            
            RemoveAllColliders(plantVisual);
        }
        else
        {
            plantVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plantVisual.transform.SetParent(transform);
            plantVisual.transform.localPosition = Vector3.up * 0.5f;
            plantVisual.transform.localScale = Vector3.one * 0.3f;
            plantVisual.GetComponent<Renderer>().material.color = new Color(0.8f, 0.7f, 0.3f);
            Destroy(plantVisual.GetComponent<Collider>());
        }
    }

    private void UpdatePlantVisual()
    {
        if (plantVisual != null)
        {
            Destroy(plantVisual);
        }

        GameObject prefabToUse = GetPrefabForStage(growthStage);
        
        if (prefabToUse != null)
        {
            plantVisual = Instantiate(prefabToUse, transform);
            
            float heightByStage = 1.0f + (growthStage * 0.3f);
            float heightScale = 1.0f + (growthStage * 3.0f);
            float widthScale = 0.8f + (growthStage * 0.2f);

            if (growthStage == 1)
            {
                widthScale = 0.3f;
            }
            
            plantVisual.transform.localPosition = Vector3.up * heightByStage;
            plantVisual.transform.localRotation = Quaternion.identity;
            plantVisual.transform.localScale = new Vector3(widthScale, heightScale, widthScale);
            
            RemoveAllColliders(plantVisual);
        }
        else
        {
            plantVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            plantVisual.transform.SetParent(transform);
            
            float size = 0.3f + (growthStage * 0.2f);
            float height = 0.5f + (growthStage * 0.15f);
            
            plantVisual.transform.localPosition = Vector3.up * height;
            plantVisual.transform.localScale = Vector3.one * size;
            
            Color[] stageColors = new Color[]
            {
                new Color(0.8f, 0.7f, 0.3f),
                new Color(0.4f, 0.8f, 0.3f),
                new Color(0.6f, 0.9f, 0.4f),
                new Color(0.9f, 0.8f, 0.2f)
            };
            
            plantVisual.GetComponent<Renderer>().material.color = stageColors[growthStage];
            Destroy(plantVisual.GetComponent<Collider>());
        }
        
        Debug.Log($"Crop grew to stage {growthStage}");
    }

    private GameObject GetPrefabForStage(int stage)
    {
        switch (stage)
        {
            case 0: return stage0Prefab;
            case 1: return stage1Prefab;
            case 2: return stage2Prefab;
            case 3: return stage3Prefab;
            default: return null;
        }
    }

    private void Harvest(GameObject player)
    {
        InvManager inventoryManager = FindObjectOfType<InvManager>();
        if (inventoryManager != null && plantedCrop != null)
        {
            if (plantedCrop.harvestItem != null)
            {
                int cropAmount = plantedCrop.harvestYield > 0 ? plantedCrop.harvestYield : 1;
                inventoryManager.AddItem(plantedCrop.harvestItem, cropAmount);
                Debug.Log($"Harvested {cropAmount}x {plantedCrop.harvestItem.itemName}!");
            }
            
            if (plantedCrop.seedItem != null)
            {
                int seedAmount = plantedCrop.seedYield > 0 ? plantedCrop.seedYield : 1;
                inventoryManager.AddItem(plantedCrop.seedItem, seedAmount);
                Debug.Log($"Received {seedAmount}x {plantedCrop.seedItem.itemName}!");
            }
            else
            {
                Debug.LogWarning($"No seed item set for {plantedCrop.itemName}!");
            }
        }

        if (plantVisual != null) Destroy(plantVisual);
        
        isPlanted = false;
        plantedCrop = null;
        growthStage = 0;
        plantedAtHour = 0f;
    }



    private void RemoveAllColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            Destroy(col);
        }
    }
}
