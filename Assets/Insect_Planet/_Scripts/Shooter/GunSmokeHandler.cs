using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class which handles gun smoke realism
/// </summary>
public class GunSmokeHandler : MonoBehaviour
{
    // Delegate that is invoked when a gun fires
    public delegate void OnGunFireDelegate(Gun gun);
    // static instance of the delegate to be invoked when a gun fires
    public static OnGunFireDelegate OnGunFire = delegate { };

    [Tooltip("The gun that this script manages smoke for")]
    public Gun gun = null;
    [Tooltip("The particle system that creates the smoke effect")]
    public ParticleSystem gunSmokeParticles = null;
    [Tooltip("The curve which defines how many particles should be emitted for each given heat value")]
    public AnimationCurve SmokeVSHeat = new AnimationCurve();
    // The current heat of the gun.
    private float gunHeat = 0;
    [Tooltip("The amount to increase the gun's heat by when fired.")]
    public float heatIncrementOnFire = 0.15f;
    [Tooltip("The rate at which 'heat' leaves the gun and smoke fades.")]
    public float heatDissipationRate = 0.1f;

    /// <summary>
    /// Description:
    /// When enabled, this gun subscribes to the OnGunFire delegate to be notified when it's gun fires
    /// Inputs: N/A
    /// Outupts: N/A
    /// </summary>
    private void OnEnable()
    {
        OnGunFire += OnFire;
    }

    /// <summary>
    /// Description:
    /// When disabled, this gun unsubscribes to the OnGunFire delegate to no longer be notified when it's gun fires
    /// Inputs: N/A
    /// Outupts: N/A
    /// </summary>
    private void OnDisable()
    {
        OnGunFire -= OnFire;
    }

    /// <summary>
    /// Description:
    /// every update, decrease the gun heat and update the amount of smoke emitted.
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    private void Update()
    {
        gunHeat = Mathf.Max(gunHeat - heatDissipationRate, 0);
        SetSmokeAmount();
    }

    /// <summary>
    /// Description:
    /// Function to be called when a gun is fired, used to increment smoke amount
    /// Inputs: Gun firedGun
    /// Outputs: N/A
    /// </summary>
    /// <param name="firedGun">The gun that was fired, to be compared with this script's gun.</param>
    public void OnFire(Gun firedGun)
    {
        if (firedGun == gun)
        {
            gunHeat += heatIncrementOnFire;
        }
    }

    /// <summary>
    /// Description:
    /// Sets the amount of smoke released by this gun's gun smoke particle system.
    /// Inputs: N/A
    /// Outputs: N/A
    /// </summary>
    private void SetSmokeAmount()
    {
        if (gunSmokeParticles != null)
        {
            int expectedParticles = (int)SmokeVSHeat.Evaluate(gunHeat);
            ParticleSystem.EmissionModule emmission = gunSmokeParticles.emission;
            ParticleSystem.MinMaxCurve rateCurve = new ParticleSystem.MinMaxCurve();
            rateCurve.constant = expectedParticles;
            emmission.rateOverTime = rateCurve;
        }
    }
}
