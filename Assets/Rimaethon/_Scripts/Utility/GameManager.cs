using System.Collections;
using System.Collections.Generic;
using Rimaethon._Scripts.Utility;
using UnityEngine;

 public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public GameStates State;



        
        private void OnEnable()
        {
        }
        private void OnDisable()
        {
        }
        
        private void Awake()
        {
            instance = this;
            UpdateGameState(GameStates.OnBeforeGameStart);
        }

     


        public void UpdateGameState(GameStates newState)
        {
            State = newState;
            switch (newState)
            {
                case GameStates.OnBeforeGameStart:
                    Debug.Log("I called OnBeforeGameStart");
                    
                    break;
                case GameStates.OnGameStart:
                    EventManager.Instance.Broadcast(GameStates.OnGameStart);
                    Debug.Log("I called OnGameStart");
                    break;
                case GameStates.OnObjectsInstantiated:
                    break;
                case GameStates.OnCharacterLevelChange:
                    // Do something for OnCharacterLevelChange state
                    break;
                case GameStates.OnCollectingBrick:
                    
                    break;
                case GameStates.OnPuttingBrick:
                    // Do something for OnPuttingBrick state
                    break;
                case GameStates.OnClimbingStair:
                    // Do something for OnClimbingStair state
                    break;
                case GameStates.OnOpeningDoor:
                    // Do something for OnOpeningDoor state
                    break;
                case GameStates.OnUpdateUI:
                    // Do something for OnUpdateUI state
                    break;
                case GameStates.OnCharacterDeath:
                    // Do something for OnCharacterDeath state
                    break;
                case GameStates.OnLosing:
                    // Do something for OnLosing state
                    break;
                case GameStates.OnWinning:
                    // Do something for OnWinning state
                    break;
                default:
                    break;
            }
        }

}
