using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Rimaethon._Scripts.UI
{
    public class UIAnimationHelperMethods : MonoBehaviour
    {
        private static bool isAnimating;

        public static IEnumerator ZoomIn(RectTransform Transform, float Speed, UnityEvent OnEnd)
        {
            if (isAnimating)
                yield break;

            isAnimating = true;

            float time = 0;
            while (time < 1)
            {
                Transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            Transform.localScale = Vector3.one;

            OnEnd?.Invoke();

            isAnimating = false;
        }

        public static IEnumerator ZoomOut(RectTransform Transform, float Speed, UnityEvent OnEnd)
        {
            if (isAnimating)
                yield break;

            isAnimating = true;

            float time = 0;
            while (time < 1)
            {
                Transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            Transform.localScale = Vector3.zero;
            OnEnd?.Invoke();

            isAnimating = false;
        }

        public static IEnumerator FadeIn(CanvasGroup CanvasGroup, float Speed, UnityEvent OnEnd)
        {
            if (isAnimating)
                yield break;

            isAnimating = true;

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

            isAnimating = false;
        }

        public static IEnumerator FadeOut(CanvasGroup CanvasGroup, float Speed, UnityEvent OnEnd)
        {
            if (isAnimating)
                yield break;

            isAnimating = true;

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

            isAnimating = false;
        }

        public static IEnumerator SlideIn(RectTransform Transform, UIDirection UIDirection, float Speed,
            UnityEvent OnEnd)
        {
            if (isAnimating)
                yield break;

            isAnimating = true;

            Vector2 startPosition;
            switch (UIDirection)
            {
                case UIDirection.UP:
                    startPosition = new Vector2(0, -Screen.height);
                    break;
                case UIDirection.RIGHT:
                    startPosition = new Vector2(-Screen.width, 0);
                    break;
                case UIDirection.DOWN:
                    startPosition = new Vector2(0, Screen.height);
                    break;
                case UIDirection.LEFT:
                    startPosition = new Vector2(Screen.width, 0);
                    break;
                default:
                    startPosition = new Vector2(0, -Screen.height);
                    break;
            }

            float time = 0;
            while (time < 1)
            {
                Transform.anchoredPosition = Vector2.Lerp(startPosition, Vector2.zero, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            Transform.anchoredPosition = Vector2.zero;
            OnEnd?.Invoke();

            isAnimating = false;
        }

        public static IEnumerator SlideOut(RectTransform Transform, UIDirection UIDirection, float Speed,
            UnityEvent OnEnd)
        {
            if (isAnimating)
                yield break;

            isAnimating = true;

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

            float time = 0;
            while (time < 1)
            {
                Transform.anchoredPosition = Vector2.Lerp(Vector2.zero, endPosition, time);
                yield return null;
                time += Time.deltaTime * Speed;
            }

            Transform.anchoredPosition = endPosition;
            OnEnd?.Invoke();

            isAnimating = false;
        }
    }
}