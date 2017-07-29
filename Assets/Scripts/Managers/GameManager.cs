using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum GameState { Menu, Playing, Pause, End }

public class GameManager : Singleton<GameManager> 
{
	public Action OnMenu;
	public Action OnPlaying;

	[Header ("Game State")]
	public GameState gameState = GameState.Playing;

	// Use this for initialization
	void Awake () 
	{
		if (gameState == GameState.Playing)
		{
			MenuManager.Instance.gameObject.SetActive (false);
			//MenuManager.Instance.menuParent.gameObject.SetActive (false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartLevel ()
	{
		gameState = GameState.Playing;

		if (OnPlaying != null)
			OnPlaying ();
	}

	public void LevelEndOrders ()
	{
		if (gameState == GameState.Menu)
			return;

		Debug.Log ("ORDERS LEVEL" + (LevelsManager.Instance.levelIndex + 1).ToString () + " END!");

		LevelEnd ();
	}

	public void LevelEndTrains ()
	{
		if (gameState == GameState.Menu)
			return;

		Debug.Log ("TRAINS LEVEL" + (LevelsManager.Instance.levelIndex + 1).ToString () + " END!");

		LevelEnd ();
	}

	public void LevelEnd ()
	{
		gameState = GameState.Menu;

		if (OnMenu != null)
			OnMenu ();
	}
}
