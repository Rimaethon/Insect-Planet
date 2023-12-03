using System;
using Unity.Mathematics;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform target;
    public Transform QuadTransform;
    private void Start()
    {
        target = Camera.main.transform;
        QuadTransform = transform;
    }

    void FixedUpdate()
    {
           Vector3 directionToTarget = transform.position- target.position ;
           transform.rotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
    }
}
