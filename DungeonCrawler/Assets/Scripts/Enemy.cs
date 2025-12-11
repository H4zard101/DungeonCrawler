using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 5;
    public int currentHealth = 0;



    [Header("Objective")]
    [Tooltip("If true, this enemy is counted for the 'Defeat all the enemies' objective.")]
    public bool countsForObjective = true;   // turn OFF on boss

    [Tooltip("If true, this enemy is the barracks boss.")]
    public bool isBoss = false;              // turn ON on boss

    private bool registered;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // Only some enemies should count towards the initial objective
        if (!countsForObjective)
            return;

        if (ObjectiveManager.Instance != null && !registered)
        {
            ObjectiveManager.Instance.RegisterEnemy();
            registered = true;
            Debug.Log($"REGISTER ENEMY → alive now: {ObjectiveManager.Instance.EnemiesAlive}");
        }
        else
        {
            Debug.Log($"{name} could NOT register (ObjectiveManager.Instance == null?)");
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage, HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        NotifyDeath();
        HandleBossDeath();

        Destroy(gameObject);
    }

    private void NotifyDeath()
    {
        // Only decrement once and only if this enemy actually counted
        if (!countsForObjective) return;
        if (!registered) return;

        registered = false;

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.EnemyDied();
            Debug.Log($"{name} died → alive now: {ObjectiveManager.Instance.EnemiesAlive}");
        }
    }

    private void HandleBossDeath()
    {
        if (!isBoss) return;
        if (ObjectiveManager.Instance == null) return;

        // Only advance if we are actually in the boss objective
        if (ObjectiveManager.Instance.CurrentStage == ObjectiveStage.KillBoss)
        {
            ObjectiveManager.Instance.SetStage(ObjectiveStage.ReturnToBlacksmith);
            Debug.Log("BOSS DEFEATED → ReturnToBlacksmith");
        }
    }
}