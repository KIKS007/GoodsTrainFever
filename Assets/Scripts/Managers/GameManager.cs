using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum GameState
{
	Menu,
	Playing,
	Pause,
	End

}

public enum LevelEndType
{
	Orders,
	Trains,
	Errors

}

public class GameManager : Singleton<GameManager>
{
	public Action OnMenu;
	public Action OnPlaying;
	public Action OnLevelEnd;

	[Header ("Game State")]
	public GameState gameState = GameState.Playing;

	// Use this for initialization
	void Awake ()
	{
		Application.targetFrameRate = -1;
	}

	public void StartLevel ()
	{
		gameState = GameState.Playing;

		if (OnPlaying != null)
			OnPlaying ();
	}

	public void EndLevel ()
	{
		gameState = GameState.End;

		if (OnLevelEnd != null)
			OnLevelEnd ();
	}

	public void LevelEndOrders ()
	{
		//	Debug.Log ("ORDERS LEVEL" + (LevelsManager.Instance.levelIndex + 1).ToString () + " END!");
	}

	public void LevelEndTrains ()
	{
		//	Debug.Log ("TRAINS LEVEL" + (LevelsManager.Instance.levelIndex + 1).ToString () + " END!");
	}

	public void LevelEndErrors ()
	{
		//	Debug.Log ("ERRORS LEVEL" + (LevelsManager.Instance.levelIndex + 1).ToString () + " END!");
	}

	public void LevelEnd (LevelEndType levelEndType)
	{
		if (gameState == GameState.Menu)
			return;

		gameState = GameState.Menu;

		switch (levelEndType) {
		case LevelEndType.Orders:
			LevelEndOrders ();
			break;
		case LevelEndType.Trains:
			LevelEndTrains ();
			break;
		case LevelEndType.Errors:
			LevelEndErrors ();
			break;
		}

		if (OnMenu != null)
			OnMenu ();
	}
}

