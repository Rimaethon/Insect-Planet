using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Team Settings")] [Tooltip("The team associated with this damage")]
    public int teamId = 0;

    [Header("Damage Settings")] [Tooltip("How much damage to deal")]
    public int damageAmount = 1;

    [Tooltip("Whether or not to destroy the attached game object after dealing damage")]
    public bool destroyAfterDamage = true;

    [Tooltip("Whether or not to apply damage when triggers collide")]
    public bool dealDamageOnTriggerEnter = false;

    [Tooltip("Whether or not to apply damage when triggers stay, for damage over time")]
    public bool dealDamageOnTriggerStay = false;

    [Tooltip("Whether or not to apply damage on non-trigger collider collisions")]
    public bool dealDamageOnCollision = false;

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
        if (dealDamageOnTriggerEnter) DealDamage(collision);
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
        if (dealDamageOnTriggerStay) DealDamage(collision);
    }

    /// <summary>
    /// Description:
    /// Built-in Unity function that is called whenever a non-trigger collider hits another non-trigger collider
    /// Input:
    /// Collision collision
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The collision that caused this function call</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (dealDamageOnCollision) DealDamage(collision);
    }

    /// <summary>
    /// Description:
    /// This function deals damage to a health component if the collided with gameobject has a health component attached AND it is on a different team.
    /// Input:
    /// Collision collision
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The Collision that has been collided with</param>
    private void DealDamage(Collision collision)
    {
        var collidedHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
        if (collidedHealth == null && collision.rigidbody != null)
            collidedHealth = collision.rigidbody.GetComponent<EnemyHealth>();
        if (collidedHealth != null)
            if (collidedHealth.teamId != teamId)
            {
                collidedHealth.TakeDamage(damageAmount);
                if (destroyAfterDamage) Destructable.DoDestroy(gameObject);
            }
    }

    /// <summary>
    /// Description:
    /// This function deals damage to a health component if the collided with gameobject has a health component attached AND it is on a different team.
    /// Input:
    /// Collider collision
    /// Return:
    /// void (no return)
    /// </summary>
    /// <param name="collision">The Collider that has been collided with</param>
    private void DealDamage(Collider collision)
    {
        var collidedHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
        if (collidedHealth == null && collision.attachedRigidbody != null)
            collidedHealth = collision.attachedRigidbody.GetComponent<EnemyHealth>();
        if (collidedHealth != null)
            if (collidedHealth.teamId != teamId)
            {
                collidedHealth.TakeDamage(damageAmount);
                if (destroyAfterDamage) Destructable.DoDestroy(gameObject);
            }
    }
}