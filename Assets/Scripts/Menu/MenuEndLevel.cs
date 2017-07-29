using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuEndLevel : MenuComponent 
{
	[Header ("Level Infos")]
	public Text levelTitle;
	public Text ordersSent;
	public Text trainsSent;
	public Text duration;

	[Header ("Stars")]
	public RectTransform[] starsOuter = new RectTransform[3];
	public Image[] starsInner = new Image[3];
	public float starsUnlockScalePunch = 0.5f;
	public float starsDelay = 0.1f;
	public float starsBetweenDelay = 0.1f;

	[Header ("Errors")]
	public GameObject errorsParent;
	public Text errorsCount;

	[Header ("Level Success")]
	public RectTransform success;
	public RectTransform defeat;

	[Header ("Next")]
	public GameObject nextLevel;

	public override void Show ()
	{
		base.Show ();

		StartCoroutine (LevelInfos ());
	}

	IEnumerator LevelInfos ()
	{
		levelTitle.text = "Level " + (LevelsManager.Instance.levelIndex + 1).ToString ();

		ordersSent.text = OrdersManager.Instance.ordersSentCount.ToString () + "/" + LevelsManager.Instance.orders.Count;
		trainsSent.text = LevelsManager.Instance.trainsUsed.ToString ();
		duration.text = LevelsManager.Instance.levelDuration.ToString ();

		if (LevelsManager.Instance.levelIndex + 1 == LevelsManager.Instance.levelsCount || !ScoreManager.Instance.IsLevelUnlocked (LevelsManager.Instance.levelIndex + 1))
			nextLevel.SetActive (false);
		else
			nextLevel.SetActive (true);

		success.gameObject.SetActive (false);
		defeat.gameObject.SetActive (false);

		errorsCount.text = LevelsManager.Instance.errorsLocked.ToString ();

		if (LevelsManager.Instance.errorsLocked == 0)
			errorsParent.SetActive (false);
		else
			errorsParent.SetActive (true);

		if(ScoreManager.Instance.success)
			success.gameObject.SetActive (true);
		else
			defeat.gameObject.SetActive (true);

		for(int i = 0; i < LevelsManager.Instance.currentLevel.starsStates.Length; i++)
		{
			RectTransform starOuter = starsOuter [i];
			Image starInner = starsInner [i];

			//Reset Color
			starOuter.GetComponent<Image> ().color = GlobalVariables.Instance.normalStarColor;

			switch (LevelsManager.Instance.currentLevel.starsStates [i])
			{
			case StarState.Locked:
				starInner.DOFade (1, 0);
				break;
			case StarState.Unlocked:
				starInner.DOFade (1, 0);

				if(ScoreManager.Instance.success)
				DOVirtual.DelayedCall (MenuManager.Instance.menuAnimationDuration + starsDelay + starsBetweenDelay * i, ()=>
					{
						starInner.DOFade (0, MenuManager.Instance.menuAnimationDuration);
						starOuter.DOPunchScale (Vector3.one * starsUnlockScalePunch, MenuManager.Instance.menuAnimationDuration);
					});

				break;
			case StarState.Saved:
				starInner.DOFade (0, 0);

				RectTransform star = starsOuter [i];

				if(ScoreManager.Instance.success)
				DOVirtual.DelayedCall (MenuManager.Instance.menuAnimationDuration + starsDelay + starsBetweenDelay * i, ()=>
					{
						star.DOPunchScale (Vector3.one * starsUnlockScalePunch, MenuManager.Instance.menuAnimationDuration);
					});

				break;

			case StarState.ErrorLocked:
				starInner.DOFade (1, 0);

				DOVirtual.DelayedCall (MenuManager.Instance.menuAnimationDuration + starsDelay + starsBetweenDelay * i, ()=>
					{
						//starInner.DOFade (0, MenuManager.Instance.menuAnimationDuration);
						starOuter.GetComponent<Image> ().DOColor (GlobalVariables.Instance.errorLockedStarColor, MenuManager.Instance.menuAnimationDuration);

					});
				

				break;
			}
		}

		yield return 0;
	}
}
