using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;

public class StatsManager : Singleton<StatsManager>
{
	[Header ("Level Rating")]
	public bool RateLevels = false;
	public MenuComponent RatingMenu;
	public GameObject GO_RatingMenu;

	[Header ("Level Rating Passtrough")]
	public UnityEvent NextLevel;
	public UnityEvent MainMenu;


	[Header ("Level Analytic Systems")]
	public bool UseUnityAnalytics;
	public bool UseFenalytics;
	public bool UseGameAnalytics;
	public bool DebugLogDataSended;

	private bool BeforeRatingButton;



	private int TimerValue = 0;
	private int Trials = 0;
	private bool isTimerStopped = false;

	void Start ()
	{
		StartTrackingData ();
		OnMainMenu ();
	}

	public void StartTrackingData ()
	{
		GameManager.Instance.OnPlaying += () => {
			StartLevelTrack ();
		};
		GameManager.Instance.OnLevelEnd += () => {
			StopCoroutine ("Timer");
			isTimerStopped = true;
		};	

		MenuManager.Instance.OnMainMenu += () => {
			Fenalytics.To ("menu.main");
		};
		//This show up on level end screen
		/*GameManager.Instance.OnMenu += () => {
			Fenalytics.To ("menu.main");
		};*/

	}

	public void OnMainMenu ()
	{
		Fenalytics.To ("menu.main");
	}

	private void SendUnfinishedLevelData ()
	{
		int id;
		if (LevelsManager.Instance != null) {
			id = GetStringLevelID ();
		} else {
			id = -1;
		}


		Dictionary<string, object> UnfinishedLevelDataDictionnary = new Dictionary<string, object> {
			{ "ID",  id },
			{ "Name",  LevelsManager.Instance.currentLevel.name },
			{ "Total Stars",  ScoreManager.Instance.starsEarned },
			{ "Errors",  LevelsManager.Instance.errorsLocked },
			{ "Trials",  Trials },
			{ "Time",  TimerValue }
		};

		if (UseUnityAnalytics) {
			Analytics.CustomEvent ("UnfinishedLevelData-" + (id + 1), UnfinishedLevelDataDictionnary);	
		}
		if (UseFenalytics) {
			Fenalytics.Ev ("LevelEnd", UnfinishedLevelDataDictionnary);
		}
		if (UseGameAnalytics) {
			SendDataToGameAnalytic (id.ToString (), LevelsManager.Instance.currentLevel.name, "ERROR", "ERROR", ScoreManager.Instance.starsEarned.ToString (), LevelsManager.Instance.errorsLocked.ToString (), Trials.ToString (), TimerValue.ToString (), false, false);
		}
		if (DebugLogDataSended) {
			DebugLogData (id, UnfinishedLevelDataDictionnary);
		}
		Trials = 0;
	}

	private void SendLevelData ()
	{
		int id = GetStringLevelID ();

		Dictionary<string, object> LevelDataDictionnary = new Dictionary<string, object> {
			{ "ID",  id },
			{ "Name",  LevelsManager.Instance.currentLevel.name },
			{ "Level Stars",  LevelsManager.Instance.currentLevel.starsEarned },
			{ "Total Stars",  ScoreManager.Instance.starsEarned },
			{ "Errors",  LevelsManager.Instance.errorsLocked },
			{ "Trials",  Trials },
			{ "Time",  TimerValue }
		};

		if (UseUnityAnalytics) {
			Analytics.CustomEvent ("LevelData-" + (id + 1), LevelDataDictionnary);	
		}
		if (UseFenalytics) {
			Fenalytics.Ev ("LevelEnd", LevelDataDictionnary);
		}
		if (UseGameAnalytics) {
			SendDataToGameAnalytic (id.ToString (), LevelsManager.Instance.currentLevel.name, "ERROR", "ERROR", LevelsManager.Instance.currentLevel.starsEarned.ToString (), LevelsManager.Instance.errorsLocked.ToString (), Trials.ToString (), TimerValue.ToString (), false, true);
		}
		if (DebugLogDataSended) {
			DebugLogData (id, LevelDataDictionnary);
		}
		Trials = 0;
	}

