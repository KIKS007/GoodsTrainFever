using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Menu, Game }

public class GameManager : Singleton<GameManager> 
{
	[Header ("Game State")]
	public GameState gameState = GameState.Game;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LevelEndOrders ()
	{
		LevelEnd ();
	}

	public void LevelEndTrains ()
	{
		LevelEnd ();
	}

	public void LevelEnd ()
	{
		Debug.Log ("LEVEL" + LevelsManager.Instance.levelIndex + " END!");
	}
}
