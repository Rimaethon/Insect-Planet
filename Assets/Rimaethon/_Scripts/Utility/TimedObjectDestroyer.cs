using UnityEngine;

/// <summary>
///     A class which destroys it's gameobject after a certain amount of time
/// </summary>
public class TimedObjectDestroyer : MonoBehaviour
{
    // Flag which tells whether the application is shutting down (helps avoid errors)
    public static bool quitting;

    [Header("Settings")] [Tooltip("The lifetime of this gameobject")]
    public float lifetime = 5.0f;

    [Tooltip("Whether to destroy child gameobjects when this gameobject is destroyed")]
    public bool destroyChildrenOnDeath = true;

    // The amount of time this gameobject has already existed in play mode
    private float timeAlive;

    /// <summary>
    ///     Description:
    ///     Standard Unity function called once per frame
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void Update()
    {
        // Every frame, increment the amount of time that this gameobject has been alive,
        // or if it has exceeded it's maximum lifetime, destroy it
        if (timeAlive > lifetime)
        {
            DetachChildren();
            Destructable.DoDestroy(gameObject);
        }
        else
        {
            timeAlive += Time.deltaTime;
        }
    }

    /// <summary>
    ///     Description:
    ///     Standard Unity function called when the game is being quit or the play mode is exited
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void OnApplicationQuit()
    {
        // Ensures that the quitting flag gets set correctly to avoid work as the application quits
        quitting = true;
        DetachChildren();
        DestroyImmediate(gameObject);
    }

    /// <summary>
    ///     Description:
    ///     Detaches children if the setting was set to true
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void DetachChildren()
    {
        if (destroyChildrenOnDeath && !quitting && Application.isPlaying)
        {
            var childCount = transform.childCount;
            for (var i = childCount - 1; i >= 0; i--)
            {
                var childObject = transform.GetChild(i).gameObject;
                if (childObject != null) Destructable.DoDestroy(childObject);
            }
        }

        transform.DetachChildren();
    }
}