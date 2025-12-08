using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Base attack damage")]
    public float baseDamage = 20f;
    
    [Tooltip("Attack range in meters")]
    public float attackRange = 2f;
    
    [Tooltip("Cooldown between attacks")]
    public float attackCooldown = 0.5f;
    
    [Tooltip("Angle of attack arc")]
    public float attackAngle = 60f;
    
    [Header("Visual Feedback")]
    [Tooltip("UI Image to flash when attacking")]
    public Image attackFlashImage;
    
    [Tooltip("Flash color")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    
    [Tooltip("How long the flash lasts")]
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
            else
            {
                Debug.LogWarning("PlayerCombat: Orientation not found! Attacks will use player body direction.");
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
        
        UpdateFlash();
    }

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

    private void PerformAttack(ItemSO weapon)
    {
        float totalDamage = baseDamage + weapon.attackDamage;
        
        Vector3 attackDirection = orientation != null ? orientation.forward : transform.forward;
        Vector3 attackOrigin = transform.position + Vector3.up * 0.5f;
        
        Collider[] hitColliders = Physics.OverlapSphere(attackOrigin, attackRange);
        
        foreach (Collider col in hitColliders)
        {
            if (col.gameObject == gameObject) continue;
            
            Vector3 targetCenter = col.bounds.center;
            Vector3 directionToTarget = (targetCenter - attackOrigin).normalized;
            float angleToTarget = Vector3.Angle(attackDirection, directionToTarget);
            
            if (angleToTarget > attackAngle / 2f) continue;
            
            Health targetHealth = col.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(totalDamage);
            }
        }
    }

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