	public void SendRatedLevelData (int rate, int diffRate)
	{
		//Debug.Log ("RATING SENDED" + rate + " " + diffRate);
		int id = GetStringLevelID ();

		Dictionary<string, object> RatedLevelDataDictionnary = new Dictionary<string, object> {
			{ "ID",  id },
			{ "Name",  LevelsManager.Instance.currentLevel.name },
			{ "Rate",  rate },
			{ "Difficulty Rate",  diffRate },
			{ "Level Stars", LevelsManager.Instance.currentLevel.starsEarned },
			{ "Total Stars",  ScoreManager.Instance.starsEarned },
			{ "Errors",  LevelsManager.Instance.errorsLocked },
			{ "Trials",  Trials },
			{ "Time",  TimerValue }
		};

		if (UseUnityAnalytics) {
			Analytics.CustomEvent ("Rated-LevelData-" + (id + 1), RatedLevelDataDictionnary);
		}
		if (UseFenalytics) {
			Fenalytics.Ev ("LevelEnd", RatedLevelDataDictionnary);
		}

		if (UseGameAnalytics) {
			SendDataToGameAnalytic (id.ToString (), LevelsManager.Instance.currentLevel.name, rate.ToString (), diffRate.ToString (), LevelsManager.Instance.currentLevel.starsEarned.ToString (), LevelsManager.Instance.errorsLocked.ToString (), Trials.ToString (), TimerValue.ToString (), true, true);
		}
		if (DebugLogDataSended) {
			DebugLogData (id, RatedLevelDataDictionnary);
		}
		Trials = 0;
	}


	private void SendDataToGameAnalytic (string id, string name, string rate, string diffRate, string score, string errors, string trials, string time, bool Rated, bool Finished)
	{

//		if (Rated) {
//			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":ID:" + id);
//			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Rate:" + rate);
//			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":DifficultyRate:" + diffRate);
//			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Score:" + score);
//			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Errors:" + errors);
//			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Trials:" + trials);
//			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Time:" + time);
//		} else {
//			if (Finished) {
//				GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":ID:" + id);
//				GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":Score:" + score);
//				GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":Errors:" + errors);
//				GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":Trials:" + trials);
//				GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":Time:" + time);
//			} else {
//				GameAnalytics.NewDesignEvent ("Levels:UnfinishedNotRated:" + id + ":ID:" + id);
//				GameAnalytics.NewDesignEvent ("Levels:UnfinishedNotRated:" + id + ":Errors:" + errors);
//				GameAnalytics.NewDesignEvent ("Levels:UnfinishedNotRated:" + id + ":Trials:" + trials);
//				GameAnalytics.NewDesignEvent ("Levels:UnfinishedNotRated:" + id + ":Time:" + time);
//			}
//
//		}

	}

	private void DebugLogData (int id, Dictionary<string,object> dic)
	{
		Debug.Log ("------------ START " + id + "-----------");
		foreach (KeyValuePair<string,object> data in dic) {
			Debug.Log ("DATA: " + data.Key + " | " + data.Value);
		}
		Debug.Log ("------------ END   " + id + "-----------");
	}

	private void DebugLogSimple (string name, string data)
	{
		Debug.Log ("------------ START " + name + "-----------");
		Debug.Log ("DATA: " + data);
		Debug.Log ("------------ END   " + name + "-----------");
	}

	public void StartLevelTrack ()
	{
		if (Trials == 0) {
			IncTrials ();
			if (UseFenalytics) {
				Fenalytics.To ("level." + GetStringLevelID ().ToString ());
			}
			TimerValue = 0;
			isTimerStopped = false;
			StartCoroutine ("Timer");
		} else {
			if (isTimerStopped) {
				isTimerStopped = false;
				StartCoroutine ("Timer");
			}
		}
	}

	public int GetStringLevelID ()
	{
		return LevelsManager.Instance.currentLevel.transform.GetSiblingIndex () + 1;
	//	return int.Parse (LevelsManager.Instance.currentLevel.name.Split ('#') [LevelsManager.Instance.currentLevel.name.Split ('#').Length - 1]);
	}

