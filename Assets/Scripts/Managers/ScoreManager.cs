using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ScoreManager : Singleton<ScoreManager>
{
	[Header ("Stars")]
	public int starsEarned;

	[Header ("Settings")]
	public bool clearOnStart = false;
	public bool loadOnStart = true;
	public bool saveOnStop = true;

	// Use this for initialization
	void Start ()
	{
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
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoadLevelStars ()
	{
		for(int i = 0; i < LevelsManager.Instance.transform.childCount; i++)
		{
			if (PlayerPrefs.HasKey ("Stars" + i))
				LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ().starsEarned = PlayerPrefs.GetInt ("Stars" + i);	
		}

		UpdateStars ();
	}

	public void SaveLevelStars ()
	{
		for(int i = 0; i < LevelsManager.Instance.transform.childCount; i++)
		{
			PlayerPrefs.SetInt ("Stars" + i, LevelsManager.Instance.transform.GetChild (i).GetComponent<Level> ().starsEarned);	
		}
	}

	public void UpdateStars ()
	{
		starsEarned = 0;

		foreach(Transform t in LevelsManager.Instance.transform)
		{
			Level level = t.GetComponent<Level> ();

			starsEarned += level.starsEarned;
		}
	}

	public void UnlockStars (int ordersPrepared, int trainsCount, int levelIndex)
	{
		Level level = LevelsManager.Instance.transform.GetChild (levelIndex).GetComponent<Level> ();

		MostOrdersStar (ordersPrepared, level, levelIndex);
		AllOrdersStar (ordersPrepared, trainsCount, level, levelIndex);

		UpdateStars ();

		Debug.Log ("LEVEL#" + levelIndex + " - Stars: " + level.starsEarned);
	}

	void MostOrdersStar (int ordersPrepared, Level level, int levelIndex)
	{
		if (PlayerPrefs.HasKey ("MostOrdersStar" + levelIndex))
			return;

		if(ordersPrepared > level.mostOrdersCount)
		{
			level.starsEarned++;
			PlayerPrefs.SetInt ("MostOrdersStar" + levelIndex, 1);
		}
	}

	void AllOrdersStar (int ordersPrepared, int trainsCount, Level level, int levelIndex)
	{
		if(ordersPrepared == level.orders.Count)
		{
			LeastTrainsStar (trainsCount, level, levelIndex);

			if (!PlayerPrefs.HasKey ("AllOrdersStar" + levelIndex))
			{
				level.starsEarned++;
				PlayerPrefs.SetInt ("AllOrdersStar" + levelIndex, 1);
			}
		}
	}

	void LeastTrainsStar (int trainsCount, Level level, int levelIndex)
	{
		if (PlayerPrefs.HasKey ("LeastTrainsStar" + levelIndex))
			return;

		if(trainsCount <= level.leastTrainsCount)
		{
			level.starsEarned++;
			PlayerPrefs.SetInt ("LeastTrainsStar" + levelIndex, 1);
		}
	}

	void OnApplicationQuit ()
	{
		if (saveOnStop)
			SaveLevelStars ();
	}

	void OnApplicationFocus (bool hasFocus)
	{
		if (saveOnStop && !hasFocus)
			SaveLevelStars ();
	}
}
