using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Team Settings")] [Tooltip("The team associated with this damage")]
    public int teamId;


    [Header("Enemy Health Settings")] [SerializeField]
    private float defaultEnemyHealth = 1f;

    [SerializeField] private float _currentHealth;


    public GameObject deathEffect;

    public GameObject hitEffect;

    public RagdollHandler ragdollHandler;


    private void Start()
    {
        _currentHealth = defaultEnemyHealth;
    }


    private void Update()
    {
    }


    public void TakeDamage(int damageAmount)
    {
        if (_currentHealth <= 0) return;

        if (hitEffect != null) Instantiate(hitEffect, transform.position, transform.rotation, null);

        _currentHealth = Mathf.Clamp(_currentHealth - damageAmount, 0, defaultEnemyHealth);
        CheckDeath();
    }


    private bool CheckDeath()
    {
        if (_currentHealth <= 0)
        {
            Die();
            return true;
        }

        return false;
    }

    private void Die()
    {
        if (deathEffect != null)
        {
            if (ragdollHandler != null) ragdollHandler.EnableRagdoll();

            if (deathEffect != null) Instantiate(deathEffect, transform.position, transform.rotation, null);

            // Do on death events
        }
    }
}