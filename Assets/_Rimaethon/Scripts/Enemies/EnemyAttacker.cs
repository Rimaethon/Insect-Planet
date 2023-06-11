using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class which handles enemy attacks
/// </summary>
public abstract class EnemyAttacker : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("The amount of time needed to complete an attack.")]
    public float attackDuration = 0.5f;
    [Tooltip("The minimum amount of time between attacks.")]
    public float cooldownDuration = 1.0f;
    [Header("Timing by animation clip")]
    [Tooltip("The attack animation clip to use for timing")]
    public AnimationClip attackClip;
    // Whether or not the enemy can attack
    private bool canAttack = true;

    /// <summary>
    /// Description:
    /// Standard unity function called once before the first update
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void Start()
    {
        SetDurationByClip();
    }

    void SetDurationByClip()
    {
        if (attackClip != null)
        {
            attackDuration = attackClip.length;
        }
    }

    /// <summary>
    /// Description:
    /// Determines whether an attack is allowed currently
    /// Made virtual for inheritting classes to override
    /// Input: 
    /// none
    /// Return: 
    /// bool
    /// </summary>
    /// <returns>bool: Whether or not an attack is possible currently</returns>
    protected virtual bool AttackPossible()
    {
        return canAttack;
    }

    /// <summary>
    /// Description:
    /// Function to be called by other scripts to start an attack if possible
    /// Input: 
    /// none
    /// Return: 
    /// bool
    /// </summary>
    /// <returns>bool: Whether or not the attack was launched</returns>
    public bool Attack()
    {
        if (AttackPossible())
        {
            StartCoroutine("PerformAttack");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Description:
    /// Coroutine which actually performs an attack.
    /// Input: 
    /// none
    /// Ouptuts: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Coroutine that performs the attack</returns>
    protected virtual IEnumerator PerformAttack()
    {
        OnAttackStart();
        yield return null;
        Debug.Log("Attack Made");
        OnAttackEnd();
    }

    /// <summary>
    /// Description:
    /// Coroutine which handles the cooldown on this enemy's attacks
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Coroutine that handles the attack cooldown</returns>
    protected IEnumerator Cooldown()
    {
        float t = 0;
        while (t < cooldownDuration)
        {
            yield return null;
            t += Time.deltaTime;
        }
        canAttack = true;
    }

    /// <summary>
    /// Description:
    /// Called when an attack is started, and prevents multiple attacks at once
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    protected void OnAttackStart()
    {
        canAttack = false;
    }

    /// <summary>
    /// Description:
    /// Called when an attack finishes, starts the cooldown
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    protected void OnAttackEnd()
    {
        StartCoroutine("Cooldown");
    }
}
