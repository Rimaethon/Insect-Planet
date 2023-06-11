using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pickup-derived class which adds a gun to a player's shooter component
/// </summary>
public class GunPickup : Pickup
{
    [Header("Gun Pickup Settings")]
    [Tooltip("The index of the gun to make available in the shooter script")]
    public int gunIndexToMakeAvailable = 0;

    /// <summary>
    /// Description:
    /// Adds a gun to the player's shooter component when picked up
    /// Input: 
    /// Collider collision
    /// Return: 
    /// void (no return)
    /// </summary>
    /// <param name="collision">The collider that is picking this up</param>
    public override void DoOnPickup(Collider collision)
    {
        Shooter shooter = collision.gameObject.GetComponentInChildren<Shooter>();
        if (collision.tag == "Player" && shooter != null)
        {
            shooter.MakeGunAvailable(gunIndexToMakeAvailable);
        }
        base.DoOnPickup(collision);
    }
}
