using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : MonoBehaviour
{
    [Header("AI Behavior")]
    [Tooltip("How far the predator can detect targets")]
    public float detectionRange = 20f;
    
    [Tooltip("Movement speed")]
    public float moveSpeed = 3.5f;
    
    [Tooltip("How close before attacking")]
    public float attackRange = 2f;
    
    [Tooltip("Damage per attack")]
    public float attackDamage = 10f;
    
    [Tooltip("Time between attacks (seconds)")]
    public float attackCooldown = 2f;

    [Header("Target Priority")]
    [Tooltip("Prefer cows over player?")]
    public bool preferCows = true;
    
    [Tooltip("How long to focus on attacker after being hit (seconds)")]
    public float aggroDuration = 10f;

    [Header("Loot")]
    [Tooltip("Item to drop on death")]
    public ItemSO lootItem;
    
    [Tooltip("Amount to drop")]
    public int lootAmount = 1;

    [Header("References")]
    public GameObject lootPrefab;

    private Transform currentTarget;
    private Transform aggroTarget;
    private float aggroEndTime;
    private Health health;
    private Rigidbody rb;
    private float lastAttackTime;
    private bool isDead = false;

    private void Start()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody>();

        if (health != null)
        {
            health.onDeath.AddListener(OnDeath);
            health.onDamageTaken.AddListener(OnDamageTaken);
        }
    }

    private void Update()
    {
        if (isDead) return;

        FindTarget();

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            if (distance <= attackRange)
            {
                AttackTarget();
            }
            else if (distance <= detectionRange)
            {
                MoveTowardsTarget();
            }
        }
    }

    private void OnDamageTaken(float damage, float currentHealth)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            
            if (distanceToPlayer <= detectionRange * 1.5f)
            {
                aggroTarget = player.transform;
                aggroEndTime = Time.time + aggroDuration;
                Debug.Log($"{gameObject.name} is now aggro on the player!");
            }
        }
    }

    private void FindTarget()
    {
        if (aggroTarget != null && Time.time < aggroEndTime)
        {
            currentTarget = aggroTarget;
            return;
        }
        else if (Time.time >= aggroEndTime)
        {
            aggroTarget = null;
        }

        GameObject bestTarget = null;
        float closestDistance = detectionRange;

        if (preferCows)
        {
            GameObject[] cows = GameObject.FindGameObjectsWithTag("Cow");
            foreach (GameObject cow in cows)
            {
                float distance = Vector3.Distance(transform.position, cow.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = cow;
                }
            }
        }

        if (bestTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= detectionRange)
                {
                    bestTarget = player;
                }
            }
        }

        currentTarget = bestTarget != null ? bestTarget.transform : null;
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;

        if (rb != null)
        {
            Vector3 targetVelocity = direction * moveSpeed;
            targetVelocity.y = rb.velocity.y;
            rb.velocity = targetVelocity;
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void AttackTarget()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        Health targetHealth = currentTarget.GetComponent<Health>();
        if (targetHealth != null)
        {
            Vector3 attackDirection = (currentTarget.position - transform.position).normalized;
            targetHealth.TakeDamage(attackDamage, attackDirection);
            lastAttackTime = Time.time;

            Debug.Log($"{gameObject.name} attacked {currentTarget.name} for {attackDamage} damage!");
        }
    }

    private void OnDeath()
    {
        isDead = true;
        DropLoot();
        Destroy(gameObject, 0.5f);
    }

    private void DropLoot()
    {
        if (lootItem != null && lootPrefab != null)
        {
            GameObject droppedItem = Instantiate(lootPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            
            Item itemComponent = droppedItem.GetComponent<Item>();
            if (itemComponent != null)
            {
                itemComponent.itemData = lootItem;
                itemComponent.quantity = lootAmount;
            }

            Debug.Log($"{gameObject.name} dropped {lootAmount}x {lootItem.itemName}!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        if (aggroTarget != null && Time.time < aggroEndTime)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, aggroTarget.position);
        }
    }
}
