using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to make projectiles move
/// </summary>
public class Projectile : MonoBehaviour
{
    // The speed of this projectile in units per second
    [Tooltip("The distance this projectile will move each second.")]
    public float projectileSpeed = 3.0f;
    
    /// <summary>
    /// Description:
    /// Every frame, move the projectile in the direction it is heading
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    protected virtual void Update()
    {
        transform.position = transform.position + transform.forward * projectileSpeed * Time.deltaTime;
    }
}
