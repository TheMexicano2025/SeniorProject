using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TillingZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("Visual color of the zone bounds (Scene view only)")]
    public Color zoneColor = new Color(0f, 1f, 0f, 0.2f);
    
    [Tooltip("Show zone bounds in Scene view")]
    public bool showGizmos = true;

    private BoxCollider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider>();
        if (zoneCollider == null)
        {
            Debug.LogWarning("TillingZone: No BoxCollider found! Adding one...");
            zoneCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        zoneCollider.isTrigger = true;
    }

    public bool IsPositionInZone(Vector3 position)
    {
        if (zoneCollider == null) return false;
        
        Bounds bounds = zoneCollider.bounds;
        
        bool inXRange = position.x >= bounds.min.x && position.x <= bounds.max.x;
        bool inZRange = position.z >= bounds.min.z && position.z <= bounds.max.z;
        
        return inXRange && inZRange;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;
        
        Gizmos.color = zoneColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(col.center, col.size);
        
        Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
        Gizmos.DrawWireCube(col.center, col.size);
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(col.center, col.size);
    }
}