	public void StopLevelTrack (bool Completed)
	{
		StopCoroutine ("Timer");
		if (RateLevels) {
			if (Completed) {
				GO_RatingMenu.SetActive (true);
				MenuManager.Instance.HideEndLevel ();
				MenuManager.Instance.PauseAndShowMenu (RatingMenu);
			}
		} else {
			if (Completed) {
				SendLevelData ();
			} else {
				SendUnfinishedLevelData ();
			}
		}
	}

	public void IncTrials ()
	{
		Trials++;
	}

	public void ButtonPasstrough (bool type)
	{
		if (RateLevels) {
			BeforeRatingButton = type;
		} else {
			if (type) {
				NextLevel.Invoke ();
			} else {
				MainMenu.Invoke ();
			}
		}
	}

	public void EndRatingPasstrough ()
	{
		if (BeforeRatingButton) {
			NextLevel.Invoke ();
		} else {
			MainMenu.Invoke ();
		}
	}

	public void SetRatingStatus (bool status)
	{
		RateLevels = status;
		if (UseFenalytics) {
			Fenalytics.Ev ("RatingLevelStatus", status);
		}

		if (UseGameAnalytics) {
//			GameAnalytics.NewDesignEvent ("Other:Device:SetRatingStatus:True");
		}
	}

	public void ResetAll ()
	{
		if (UseFenalytics) {
			Fenalytics.Ev ("ResetPlayerPrefs", true);
		}

		if (UseGameAnalytics) {
//			GameAnalytics.NewDesignEvent ("Other:Device:ResetAll:True");
		}
	}

	public void UnlockLevels ()
	{
		if (UseFenalytics) {
			Fenalytics.Ev ("UnlockAllLevels", true);
		}

		if (UseGameAnalytics) {
//			GameAnalytics.NewDesignEvent ("Other:Device:UnlockLevels:True");
		}
	}

	public void ResetStars ()
	{
		if (UseFenalytics) {
			Fenalytics.Ev ("ResetStars", true);
		}

		if (UseGameAnalytics) {
//			GameAnalytics.NewDesignEvent ("Other:Device:ResetStars:True");
		}
	}

	public void ResetAutoScaling ()
	{
		if (UseFenalytics) {
			Fenalytics.Ev ("ResetAutoScaling", true);
		}

		if (UseGameAnalytics) {
//			GameAnalytics.NewDesignEvent ("Other:Device:ResetAutoScaling:True");
		}
	}

	public void SendOptiAnalytics (int DefaultDeviceHeight, int FramerateDeviceHeight, float RatioDeviceHeight)
	{
		if (UseUnityAnalytics) {
			Analytics.CustomEvent ("OptiSettings", new Vector3 (DefaultDeviceHeight, FramerateDeviceHeight, RatioDeviceHeight));
		}

		if (UseFenalytics) {
			Fenalytics.Ev ("OptiRatio", RatioDeviceHeight);
		}

		if (UseGameAnalytics) {
//			GameAnalytics.NewDesignEvent ("Other:Device:DefaultDeviceHeight:" + DefaultDeviceHeight);
//			GameAnalytics.NewDesignEvent ("Other:Device:FramerateDeviceHeight:" + FramerateDeviceHeight);
//			GameAnalytics.NewDesignEvent ("Other:Device:RatioDeviceHeight:" + RatioDeviceHeight);
		}
		if (DebugLogDataSended) {
			DebugLogSimple ("DefaultDeviceHeight", DefaultDeviceHeight.ToString ());
			DebugLogSimple ("FramerateDeviceHeight", FramerateDeviceHeight.ToString ());
			DebugLogSimple ("RatioDeviceHeight", RatioDeviceHeight.ToString ());
		}

	}

	IEnumerator Timer ()
	{
		yield return new WaitForSecondsRealtime (1);
		TimerValue++;
		if (!isTimerStopped)
			StartCoroutine ("Timer");
	}
		
		
}
	