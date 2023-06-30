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
            UpdateGameState(GameEvents.OnBeforeGameStart);
        }

     


        public void UpdateGameState(GameEvents newEvent)
        {
            @event = newEvent;
            switch (newEvent)
            {
                case GameEvents.OnBeforeGameStart:
                    Debug.Log("I called OnBeforeGameStart");
                    
                    break;
                case GameEvents.OnGameStart:
                    EventManager.Instance.Broadcast(GameEvents.OnGameStart);
                    Debug.Log("I called OnGameStart");
                    break;
                case GameEvents.OnObjectsInstantiated:
                    break;
                case GameEvents.OnCharacterLevelChange:
                    // Do something for OnCharacterLevelChange state
                    break;
                case GameEvents.OnCollectingBrick:
                    
                    break;
                case GameEvents.OnPuttingBrick:
                    // Do something for OnPuttingBrick state
                    break;
                case GameEvents.OnClimbingStair:
                    // Do something for OnClimbingStair state
                    break;
                case GameEvents.OnOpeningDoor:
                    // Do something for OnOpeningDoor state
                    break;
                case GameEvents.OnUpdateUI:
                    // Do something for OnUpdateUI state
                    break;
                case GameEvents.OnCharacterDeath:
                    // Do something for OnCharacterDeath state
                    break;
                case GameEvents.OnLosing:
                    // Do something for OnLosing state
                    break;
                case GameEvents.OnWinning:
                    // Do something for OnWinning state
                    break;
                default:
                    break;
            }
        }

}
