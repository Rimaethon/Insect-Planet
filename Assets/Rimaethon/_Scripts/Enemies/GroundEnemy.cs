using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class GroundEnemy : Enemy
{
    public NavMeshAgent agent;
    public float stopDistance = 2.0f;
    public bool lineOfSightToStop = true;
    public bool alwaysFacePlayer = true;

   
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

   
    public void GoToTaget(Vector3 target, float timeToSpend)
    {
        agent.SetDestination(target);
        travelingToSpecificTarget = true;
        timeToStopTrying = Time.time + timeToSpend;
    }

    bool travelingToSpecificTarget = false;
    float timeToStopTrying = 0;


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

  
    bool ShouldMove()
    {
      
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


    protected override Vector3 CalculateDesiredMovement()
    {
        if (agent != null)
        {
            return agent.desiredVelocity * Time.deltaTime;
        }
        return base.CalculateDesiredMovement();
    }

 
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
