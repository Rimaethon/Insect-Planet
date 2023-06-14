using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyAttacker subclass which self-detonates upon activation (this makes an enemy a suicide bomber)
/// </summary>
public class EnemyAttackerExplosive : EnemyAttacker
{
    [Header("ExplosionSettings")]
    [Tooltip("Whether or not this enemy dies when it explodes.")]
    public bool dieOnExplosion = true;
    [Tooltip("The gameobject which creates the explosion effect.")]
    public GameObject explostionEffect = null;

    /// <summary>
    /// Description:
    /// Causes this enemy to charge up, then explode
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Coroutine that performs the attack</returns>
    protected override IEnumerator PerformAttack()
    {
        OnAttackStart();
        float t = 0;
        while (t < attackDuration)
        {
            yield return null;
            t += Time.deltaTime;
        }
        SpawnExplosion();
        OnAttackEnd();
    }

    /// <summary>
    /// Description:
    /// Spawns the explosion effect on this enemy, then dies if dieOnExplosion is set to true
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void SpawnExplosion()
    {
        if (explostionEffect != null)
        {
            GameObject obj = Instantiate(explostionEffect, transform.position, transform.rotation, null);
        }
        TryDie();
    }

    /// <summary>
    /// Description:
    /// Dies if dieOnExplosion is set to true
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void TryDie()
    {
        if (dieOnExplosion)
        {
            Destructable.DoDestroy(this.gameObject);
        }
    }
}
