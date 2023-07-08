using UnityEngine;
using UnityEngine.EventSystems;

namespace Rimaethon._Scripts.UI
{
    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float scaleUpAmount = 1.2f;
        private RectTransform _buttonRectTransform;
        private readonly float _scaleDuration = 0.2f;


        private void Awake()
        {
            _buttonRectTransform = gameObject.GetComponent<RectTransform>();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            LeanTween.scale(_buttonRectTransform, _buttonRectTransform.localScale * scaleUpAmount,
                    _scaleDuration)
                .setEase(LeanTweenType.easeInOutSine);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            LeanTween.scale(_buttonRectTransform, Vector3.one, _scaleDuration)
                .setEase(LeanTweenType.easeInOutSine);
        }
    }
}