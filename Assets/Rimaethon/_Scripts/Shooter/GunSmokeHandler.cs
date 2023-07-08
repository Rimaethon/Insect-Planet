﻿using UnityEngine;

/// <summary>
///     Class which handles gun smoke realism
/// </summary>
public class GunSmokeHandler : MonoBehaviour
{
    // Delegate that is invoked when a gun fires
    public delegate void OnGunFireDelegate(Gun gun);

    // static instance of the delegate to be invoked when a gun fires
    public static OnGunFireDelegate OnGunFire = delegate { };

    [Tooltip("The gun that this script manages smoke for")]
    public Gun gun;

    [Tooltip("The particle system that creates the smoke effect")]
    public ParticleSystem gunSmokeParticles;

    [Tooltip("The curve which defines how many particles should be emitted for each given heat value")]
    public AnimationCurve SmokeVSHeat = new();

    [Tooltip("The amount to increase the gun's heat by when fired.")]
    public float heatIncrementOnFire = 0.15f;

    [Tooltip("The rate at which 'heat' leaves the gun and smoke fades.")]
    public float heatDissipationRate = 0.1f;

    // The current heat of the gun.
    private float gunHeat;

    /// <summary>
    ///     Description:
    ///     every update, decrease the gun heat and update the amount of smoke emitted.
    ///     Inputs: N/A
    ///     Outputs: N/A
    /// </summary>
    private void Update()
    {
        gunHeat = Mathf.Max(gunHeat - heatDissipationRate, 0);
        SetSmokeAmount();
    }

    /// <summary>
    ///     Description:
    ///     When enabled, this gun subscribes to the OnGunFire delegate to be notified when it's gun fires
    ///     Inputs: N/A
    ///     Outupts: N/A
    /// </summary>
    private void OnEnable()
    {
        OnGunFire += OnFire;
    }

    /// <summary>
    ///     Description:
    ///     When disabled, this gun unsubscribes to the OnGunFire delegate to no longer be notified when it's gun fires
    ///     Inputs: N/A
    ///     Outupts: N/A
    /// </summary>
    private void OnDisable()
    {
        OnGunFire -= OnFire;
    }

    /// <summary>
    ///     Description:
    ///     Function to be called when a gun is fired, used to increment smoke amount
    ///     Inputs: Gun firedGun
    ///     Outputs: N/A
    /// </summary>
    /// <param name="firedGun">The gun that was fired, to be compared with this script's gun.</param>
    public void OnFire(Gun firedGun)
    {
        if (firedGun == gun) gunHeat += heatIncrementOnFire;
    }

    /// <summary>
    ///     Description:
    ///     Sets the amount of smoke released by this gun's gun smoke particle system.
    ///     Inputs: N/A
    ///     Outputs: N/A
    /// </summary>
    private void SetSmokeAmount()
    {
        if (gunSmokeParticles != null)
        {
            var expectedParticles = (int)SmokeVSHeat.Evaluate(gunHeat);
            var emmission = gunSmokeParticles.emission;
            var rateCurve = new ParticleSystem.MinMaxCurve();
            rateCurve.constant = expectedParticles;
            emmission.rateOverTime = rateCurve;
        }
    }
}