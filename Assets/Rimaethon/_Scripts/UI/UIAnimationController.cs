using Rimaethon._Scripts.Core.Enums;
using UnityEngine;
using UnityEngine.UI;



public class UIAnimationController : MonoBehaviour, IPlayAnimation
{
    [SerializeField] private UIAnimationTypes animationType;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float animationDuration = 1f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private void Start()
    {
        originalPosition = rectTransform.position;
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.rotation;

        canvasGroup.alpha = 0f;
        rectTransform.position = CalculateStartPosition();
        rectTransform.localScale = Vector3.zero;
    }

    public void PlayAnimation()
    {
        switch (animationType)
        {
            case UIAnimationTypes.FadeIn:
                StartCoroutine(FadeIn());
                break;
            case UIAnimationTypes.SlideIn:
                StartCoroutine(SlideIn());
                break;
            case UIAnimationTypes.ScaleUp:
                StartCoroutine(ScaleUp());
                break;
            case UIAnimationTypes.Flip:
                StartCoroutine(Flip());
                break;
        }
    }

    private System.Collections.IEnumerator FadeIn()
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

    private System.Collections.IEnumerator SlideIn()
    {
        Vector3 startPosition = CalculateStartPosition();
        Vector3 endPosition = originalPosition;
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            rectTransform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rectTransform.position = endPosition;
    }

    private System.Collections.IEnumerator ScaleUp()
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            float scale = Mathf.Lerp(0f, 1f, elapsedTime / animationDuration);
            rectTransform.localScale = originalScale * scale;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rectTransform.localScale = originalScale;
    }

    private System.Collections.IEnumerator Flip()
    {
        Quaternion startRotation = Quaternion.Euler(0f, 180f, 0f);
        Quaternion endRotation = originalRotation;
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            rectTransform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rectTransform.rotation = endRotation;
    }

    private Vector3 CalculateStartPosition()
    {
        Vector3 startPosition = originalPosition;
        switch (animationType)
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
        Vector2 anchoredPosition = rectTransform.anchoredPosition;
        switch (animationType)
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
        switch (animationType)
        {
            case UIAnimationTypes.Flip:
                startPosition += Vector3.back * 100f;
                break;
        }
        return startPosition;
    }
}
