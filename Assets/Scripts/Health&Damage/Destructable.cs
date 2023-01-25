using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A class meant to handle the destruction of a game object and handle the creation of death effects
/// </summary>
public class Destructable : MonoBehaviour
{
    [Tooltip("A prefab or gameobject to create in this game object's place before destruction.")]
    [SerializeField] private GameObject destroyEffect = null;
    [Tooltip("An event to call when this object is destroyed")]
    public UnityEvent onDestroyEvent = new UnityEvent();

    /// <summary>
    /// Description:
    /// Instantiates an effect prefab where this gameobject is, then destroys this gameobject.
    /// Inputs:
    /// N/A
    /// Outputs:
    /// N/A
    /// </summary>
    public void DestroyWithEffect()
    {
        if (destroyEffect != null)
        {
            GameObject obj = Instantiate(destroyEffect, transform.position, transform.rotation, null);
        }
        if (onDestroyEvent != null)
        {
            onDestroyEvent.Invoke();
        }
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Description:
    /// A static destruction function to be called in place of Destroy(gameobject) to enable
    /// effect creation when destroying an object.
    /// Inputs:
    /// N/A
    /// Outputs:
    /// N/A
    /// </summary>
    /// <param name="target"></param>
    public static void DoDestroy(GameObject target)
    {
        Destructable destructable = target.GetComponent<Destructable>();
        if (destructable != null)
        {
            destructable.DestroyWithEffect();
        }
        else
        {
            Destroy(target);
        }
    }
}
