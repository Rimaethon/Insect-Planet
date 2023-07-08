using System.Collections.Generic;
using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    public class UIPanel : UIPage
    {
        [SerializeField] private Transform relatedUIPages;
        private UIPage _currentPage;
        private readonly List<UIPage> _subsetPages = new();


        private void Awake()
        {
            FindChildPages();
        }


        public void ClosePanel()
        {
            foreach (var page in _subsetPages) page.ClosePage();
        }

        private void FindChildPages()
        {
            foreach (Transform child in relatedUIPages)
                if (child.GetComponent<UIPage>())
                {
                    _subsetPages.Add(child.GetComponent<UIPage>());
                    child.GetComponent<UIPage>().ClosePage();

                }
        }
    }
}