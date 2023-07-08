using System.Collections;
using Insect_Planet._Scripts.ObjectManagers;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    private readonly float launchForce = 10f;
    private readonly float scaleSpeed = 0.5f;
    private readonly float spawnInterval = 5f;

    private float timer;

    private void Start()
    {
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    private void SpawnObject()
    {
        var spawnedObject = ObjectPool.Spawn(objectToSpawn, gameObject.transform, gameObject.transform.localPosition,
            transform.rotation);
        StartCoroutine(ScaleUpObject(spawnedObject));


        var rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            var launchDirection = transform.forward; // Change the launch direction as desired
            rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);
        }
    }

    private IEnumerator ScaleUpObject(GameObject obj)
    {
        var initialScale = obj.transform.localScale;
        var targetScale = initialScale * 2f; // Change the scale factor as desired

        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * scaleSpeed;
            obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }
    }
}