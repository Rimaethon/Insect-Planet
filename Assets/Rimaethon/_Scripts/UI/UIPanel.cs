using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    public class UIPanel : UIPage
    {
        [SerializeField] private Transform relatedUIPages;
        [SerializeField] private List<UIPage> _subsetPages = new List<UIPage>();
        private UIPage _currentPage;

        private void OnEnable()
        {
            relatedUIPages.gameObject.SetActive(true);
        }
        private void OnDisable()
        {
            relatedUIPages.gameObject.SetActive(false);
        }
        public void HandlePageOpening(int index)
        {
            if (index < 0 || index >= _subsetPages.Count)
            {
                Debug.LogError("Index out of range");
                return;
            }

            if (_currentPage != null)
            {
                if(_currentPage._isAnimating|| _currentPage==_subsetPages[index])
                    return;
                _currentPage.gameObject.SetActive(true);
                _currentPage.ClosePage();
            }
            _currentPage = _subsetPages[index];
            _currentPage.gameObject.SetActive(true);
            _currentPage.OpenPage();
        }



    }
}
