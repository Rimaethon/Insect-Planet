using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlyingEnemy : Enemy
{
    public enum BehaviorAtStopDistance
    {
        Stop,
        CircleClockwise,
        CircleAnticlockwise
    }


    [SerializeField] private float stopDistance = 5.0f;
    public BehaviorAtStopDistance stopBehavior = BehaviorAtStopDistance.CircleClockwise;


    protected override Vector3 CalculateDesiredMovement()
    {
        if (target != null)
        {
            if ((target.position - transform.position).magnitude > stopDistance)
                return transform.position +
                       (target.position - transform.position).normalized * (moveSpeed * Time.deltaTime);
            else
                switch (stopBehavior)
                {
                    case BehaviorAtStopDistance.Stop:
                        break;
                    case BehaviorAtStopDistance.CircleClockwise:
                        var position = transform.position;
                        return position + Vector3.Cross(target.position - position, transform.up).normalized *
                            (moveSpeed * Time.deltaTime);
                    case BehaviorAtStopDistance.CircleAnticlockwise:
                        var transform1 = transform;
                        var position1 = transform1.position;
                        return position1 - Vector3.Cross(target.position - position1, transform1.up).normalized *
                            (moveSpeed * Time.deltaTime);
                }
        }

        return base.CalculateDesiredMovement();
    }


    protected override Quaternion CalculateDesiredRotation()
    {
        if (target != null) return Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        return base.CalculateDesiredRotation();
    }
}