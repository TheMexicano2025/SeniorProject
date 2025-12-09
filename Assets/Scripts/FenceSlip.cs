using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceSlip : MonoBehaviour
{
    [Range(0f, 100f)]
    public float slipThroughChance = 85f;
    
    public float slipThroughCooldown = 2f;
    public float teleportDistance = 2.5f;
    
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
        Debug.Log($"<color=cyan>Rolling dice: {roll:F2} vs {slipThroughChance}</color>");
        
        if (roll <= slipThroughChance)
        {
            Debug.Log("<color=green>SUCCESS! Predator slipping through!</color>");
            StartCoroutine(TeleportThroughFence(collision));
        }
        else
        {
            Debug.Log($"<color=red>FAILED! Predator blocked. Will retry in {slipThroughCooldown} seconds.</color>");
        }
    }
    
    private IEnumerator TeleportThroughFence(Collision collision)
    {
        Collider predatorCollider = collision.collider;
        Rigidbody predatorRb = collision.rigidbody;
        Transform predatorTransform = predatorCollider.transform;
        Collider fenceCollider = GetComponent<Collider>();
        
        Physics.IgnoreCollision(fenceCollider, predatorCollider, true);
        
        Vector3 throughDirection = predatorTransform.forward;
        throughDirection.y = 0;
        throughDirection.Normalize();
        
        Vector3 oldPosition = predatorTransform.position;
        Vector3 teleportPosition = oldPosition + (throughDirection * teleportDistance);
        
        Debug.Log($"<color=lime>Predator facing: {throughDirection}</color>");
        Debug.Log($"<color=lime>Teleporting from {oldPosition} to {teleportPosition}</color>");
        
        predatorTransform.position = teleportPosition;
        
        if (predatorRb != null)
        {
            predatorRb.velocity = throughDirection * 3.5f;
            Debug.Log($"<color=lime>Setting velocity: {predatorRb.velocity}</color>");
        }
        
        yield return new WaitForSeconds(1f);
        
        Physics.IgnoreCollision(fenceCollider, predatorCollider, false);
        
        if (lastSlipAttempts.ContainsKey(predatorCollider))
        {
            lastSlipAttempts.Remove(predatorCollider);
        }
        
        Debug.Log("<color=grey>Collision re-enabled</color>");
    }
}
