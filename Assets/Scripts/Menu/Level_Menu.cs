using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Level_Menu : MonoBehaviour 
{
	public int levelIndex;

	[Header ("Title")]
	public Text levelTitle;

	[Header ("Orders")]
	public Text ordersCount;

	[Header ("Trains")]
	public Text trainsCount;

	[Header ("Stars")]
	public Image[] stars = new Image[3];

	public void Setup (int index, Level level)
	{
		levelIndex = index;

		levelTitle.text = "Level " + (index + 1).ToString ();

		for (int i = 0; i < level.starsEarned; i++)
			stars [i].gameObject.SetActive (false);

		ordersCount.text = level.orders.Count.ToString ();
		trainsCount.text = (level.rail1Trains.Count + level.rail2Trains.Count).ToString ();
	}

	public void Play ()
	{
		LevelsManager.Instance.LoadLevelSettings (levelIndex);
		MenuManager.Instance.StartGame ();
	}
}
