using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// controls health for all entities in the game
// it also handles taking damage, healing, dying, and visual feedback when hit
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Death Settings")]
    public bool destroyOnDeath = false; 
    public float destroyDelay = 0f; 
    
    [Header("Hit Feedback")]
    public bool enableKnockback = true; 
    public float knockbackForce = 5f; 
    public float flashDuration = 0.1f; 
    public Color flashColor = Color.red; 
    
    [Header("Events")]
    // these events let other scripts know when things happen to health
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
        // start with full health
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth);
        
        // get rigidbody for knockback
        rb = GetComponent<Rigidbody>();
        
        // find all the renderers so we can flash them when hit
        renderers = GetComponentsInChildren<Renderer>();
        
        if (renderers.Length > 0)
        {
            originalColors = new Color[renderers.Length];
            originalMaterials = new Material[renderers.Length];
            
            // remember the original colors so we can restore them later
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

    // take damage without knockback direction
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    // take damage with knockback in a direction
    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (isDead) return; // can't damage something that's already dead
        
        float previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // don't go below zero
        
        // tell other scripts that damage happened
        onDamageTaken?.Invoke(damage, currentHealth);
        onHealthChanged?.Invoke(currentHealth);
        
        // push the object back if knockback is enabled
        if (enableKnockback && hitDirection != Vector3.zero && rb != null)
        {
            Vector3 knockbackDir = hitDirection.normalized;
            knockbackDir.y = 0.3f; 
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }
        
        // flash red to show we got hit
        StartCoroutine(FlashEffect());
        
        // check if we should die
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    // restore health
    public void Heal(float amount)
    {
        if (isDead) return; // can't heal if dead
        
        float previousHealth = currentHealth;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // don't go over max health
        
        onHealed?.Invoke();
        onHealthChanged?.Invoke(currentHealth);
    }

    // handle death
    private void Die()
    {
        isDead = true;
        onDeath?.Invoke(); // let other scripts know we died
        
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    // flash the object red when it takes damage
    private System.Collections.IEnumerator FlashEffect()
    {
        if (renderers == null || renderers.Length == 0) yield break;
        
        // turn all renderers red
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                renderers[i].material.color = flashColor;
            }
        }
        
        yield return new UnityEngine.WaitForSeconds(flashDuration);
        
        // change back to original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null && i < originalColors.Length)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }

    // helper functions other scripts can use to check health status
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

    // manually set health to a specific value
    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    // reset health back to full 
    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        onHealthChanged?.Invoke(currentHealth);
    }
}
