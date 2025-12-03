using System.Collections.Generic;
using UnityEngine;

public class farmManager : MonoBehaviour
{
    [Header("Tilling Settings")]
    public GameObject tilledPlotPrefab;
    public float plotSize = 1f;
    public float plotHeight = 0.1f;
    
    [Header("Grid Settings")]
    public float snapToGrid = 1f;

    [Header("Growth Settings")]
    [Tooltip("Hours between each growth stage")]
    public float hoursPerStage = 2f;

    [Header("Growth Stage Prefabs (Optional)")]
    [Tooltip("Leave empty to use default shapes")]
    public GameObject stage0Prefab;
    public GameObject stage1Prefab;
    public GameObject stage2Prefab;
    public GameObject stage3Prefab;

    private List<TilledPlot> tilledPlots = new List<TilledPlot>();

    [Header("Zone Restriction")]
    [Tooltip("Optional: Restrict tilling to this zone only")]
    public TillingZone allowedTillingZone;

    [Tooltip("Message shown when trying to till outside the zone")]
    public string outsideZoneMessage = "You can only till soil in the farm plot area!";

    public bool TillSoil(Vector3 position, GameObject player)
    {
        EquipManager equipManager = player.GetComponent<EquipManager>();
        if (equipManager == null)
        {
            Debug.LogWarning("FarmingManager: Player has no EquipManager!");
            return false;
        }

        ItemSO equippedItem = equipManager.GetEquippedItem();
        
        if (equippedItem == null || equippedItem.toolType != ItemSO.ToolType.Hoe)
        {
            Debug.Log("You need a hoe to till the soil!");
            return false;
        }

        Vector3 snappedPosition = SnapToGrid(position);
        
        if (allowedTillingZone != null && !allowedTillingZone.IsPositionInZone(snappedPosition))
        {
            Debug.Log(outsideZoneMessage);
            return false;
        }
        
        if (IsAlreadyTilled(snappedPosition))
        {
            Debug.Log("This soil is already tilled!");
            return false;
        }

        CreateTilledPlot(snappedPosition);
        return true;
    }


    private Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round(position.x / snapToGrid) * snapToGrid;
        float z = Mathf.Round(position.z / snapToGrid) * snapToGrid;
        return new Vector3(x, position.y, z);
    }

    private bool IsAlreadyTilled(Vector3 position)
    {
        tilledPlots.RemoveAll(plot => plot == null);
        
        foreach (TilledPlot plot in tilledPlots)
        {
            if (plot != null)
            {
                Vector3 plotPos = plot.transform.position;
                Vector3 checkPos = position;
                
                float xDiff = Mathf.Abs(plotPos.x - checkPos.x);
                float zDiff = Mathf.Abs(plotPos.z - checkPos.z);
                
                if (xDiff < (snapToGrid * 0.5f) && zDiff < (snapToGrid * 0.5f))
                {
                    Debug.LogWarning($"Cannot till - plot already exists at ({plotPos.x}, {plotPos.z})");
                    return true;
                }
            }
        }
        return false;
    }


    private void CreateTilledPlot(Vector3 position)
    {
        GameObject plotObject;

        if (tilledPlotPrefab != null)
        {
            plotObject = Instantiate(tilledPlotPrefab, position, Quaternion.identity);
        }
        else
        {
            plotObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            Vector3 adjustedPosition = new Vector3(position.x, position.y - (plotHeight / 2f), position.z);
            plotObject.transform.position = adjustedPosition;
            plotObject.transform.localScale = new Vector3(plotSize, plotHeight, plotSize);
            
            Renderer renderer = plotObject.GetComponent<Renderer>();
            renderer.material.color = new Color(0.4f, 0.25f, 0.1f);
            
            BoxCollider plotCollider = plotObject.GetComponent<BoxCollider>();
            if(plotCollider != null)
            {
                plotCollider.isTrigger = true;
                plotCollider.center = new Vector3(0, 1.5f, 0);
                plotCollider.size = new Vector3(1f, 3f, 1f);
            }
        }

        plotObject.name = $"TilledPlot_{tilledPlots.Count}";
        plotObject.layer = LayerMask.NameToLayer("Item");

        TilledPlot tilledPlot = plotObject.GetComponent<TilledPlot>();
        if (tilledPlot == null)
        {
            tilledPlot = plotObject.AddComponent<TilledPlot>();
        }
        
        tilledPlot.hoursPerStage = hoursPerStage;
        tilledPlot.stage0Prefab = stage0Prefab;
        tilledPlot.stage1Prefab = stage1Prefab;
        tilledPlot.stage2Prefab = stage2Prefab;
        tilledPlot.stage3Prefab = stage3Prefab;

        tilledPlots.Add(tilledPlot);

        Debug.Log($"Created tilled plot at {position}");
    }
}
