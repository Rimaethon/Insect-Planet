using UnityEngine;

/// <summary>
///     Class which spawns an effect gameobject when destroyed
/// </summary>
public class OnDestroyEffectCreator : MonoBehaviour
{
    // Flag which tells whether the application is shutting down (avoids errors)
    public static bool quitting;

    [Tooltip("The effect to create when destroyed.")]
    public GameObject destroyEffect;

    /// <summary>
    ///     Description:
    ///     Standard Unity function Called when this gameobject is destroyed
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void OnDestroy()
    {
        //if (Application.isPlaying && !quitting)
        //{
        //    //CreateDestroyEffect();
        //}
    }

    /// <summary>
    ///     Description:
    ///     Ensures that the quitting flag gets set correctly to avoid instantiating prefabs as the application quits
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void OnApplicationQuit()
    {
        quitting = true;
    }

    /// <summary>
    ///     Description:
    ///     Instantiates the destruction effect prefab at this gameobject's position
    ///     Input:
    ///     none
    ///     Return:
    ///     void (no return)
    /// </summary>
    private void CreateDestroyEffect()
    {
        if (destroyEffect != null)
        {
            var obj = Instantiate(destroyEffect, transform.position, transform.rotation, null);
        }
    }
}