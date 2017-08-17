using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using GameAnalyticsSDK;

public class StatsManager : Singleton<StatsManager>
{
	[Header ("Level Rating")]
	public bool RateLevels = false;
	public MenuComponent RatingMenu;

	[Header ("Level Analytic Systems")]
	public bool UseUnityAnalytics;
	public bool UseGameAnalytics;

	private int TimerValue = 0;
	private int Trials = 0;
	private bool isTimerStopped = false;

	void Start ()
	{
		StartTrackingData ();
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
			
	}


	private void SendLevelData ()
	{
		int id = LevelsManager.Instance.currentLevel.transform.GetSiblingIndex ();

		Dictionary<string, object> LevelDataDictionnary = new Dictionary<string, object> {
			{ "ID",  id },
			{ "Name",  LevelsManager.Instance.currentLevel.name },
			{ "Final Score",  ScoreManager.Instance.starsEarned },
			{ "Errors",  LevelsManager.Instance.errorsLocked },
			{ "Trials",  Trials },
			{ "Time",  TimerValue }
		};

		if (UseUnityAnalytics) {
			Analytics.CustomEvent ("LevelData-" + (id + 1), LevelDataDictionnary);	
		}
		if (UseGameAnalytics) {
			SendDataToGameAnalytic (id.ToString (), LevelsManager.Instance.currentLevel.name, "ERROR", "ERROR", ScoreManager.Instance.starsEarned.ToString (), LevelsManager.Instance.errorsLocked.ToString (), Trials.ToString (), TimerValue.ToString (), false);
		}
		Trials = 0;
	}

	public void SendRatedLevelData (int rate, int diffRate)
	{
		Debug.Log (rate + " " + diffRate);
		int id = LevelsManager.Instance.currentLevel.transform.GetSiblingIndex ();

		Dictionary<string, object> RatedLevelDataDictionnary = new Dictionary<string, object> {
			{ "ID",  id },
			{ "Name",  LevelsManager.Instance.currentLevel.name },
			{ "Rate",  rate },
			{ "Difficulty Rate",  diffRate },
			{ "Final Score",  ScoreManager.Instance.starsEarned },
			{ "Errors",  LevelsManager.Instance.errorsLocked },
			{ "Trials",  Trials },
			{ "Time",  TimerValue }
		};

		if (UseUnityAnalytics) {
			Analytics.CustomEvent ("Rated-LevelData-" + (id + 1), RatedLevelDataDictionnary);
		}

		if (UseGameAnalytics) {
			SendDataToGameAnalytic (id.ToString (), LevelsManager.Instance.currentLevel.name, rate.ToString (), diffRate.ToString (), ScoreManager.Instance.starsEarned.ToString (), LevelsManager.Instance.errorsLocked.ToString (), Trials.ToString (), TimerValue.ToString (), true);
		}
		Trials = 0;
	}


	private void SendDataToGameAnalytic (string id, string name, string rate, string diffRate, string score, string errors, string trials, string time, bool Rated)
	{

		if (Rated) {
			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":ID:" + id);
			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Rate:" + rate);
			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":DifficultyRate:" + diffRate);
			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Score:" + score);
			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Errors:" + errors);
			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Trials:" + trials);
			GameAnalytics.NewDesignEvent ("Levels:Rated:" + id + ":Time:" + time);
		} else {
			GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":ID:" + id);
			GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":Score:" + score);
			GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":Errors:" + errors);
			GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":Trials:" + trials);
			GameAnalytics.NewDesignEvent ("Levels:NotRated:" + id + ":Time:" + time);
		}

	}

	public void StartLevelTrack ()
	{
		if (Trials == 0) {
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


	public void StopLevelTrack ()
	{
		StopCoroutine ("Timer");
		if (RateLevels) {
			MenuManager.Instance.PauseAndShowMenu (RatingMenu);
		} else {
			SendLevelData ();
		}
	}

	public void IncTrials ()
	{
		Trials++;
	}


	IEnumerator Timer ()
	{
		yield return StartCoroutine (CoroutineUtil.WaitForRealSeconds (1));
		TimerValue++;
		StartCoroutine ("Timer");
	}
		
		
}

public static class CoroutineUtil
{
	public static IEnumerator WaitForRealSeconds (float time)
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time) {
			yield return null;
		}
	}
}