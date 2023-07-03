using System.Collections;
using System.Collections.Generic;
using Rimaethon._Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadingSceneController : MonoBehaviour
{
    private CanvasGroup loadingScreen;
    private float fadeDuration = 0.5f; // Minimum duration of the fade effect in seconds

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<int>(GameEvents.OnSceneChange, LoadScene);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<int>(GameEvents.OnSceneChange, LoadScene);
    }

    private void Awake()
    {
        loadingScreen = GetComponent<CanvasGroup>();
    }

    private void LoadScene(int sceneID)
    {
        StartCoroutine(LoadSceneAsync(sceneID));
    }

    IEnumerator LoadSceneAsync(int sceneID)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            yield return FadeCanvasGroup(loadingScreen, progress, fadeDuration);
        }

        yield return FadeCanvasGroup(loadingScreen, 0f, fadeDuration); // Unfade the loading screen
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0.0f;
        float actualDuration = Mathf.Max(duration, 0.5f); // Ensure a minimum duration of 0.5 seconds

        while (time < actualDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / actualDuration);
            canvasGroup.alpha = alpha;
            yield return null;
            time += Time.deltaTime;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
