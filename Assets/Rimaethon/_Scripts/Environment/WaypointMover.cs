using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This class handles moving the attached game object between waypoints
/// </summary>
public class WaypointMover : MonoBehaviour
{
    [Header("Settings")] [Tooltip("A list of transforms to move between")]
    public List<Transform> waypoints;

    [Tooltip("How fast to move the platform")]
    public float moveSpeed = 1f;

    [Tooltip("How long to wait when arriving at a waypoint")]
    public float waitTime = 3f;

    // Whether or not the waypoint mover is stopped
    [HideInInspector] public bool stopped;

    // The current direction being travelled in
    [HideInInspector] public Vector3 travelDirection;

    // The current waypoint being moved to
    private Vector3 currentTarget;

    // The index of the current Target in the waypoints list
    private int currentTargetIndex;

    // The previous waypoint or the starting position
    private Vector3 previousTarget;

    // The time at which movement is resumed
    private float timeToStartMovingAgain;

    /// <summary>
    ///     Description:
    ///     Standard Unity function called once before the first Update call
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void Start()
    {
        InitializeInformation();
    }

    /// <summary>
    ///     Description:
    ///     Standard Unity function called every fixed frame update
    ///     Input:
    ///     None
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void FixedUpdate()
    {
        ProcessMovementState();
    }

    /// <summary>
    ///     Description:
    ///     Processes current state and does movement accordingly
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void ProcessMovementState()
    {
        if (stopped)
            StartCheck();
        else
            Travel();
    }


    /// <summary>
    ///     Description:
    ///     Checks to see if the waypoint mover can start movement again
    ///     Input:
    ///     none:
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void StartCheck()
    {
        if (Time.time >= timeToStartMovingAgain)
        {
            stopped = false;
            previousTarget = currentTarget;
            currentTargetIndex += 1;
            if (currentTargetIndex >= waypoints.Count) currentTargetIndex = 0;
            currentTarget = waypoints[currentTargetIndex].position;
            CalculateTravelInformation();
        }
    }

    /// <summary>
    ///     Description:
    ///     Sets up the first previous target and current target
    ///     then calls CalculateTravelInformation to initilize travel direction
    ///     Inuput:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void InitializeInformation()
    {
        previousTarget = transform.position;
        currentTargetIndex = 0;
        if (waypoints.Count > 0)
        {
            currentTarget = waypoints[0].position;
        }
        else
        {
            waypoints.Add(transform);
            currentTarget = previousTarget;
        }

        CalculateTravelInformation();
    }

    /// <summary>
    ///     Description:
    ///     Calculates the current traveling direction using the previousTarget and the currentTarget
    ///     Inuput:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void CalculateTravelInformation()
    {
        travelDirection = (currentTarget - previousTarget).normalized;
    }

    /// <summary>
    ///     Description:
    ///     Translates the transform in the direction towards the next waypoint
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void Travel()
    {
        Debug.Log("travel called");
        transform.Translate(travelDirection * moveSpeed * Time.deltaTime);
        var overX = false;
        var overY = false;
        var overZ = false;

        var directionFromCurrentPositionToTarget = currentTarget - transform.position;

        if (directionFromCurrentPositionToTarget.x == 0 ||
            Mathf.Sign(directionFromCurrentPositionToTarget.x) != Mathf.Sign(travelDirection.x))
        {
            overX = true;
            transform.position = new Vector3(currentTarget.x, transform.position.y, transform.position.z);
        }

        if (directionFromCurrentPositionToTarget.y == 0 ||
            Mathf.Sign(directionFromCurrentPositionToTarget.y) != Mathf.Sign(travelDirection.y))
        {
            overY = true;
            transform.position = new Vector3(transform.position.x, currentTarget.y, transform.position.z);
        }

        if (directionFromCurrentPositionToTarget.z == 0 ||
            Mathf.Sign(directionFromCurrentPositionToTarget.z) != Mathf.Sign(travelDirection.z))
        {
            overZ = true;
            transform.position = new Vector3(transform.position.x, transform.position.y, currentTarget.z);
        }

        // If we are over the x, y, and z of our target we need to stop
        if (overX && overY && overZ) BeginWait();
    }

    /// <summary>
    ///     Description:
    ///     Starts the waiting, sets up the needed variables for waiting
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void BeginWait()
    {
        stopped = true;
        timeToStartMovingAgain = Time.time + waitTime;
    }
}