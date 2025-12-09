using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// this script handles player attacking with weapons
// left click to attack enemies in front of you
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public float baseDamage = 20f;
    public float attackRange = 2f;
    public float attackCooldown = 0.5f;
    public float attackAngle = 60f; // cone angle in front of player

    [Header("Visual Feedback")]
    public Image attackFlashImage;
    public Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    public float flashDuration = 0.1f;
    
    [Header("References")]
    public EquipManager equipManager;
    public Transform orientation;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    private float lastAttackTime = 0f;
    private float flashTimer = 0f;

    private void Start()
    {
        if (orientation == null)
        {
            GameObject orientationObj = GameObject.Find("Orientation");
            if (orientationObj != null)
            {
                orientation = orientationObj.transform;
            }
        }
    }

    private void Update()
    {
        // left click to attack
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
        
        UpdateFlash();
    }

    // check if player can attack
    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }
        
        if (equipManager == null)
        {
            equipManager = GetComponent<EquipManager>();
        }
        
        ItemSO equippedItem = equipManager?.GetEquippedItem();
        
        // only attack if holding a sword
        if (equippedItem == null || equippedItem.toolType != ItemSO.ToolType.Sword)
        {
            return;
        }
        
        PerformAttack(equippedItem);
        lastAttackTime = Time.time;
        
        if (attackFlashImage != null)
        {
            flashTimer = flashDuration;
        }
    }

    // deal damage to all enemies in front of player
    private void PerformAttack(ItemSO weapon)
    {
        float totalDamage = baseDamage + weapon.attackDamage;
        
        Vector3 attackDirection = orientation != null ? orientation.forward : transform.forward;
        attackDirection.y = 0;
        attackDirection.Normalize();
        
        Vector3 attackOrigin = transform.position + Vector3.up * 0.5f;
        
        // find all colliders in attack range
        Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, attackRange);
        
        foreach (Collider col in hitColliders)
        {
            if (col.gameObject == gameObject) continue;
            
            Vector3 targetCenter = col.bounds.center;
            Vector3 directionToTarget = (targetCenter - attackOrigin);
            directionToTarget.y = 0;
            directionToTarget.Normalize();
            
            if (directionToTarget.sqrMagnitude < 0.01f) continue;
            
            // check if target is in front of player within attack angle
            float angleToTarget = Vector3.Angle(attackDirection, directionToTarget);
            
            if (angleToTarget > attackAngle / 2f) continue;
            
            Health targetHealth = col.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(totalDamage);
            }
        }
    }

    // fade out the attack flash effect
    private void UpdateFlash()
    {
        if (attackFlashImage == null) return;
        
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            Color color = flashColor;
            color.a = flashColor.a * (flashTimer / flashDuration);
            attackFlashImage.color = color;
        }
        else
        {
            attackFlashImage.color = Color.clear;
        }
    }

    // draw attack range and cone in editor
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Vector3 attackDirection = orientation != null ? orientation.forward : transform.forward;
        Vector3 attackOrigin = transform.position + Vector3.up * 0.5f;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin, attackRange);
        
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2f, 0) * attackDirection * attackRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2f, 0) * attackDirection * attackRange;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(attackOrigin, attackOrigin + rightBoundary);
        Gizmos.DrawLine(attackOrigin, attackOrigin + leftBoundary);
        Gizmos.DrawLine(attackOrigin, attackOrigin + attackDirection * attackRange);
    }
}
