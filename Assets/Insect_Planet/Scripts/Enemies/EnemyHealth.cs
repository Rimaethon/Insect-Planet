using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Team Settings")] [Tooltip("The team associated with this damage")]
    public int teamId = 0;

    private EnemyPooler _enemyPooler;

    [Header("Enemy Health Settings")] [SerializeField]
    private float defaultEnemyHealth = 1f;

     [SerializeField] private float _currentHealth;


    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update call
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Start()
    {
        _currentHealth = defaultEnemyHealth;
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once very frame
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Update()
    {
        
    }



    /// <summary>
    /// Description:
    /// Applies damage to the health unless the health is invincible.
    /// Input:
    /// int damageAmount
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take</param>
    public void TakeDamage(int damageAmount)
    {
        if (_currentHealth <= 0)
        {
            return;
        }
        else
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation, null);
            }

            _currentHealth = Mathf.Clamp(_currentHealth - damageAmount, 0, defaultEnemyHealth);
            GameManager.UpdateUIElements();
            CheckDeath();
        }
    }

    /// <summary>
    /// Description:
    /// Applies healing to the health, capped out at the maximum health.
    /// Input:
    /// int healingAmount
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="healingAmount">How much healing to apply</param>


    /// <summary>
    /// Description:
    /// Gives the health script more lives if the health is using lives
    /// Input:
    /// int bonusLives
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="bonusLives">The number of lives to add</param>

    [Header("Effects & Polish")] [Tooltip("The effect to create when this health dies")]
    public GameObject deathEffect;

    [Tooltip("The effect to create when this health is damaged (but does not die)")]
    public GameObject hitEffect;

    [Tooltip("The component which turns on enemy physics")]
    public RagdollHandler ragdollHandler = null;

    [Tooltip("A list of events that occur when the health becomes 0 or lower")]


    bool CheckDeath()
    {
        if (_currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Description:
    /// Handles the death of the health. If a death effect is set, it is created. If lives are being used, the health is respawned.
    /// If lives are not being used or the lives are 0 then the health's game object is destroyed.
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    void Die()
    {
        if (deathEffect != null)
        {
            if (ragdollHandler != null)
            {
                ragdollHandler.EnableRagdoll();
            }

            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, transform.rotation, null);
            }

            // Do on death events
        }

    }
}
