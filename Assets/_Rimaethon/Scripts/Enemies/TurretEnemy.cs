using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : Enemy
{
    public Transform rotationPivotX = null;
    public Transform rotationPivotY = null;
    public Transform rotationPivotZ = null;

    protected override Vector3 CalculateDesiredMovement()
    {
        return base.CalculateDesiredMovement();
    }

    protected override Quaternion CalculateDesiredRotation()
    {
        return base.CalculateDesiredRotation();
    }

    protected override void HandleMovement()
    {
        base.HandleMovement();
    }
}
