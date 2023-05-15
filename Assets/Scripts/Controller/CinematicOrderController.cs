using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CinematicOrderController : MonoBehaviour
{
    private float rotationSpeed;
    public float accelerationTime; 
    public float maxRotationSpeed;
    public float minRotationSpeed;
    public float decelerationTime; 
    public GameObject childObject; 
    private bool isDecelerationEnded;
    [SerializeField] private float startDelay;
    private float startTimer;
    [SerializeField] private GameObject spaceship;
    [SerializeField] private GameObject portal;

    private float currentSpeed;
    private float accelerationTimer;
    private float decelerationTimer ; 

    void Update()
    {
        if (startDelay < startTimer)
        {
            portal.SetActive(true);
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

            portal.transform.Rotate(0, 0, currentSpeed * Time.deltaTime);
        
            if (currentSpeed < 0.0001)
            {
                isDecelerationEnded = true;
            }

            if (isDecelerationEnded && childObject != null)
            {
                childObject.SetActive(true);
                spaceship.SetActive(true);
            
            }
            
        }
        else
        {
            startTimer += Time.deltaTime;
        }
        
    }
    
    
}