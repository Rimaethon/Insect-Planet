using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Team Settings")] [Tooltip("The team associated with this damage")]
    public int teamId = 0;


    [Header("Enemy Health Settings")] [SerializeField]
    private float defaultEnemyHealth = 1f;

     [SerializeField] private float _currentHealth;


   
    void Start()
    {
        _currentHealth = defaultEnemyHealth;
    }

   
    void Update()
    {
        
    }



   
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
            CheckDeath();
        }
    }

 

    public GameObject deathEffect;

    public GameObject hitEffect;

    public RagdollHandler ragdollHandler = null;


    bool CheckDeath()
    {
        if (_currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

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
