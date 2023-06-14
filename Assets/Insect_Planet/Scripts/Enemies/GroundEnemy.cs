using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class which derives from the Enemy base class. Handles ground enemy movement
/// </summary>
public class GroundEnemy : Enemy
{
    [Header("Ground Enemy Settings")]
    [Tooltip("The nav mesh agent used to move this enemy")]
    public NavMeshAgent agent = null;
    [Tooltip("The distance at which to stop before reaching the objective")]
    public float stopDistance = 2.0f;
    [Tooltip("Whether this enemy can stop if it is within it's stop distance but does not have line of sight to it's target")]
    public bool lineOfSightToStop = true;
    [Tooltip("Whether this enemy should always face the player, or face in the direction it is moving")]
    public bool alwaysFacePlayer = true;

    /// <summary>
    /// Description:
    /// Overrides base function setup, sets up nav mesh agent reference
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    protected override void Setup()
    {
        base.Setup();
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        if (agent != null)
        {
            agent.updateRotation = !alwaysFacePlayer;
        }
    }

    /// <summary>
    /// Description:
    /// Makes the enemy nav agent go to a specific location and will continue to try to reach that location until
    /// the specified time has passed
    /// Input:
    /// Vector3 target, float timeToSpend
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="target">The target to travel to</param>
    /// <param name="timeToSpend">The time to spend trying to get to that target</param>
    public void GoToTaget(Vector3 target, float timeToSpend)
    {
        agent.SetDestination(target);
        travelingToSpecificTarget = true;
        timeToStopTrying = Time.time + timeToSpend;
    }

    bool travelingToSpecificTarget = false;
    float timeToStopTrying = 0;

    /// <summary>
    /// Description:
    /// Handles movement for this enemy
    /// Rotates the enemy to face it's target and sets navmesh destination
    /// Inputs:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    protected override void HandleMovement()
    {
        if (enemyRigidbody != null)
        {
            enemyRigidbody.velocity = Vector3.zero;
            enemyRigidbody.angularVelocity = Vector3.zero;
        }

        Quaternion desiredRotation = CalculateDesiredRotation();
        transform.rotation = desiredRotation;

        if (travelingToSpecificTarget)
        {
            if (Time.time >= timeToStopTrying || NavMeshAgentDestinationReached())
            {
                travelingToSpecificTarget = false;
                agent.SetDestination(target.position);
            }
        }
        else if (ShouldMove())
        {
            agent.SetDestination(target.position);
        }
        else if (agent != null)
        {
            agent.SetDestination(transform.position);
        }
    }

    /// <summary>
    /// Description:
    /// Checks to see if the nav mesh agent has reached its destination or not
    /// Input:
    /// none
    /// Returns:
    /// bool
    /// </summary>
    /// <returns>bool: Whether or not the agent has reached its destination</returns>
    bool NavMeshAgentDestinationReached()
    {
        // Check if we've reached the destination
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Description:
    /// Small function which determines whether or not this enemy should move
    /// Input: 
    /// none
    /// Return: 
    /// bool
    /// </summary>
    /// <returns>bool: Whether or not this enemy should move</returns>
    bool ShouldMove()
    {
        // If move while attack is set to true, we can move while attacking. Otherwise we just need to check isAttacking for
        // whether or not we move
        bool attackMove = moveWhileAttacking == true || isAttacking == false;

        bool hasLineOfSight = true;

        if (needsLineOfSightToMove)
        {
            hasLineOfSight = HasLineOfSight();
        }

        if (agent != null && target != null && canMove && attackMove && hasLineOfSight)
        {
            if ((target.position - transform.position).magnitude > stopDistance)
            {
                isMoving = true;
                return true;
            }
            else if (lineOfSightToStop && !HasLineOfSight())
            {
                isMoving = true;
                return true;
            }
        }

        if (isAttacking)
        {
            isMoving = false;
            isIdle = false;
        }
        else
        {
            isIdle = true;
        }
        return false;
    }

    /// <summary>
    /// Description:
    /// Calculates the movement that this enemy will make this frame.
    /// Input: 
    /// none
    /// Return:
    /// Vector3
    /// </summary>
    /// <returns>Vector3: The desired movement of this enemy</returns>
    protected override Vector3 CalculateDesiredMovement()
    {
        if (agent != null)
        {
            return agent.desiredVelocity * Time.deltaTime;
        }
        return base.CalculateDesiredMovement();
    }

    /// <summary>
    /// Description:
    /// Caclulates the desired rotation of this enemy this frame
    /// Input: 
    /// none
    /// Return: 
    /// Quaternion
    /// </summary>
    /// <returns>Quaternion: The desired rotation of the enemy</returns>
    protected override Quaternion CalculateDesiredRotation()
    {
        if (target != null)
        {
            if (alwaysFacePlayer)
            {
                Quaternion result = Quaternion.LookRotation(target.position - transform.position, transform.up);
                result = Quaternion.Euler(0, result.eulerAngles.y, 0);
                return result;
            }
        }
        return base.CalculateDesiredRotation();
    }
}
