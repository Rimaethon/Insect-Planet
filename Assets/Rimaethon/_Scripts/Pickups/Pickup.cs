using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This is the base class for other pick up scripts to inherit from
/// </summary>
public class Pickup : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The effect to create when this pickup is collected")]
    public GameObject pickUpEffect;

    /// <summary>
    /// Description:
    /// Standard unity function called when a trigger is entered by another collider
    /// Input:
    /// Collider collision
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The collider that has entered the trigger</param>
    private void OnTriggerEnter(Collider collision)
    {
        DoOnPickup(collision);
    }

    /// <summary>
    /// Description:
    /// Function called when this pickup is picked up
    /// Input: 
    /// Collider collision
    /// Return: 
    /// void (no return)
    /// </summary>
    /// <param name="collision">The collider that is picking up this pickup</param>
    public virtual void DoOnPickup(Collider collision)
    {
        if (collision.tag == "Player")
        {
            if (pickUpEffect != null)
            {
                Instantiate(pickUpEffect, transform.position, Quaternion.identity, null);
            }
            Destructable.DoDestroy(this.gameObject);
        }
    }
}
