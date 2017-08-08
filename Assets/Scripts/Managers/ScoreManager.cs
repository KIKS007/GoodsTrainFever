using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ScoreManager : Singleton<ScoreManager>
{
	[Header ("Stars")]
	public int starsEarned;

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
	void DeletePlayerPrefs ()
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
		foreach(Transform t in LevelsManager.Instance.transform)
		{
			Level level = t.GetComponent<Level> ();

			LevelSettings_LD levelLD = t.GetComponent<LevelSettings_LD> ();

			if(level != null)
				for(int i = 0; i < 3; i++)
				{
					if (level.starsStates [i] == StarState.Unlocked)
						level.starsStates [i] = StarState.Saved;
					
					if (level.starsStates [i] == StarState.ErrorLocked)
						level.starsStates [i] = StarState.Locked;
				}

			else
				for(int i = 0; i < 3; i++)
				{
					if (levelLD.starsStates [i] == StarState.Unlocked)
						levelLD.starsStates [i] = StarState.Saved;

					if (levelLD.starsStates [i] == StarState.ErrorLocked)
						levelLD.starsStates [i] = StarState.Locked;
				}
		}
	}

	public void LoadLevelStars ()
	{
		for(int i = 0; i < LevelsManager.Instance.transform.childCount; i++)
		{
			Level level = LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ();

			LevelSettings_LD levelLD = LevelsManager.Instance.transform.GetChild (i).GetComponent<LevelSettings_LD> ();

			if(level != null)
			{
				if (PlayerPrefs.HasKey ("Stars" + i))
					level.starsEarned = PlayerPrefs.GetInt ("Stars" + i);
				
				for (int j = 0; j < 3; j++)
				{
					if (j < level.starsEarned)
						level.starsStates [j] = StarState.Saved;
					else
						level.starsStates [j] = StarState.Locked;
				}
			}
			else
			{
				if (PlayerPrefs.HasKey ("Stars" + i))
					levelLD.starsEarned = PlayerPrefs.GetInt ("Stars" + i);

				for (int j = 0; j < 3; j++)
				{
					if (j < levelLD.starsEarned)
						levelLD.starsStates [j] = StarState.Saved;
					else
						levelLD.starsStates [j] = StarState.Locked;
				}
			}

			//Debug.Log ("HasKey: " + PlayerPrefs.HasKey ("Stars" + i) + " value:" + PlayerPrefs.GetInt ("Stars" + i));


			//Debug.Log (level + " : " + level.starsEarned);
		}

		UpdateStars ();
	}

	public void SaveLevelStars ()
	{
		for(int i = 0; i < LevelsManager.Instance.levelsCount; i++)
		{
			if(LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> () != null)
				PlayerPrefs.SetInt ("Stars" + i, LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ().starsEarned);	
			else
				PlayerPrefs.SetInt ("Stars" + i, LevelsManager.Instance.transform.GetChild (i).GetComponent<LevelSettings_LD> ().starsEarned);	
		}
	}

	public void UpdateStars ()
	{
		starsEarned = 0;

		foreach(Transform t in LevelsManager.Instance.transform)
		{
			Level level = t.GetComponent<Level> ();

			LevelSettings_LD levelLD = t.GetComponent<LevelSettings_LD> ();

			if(level != null)
				starsEarned += level.starsEarned;
			else
				starsEarned += levelLD.starsEarned;
		}

		menuLevels.UpdateLevels ();
	}

	public void UnlockStars (int ordersPrepared, int trainsCount, int levelIndex)
	{
		Level level = LevelsManager.Instance.transform.GetChild (levelIndex).GetComponent<Level> ();

		LevelSettings_LD levelLD = LevelsManager.Instance.transform.GetChild (levelIndex).GetComponent<LevelSettings_LD> ();

		if(level != null)
		{
			MostOrdersStar (ordersPrepared, level, levelIndex);
			AllOrdersStar (ordersPrepared, trainsCount, level, levelIndex);
			UpdateStars ();
			Debug.Log ("LEVEL#" + (levelIndex + 1).ToString () + " - Stars: " + level.starsEarned);
		}
		else
		{
			MostOrdersStar (ordersPrepared, levelLD, levelIndex);
			AllOrdersStar (ordersPrepared, trainsCount, levelLD, levelIndex);
			UpdateStars ();
			Debug.Log ("LEVEL#" + (levelIndex + 1).ToString () + " - Stars: " + levelLD.starsEarned);
		}
	}

	void MostOrdersStar (int ordersPrepared, Level level, int levelIndex)
	{
		if(!PlayerPrefs.HasKey ("MostOrdersStar" + levelIndex) && LevelsManager.Instance.errorsLocked > LevelsManager.Instance.errorsAllowed)
		{
			level.starsStates [0] = StarState.ErrorLocked;
			success = false;
			return;
		}

		if(ordersPrepared >= level.mostOrdersCount)
		{
			success = true;

			if (PlayerPrefs.HasKey ("MostOrdersStar" + levelIndex))
				return;
			
			level.starsEarned++;
			PlayerPrefs.SetInt ("MostOrdersStar" + levelIndex, 1);

			level.starsStates [0] = StarState.Unlocked;
		}
		else
			success = false;
	}

	void AllOrdersStar (int ordersPrepared, int trainsCount, Level level, int levelIndex)
	{
		if(!PlayerPrefs.HasKey ("AllOrdersStar" + levelIndex) && LevelsManager.Instance.errorsLocked > LevelsManager.Instance.errorsSecondStarAllowed)
		{
			level.starsStates [1] = StarState.ErrorLocked;
			level.starsStates [2] = StarState.ErrorLocked;
			return;
		}

		if(ordersPrepared == level.orders.Count)
		{
			LeastTrainsStar (trainsCount, level, levelIndex);

			if (!PlayerPrefs.HasKey ("AllOrdersStar" + levelIndex))
			{
				level.starsEarned++;
				PlayerPrefs.SetInt ("AllOrdersStar" + levelIndex, 1);

				level.starsStates [1] = StarState.Unlocked;
			}
		}
	}

	void LeastTrainsStar (int trainsCount, Level level, int levelIndex)
	{
		if (PlayerPrefs.HasKey ("LeastTrainsStar" + levelIndex))
			return;

		if (LevelsManager.Instance.errorsLocked > 0)
		{
			level.starsStates [2] = StarState.ErrorLocked;
			return;
		}

		if(trainsCount <= level.leastTrainsCount)
		{
			level.starsEarned++;
			PlayerPrefs.SetInt ("LeastTrainsStar" + levelIndex, 1);

			level.starsStates [2] = StarState.Unlocked;
		}
	}

	void MostOrdersStar (int ordersPrepared, LevelSettings_LD level, int levelIndex)
	{
		if(!PlayerPrefs.HasKey ("MostOrdersStar" + levelIndex) && LevelsManager.Instance.errorsLocked > LevelsManager.Instance.errorsAllowed)
		{
			level.starsStates [0] = StarState.ErrorLocked;
			success = false;
			return;
		}

		if(ordersPrepared >= level.mostOrdersCount)
		{
			success = true;

			if (PlayerPrefs.HasKey ("MostOrdersStar" + levelIndex))
				return;

			level.starsEarned++;
			PlayerPrefs.SetInt ("MostOrdersStar" + levelIndex, 1);

			level.starsStates [0] = StarState.Unlocked;
		}
		else
			success = false;
	}

	void AllOrdersStar (int ordersPrepared, int trainsCount, LevelSettings_LD level, int levelIndex)
	{
		if(!PlayerPrefs.HasKey ("AllOrdersStar" + levelIndex) && LevelsManager.Instance.errorsLocked > LevelsManager.Instance.errorsSecondStarAllowed)
		{
			level.starsStates [1] = StarState.ErrorLocked;
			level.starsStates [2] = StarState.ErrorLocked;
			return;
		}

		if(ordersPrepared == level.ordersCount)
		{
			LeastTrainsStar (trainsCount, level, levelIndex);

			if (!PlayerPrefs.HasKey ("AllOrdersStar" + levelIndex))
			{
				level.starsEarned++;
				PlayerPrefs.SetInt ("AllOrdersStar" + levelIndex, 1);

				level.starsStates [1] = StarState.Unlocked;
			}
		}
	}

	void LeastTrainsStar (int trainsCount, LevelSettings_LD level, int levelIndex)
	{
		if (PlayerPrefs.HasKey ("LeastTrainsStar" + levelIndex))
			return;

		if (LevelsManager.Instance.errorsLocked > 0)
		{
			level.starsStates [2] = StarState.ErrorLocked;
			return;
		}

		if(trainsCount <= level.leastTrainsCount)
		{
			level.starsEarned++;
			PlayerPrefs.SetInt ("LeastTrainsStar" + levelIndex, 1);

			level.starsStates [2] = StarState.Unlocked;
		}
	}

	void OnApplicationQuit ()
	{
		if (saveOnStop)
		{
			SaveLevelStars ();
			PlayerPrefs.Save ();
		}
	}

	void OnApplicationFocus (bool hasFocus)
	{
		if (saveOnStop && !hasFocus)
		{
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
