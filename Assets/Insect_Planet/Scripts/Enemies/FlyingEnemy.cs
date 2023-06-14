using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class which represents enemies that fly
/// </summary>
public class FlyingEnemy : Enemy
{
    /// <summary>
    /// Enum to help with different mmovement types/modes
    /// </summary>
    public enum BehaviorAtStopDistance
    {
        Stop,
        CircleClockwise,
        CircleAnticlockwise
    }

    [Header("Flying Enemy Settings")]
    [Tooltip("The distance at which point the stop behavior is engaged")]
    public float stopDistance = 5.0f;
    [Tooltip("The way that the enemy moves once it is within the desired range.")]
    public BehaviorAtStopDistance stopBehavior = BehaviorAtStopDistance.CircleClockwise;

    /// <summary>
    /// Description:
    /// Calculates the desired movement based on the target's position
    /// Input: 
    /// none
    /// Return: 
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The desired movement of this enemy</returns>
    protected override Vector3 CalculateDesiredMovement()
    {
        if (target != null)
        {
            if ((target.position - transform.position).magnitude > stopDistance)
            {
                return transform.position + (target.position - transform.position).normalized * moveSpeed * Time.deltaTime;
            }
            else
            {
                switch (stopBehavior)
                {
                    case BehaviorAtStopDistance.Stop:
                        break;
                    case BehaviorAtStopDistance.CircleClockwise:
                        return transform.position + Vector3.Cross((target.position - transform.position), transform.up).normalized * moveSpeed * Time.deltaTime;
                    case BehaviorAtStopDistance.CircleAnticlockwise:
                        return transform.position - Vector3.Cross((target.position - transform.position), transform.up).normalized * moveSpeed * Time.deltaTime;
                }
            }
        }
        return base.CalculateDesiredMovement();
    }

    /// <summary>
    /// Description:
    /// Calculates the rotation that this enemy should have while flying
    /// Input: 
    /// none
    /// Return: 
    /// Quaternion
    /// </summary>
    /// <returns>The desired rotation of this enemy</returns>
    protected override Quaternion CalculateDesiredRotation()
    {
        if (target != null)
        {
            return Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        }
        return base.CalculateDesiredRotation();
    }
}
