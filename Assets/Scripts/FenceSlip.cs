using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FenceSlip : MonoBehaviour
{
    [Range(0f, 100f)]
    public float slipThroughChance = 85f;
    public float slipThroughCooldown = 2f;
    public float teleportDistance = 2.5f;
    public float groundCheckDistance = 10f;
    public LayerMask groundLayer;
    
    private Dictionary<Collider, float> lastSlipAttempts = new Dictionary<Collider, float>();
    
    private void OnCollisionEnter(Collision collision)
    {
        TrySlipThrough(collision);
    }
    
    private void OnCollisionStay(Collision collision)
    {
        TrySlipThrough(collision);
    }
    
    private void TrySlipThrough(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Pig"))
            return;
        
        Collider predatorCollider = collision.collider;
        
        if (!lastSlipAttempts.ContainsKey(predatorCollider))
        {
            lastSlipAttempts[predatorCollider] = 0f;
        }
        
        if (Time.time - lastSlipAttempts[predatorCollider] < slipThroughCooldown)
        {
            return;
        }
        
        lastSlipAttempts[predatorCollider] = Time.time;
        
        float roll = Random.Range(0f, 100f);
        
        if (roll <= slipThroughChance)
        {
            StartCoroutine(TeleportThroughFence(collision));
        }
    }
    
    private IEnumerator TeleportThroughFence(Collision collision)
    {
        Collider predatorCollider = collision.collider;
        Transform predatorTransform = predatorCollider.transform;
        NavMeshAgent predatorAgent = predatorCollider.GetComponent<NavMeshAgent>();
        Collider fenceCollider = GetComponent<Collider>();
        
        if (predatorAgent != null)
        {
            predatorAgent.enabled = false;
        }
        
        Physics.IgnoreCollision(fenceCollider, predatorCollider, true);
        
        Vector3 throughDirection = predatorTransform.forward;
        throughDirection.y = 0;
        throughDirection.Normalize();
        
        Vector3 teleportPosition = predatorTransform.position + (throughDirection * teleportDistance);
        
        RaycastHit hit;
        if (Physics.Raycast(teleportPosition + Vector3.up * 5f, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            teleportPosition.y = hit.point.y;
        }
        
        predatorTransform.position = teleportPosition;
        
        yield return new WaitForSeconds(0.1f);
        
        if (predatorAgent != null)
        {
            predatorAgent.enabled = true;
            
            if (predatorAgent.isOnNavMesh)
            {
                predatorAgent.Warp(teleportPosition);
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        Physics.IgnoreCollision(fenceCollider, predatorCollider, false);
        
        if (lastSlipAttempts.ContainsKey(predatorCollider))
        {
            lastSlipAttempts.Remove(predatorCollider);
        }
    }
}
