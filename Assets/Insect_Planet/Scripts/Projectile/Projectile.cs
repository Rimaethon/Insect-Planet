using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float projectileSpeed = 3.0f;
    public float blastDistance = 1.0f;
    private Rigidbody rb;
    private Vector3 positionHolder;
    private bool hasHitTerrain = false;
    [SerializeField] private GameObject bulletBlast;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        if (!hasHitTerrain)
        {
            // Move the projectile forward
            transform.position += transform.forward * (projectileSpeed * Time.deltaTime);

            // Check the distance to the target
            if (Vector3.Distance(transform.position, bulletBlast.transform.position) <= blastDistance)
            {
                // Instantiate bullet blast and destroy the projectile
                Instantiate(bulletBlast, bulletBlast.transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }

  
}