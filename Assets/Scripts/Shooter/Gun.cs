using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles the setup of a gun
/// </summary>
public class Gun : MonoBehaviour
{
    [Header("Aim Settings")]
    [Tooltip("The gameobject to raycast forward from")]
    public GameObject raycastFrom;
    [Tooltip("The minimum distance away this gun will look towards when adjusting aim")]
    public float minimumDistanceToAim = 10f;
    [Tooltip("The maximum distance away this gun will look towards when adjusting aim")]
    public float maxDistanceToAim = 1000f;
    [Header("Enemy Aiming")]
    [Tooltip("Whether or not this gun always shoots towards an enemy target")]
    public bool alwaysShootAtTarget = true;
    [Tooltip("The enemy whos target will be used")]
    public Enemy enemy;

    [Header("Prefab Settings")]
    [Tooltip("The projectile game object to instantiate when firing this gun")]
    public GameObject projectileGameObject;
    [Tooltip("Whether or not the fired projectile should be a child of the fire location")]
    public bool childProjectileToFireLocation = false;
    [Tooltip("The effect prefab to instantiate when firing this gun")]
    public GameObject fireEffect;

    [Header("Fire settings")]
    [Tooltip("The transform whos location this fires from")]
    public Transform fireLocationTransform;
    [Tooltip("How long to wait before being able to fire again, if no animator is set")]
    public float fireDelay = 0.02f;
    [Tooltip("The fire type of the weapon")]
    public FireType fireType = FireType.semiAutomatic;
    
    // enum for setting the fire type
    public enum FireType { semiAutomatic, automatic };

    // The time when this gun will be able to fire again
    private float ableToFireAgainTime = 0;

    [Tooltip("The number of projectiles to fire when firing")]
    public int maximumToFire = 1;
    [Tooltip("The maximum degree (eular angle) of spread shots can be fired in")]
    [Range(0, 45)]
    public float maximumSpreadDegree = 0;

    [Header("Equipping settings")]
    [Tooltip("Whether or not this gun is available for use")]
    public bool available = false;

    [Header("Animation Settings")]
    [Tooltip("The animator that animates this gun.")]
    public Animator gunAnimator = null;
    [Tooltip("Shoot animator trigger name")]
    public string shootTriggerName = "Shoot";
    [Tooltip("The animation state anme when the gun is idle (used to handle when we are able to fire again)")]
    public string idleAnimationName = "Idle";

    [Header("Ammo Settings")]
    [Tooltip("Whether this gun requires ammunition.")]
    public bool useAmmo = false;
    [Tooltip("The ID of ammo that can be used with this gun.")]
    public int ammunitionID = 0;
    [Tooltip("Whether this gun must be reloaded.")]
    public bool mustReload = false;
    [Tooltip("The number of shots that can be fired without reloading. \n" +
        "A magazine size of 1 means the player must reload after every shot.")]
    public int magazineSize = 1;
    [Tooltip("The number of shots currently loaded into this gun")]
    public int roundsLoaded = 0;
    [Tooltip("The time it takes to reload")]
    public float reloadTime = 1.0f;

    [Header("UI Display Settings")]
    [Tooltip("The weapon Image to display on the UI")]
    public Sprite weaponImage;
    [Tooltip("The ammo image to display on the UI")]
    public Sprite ammoImage;

    /// <summary>
    /// Description:
    /// Standard Unity function called before Update
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void Start()
    {
        Setup();
    }

    /// <summary>
    /// Description:
    /// Setup this script if things are not set
    /// Input:
    /// None
    /// Return:
    /// void (no return)
    /// </summary>
    private void Setup()
    {
        if (raycastFrom == null)
        {
            raycastFrom = gameObject;
            Debug.LogWarning("The gun script on: " + name + " does not have a raycast from set. \n" +
                "This can cause aiming to be inaccurate.");
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Input:
    /// None
    /// Return:
    /// void (no return)
    /// </summary>
    private void Update()
    {
        AdjustAim();
    }

    /// <summary>
    /// Description:
    /// Adjusts the guns aim to make it look at the spot the player's raycast is hitting
    /// Input:
    /// None
    /// Return:
    /// void (no return)
    /// </summary>
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

    /// <summary>
    /// Description:
    /// Fires the gun, creating both the projectile and fire effect
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
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
            canFire = ableToFireAgainTime <= Time.time;
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

            ableToFireAgainTime = Time.time + fireDelay;
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

    /// <summary>
    /// Description:
    /// Returns whether this gun has the ammunition it needs to be able to fire.
    /// Inputs: N/A
    /// Outputs: bool
    /// </summary>
    /// <returns>Whether this gun has ammo it needs to fire.</returns>
    public bool HasAmmo()
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

    /// <summary>
    /// Description:
    /// Coroutine that reloads this gun
    /// Inputs: N/A
    /// Outputs: IEnumerator
    /// </summary>
    /// <returns>Coroutine</returns>
    private IEnumerator Reload()
    {
        ableToFireAgainTime = Time.time + reloadTime;
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

    /// <summary>
    /// Description:
    /// Tries to play a shoot animation on the gun
    /// Input:
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    public void PlayShootAnimation()
    {
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger(shootTriggerName);
        }
    }
}
