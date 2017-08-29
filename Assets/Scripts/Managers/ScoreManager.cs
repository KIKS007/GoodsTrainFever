using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ScoreManager : Singleton<ScoreManager>
{
	[Header ("Stars")]
	public int starsEarned;

	[Header ("Orders Percentages")]
	[Range (0, 100)]
	public int stars1OrdersPercentage = 20;
	[Range (0, 100)]
	public int stars2OrdersPercentage = 50;
	[Range (0, 100)]
	public int stars3OrdersPercentage = 100;

	[Header ("Success")]
	public bool success;

	[Header ("Settings")]
	public bool clearOnStart = false;
	public bool loadOnStart = true;
	public bool saveOnStop = true;

	[Header ("Stages")]
	public MenuLevels menuLevels;
	public List<Stage> levelStages = new List<Stage> ();

	// Use this for initialization
	void Awake ()
	{
		MenuManager.Instance.OnLevelStart += OnLevelStart;

		if (loadOnStart)
			LoadLevelStars ();

		if (clearOnStart)
			DeletePlayerPrefs ();
	}

	[PropertyOrder (-1)]
	[ButtonAttribute]
	public void DeletePlayerPrefs ()
	{
		PlayerPrefs.DeleteAll ();

		if (Application.isPlaying)
			saveOnStop = false;
	}

	public bool IsLevelUnlocked (int levelIndex)
	{
		return menuLevels._levelsMenu [levelIndex].isUnlocked;
	}

	void OnLevelStart ()
	{
		foreach (Transform t in LevelsManager.Instance.transform) {
			Level level = t.GetComponent<Level> ();


			for (int i = 0; i < 3; i++) {
				if (level.starsStates [i] == StarState.Unlocked)
					level.starsStates [i] = StarState.Saved;
				
				if (level.starsStates [i] == StarState.ErrorLocked)
					level.starsStates [i] = StarState.Locked;
			}
		}
	}

	public void ResetAllLevelsStars ()
	{
		for (int i = 0; i < LevelsManager.Instance.transform.childCount; i++) {
			ResetLevelStars (i);
		}
	}

	public void UnlockAllLevels ()
	{
		for (int i = 0; i < LevelsManager.Instance.transform.childCount; i++) {
			UnlockStars (100, 1, i);
		}
		//Comment if you don't want unlocking levels to have wierd stars state
		//ResetAllLevelsStars ();
	}

	public void LoadLevelStars ()
	{
		for (int i = 0; i < LevelsManager.Instance.transform.childCount; i++) {
			Level level = LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ();

			if (PlayerPrefs.HasKey ("Stars" + i))
				level.starsEarned = PlayerPrefs.GetInt ("Stars" + i);
			
			for (int j = 0; j < 3; j++) {
				if (j < level.starsEarned)
					level.starsStates [j] = StarState.Saved;
				else
					level.starsStates [j] = StarState.Locked;
			}
			
			//Debug.Log ("HasKey: " + PlayerPrefs.HasKey ("Stars" + i) + " value:" + PlayerPrefs.GetInt ("Stars" + i));


			//Debug.Log (level + " : " + level.starsEarned);
		}

		UpdateStars ();
	}

	public void SaveLevelStars ()
	{
		for (int i = 0; i < LevelsManager.Instance.levelsCount; i++) {
			PlayerPrefs.SetInt ("Stars" + i, LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ().starsEarned);	
		}
	}

	public void ResetLevelStars (int levelIndex)
	{
		PlayerPrefs.DeleteKey ("Stars" + levelIndex);

		PlayerPrefs.DeleteKey ("FirstStar" + levelIndex);
		PlayerPrefs.DeleteKey ("SecondStar" + levelIndex);
		PlayerPrefs.DeleteKey ("ThirdStar" + levelIndex);

		Level level = LevelsManager.Instance.transform.GetChild (levelIndex).GetComponent<Level> ();

		level.starsEarned = 0;

		for (int i = 0; i < level.starsStates.Length; i++)
			level.starsStates [i] = StarState.Locked;

		FindObjectOfType<MenuLevels> ().UpdateLevels ();
	}

	public void UpdateStars ()
	{
		starsEarned = 0;

		foreach (Transform t in LevelsManager.Instance.transform) {
			Level level = t.GetComponent<Level> ();
			
			starsEarned += level.starsEarned;
		}

		menuLevels.UpdateLevels ();
	}

	public void UnlockStars (int ordersPrepared, int trainsCount, int levelIndex)
	{
		Level level = LevelsManager.Instance.transform.GetChild (levelIndex).GetComponent<Level> ();
		
		int ordersPreparedPercentage = Mathf.RoundToInt (((float)ordersPrepared / (float)level.ordersCount) * 100f);

		//Debug.Log (ordersPreparedPercentage + "% orders done");

		FirstStar (ordersPreparedPercentage, level, levelIndex);
		SecondStar (ordersPreparedPercentage, level, levelIndex);
		ThirdStar (ordersPreparedPercentage, level, levelIndex);

		UpdateStars ();

		//Debug.Log ("LEVEL#" + (levelIndex + 1).ToString () + " - Stars: " + level.starsEarned + " - " + ordersPreparedPercentage + "% orders done");
	}

	void FirstStar (int ordersPercentage, Level level, int levelIndex)
	{
		if (!PlayerPrefs.HasKey ("FirstStar" + levelIndex) && LevelsManager.Instance.errorsLocked > LevelsManager.Instance.errorsAllowed) {
			level.starsStates [0] = StarState.ErrorLocked;
			success = false;
			return;
		}

		if (ordersPercentage >= stars1OrdersPercentage) {
			success = true;

			if (PlayerPrefs.HasKey ("FirstStar" + levelIndex))
				return;

			level.starsEarned++;
			PlayerPrefs.SetInt ("FirstStar" + levelIndex, 1);

			level.starsStates [0] = StarState.Unlocked;
		} else
			success = false;
	}

	void SecondStar (int ordersPercentage, Level level, int levelIndex)
	{
		if (!PlayerPrefs.HasKey ("SecondStar" + levelIndex) && LevelsManager.Instance.errorsLocked > LevelsManager.Instance.errorsSecondStarAllowed) {
			level.starsStates [1] = StarState.ErrorLocked;
			level.starsStates [2] = StarState.ErrorLocked;
			return;
		}

		if (ordersPercentage >= stars2OrdersPercentage) {
			//LeastTrainsStar (trainsCount, level, levelIndex);

			if (!PlayerPrefs.HasKey ("SecondStar" + levelIndex)) {
				level.starsEarned++;
				PlayerPrefs.SetInt ("SecondStar" + levelIndex, 1);

				level.starsStates [1] = StarState.Unlocked;
			}
		}
	}

	void ThirdStar (int ordersPercentage, Level level, int levelIndex)
	{
		if (PlayerPrefs.HasKey ("ThirdStar" + levelIndex))
			return;

		if (LevelsManager.Instance.errorsLocked > 0) {
			level.starsStates [2] = StarState.ErrorLocked;
			return;
		}

		if (ordersPercentage >= stars3OrdersPercentage) {
			level.starsEarned++;
			PlayerPrefs.SetInt ("ThirdStar" + levelIndex, 1);

			level.starsStates [2] = StarState.Unlocked;
		}
	}

	void OnApplicationQuit ()
	{
		if (saveOnStop) {
			SaveLevelStars ();
			PlayerPrefs.Save ();
			//StatsManager.Instance.StopLevelTrack (false);
		}
	}

	void OnApplicationFocus (bool hasFocus)
	{
		if (saveOnStop && !hasFocus) {
			SaveLevelStars ();
			PlayerPrefs.Save ();
		}
	}
}

[System.Serializable]
public class Stage
{
	[Header ("Stars")]
	public int index;
	public int starsRequired;
	public Stage_Menu stage;

	[Header ("Trophy")]
	public GameObject trophy;

}
