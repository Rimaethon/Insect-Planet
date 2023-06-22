using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using Insect_Planet._Scripts.ObjectManagers;
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    private float spawnInterval = 5f;
    private float scaleSpeed = 0.5f;
    private float launchForce = 10f;

    private float timer;

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
        GameObject spawnedObject = ObjectPool.Spawn( objectToSpawn,gameObject.transform,gameObject.transform.localPosition, transform.rotation);
        StartCoroutine(ScaleUpObject(spawnedObject));

        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 launchDirection = transform.forward; // Change the launch direction as desired
            rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);
        }
    }

    private IEnumerator ScaleUpObject(GameObject obj)
    {
        Vector3 initialScale = obj.transform.localScale;
        Vector3 targetScale = initialScale * 2f; // Change the scale factor as desired

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * scaleSpeed;
            obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }
    }
}