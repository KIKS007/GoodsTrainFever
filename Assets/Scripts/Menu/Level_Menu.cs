using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Level_Menu : MonoBehaviour 
{
	public int levelIndex;
	public bool isUnlocked = true;
	public Stage_Menu levelStage = null;

	[Header ("Elements")]
	public Text levelTitle;
	public Text ordersCount;
	public Text trainsCount;
	public Text errorsCount;
	public GameObject lockImage;
	public GameObject playButton;

	[Header ("Stars")]
	public Image[] stars = new Image[3];

	public void Setup (int index, Level level, Stage_Menu stage = null)
	{
		levelIndex = index;

		if(stage != null)
			levelStage = stage;

		levelTitle.text = "Level " + (index + 1).ToString ();

		for (int i = 0; i < 3; i++)
		{
			if (i < level.starsEarned)
				stars [i].gameObject.SetActive (false);
			else
				stars [i].gameObject.SetActive (true);
		}

		if (levelStage != null && !levelStage.isUnlocked)
		{
			playButton.SetActive (false);
			lockImage.SetActive (true);
		}
		else
		{
			playButton.SetActive (true);
			lockImage.SetActive (false);
		}

		if (levelStage == null || levelStage.isUnlocked)
			isUnlocked = true;
		else
			isUnlocked = false;

		ordersCount.text = level.orders.Count.ToString ();
		trainsCount.text = (level.rail1Trains.Count + level.rail2Trains.Count).ToString ();
		errorsCount.text = level.errorsAllowed.ToString ();
	}

	public void Play ()
	{
		LevelsManager.Instance.LoadLevelSettings (levelIndex);
		MenuManager.Instance.StartLevel ();
	}
}
