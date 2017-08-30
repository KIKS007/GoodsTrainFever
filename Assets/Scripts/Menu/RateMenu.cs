using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RateMenu : MonoBehaviour
{
	public Color DefaultColor;
	public Image[] RatingStars = new Image[5];
	public Image[] DifficultyRatingStars = new Image[5];

	private int rate1 = 0;
	private int rate2 = 0;
	// Update is called once per frame
	void OnEnable ()
	{
		SetRatingTo (0);
		SetDifficultyRatingTo (0);
	}

	public void SetRatingTo (int value)
	{
		for (int i = 0; i < value; i++) {
			RatingStars [i].color = Color.white;
		}
		for (int i = value; i < 5; i++) {
			RatingStars [i].color = DefaultColor;
		}
		rate1 = value;
	}

	public void SetDifficultyRatingTo (int value)
	{
		for (int i = 0; i < value; i++) {
			DifficultyRatingStars [i].color = Color.white;
		}

		for (int i = value; i < 5; i++) {
			DifficultyRatingStars [i].color = DefaultColor;
		}
		rate2 = value;
	}

	public void ValidateRating ()
	{
		StatsManager.Instance.SendRatedLevelData (rate1, rate2);
		StatsManager.Instance.EndRatingPasstrough ();
	}
}
