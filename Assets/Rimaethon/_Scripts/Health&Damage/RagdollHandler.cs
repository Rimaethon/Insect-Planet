using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class that handles transforming an object into a ragdoll
/// </summary>
public class RagdollHandler : MonoBehaviour
{
    [Tooltip("The animator that contolls the parts to be made into a ragdoll")]
    public Animator ragdollAnimator = null;
    [Tooltip("The list of components to disable when this script is activated")]
    public List<Component> componentsToDisable = new List<Component>();
    [Tooltip("A timed object destroyer which will be turned on when made into a ragdoll")]
    public TimedObjectDestroyer timedDestroyer = null;

    /// <summary>
    /// Description:
    /// destroys components, adds rigidbodies to every collider, and turns on a timed object destroyer
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    public void EnableRagdoll()
    {
        Vector3 position = transform.position;
        foreach (Component c in componentsToDisable)
        {
            Destroy(c);
        }
        if (ragdollAnimator != null)
        {
            ragdollAnimator.enabled = false;
            List<Collider> colliders = ragdollAnimator.GetComponentsInChildren<Collider>().ToList();
            foreach (Collider c in colliders)
            {
                Component test = null;
                if (c.enabled && !c.gameObject.TryGetComponent(typeof(Rigidbody), out test))
                {
                    Rigidbody r = c.gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;
                    r.MovePosition(position);
                    r.freezeRotation = false;
                    r.useGravity = true;
                }
                else if (c.enabled && test != null)
                {
                    Rigidbody r = c.gameObject.GetComponent<Rigidbody>();
                    r.freezeRotation = false;
                    r.useGravity = true;
                }
            }
        }
        if (timedDestroyer != null)
        {
            timedDestroyer.enabled = true;
        }
        StartCoroutine(MoveToPositionNextFrame(position));
    }

    /// <summary>
    /// Description:
    /// Moves the object to a position after one frame elapses
    /// Inputs: Vector3 position
    /// Outputs: IEnumerator
    /// </summary>
    /// <param name="position">The position to move the object to.</param>
    /// <returns>Coroutine</returns>
    public IEnumerator MoveToPositionNextFrame(Vector3 position)
    {
        yield return null;
        transform.position = position;
    }
}
