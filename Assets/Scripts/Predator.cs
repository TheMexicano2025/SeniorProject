using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script controls enemy AI behavior
// predators hunt for cows and attack the player if damaged
public class Predator : MonoBehaviour
{
    [Header("AI Behavior")]
    public float detectionRange = 20f;
    public float moveSpeed = 3.5f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;

    [Header("Target Priority")]
    public bool preferCows = true; // will hunt cows before attacking player
    public float aggroDuration = 10f; // how long to chase player after being hit

    [Header("Loot")]
    public ItemSO lootItem;
    public int lootAmount = 1;

    [Header("References")]
    public GameObject lootPrefab;

    private Transform currentTarget;
    private Transform aggroTarget; // player who damaged us
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

    // when damaged switch to attacking the player
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
            }
        }
    }

    // find the best target to attack
    private void FindTarget()
    {
        // if we're angry at the player attack them first
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

        // look for cows first if we prefer them
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

        // if no cows nearby go for the player
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

    // move toward the current target
    private void MoveTowardsTarget()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;

        if (rb != null)
        {
            Vector3 targetVelocity = direction * moveSpeed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        // face the target
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // attack the target if cooldown is ready
    private void AttackTarget()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        // stop moving while attacking
        if (rb != null)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }

        Health targetHealth = currentTarget.GetComponent<Health>();
        if (targetHealth != null)
        {
            Vector3 attackDirection = (currentTarget.position - transform.position).normalized;
            targetHealth.TakeDamage(attackDamage, attackDirection);
            lastAttackTime = Time.time;
        }
    }

    private void OnDeath()
    {
        isDead = true;
        DropLoot();
        Destroy(gameObject, 0.5f);
    }

    // drop loot when killed
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
        }
    }

    // draw detection and attack ranges in editor
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
