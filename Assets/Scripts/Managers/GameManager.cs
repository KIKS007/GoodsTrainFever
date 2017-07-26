using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Menu, Playing, End }

public class GameManager : Singleton<GameManager> 
{
	[Header ("Game State")]
	public GameState gameState = GameState.Playing;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LevelEndOrders ()
	{
		if (gameState == GameState.End)
			return;

		Debug.Log ("ORDERS LEVEL" + LevelsManager.Instance.levelIndex + " END!");

		LevelEnd ();
	}

	public void LevelEndTrains ()
	{
		if (gameState == GameState.End)
			return;

		Debug.Log ("TRAINS LEVEL" + LevelsManager.Instance.levelIndex + " END!");

		LevelEnd ();
	}

	public void LevelEnd ()
	{
		gameState = GameState.End;
	}
}
