using System.Collections;
using UnityEngine;

public class MoveToDestination : MonoBehaviour
{
    public Transform targetObject;
    [SerializeField] private ParticleSystem flame;
    private float acceleration = 1f;
    private readonly float accelerationTime = 3f;
    private float currentSpeed = 50;
    private bool isLoweringStarted;
    private readonly int LoweringStepCount = 30;
    private float moveSpeed = 500f;
    private float particleSystemRadius1 = 1f;
    private float particleSystemRadius2 = 20f;
    private float particleSystemRadiusTransitionTime = 0.5f;
    private float shapeRadius;

    private Vector3 targetPosition;
    private float timer;

    private void Start()
    {
        targetPosition = targetObject.position;
    }

    private void Update()
    {
        var direction = targetPosition - transform.position;
        var distance = direction.magnitude;

        if (distance > 0.01f)
        {
            if (timer < accelerationTime) timer += Time.deltaTime;
            //Debug.Log("timer is "+ timer+"acceleration time is"+accelerationTime);

            var movement = direction.normalized * (currentSpeed * Time.deltaTime);
            transform.position += movement;

            if (timer > accelerationTime) StartCoroutine(AdjustFloatWithTime());
        }
    }

    private IEnumerator AdjustFloatWithTime()
    {
        float loweringSubject = 1;

        var shapeModule = flame.shape;
        for (var i = 0; i < LoweringStepCount; i++)
        {
            loweringSubject += 1;
            shapeModule.radius = loweringSubject;
            currentSpeed += 10;
            yield return new WaitForSeconds(0.01f);
        }

        currentSpeed = 400;
        for (var i = 0; i < LoweringStepCount; i++)
        {
            loweringSubject -= 1;
            shapeModule.radius = loweringSubject;
            yield return new WaitForSeconds(0.01f);
        }
    }
}