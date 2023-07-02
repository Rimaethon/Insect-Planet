using System.Collections;
using Rimaethon._Scripts.Core.Enums;
using UnityEngine;

namespace Rimaethon._Scripts.UI
{
    public class UIAnimationController : MonoBehaviour, IPlayAnimation
    {
        [SerializeField] private UIAnimationTypes uiAnimationTypes;
        [SerializeField] private AnimationStates animationType;
        [SerializeField] private CanvasGroup canvasGroup;
        private RectTransform _rectTransform;
        [SerializeField] private float animationDuration = 1f;
        

        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Quaternion originalRotation;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            CloseAnimations();
        }

        private void Start()
        {
            originalPosition = _rectTransform.position;
            originalScale = _rectTransform.localScale;
            originalRotation = _rectTransform.rotation;
        }

        public void PlayAnimation()
        {
            switch (animationType)
            {
                case AnimationStates.Opening:
                    StartCoroutine(PlayOpeningAnimation());
                    break;
                case AnimationStates.Closing:
                    StartCoroutine(PlayClosingAnimation());
                    break;
            }
        }

        private System.Collections.IEnumerator PlayOpeningAnimation()
        {
            OpenAnimations();
            yield return null;

            switch (uiAnimationTypes)
            {
                case UIAnimationTypes.FadeIn:
                    yield return StartCoroutine(FadeIn());
                    break;
                case UIAnimationTypes.SlideIn:
                    yield return StartCoroutine(SlideIn());
                    break;
                case UIAnimationTypes.ScaleUp:
                    yield return StartCoroutine(ScaleUp());
                    break;
                case UIAnimationTypes.Flip:
                    yield return StartCoroutine(Flip());
                    break;
            }
        }

        private IEnumerator PlayClosingAnimation()
        {
            switch (uiAnimationTypes)
            {
                case UIAnimationTypes.FadeIn:
                    yield return StartCoroutine(FadeOut());
                    break;
                case UIAnimationTypes.SlideIn:
                    yield return StartCoroutine(SlideOut());
                    break;
                case UIAnimationTypes.ScaleUp:
                    yield return StartCoroutine(ScaleDown());
                    break;
                case UIAnimationTypes.Flip:
                    yield return StartCoroutine(FlipBack());
                    break;
            }

            CloseAnimations();
        }

        private IEnumerator FadeIn()
        {
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / animationDuration);
                canvasGroup.alpha = alpha;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / animationDuration);
                canvasGroup.alpha = alpha;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        private IEnumerator SlideIn()
        {
            Vector3 startPosition = CalculateStartPosition();
            Vector3 endPosition = originalPosition;
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                _rectTransform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / animationDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _rectTransform.position = endPosition;
        }

        private IEnumerator SlideOut()
        {
            Vector3 startPosition = originalPosition;
            Vector3 endPosition = CalculateStartPosition();
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                _rectTransform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / animationDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _rectTransform.position = endPosition;
        }

        private IEnumerator ScaleUp()
        {
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                float scale = Mathf.Lerp(0f, 1f, elapsedTime / animationDuration);
                _rectTransform.localScale = originalScale * scale;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _rectTransform.localScale = originalScale;
        }

        private IEnumerator ScaleDown()
        {
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                float scale = Mathf.Lerp(1f, 0f, elapsedTime / animationDuration);
                _rectTransform.localScale = originalScale * scale;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _rectTransform.localScale = Vector3.zero;
        }

        private IEnumerator Flip()
        {
            Quaternion startRotation = Quaternion.Euler(0f, 180f, 0f);
            Quaternion endRotation = originalRotation;
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                _rectTransform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / animationDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _rectTransform.rotation = endRotation;
        }

        private IEnumerator FlipBack()
        {
            Quaternion startRotation = originalRotation;
            Quaternion endRotation = Quaternion.Euler(0f, 180f, 0f);
            float elapsedTime = 0f;
            while (elapsedTime < animationDuration)
            {
                _rectTransform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / animationDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            _rectTransform.rotation = endRotation;
        }

        private Vector3 CalculateStartPosition()
        {
            Vector3 startPosition = originalPosition;
            switch (uiAnimationTypes)
            {
                case UIAnimationTypes.SlideIn:
                    startPosition = CalculateSlideStartPosition();
                    break;
                case UIAnimationTypes.Flip:
                    startPosition = CalculateFlipStartPosition();
                    break;
            }
            return startPosition;
        }

        private Vector3 CalculateSlideStartPosition()
        {
            Vector3 startPosition = originalPosition;
            Vector2 anchoredPosition = _rectTransform.anchoredPosition;
            switch (uiAnimationTypes)
            {
                case UIAnimationTypes.SlideIn:
                    if (anchoredPosition.x > 0f)
                        startPosition += Vector3.right * Screen.width;
                    else if (anchoredPosition.x < 0f)
                        startPosition += Vector3.left * Screen.width;
                    else if (anchoredPosition.y > 0f)
                        startPosition += Vector3.up * Screen.height;
                    else if (anchoredPosition.y < 0f)
                        startPosition += Vector3.down * Screen.height;
                    break;
            }
            return startPosition;
        }

        private Vector3 CalculateFlipStartPosition()
        {
            Vector3 startPosition = originalPosition;
            switch (uiAnimationTypes)
            {
                case UIAnimationTypes.Flip:
                    startPosition += Vector3.back * 100f;
                    break;
            }
            return startPosition;
        }

        private void CloseAnimations()
        {
            switch (uiAnimationTypes)
            {
                case UIAnimationTypes.FadeIn:
                    canvasGroup.alpha = 0f;
                    break;
                case UIAnimationTypes.SlideIn:
                    _rectTransform.position = CalculateSlideStartPosition();
                    break;
                case UIAnimationTypes.ScaleUp:
                    _rectTransform.localScale = Vector3.zero;
                    break;
                case UIAnimationTypes.Flip:
                    _rectTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
            }
        }

        private void OpenAnimations()
        {
            switch (uiAnimationTypes)
            {
                case UIAnimationTypes.FadeIn:
                    canvasGroup.alpha = 1f;
                    break;
                case UIAnimationTypes.SlideIn:
                    _rectTransform.position = originalPosition;
                    break;
                case UIAnimationTypes.ScaleUp:
                    _rectTransform.localScale = originalScale;
                    break;
                case UIAnimationTypes.Flip:
                    _rectTransform.rotation = originalRotation;
                    break;
            }
        }
    }
}


