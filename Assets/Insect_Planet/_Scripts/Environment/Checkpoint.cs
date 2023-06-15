using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class which manages a checkpoint
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The location this checkpoint will respawn the player at")]
    public Transform respawnLocation;
    [Tooltip("The animator for this checkpoint")]
    public Animator checkpointAnimator = null;
    [Tooltip("The name of the parameter in the animator which determines if this checkpoint displays as active")]
    public string animatorActiveParameter = "isActive";
    [Tooltip("The effect to create when activating the checkpoint")]
    public GameObject checkpointActivationEffect;

    /// <summary>
    /// Description:
    /// Standard unity function called when a trigger is entered by another collider
    /// Input:
    /// Collider collision
    /// Returns:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The collider that has entered the trigger</param>
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player" && collision.gameObject.GetComponent<Health>() != null)
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            playerHealth.SetRespawnPoint(respawnLocation.position);

            // Reset the last checkpoint if it exists
            if (CheckpointTracker.currentCheckpoint != null)
            {
                if (CheckpointTracker.currentCheckpoint.checkpointAnimator != null)
                {
                    CheckpointTracker.currentCheckpoint.checkpointAnimator.SetBool(animatorActiveParameter, false);
                }
            }

            if (CheckpointTracker.currentCheckpoint != this && checkpointActivationEffect != null)
            {
                Instantiate(checkpointActivationEffect, transform.position, Quaternion.identity, null);
            }

            // Set current checkpoint to this and set up its animation
            CheckpointTracker.currentCheckpoint = this;
            if (checkpointAnimator != null)
            {
                checkpointAnimator.SetBool(animatorActiveParameter, true);
            }
        }
    }
}
