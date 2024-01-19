using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Rimaethon._Scripts.UI
{
    public class UIAnimationHelperMethods
    {
        private static bool isSlideInAnimating;
        private static bool isSlideOutAnimating;

        public static IEnumerator ZoomIn(RectTransform Transform, float Speed, UnityEvent OnEnd)
        {
            if (isSlideInAnimating)
                yield break;

            isSlideInAnimating = true;

            float time = 0;
            while (time < 1)
            {
                Transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            Transform.localScale = Vector3.one;

            OnEnd?.Invoke();

            isSlideInAnimating = false;
        }

        public static IEnumerator ZoomOut(RectTransform Transform, float Speed, UnityEvent OnEnd)
        {
            if (isSlideInAnimating)
                yield break;

            isSlideInAnimating = true;

            float time = 0;
            while (time < 1)
            {
                Transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            Transform.localScale = Vector3.zero;
            OnEnd?.Invoke();

            isSlideInAnimating = false;
        }

        public static IEnumerator FadeIn(CanvasGroup CanvasGroup, float Speed, UnityEvent OnEnd)
        {
            if (isSlideInAnimating)
                yield break;

            isSlideInAnimating = true;

            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.interactable = true;

            float time = 0;
            while (time < 1)
            {
                CanvasGroup.alpha = Mathf.Lerp(0, 1, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            CanvasGroup.alpha = 1;
            OnEnd?.Invoke();

            isSlideInAnimating = false;
        }

        public static IEnumerator FadeOut(CanvasGroup CanvasGroup, float Speed, UnityEvent OnEnd)
        {
            if (isSlideInAnimating)
                yield break;

            isSlideInAnimating = true;

            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.interactable = false;

            float time = 0;
            while (time < 1)
            {
                CanvasGroup.alpha = Mathf.Lerp(1, 0, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            CanvasGroup.alpha = 0;
            OnEnd?.Invoke();

            isSlideInAnimating = false;
        }

        public static IEnumerator SlideIn(RectTransform Transform, UIDirection UIDirection, float Speed)
        {
            if (isSlideInAnimating)
                yield break;
            Transform.gameObject.SetActive(true);
            isSlideInAnimating = true;
            Transform.gameObject.GetComponent<UIPage>()._isAnimating= true;
            Vector2 startPosition;
            Vector2 endPosition=Vector2.zero;
            switch (UIDirection)
            {
                case UIDirection.UP:
                    startPosition = new Vector2(0, -Screen.height);
                    startPosition.x = Transform.anchoredPosition.x;
                    endPosition.x=Transform.anchoredPosition.x;
                    break;
                case UIDirection.RIGHT:
                    startPosition = new Vector2(-Screen.width, 0);
                    startPosition.y = Transform.anchoredPosition.y;
                    endPosition.y=Transform.anchoredPosition.y;
                    break;
                case UIDirection.DOWN:
                    startPosition = new Vector2(0, Screen.height);
                    startPosition.x = Transform.anchoredPosition.x;
                    endPosition.x=Transform.anchoredPosition.x;
                    break;
                case UIDirection.LEFT:
                    startPosition = new Vector2(Screen.width, 0);
                    startPosition.y = Transform.anchoredPosition.y;
                    endPosition.y=Transform.anchoredPosition.y;
                    break;
                default:
                    startPosition = new Vector2(0, -Screen.height);
                    startPosition.x = Transform.anchoredPosition.x;
                    endPosition.x=Transform.anchoredPosition.x;
                    break;
            }

            float time = 0;
            while (time < 1)
            {
                Transform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            Transform.anchoredPosition = Vector2.zero;
            Transform.gameObject.GetComponent<UIPage>()._isAnimating= false;
            isSlideInAnimating = false;
        }

        public static IEnumerator SlideOut(RectTransform Transform, UIDirection UIDirection, float Speed)
        {
            if (isSlideOutAnimating)
                yield break;

            isSlideOutAnimating = true;
            Transform.gameObject.GetComponent<UIPage>()._isAnimating= true;
            Vector2 endPosition;
            switch (UIDirection)
            {
                case UIDirection.UP:
                    endPosition = new Vector2(0, Screen.height);
                    break;
                case UIDirection.RIGHT:
                    endPosition = new Vector2(Screen.width, 0);
                    break;
                case UIDirection.DOWN:
                    endPosition = new Vector2(0, -Screen.height);
                    break;
                case UIDirection.LEFT:
                    endPosition = new Vector2(-Screen.width, 0);
                    break;
                default:
                    endPosition = new Vector2(0, Screen.height);
                    break;
            }
            endPosition+=Transform.anchoredPosition;
            float time = 0;
            while (time < 1)
            {
                Transform.anchoredPosition = Vector2.Lerp(Vector2.zero, endPosition, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            Transform.anchoredPosition = new Vector2(0, -Screen.height);
            Transform.gameObject.SetActive(false);
            Transform.gameObject.GetComponent<UIPage>()._isAnimating= false;
            isSlideOutAnimating = false;
        }
    }
}
