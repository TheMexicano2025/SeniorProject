using System.Collections.Generic;
using UnityEngine;

// this script manages the farming system
// it creates tilled plots when you use a hoe and handles grid snapping
public class farmManager : MonoBehaviour
{
    [Header("Tilling Settings")]
    public GameObject tilledPlotPrefab;
    public float plotSize = 1f;
    public float plotHeight = 0.1f;
    
    [Header("Grid Settings")]
    public float snapToGrid = 1f;

    [Header("Growth Settings")]
    public float hoursPerStage = 2f;

    [Header("Growth Stage Prefabs (Optional)")]
    public GameObject stage0Prefab;
    public GameObject stage1Prefab;
    public GameObject stage2Prefab;
    public GameObject stage3Prefab;

    private List<TilledPlot> tilledPlots = new List<TilledPlot>();

    [Header("Zone Restriction")]
    public TillingZone allowedTillingZone;
    public string outsideZoneMessage = "You can only till soil in the farm plot area!";

    public bool TillSoil(Vector3 position, GameObject player)
    {
        EquipManager equipManager = player.GetComponent<EquipManager>();
        if (equipManager == null) return false;

        ItemSO equippedItem = equipManager.GetEquippedItem();
        
        // make sure player has a hoe equipped
        if (equippedItem == null || equippedItem.toolType != ItemSO.ToolType.Hoe)
        {
            return false;
        }

        Vector3 snappedPosition = SnapToGrid(position);
        
        // check if we're inside the allowed farming zone
        if (allowedTillingZone != null && !allowedTillingZone.IsPositionInZone(snappedPosition))
        {
            return false;
        }
        
        // don't till the same spot twice
        if (IsAlreadyTilled(snappedPosition))
        {
            return false;
        }

        CreateTilledPlot(snappedPosition);
        return true;
    }

    // snap position to grid so plots line up nicely
    private Vector3 SnapToGrid(Vector3 position)
    {
        float x = Mathf.Round(position.x / snapToGrid) * snapToGrid;
        float z = Mathf.Round(position.z / snapToGrid) * snapToGrid;
        return new Vector3(x, position.y, z);
    }

    // check if there's already a plot at this position
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
                    return true;
                }
            }
        }
        return false;
    }

    // create a new tilled plot at the position
    private void CreateTilledPlot(Vector3 position)
    {
        GameObject plotObject;

        if (tilledPlotPrefab != null)
        {
            plotObject = Instantiate(tilledPlotPrefab, position, Quaternion.identity);
        }
        else
        {
            // if no prefab assigned make a simple brown cube
            plotObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            Vector3 adjustedPosition = new Vector3(position.x, position.y - (plotHeight / 2f), position.z);
            plotObject.transform.position = adjustedPosition;
            plotObject.transform.localScale = new Vector3(plotSize, plotHeight, plotSize);
            
            Renderer renderer = plotObject.GetComponent<Renderer>();
            renderer.material.color = new Color(0.4f, 0.25f, 0.1f);
            
            BoxCollider plotCollider = plotObject.GetComponent<BoxCollider>();
            if (plotCollider != null)
            {
                plotCollider.isTrigger = true;
                plotCollider.center = new Vector3(0, 1.5f, 0);
                plotCollider.size = new Vector3(1f, 3f, 1f);
            }
        }

        plotObject.name = $"TilledPlot_{tilledPlots.Count}";
        plotObject.layer = LayerMask.NameToLayer("Item");

        // add or get the TilledPlot component
        TilledPlot tilledPlot = plotObject.GetComponent<TilledPlot>();
        if (tilledPlot == null)
        {
            tilledPlot = plotObject.AddComponent<TilledPlot>();
        }
        
        // pass growth settings to the plot
        tilledPlot.hoursPerStage = hoursPerStage;
        tilledPlot.stage0Prefab = stage0Prefab;
        tilledPlot.stage1Prefab = stage1Prefab;
        tilledPlot.stage2Prefab = stage2Prefab;
        tilledPlot.stage3Prefab = stage3Prefab;

        tilledPlots.Add(tilledPlot);
    }
}
