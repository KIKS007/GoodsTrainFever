using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class LoadingScreen : MonoBehaviour
{
	public Image IE_Square, IE_I, IE_E, Logos;
	public Text IE_Ron, IE_Qual;
	public CanvasGroup IE_Logo, IE_Letters;

	private bool _animEnd = false;
	// Use this for initialization
	void Start ()
	{
		IE_Logo.DOFade (0, 0.2f).From ().SetDelay (0.5f);
		IE_Logo.transform.DOScale (0, 0.6f).From ().SetEase (Ease.OutExpo).SetDelay (0.5f);
		IE_Logo.transform.DORotate (new Vector3 (0, 0, 20), 0.5f).From ().SetEase (Ease.OutBack).SetDelay (0.5f);

		IE_Letters.transform.DOScale (0.5f, 0.3f).SetEase (Ease.OutBack, 5).SetDelay (1.1f);
		IE_Letters.transform.DORotate (new Vector3 (0, 0, 90), 0.3f).SetEase (Ease.OutBack).SetDelay (1.25f);

		IE_Square.transform.DOScale (1.5f, 0.3f).SetDelay (1.1f).SetEase (Ease.InBack, 4);
		IE_Square.DOFade (0, 0.3f).SetDelay (1.1f);
		IE_Square.transform.DORotate (new Vector3 (0, 0, 90), 0.3f).SetDelay (1.1f).SetEase (Ease.OutExpo);

		IE_I.transform.DOLocalMoveY (730, 0.5f).SetDelay (1.45f).SetEase (Ease.OutExpo);
		IE_Ron.DOFade (0, 0.5f).From ().SetDelay (1.5f);
		IE_Qual.DOFade (0, 0.5f).From ().SetDelay (1.5f);

		Logos.DOFade (0, 0.5f).SetDelay (3.0f).From ();
		StartCoroutine (SplashScreen ());
		DOVirtual.DelayedCall (5f, () => _animEnd = true);

	}

	IEnumerator SplashScreen ()
	{
		AsyncOperation loadingScene = SceneManager.LoadSceneAsync (SceneManager.GetActiveScene ().buildIndex + 1);
		loadingScene.allowSceneActivation = false;
		yield return new WaitUntil (() => loadingScene.progress >= 0.9f);
		yield return new WaitUntil (() => _animEnd);
		loadingScene.allowSceneActivation = true;
	}
}
