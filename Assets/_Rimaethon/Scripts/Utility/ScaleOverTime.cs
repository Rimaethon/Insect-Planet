using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class which scales the attached game object over time
/// </summary>
public class ScaleOverTime : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The curve along which this object will be scaled")]
    public AnimationCurve scaleCurve = new AnimationCurve();
    [Tooltip("How fast this object goes through the scale curve.")]
    public float scaleSpeed = 1.0f;
    [Tooltip("Whether the scale curve should be looped")]
    public bool looping = true;
    // The base scale of this game object
    private Vector3 baseScale = Vector3.one;
    // The time that the animation started
    private float startTime = 0;

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update
    /// Input: 
    /// none
    /// Return:
    /// void (no return)
    /// </summary>
    private void Start()
    {
        // When this gameobject starts up, sets this component up
        startTime = Time.timeSinceLevelLoad;
        baseScale = transform.localScale;
        Scale();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once every frame
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    private void Update()
    {
        // Every Update, try to scale this gameobject accordingly
        Scale();
    }

    /// <summary>
    /// Description:
    /// Scales this game object according to how long it has been active in the scene
    /// Input: 
    /// none
    /// Return: 
    /// void (no return)
    /// </summary>
    public void Scale()
    {
        float curveTime = GetCurveTime();
        transform.localScale = baseScale * scaleCurve.Evaluate(curveTime);
    }

    /// <summary>
    /// Description:
    /// Determines the point at which to evaluate the scale curve
    /// Input: 
    /// none
    /// Return: 
    /// float
    /// </summary>
    /// <returns>float: The time at which to evaluate the scale curve.</returns>
    public float GetCurveTime()
    {
        float curveTime = (Time.timeSinceLevelLoad - startTime) * scaleSpeed;
        if (looping)
        {
            curveTime = curveTime % scaleSpeed;
        }
        curveTime = Mathf.Clamp(curveTime, 0, 1);
        return curveTime;
    }
}
