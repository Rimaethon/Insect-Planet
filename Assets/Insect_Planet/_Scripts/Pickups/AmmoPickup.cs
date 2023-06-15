using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pickup-derived class which grants ammo to the player that picks it up.
/// </summary>
public class AmmoPickup : Pickup
{
    [Tooltip("The ID of the ammunition to be added.")]
    public int ammunitionID = 0;
    [Tooltip("The amount of ammunition to add.")]
    public int amount = 1;

    /// <summary>
    /// Description:
    /// When picked up, add ammunition to the ammo tracker
    /// Inputs: Collider collision
    /// Outputs: N/A
    /// </summary>
    /// <param name="collision">The collider that is attempting to pick up this pickup</param>
    public override void DoOnPickup(Collider collision)
    {
        if (collision.tag == "Player")
        {
            AmmoTracker.AddAmmunition(ammunitionID, amount);
        }
        base.DoOnPickup(collision);
    }
}
