using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PortalSpeedController : MonoBehaviour
{
    [SerializeField] float portalStartTime; 
    [SerializeField] float accelerationEndTime;
    [SerializeField] float decelerationEndTime; 
    [SerializeField] float maxRotationSpeed;
    [SerializeField] float minRotationSpeed;
    
    [SerializeField] private float portalBlockTime;

    private GameObject childObject; 
    
    private float timer;
    private float currentSpeed;
    private float rotationSpeed;
    
    private void Start()
    {
        childObject = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (portalStartTime < timer)
        {
            gameObject.SetActive(true);
            
            if (  timer<accelerationEndTime)
            {
                currentSpeed = Mathf.Lerp(0f, maxRotationSpeed, timer /accelerationEndTime);
            }
            else if (timer < decelerationEndTime)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, minRotationSpeed, timer /decelerationEndTime);
            }
            else
            {
                currentSpeed = rotationSpeed;
            }

            gameObject.transform.Rotate(0, 0, currentSpeed * Time.deltaTime);
        
            if (timer>decelerationEndTime+1)
            {
                childObject.SetActive(true);

            }

            if (timer>decelerationEndTime+portalBlockTime)
            {
                childObject.SetActive(false);
            }

        }


    }
    
    
}