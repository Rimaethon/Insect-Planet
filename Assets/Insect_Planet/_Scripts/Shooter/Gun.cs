using System.Collections;
using UnityEngine;

namespace Insect_Planet.Scripts.Shooter
{
    public class Gun : MonoBehaviour
    {
        [SerializeField] private GameObject raycastFrom;
        [SerializeField] private float minimumDistanceToAim = 10f;
        [SerializeField] private float maxDistanceToAim = 1000f;
        [SerializeField] private bool alwaysShootAtTarget = true;
        [SerializeField] private Enemy enemy;

        [SerializeField] private GameObject projectileGameObject;
        [SerializeField] private bool childProjectileToFireLocation;
        [SerializeField] private GameObject fireEffect;

        [SerializeField] private Transform fireLocationTransform;
        [SerializeField] private float fireDelay = 0.02f;
        public FireType fireType = FireType.SemiAutomatic;
    
        public enum FireType { SemiAutomatic, Automatic };

        private float _ableToFireAgainTime;
    
        [SerializeField] private int maximumToFire = 1;
        [Range(0, 45)]
        [SerializeField] private float maximumSpreadDegree ;

        public bool available;

        [SerializeField] private Animator gunAnimator = null;
        [SerializeField] private string shootTriggerName = "Shoot";
        [SerializeField] private string idleAnimationName = "Idle";

        public bool useAmmo = false;
        public int ammunitionID = 0;
        [SerializeField] private bool mustReload = false;
        public int magazineSize = 1;
        public int roundsLoaded = 0;
        [SerializeField] private float reloadTime = 1.0f;

        public Sprite weaponImage;
        public Sprite ammoImage;

        private void Start()
        {
            Setup();
        }

    
        private void Setup()
        {
            if (raycastFrom == null)
            {
                raycastFrom = gameObject;
                Debug.LogWarning("The gun script on: " + name + " does not have a raycast from set. \n" +
                                 "This can cause aiming to be inaccurate.");
            }
        }

 
        private void Update()
        {
            //AdjustAim();
        }

   
        public void AdjustAim()
        {
            // Special aiming for enemies
            if (alwaysShootAtTarget && enemy != null)
            {
                fireLocationTransform.LookAt(enemy.target);
                return;
            }

            RaycastHit hitInformation;
            Vector3 aimAtPosition = raycastFrom.transform.position + raycastFrom.transform.forward * maxDistanceToAim;
            bool hitSomething = Physics.Raycast(raycastFrom.transform.position, raycastFrom.transform.forward, out hitInformation);
            if (!hitSomething || hitInformation.distance > maxDistanceToAim || hitInformation.transform.tag == "Projectile")
            {
                fireLocationTransform.LookAt(aimAtPosition);
            }
            else if (hitInformation.distance < minimumDistanceToAim)
            {
                aimAtPosition = raycastFrom.transform.position + raycastFrom.transform.forward * minimumDistanceToAim;
                fireLocationTransform.LookAt(aimAtPosition);
            }
            else
            {
                aimAtPosition = raycastFrom.transform.position + raycastFrom.transform.forward * hitInformation.distance;
                fireLocationTransform.LookAt(aimAtPosition);
            }
        }

 
        public void Fire()
        {
            bool canFire = false;

            // use the animator for fire delay if possible
            // otherwise use the timing set in the inspector
            if (gunAnimator != null)
            {
                canFire = gunAnimator.GetCurrentAnimatorStateInfo(0).IsName(idleAnimationName);
            }
            else
            {
                canFire = _ableToFireAgainTime <= Time.time;
            }

            if (canFire && HasAmmo())
            {
                if (projectileGameObject != null)
                {
                    for (int i = 0; i < maximumToFire; i++)
                    {
                        float fireDegreeX = Random.Range(-maximumSpreadDegree, maximumSpreadDegree);
                        float fireDegreeY = Random.Range(-maximumSpreadDegree, maximumSpreadDegree);
                        Vector3 fireRotationInEular = fireLocationTransform.rotation.eulerAngles + new Vector3(fireDegreeX, fireDegreeY, 0);
                        GameObject projectile = Instantiate(projectileGameObject, fireLocationTransform.position, 
                            Quaternion.Euler(fireRotationInEular), null);
                        if (childProjectileToFireLocation)
                        {
                            projectile.transform.SetParent(fireLocationTransform);
                        }
                    }
                }

                if (fireEffect != null)
                {
                    Instantiate(fireEffect, fireLocationTransform.position, fireLocationTransform.rotation, fireLocationTransform);
                }

                _ableToFireAgainTime = Time.time + fireDelay;
                PlayShootAnimation();

                GunSmokeHandler.OnGunFire(this);

                if (useAmmo)
                {
                    AmmoTracker.OnFire(this);
                    roundsLoaded = Mathf.Clamp(roundsLoaded - 1, 0, magazineSize);
                }
            }
            else if (useAmmo && mustReload && roundsLoaded == 0)
            {
                StartCoroutine(Reload());
            }
            GameManager.UpdateUIElements();
        }


        private bool HasAmmo()
        {
            if (useAmmo)
            {
                if (mustReload)
                {
                    return roundsLoaded > 0;
                }
                else
                {
                    return AmmoTracker.HasAmmo(this);
                }
            }
            else
            {
                return true;
            }
        }

        private IEnumerator Reload()
        {
            _ableToFireAgainTime = Time.time + reloadTime;
            if (AmmoTracker.HasAmmo(this))
            {
                float t = 0;
                while (t < reloadTime)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
                AmmoTracker.Reload(this);
            }
        }


        private void PlayShootAnimation()
        {
            if (gunAnimator != null)
            {
                gunAnimator.SetTrigger(shootTriggerName);
            }
        }
    }
}
