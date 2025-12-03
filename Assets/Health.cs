using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points")]
    public float maxHealth = 100f;
    
    [Tooltip("Current health points")]
    [SerializeField] private float currentHealth;
    
    [Header("Death Settings")]
    [Tooltip("Should this entity be destroyed on death?")]
    public bool destroyOnDeath = false;
    
    [Tooltip("Delay before destroying (seconds)")]
    public float destroyDelay = 0f;
    
    [Header("Hit Feedback")]
    [Tooltip("Should entity be knocked back when hit?")]
    public bool enableKnockback = true;
    
    [Tooltip("Force of knockback")]
    public float knockbackForce = 5f;
    
    [Tooltip("Duration of hit flash effect")]
    public float flashDuration = 0.1f;
    
    [Tooltip("Color to flash when hit")]
    public Color flashColor = Color.red;
    
    [Header("Events")]
    public UnityEvent<float> onHealthChanged;
    public UnityEvent<float, float> onDamageTaken;
    public UnityEvent onDeath;
    public UnityEvent onHealed;
    
    private bool isDead = false;
    private Rigidbody rb;
    private Renderer[] renderers;
    private Color[] originalColors;
    private Material[] originalMaterials;

    private void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth);
        
        rb = GetComponent<Rigidbody>();
        renderers = GetComponentsInChildren<Renderer>();
        
        if (renderers.Length > 0)
        {
            originalColors = new Color[renderers.Length];
            originalMaterials = new Material[renderers.Length];
            
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material != null)
                {
                    originalMaterials[i] = renderers[i].material;
                    originalColors[i] = renderers[i].material.color;
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (isDead) return;
        
        float previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        onDamageTaken?.Invoke(damage, currentHealth);
        onHealthChanged?.Invoke(currentHealth);
        
        if (enableKnockback && hitDirection != Vector3.zero && rb != null)
        {
            Vector3 knockbackDir = hitDirection.normalized;
            knockbackDir.y = 0.3f;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }
        
        StartCoroutine(FlashEffect());
        
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        
        float previousHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
        
        onHealed?.Invoke();
        onHealthChanged?.Invoke(currentHealth);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} has died!");
        
        onDeath?.Invoke();
        
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private IEnumerator FlashEffect()
    {
        if (renderers == null || renderers.Length == 0) yield break;
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = flashColor;
            }
        }
        
        yield return new WaitForSeconds(flashDuration);
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null && i < originalColors.Length)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth);
    }
}