using UnityEngine;

public class TimedObjectDestroyer : MonoBehaviour
{
    public float lifetime ;

    private float timeAlive = 0.0f;

    public bool destroyChildrenOnDeath = true;

    private static bool quitting ;


    private void OnApplicationQuit()
    {
        quitting = true;
        DetachChildren();
        DestroyImmediate(this.gameObject);
    }

    void Update()
    {
    
        if (timeAlive > lifetime)
        {
            DetachChildren();
            Destructable.DoDestroy(this.gameObject);
        }
        else
        {
            timeAlive += Time.deltaTime;
        }
    }


    private void DetachChildren()
    {
        if (destroyChildrenOnDeath && !quitting && Application.isPlaying)
        {
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject childObject = transform.GetChild(i).gameObject;
                if (childObject != null)
                {
                    Destructable.DoDestroy(childObject);
                }
            }
        }
        transform.DetachChildren();
    }
}
