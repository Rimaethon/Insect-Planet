using System.Collections.Generic;
using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    public class UIPanel : MonoBehaviour
    {
        private List<UIPage> _subsetPages=new List<UIPage>();
        [SerializeField] private Transform relatedUIPages;
        private UIPage _currentPage;
        
        
        public void ClosePanel()
        {
            foreach (var page in _subsetPages)
            {
                page.ClosePage();
            }
        }
        
 
            
        
        private void Awake()
        {
            FindChildPages();
        }
    
        private void FindChildPages()
        {
            foreach (Transform child in relatedUIPages)
            {
                if (child.GetComponent<UIPage>())
                {
                    _subsetPages.Add(child.GetComponent<UIPage>());
                }
            }
        }
    }
}
