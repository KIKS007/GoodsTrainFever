using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DarkTonic.MasterAudio;

public class MenuEndLevel : MenuComponent
{
	[Header ("Level Infos")]
	public Text levelTitle;
	public Text ordersSent;
	public Text duration;

	[Header ("Stars")]
	public List<Image> starsEmpty = new List<Image> ();
	public List<Image> starsFilled = new List<Image> ();

	public float starsUnlockScalePunch = 0.5f;
	public float starsDelay = 0.1f;
	public float starsBetweenDelay = 0.1f;

	[Header ("Errors")]
	public GameObject errorsParent;
	public Text errorsCount;
	public Text errorsAllowedCount;

	[Header ("Level Success")]
	public RectTransform success;
	public RectTransform defeat;

	[Header ("Next")]
	public GameObject nextLevel;

	public override void OnShow ()
	{
		base.OnShow ();

		if (this.gameObject.activeInHierarchy == false) {
			this.gameObject.SetActive (true);
		}

		StartCoroutine (LevelInfos ());
	}

	IEnumerator LevelInfos ()
	{
		levelTitle.text = "Niveau " + (LevelsManager.Instance.levelIndex + 1).ToString ();

		ordersSent.text = OrdersManager.Instance.ordersSentCount.ToString () + "/" + LevelsManager.Instance.orders.Count;
		duration.text = LevelsManager.Instance.levelDuration.ToString ();

		if (LevelsManager.Instance.levelIndex + 1 == LevelsManager.Instance.levelsCount || !ScoreManager.Instance.IsLevelUnlocked (LevelsManager.Instance.levelIndex + 1))
			nextLevel.SetActive (false);
		else
			nextLevel.SetActive (true);

		success.gameObject.SetActive (false);
		defeat.gameObject.SetActive (false);

		errorsParent.gameObject.SetActive (LevelsManager.Instance.errorsLocked > 0);

		errorsCount.text = LevelsManager.Instance.errorsLocked.ToString ();
		errorsAllowedCount.text = "Sur " + LevelsManager.Instance.errorsAllowed.ToString () + " autorisées";

		/*	if (LevelsManager.Instance.errorsLocked == 0)
			errorsParent.SetActive (false);
		else
			errorsParent.SetActive (true);*/

		if (ScoreManager.Instance.success)
			success.gameObject.SetActive (true);
		else
			defeat.gameObject.SetActive (true);

		for (int i = 0; i < LevelsManager.Instance.currentLevel.starsStates.Length; i++) 
		{
			//RectTransform starOuter = starsOuter [i];
			//Image starInner = starsInner [i];

			Image sFilled = starsFilled [i];
			Image sEmpty = starsEmpty [i];

			//Reset Color
			sEmpty.color = GlobalVariables.Instance.normalStarColor;

			switch (LevelsManager.Instance.currentLevel.starsStates [i]) 
			{
			case StarState.Locked:
				sFilled.DOFade (0, 0);

				DOVirtual.DelayedCall (MenuManager.Instance.menuAnimationDuration + starsDelay + starsBetweenDelay * i, () => {
					sEmpty.rectTransform.DOPunchScale (Vector3.one * starsUnlockScalePunch, MenuManager.Instance.menuAnimationDuration);
				});

				break;

			case StarState.Unlocked:
				sFilled.DOFade (0, 0);

				if (ScoreManager.Instance.success)
					
					DOVirtual.DelayedCall (MenuManager.Instance.menuAnimationDuration + starsDelay + starsBetweenDelay * i, () => 
					{
						MasterAudio.PlaySound ("SFX_Pop");
							sFilled.DOFade (1, MenuManager.Instance.menuAnimationDuration);
							sFilled.rectTransform.DOPunchScale (Vector3.one * starsUnlockScalePunch, MenuManager.Instance.menuAnimationDuration);
					});

				break;

			case StarState.Saved:
				
				sFilled.DOFade (1, 0);

				if (ScoreManager.Instance.success)
					DOVirtual.DelayedCall (MenuManager.Instance.menuAnimationDuration + starsDelay + starsBetweenDelay * i, () => {
						sFilled.rectTransform.DOPunchScale (Vector3.one * starsUnlockScalePunch, MenuManager.Instance.menuAnimationDuration);
					});

				break;

			case StarState.ErrorLocked:
				
				sFilled.DOFade (0, 0);

				DOVirtual.DelayedCall (MenuManager.Instance.menuAnimationDuration + starsDelay + starsBetweenDelay * i, () => {
					//starInner.DOFade (0, MenuManager.Instance.menuAnimationDuration);
					sEmpty.DOColor (GlobalVariables.Instance.errorLockedStarColor, MenuManager.Instance.menuAnimationDuration);
					sEmpty.rectTransform.DOPunchScale (Vector3.one * starsUnlockScalePunch, MenuManager.Instance.menuAnimationDuration);

				});
				
				break;
			}
		}

		yield return 0;
	}
}
