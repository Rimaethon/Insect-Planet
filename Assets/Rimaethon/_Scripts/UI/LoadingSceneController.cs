using System.Collections;
using Rimaethon._Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneController : MonoBehaviour
{
    private readonly float fadeDuration = 0.5f; // Minimum duration of the fade effect in seconds
    private CanvasGroup loadingScreen;

    private void Awake()
    {
        loadingScreen = GetComponent<CanvasGroup>();
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        Debug.Log(loadingScreen.alpha + "alpha is this and ");
    }

    private void OnEnable()
    {
        EventManager.Instance.AddHandler<int>(GameEvents.OnSceneChange, LoadScene);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveHandler<int>(GameEvents.OnSceneChange, LoadScene);
    }

    private void LoadScene(int sceneID)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeCanvasGroup(loadingScreen, 1, fadeDuration));
    }

    private IEnumerator LoadSceneAsync(int sceneID)
    {
        var operation = SceneManager.LoadSceneAsync(sceneID);

        while (!operation.isDone) yield return null;

        yield return StartCoroutine(FadeCanvasGroup(loadingScreen, 0, fadeDuration));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        var startAlpha = canvasGroup.alpha;
        var time = 0.0f;
        var actualDuration = Mathf.Max(duration, 0.5f); // Ensure a minimum duration of 0.5 second

        while (time < actualDuration)
        {
            var alpha = Mathf.Lerp(startAlpha, targetAlpha, actualDuration);
            canvasGroup.alpha = alpha;
            yield return null;
            time += Time.deltaTime;
        }

        canvasGroup.alpha = targetAlpha;
        yield return StartCoroutine(LoadSceneAsync(1));
    }
}