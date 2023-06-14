using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The purpose of this class is to turn around AI agents when they attempt to move through something they should not move through
/// It allows us to use simpler navmesh solutions while preventing the enemies from wandering through doors and the like
/// </summary>
public class TurnArounder : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Whether or not this script is running")]
    public bool on;
    [Tooltip("A transform location to travel towards")]
    public Transform targetLocationTransform;
    [Min(0)]
    [Tooltip("How far away from the target to vary from when assigning the target location of an AI agent")]
    public float locationVariance = 0;
    [Min(0)]
    [Tooltip("The minimum amount of time the AI should spend trying to get to the target")]
    public float minimumAmountOfTimeToTry = 1f;
    [Tooltip("The maximum amount of time the AI should spend trying to get to the target")]
    public float maximumAmountofTimetToTry = 5f;

    /// <summary>
    /// Description:
    /// If the Collider passed to this has a ground enemy script, this function will change that
    /// script's navmesh target to a different one based on the settings
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The collider that we might want to adjust the AI for</param>
    void AssignNewLocation(Collider collision)
    {
        GroundEnemy groundEnemy = null;
        if (collision.attachedRigidbody != null)
        {
            groundEnemy = collision.attachedRigidbody.GetComponent<GroundEnemy>();
        }
        if (on && groundEnemy != null)
        {
            Debug.Log("Turn around");
            Vector3 targetLocation = targetLocationTransform.position +
                new Vector3(Random.Range(-locationVariance, locationVariance), 0, Random.Range(-locationVariance, locationVariance));
            groundEnemy.GoToTaget(targetLocation, Random.Range(minimumAmountOfTimeToTry, maximumAmountofTimetToTry));
        }
    }

    /// <summary>
    /// Description:
    /// Built-in Unity function that is called whenever a trigger collider is entered by another collider
    /// Input:
    /// Collider collision
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The collider that entered the trigger</param>
    private void OnTriggerEnter(Collider collision)
    {
        AssignNewLocation(collision);
    }

    /// <summary>
    /// Description:
    /// Built-in Unity function that is called every frame a trigger collider stays inside another collider
    /// Input:
    /// Collider collision
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The collider that is still in the trigger</param>
    private void OnTriggerStay(Collider collision)
    {
        AssignNewLocation(collision);
    }

    /// <summary>
    /// Description:
    /// Turns the script off
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void TurnOff()
    {
        on = false;
    }

    /// <summary>
    /// Description:
    /// Turns the script on
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void TurnOn()
    {
        on = true;
    }
}
