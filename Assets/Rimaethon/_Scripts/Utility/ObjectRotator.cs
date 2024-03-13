using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    public float rotationSpeed = 5.0f;

    void Update()
    {
        // Rotate the object around its up axis (Y-axis)
        transform.Rotate(Vector3.up, rotationSpeed );
    }
}