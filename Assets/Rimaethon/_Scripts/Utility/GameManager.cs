using System.Collections;
using System.Collections.Generic;
using Rimaethon._Scripts.Utility;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [FormerlySerializedAs("State")] public GameEvents @event;


    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void Awake()
    {
        instance = this;
    }
}