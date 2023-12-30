using System.Collections;
using Rimaethon.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : PersistentSingleton<SceneController>
{
	public Image m_targetImage;
	public float m_fadeDuration = 1f;
	public float m_stayDuration = 1f;
	public AnimationCurve m_smoothCurve = new AnimationCurve( new Keyframe[] { new Keyframe( 0f, 0f ), new Keyframe( 1f, 1f ) } );
	private float m_timerCurrent;


	public void HandleSceneChange(int sceneID)
	{
		StartCoroutine( FadeInOut() );
	}
	private IEnumerator FadeInOut()
	{
		float start = 0f;
		float end = 1f;
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
		asyncLoad.allowSceneActivation = false;
		yield return StartCoroutine( Fade( start, end ) ); // Fade in.

		while (asyncLoad.progress < 0.9f)
		{
			yield return null;
		}
		asyncLoad.allowSceneActivation = true;
		yield return StartCoroutine( Fade( end, start ) ); // Fade out.
	}

	private IEnumerator Fade(float start, float end)
	{
		m_timerCurrent = 0f;

		while (m_timerCurrent <= m_fadeDuration)
		{
			m_timerCurrent += Time.deltaTime;
			Color c = m_targetImage.color;
			m_targetImage.color = new Color( c.r, c.g, c.b, Mathf.Lerp( start, end, m_smoothCurve.Evaluate( m_timerCurrent / m_fadeDuration ) ) );
			yield return new WaitForEndOfFrame();
		}
	}
}
