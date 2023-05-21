using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CinematicOrderController : MonoBehaviour
{
    private float rotationSpeed;
    [SerializeField] float accelerationTime; 
    [SerializeField] float maxRotationSpeed;
    [SerializeField] float minRotationSpeed;
    [SerializeField] float decelerationTime; 
    private GameObject childObject; 
    private bool isDecelerationEnded;
    [SerializeField] private float startDelay;
    
    private float startTimer;
    private float currentSpeed;
    private float accelerationTimer;
    private float decelerationTimer ;

    private void Start()
    {
        childObject = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (startDelay < startTimer)
        {
            gameObject.SetActive(true);
            if (accelerationTimer < accelerationTime)
            {
                accelerationTimer += Time.deltaTime;
            }
            if (decelerationTimer < decelerationTime)
            {
                decelerationTimer += Time.deltaTime;
            }

            if (accelerationTimer < accelerationTime)
            {
                currentSpeed = Mathf.Lerp(0f, maxRotationSpeed, accelerationTimer / accelerationTime);
            }
            else if (decelerationTimer < decelerationTime)
            {
                currentSpeed = Mathf.Lerp(minRotationSpeed, rotationSpeed, decelerationTimer / decelerationTime);
            

            }
            else
            {
                currentSpeed = rotationSpeed;
            }

            gameObject.transform.Rotate(0, 0, currentSpeed * Time.deltaTime);
        
            if (currentSpeed < 0.0001)
            {
                isDecelerationEnded = true;
            }

            if (isDecelerationEnded && childObject != null)
            {
                childObject.SetActive(true);
            
            }
            
        }
        else
        {
            startTimer += Time.deltaTime;
        }
        
    }
    
    
}