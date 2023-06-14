using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 3.0f;
    private Rigidbody rb;

    private bool hasHitTerrain = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        if (!hasHitTerrain)
        {
            transform.position += transform.forward * (projectileSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            hasHitTerrain = true;
            Debug.Log("Projectile hit the terrain!");

            // You can perform any desired actions here, such as destroying the projectile or spawning effects.
            //Destroy(gameObject);
        }
    }
}