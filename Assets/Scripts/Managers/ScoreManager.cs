using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>
{
	[Header ("Stars")]
	public int starsEarned;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
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
}
