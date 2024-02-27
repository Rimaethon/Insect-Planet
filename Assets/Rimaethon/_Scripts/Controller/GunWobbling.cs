using System;
using System.Collections;
using System.Collections.Generic;
using Rimaethon._Scripts.Utility;
using Rimaethon.Scripts.Managers;
using UnityEngine;

public class GunWobbling : MonoBehaviour
{

    
    public FirstPersonController mover;

    [Header("Sway")]
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;
    Vector3 swayPos;

    [Header("Sway Rotation")]
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    Vector3 swayEulerRot; 

    public float smooth = 10f;
    float smoothRot = 12f;

    [Header("Bobbing")]
    public float speedCurve;
    float curveSin {get => Mathf.Sin(speedCurve);}
    float curveCos {get => Mathf.Cos(speedCurve);}

    public Vector3 travelLimit = Vector3.one * 0.025f;
    public Vector3 bobLimit = Vector3.one * 0.01f;
    Vector3 bobPosition;

    public float bobExaggeration;

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    Vector3 bobEulerRotation;
    private Vector2 _lookVector;
    private Vector2 _moveVector;
    // Update is called once per frame
    private void OnEnable()
    {
        EventManager.Instance.AddHandler<Vector2>(GameEvents.OnPlayerMove, movementVector => 
        {
            _moveVector = movementVector;
        });
        EventManager.Instance.AddHandler<Vector2>(GameEvents.OnPlayerLook, lookVector =>
        {
            _lookVector = lookVector;
        });
    }

    private void OnDisable()
    {
        if (EventManager.Instance == null) return;
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnPlayerMove,
            movementVector => { _moveVector = new Vector3(movementVector.x,movementVector.y); });
        EventManager.Instance.RemoveHandler<Vector2>(GameEvents.OnPlayerLook,
            lookVector => { _lookVector = new Vector3(lookVector.x,lookVector.y); });
    }

    void Update()
    {
        Sway();
        SwayRotation();
        BobOffset();
        BobRotation();

        CompositePositionRotation();
    }


  

    void Sway(){
        Vector3 invertLook = _lookVector *-step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);
        swayPos = invertLook;
    }

    void SwayRotation(){
        Vector2 invertLook = _lookVector * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
        swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    void CompositePositionRotation(){
        transform.localPosition = Vector3.Lerp(transform.localPosition, swayPos + bobPosition, Time.deltaTime * smooth);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation), Time.deltaTime * smoothRot);
    }

    void BobOffset(){
        speedCurve += Time.deltaTime * (mover._isGrounded ? (_moveVector.x+ _moveVector.y)*bobExaggeration : 1f) + 0.01f;

        bobPosition.x = (curveCos*bobLimit.x*(mover._isGrounded ? 1:0))-(_moveVector.x * travelLimit.x);
        bobPosition.y = (curveSin*bobLimit.y)-(_moveVector.y* travelLimit.y);
        bobPosition.z = -(_moveVector.y * travelLimit.z);
    }

    void BobRotation(){
        bobEulerRotation.x = (_moveVector != Vector2.zero ? multiplier.x * (Mathf.Sin(2*speedCurve)) : multiplier.x * (Mathf.Sin(2*speedCurve) / 2));
        bobEulerRotation.y = (_moveVector != Vector2.zero ? multiplier.y * curveCos : 0);
        bobEulerRotation.z = (_moveVector != Vector2.zero ? multiplier.z * curveCos * _moveVector.x : 0);
    }

}