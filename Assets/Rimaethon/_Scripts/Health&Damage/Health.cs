using UnityEngine;
using UnityEngine.Events;


public class Health : MonoBehaviour
{
    public int teamId;
    public int defaultHealth = 1;
    public int maximumHealth = 1;
    public int currentHealth = 1;
    public float invincibilityTime = 3f;
    public bool isAlwaysInvincible;
    public bool useLives;
    public int currentLives = 3;
    public int maximumLives = 5;
    public float respawnWaitTime = 3f;
    public GameObject deathEffect;
    public GameObject hitEffect;
    public RagdollHandler ragdollHandler;
    public UnityEvent eventsOnDeath;
    public UnityEvent eventsOnRespawn;

    private bool isInvincableFromDamage;
    private Vector3 respawnPosition;
    private float respawnTime;
    private float timeToBecomeDamagableAgain;

    private void Start()
    {
        SetRespawnPoint(transform.position);
    }

    private void Update()
    {
        InvincibilityCheck();
        RespawnCheck();
    }

    private void RespawnCheck()
    {
        if (respawnWaitTime != 0 && currentHealth <= 0 && currentLives > 0)
            if (Time.time >= respawnTime)
                Respawn();
    }

    private void InvincibilityCheck()
    {
        if (timeToBecomeDamagableAgain <= Time.time) isInvincableFromDamage = false;
    }

    public void SetRespawnPoint(Vector3 newRespawnPosition)
    {
        respawnPosition = newRespawnPosition;
    }

    private void Respawn()
    {
        if (GetComponent<CharacterController>() != null)
        {
            GetComponent<CharacterController>().enabled = false;
            transform.position = respawnPosition;
            GetComponent<CharacterController>().enabled = true;
        }

        if (gameObject.GetComponent<PlayerController>() != null &&
            gameObject.GetComponent<PlayerController>().playerWeaponController != null)
        {
            var playerShooter = gameObject.GetComponent<PlayerController>().playerWeaponController;
            foreach (var gun in playerShooter.guns)
            {
                var rotation = gun.transform.localRotation.eulerAngles;
                gun.transform.localRotation = Quaternion.Euler(new Vector3(0, rotation.y, rotation.z));
            }
        }

        // Do on respawn events
        if (eventsOnRespawn != null) eventsOnRespawn.Invoke();

        currentHealth = defaultHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincableFromDamage || currentHealth <= 0 || isAlwaysInvincible) return;

        if (hitEffect != null) Instantiate(hitEffect, transform.position, transform.rotation, null);
        timeToBecomeDamagableAgain = Time.time + invincibilityTime;
        isInvincableFromDamage = true;
        currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0, maximumHealth);
        CheckDeath();
    }

    public void ReceiveHealing(int healingAmount)
    {
        currentHealth += healingAmount;
        if (currentHealth > maximumHealth) currentHealth = maximumHealth;
        CheckDeath();
    }

    public void AddLives(int bonusLives)
    {
        if (useLives)
        {
            currentLives += bonusLives;
            if (currentLives > maximumLives) currentLives = maximumLives;
        }
    }

    private bool CheckDeath()
    {
        if (currentHealth <= 0)
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
        }

        // Do on death events
        if (eventsOnDeath != null) eventsOnDeath.Invoke();

        if (useLives)
        {
            currentLives -= 1;
            if (currentLives > 0)
            {
                if (respawnWaitTime == 0)
                    Respawn();
                else
                    respawnTime = Time.time + respawnWaitTime;
            }
            else
            {
                if (respawnWaitTime != 0)
                    respawnTime = Time.time + respawnWaitTime;
                else if (ragdollHandler != null)
                    ragdollHandler.EnableRagdoll();
                else
                    Destructable.DoDestroy(gameObject);
                GameOver();
            }
        }
        else
        {
            GameOver();
            if (ragdollHandler != null)
                ragdollHandler.EnableRagdoll();
            else
                Destructable.DoDestroy(gameObject);
        }
    }

    public void GameOver()
    {
    }
}
