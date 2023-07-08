using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    public class UIPage : UIElement
    { 
        bool _isOpen;
        
        public void ClosePage()
        {
            if (!_isOpen)
            {
                return;
            }
            StartCoroutine(UIAnimationHelperMethods.SlideOut(gameObject.GetComponent<RectTransform>(), UIDirection.UP, 0.5f,
               null)) ;
            Debug.Log("Closing Page");
            _isOpen = false;
        }
        public void OpenPage()
        {
            if (_isOpen)
            {
                return;
            }
            StartCoroutine(UIAnimationHelperMethods.SlideIn(gameObject.GetComponent<RectTransform>(), UIDirection.UP, 0.5f,
                null));
            Debug.Log("Opening Page");
            _isOpen= true;
        }
    }
}
