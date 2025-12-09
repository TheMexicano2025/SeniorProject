using UnityEngine;

public class terrainInteractable : MonoBehaviour, Interactable
{
    [Header("References")]
    public farmManager farmingManager;

    private void Start()
    {
        if (farmingManager == null)
        {
            farmingManager = FindObjectOfType<farmManager>();
        }
    }

    public string GetInteractionPrompt()
    {
        return "Press E to Till Soil";
    }

    public bool CanInteract(GameObject player)
    {
        EquipManager equipManager = player.GetComponent<EquipManager>();
        if (equipManager == null) return false;

        ItemSO equippedItem = equipManager.GetEquippedItem();
        return equippedItem != null && equippedItem.toolType == ItemSO.ToolType.Hoe;
    }

    public void Interact(GameObject player)
    {
        if (farmingManager == null) return;

        Transform orientation = player.transform.Find("Orientation");
        
        Vector3 tillDirection;
        if (orientation != null)
        {
            tillDirection = orientation.forward;
        }
        else
        {
            tillDirection = player.transform.forward;
        }
        
        tillDirection.y = 0;
        tillDirection.Normalize();
        
        float tillDistance = 1.5f;
        Vector3 tillPositionHorizontal = player.transform.position + (tillDirection * tillDistance);
        
        RaycastHit groundHit;
        Vector3 rayStart = new Vector3(tillPositionHorizontal.x, player.transform.position.y + 2f, tillPositionHorizontal.z);
        
        if (Physics.Raycast(rayStart, Vector3.down, out groundHit, 10f))
        {
            Vector3 tillPosition = new Vector3(tillPositionHorizontal.x, groundHit.point.y + 0.02f, tillPositionHorizontal.z);
            farmingManager.TillSoil(tillPosition, player);
        }
    }
}
