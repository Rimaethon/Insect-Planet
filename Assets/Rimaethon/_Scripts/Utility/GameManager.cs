using Rimaethon._Scripts.Utility;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }


    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }
}