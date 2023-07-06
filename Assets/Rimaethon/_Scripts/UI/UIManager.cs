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

        public void ClosePanelVertically(GameObject panel)
        {
            panelRectTransform=panel.GetComponent<RectTransform>();
            var position = panelRectTransform.anchoredPosition;
            panelClosePosition =new Vector2(position.x,position.y-1000f);
            LeanTween.move(panelRectTransform,panelClosePosition,0.5f).setOnComplete(()=>{panel.SetActive(false);});
        }

        public void OpenPanelVertically(GameObject panel)
        {
            panel.SetActive(true);
            panelRectTransform=panel.GetComponent<RectTransform>();
            var position = panelRectTransform.anchoredPosition;
            panelClosePosition =new Vector2(position.x,position.y+1000f);
            LeanTween.move(panelRectTransform,position,0.5f);
        }
        public void LoadScene(int sceneID)
        { 
            EventManager.Instance.Broadcast<int>(GameEvents.OnSceneChange,sceneID);
        }
    }
}
