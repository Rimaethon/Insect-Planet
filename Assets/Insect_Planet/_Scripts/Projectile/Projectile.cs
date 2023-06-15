using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float projectileSpeed = 3.0f;
    [SerializeField] private float blastDistance;
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
            rb.velocity = transform.forward * projectileSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Terrain"))
        {
            hasHitTerrain = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            positionHolder = transform.position;
            positionHolder.z += blastDistance;
            transform.rotation = Quaternion.identity;
            Instantiate(bulletBlast, positionHolder, Quaternion.identity);
            Debug.Log("Bullet has hit terrain");
            Destroy(gameObject);
            
        }
    }
}