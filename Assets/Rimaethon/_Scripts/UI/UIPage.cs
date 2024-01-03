using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    public class UIPage : MonoBehaviour
    {
        public bool _isOpen;
        public bool _isAnimating;
        public void ClosePage()
        {
            if (!_isOpen) return;

            StartCoroutine(UIAnimationHelperMethods.SlideOut(gameObject.GetComponent<RectTransform>(), UIDirection.UP,
                2f));
            Debug.Log("Closing Page");
            _isOpen = false;
        }

        public void OpenPage()
        {
            if (_isOpen) return;
            StartCoroutine(UIAnimationHelperMethods.SlideIn(gameObject.GetComponent<RectTransform>(), UIDirection.UP,
                2f));
            Debug.Log("Opening Page");
            _isOpen = true;
        }
    }
}
