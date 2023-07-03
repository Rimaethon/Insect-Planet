using System.Collections.Generic;
using Rimaethon._Scripts.Utility;
using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> panels;
        void PauseGame()
        {
        }

        void LoadScene(int sceneID)
        { 
            EventManager.Instance.Broadcast<int>(GameEvents.OnSceneChange,sceneID);
        }
    }
}
