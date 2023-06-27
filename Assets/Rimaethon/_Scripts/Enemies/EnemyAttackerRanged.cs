using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemyAttacker subclass for ranged enemies
/// </summary>
public class EnemyAttackerRanged : EnemyAttacker
{
    [Tooltip("The shooter component that this enemy will use to shoot")]
    public Shooter shooter = null;

    /// <summary>
    /// Description:
    /// Returns whether an attack can be made currently
    /// Input: 
    /// none
    /// Return: 
    /// bool
    /// </summary>
    /// <returns></returns>
    protected override bool AttackPossible()
    {
        return base.AttackPossible() && shooter != null;
    }

    /// <summary>
    /// Description:
    /// Coroutine which fires a gun
    /// Input: 
    /// none
    /// Return: 
    /// IEnumerator
    /// </summary>
    /// <returns>IEnumerator: Coroutine that performs the attaack</returns>
    protected override IEnumerator PerformAttack()
    {
        OnAttackStart();
        shooter.FireEquippedGun();
        float t = 0;
        while (t < attackDuration)
        {
            yield return null;
            t += Time.deltaTime;
        }
        OnAttackEnd();
    }
}
