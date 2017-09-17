using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stage_Menu : MonoBehaviour
{
	public static System.Action<Stage_Menu> OnStageUnlock;

	public int starsRequired;
	public bool isUnlocked = false;

	[Header ("Elements")]
	public GameObject innerStar;
	public Text starsCount;
	public GameObject lockImage;
	public Button trophyButton;
	public Text trophyTitle;

	[HideInInspector]
	public int trophyStageIndex = 0;
	[HideInInspector]
	public bool _allTrophiesMenu = false;

	void Start ()
	{
		trophyButton.onClick.AddListener (() => {
			MenuManager.Instance.menuTrophies.stageMenu = this;
			MenuManager.Instance.ToMenu (MenuManager.Instance.menuTrophies);
		});
	}

	public void Setup (bool unlock, int stars)
	{
		starsRequired = stars;
		starsCount.text = starsRequired.ToString ();

		innerStar.SetActive (!unlock);

		lockImage.SetActive (!unlock);

		trophyTitle.gameObject.SetActive (unlock);

		trophyButton.gameObject.SetActive (unlock);

		starsCount.transform.parent.gameObject.SetActive (!unlock);

		if (unlock && !isUnlocked && !_allTrophiesMenu) {
			if (OnStageUnlock != null)
				OnStageUnlock (this);
		}

		isUnlocked = unlock;

		//trophyTitle.text = ScoreManager.Instance.levelStages [trophyStageIndex].trophy.GetComponent<Trophy_Menu> ().meshTitle;

		trophyTitle.text = 
			"PALIER " + (trophyStageIndex + 1).ToString () + "\n DEBLOQUé !";
	}
}
