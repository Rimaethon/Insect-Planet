using System;
using System.Collections.Generic;
using Rimaethon._Scripts.Utility;
using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private List<GameObject> panels;
        private Vector3 panelClosePosition;
        private RectTransform panelRectTransform;
        
        

        void PauseGame()
        {
        }
        public void HandlePanel(GameObject panel)
        {
            if (panel.activeSelf)
            {
                ClosePanelVertically(panel);
            }
            else
            {
                OpenPanelVertically(panel);
            }
        }

        private void ClosePanelVertically(GameObject panel)
        {
            panelRectTransform=panel.GetComponent<RectTransform>();
           
            LeanTween.moveY(panelRectTransform, -1300, 0.5f).setOnComplete(() => panel.SetActive(false));
            

        }

        private void OpenPanelVertically(GameObject panel)
        {
            panel.SetActive(true);
            panelRectTransform=panel.GetComponent<RectTransform>();
            LeanTween.moveY(panelRectTransform, 0, 0.5f);
        }
        public void LoadScene(int sceneID)
        { 
            EventManager.Instance.Broadcast<int>(GameEvents.OnSceneChange,sceneID);
        }
    }
}
