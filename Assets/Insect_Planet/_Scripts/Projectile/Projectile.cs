using UnityEngine;

namespace Insect_Planet._Scripts.Projectile
{
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
           
                positionHolder = gameObject.transform.position;
                positionHolder.z += blastDistance;
                transform.rotation = Quaternion.identity;
                Instantiate(bulletBlast, positionHolder, gameObject.transform.rotation);
                Debug.Log("Bullet has hit terrain");
                Destroy(gameObject);
            
            }
        }
    }
}