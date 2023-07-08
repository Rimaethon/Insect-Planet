using UnityEngine;

public class TurretEnemy : Enemy
{
    public Transform rotationPivotX;
    public Transform rotationPivotY;
    public Transform rotationPivotZ;

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