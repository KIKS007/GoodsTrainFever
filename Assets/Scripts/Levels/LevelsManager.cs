using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelsManager : Singleton<LevelsManager> 
{
	public Transform levelToStart;

	[Header ("Level")]
	public int levelIndex;
	public Transform level;

	[Header ("Orders")]
	public List<Level_Order> orders = new List<Level_Order> ();

	[Header ("Storage")]
	public List<Level_Container> storageContainers = new List<Level_Container> ();

	[Header ("Trains")]
	public List<Level_Train> rail1Trains = new List<Level_Train> ();
	public List<Level_Train> rail2Trains = new List<Level_Train> ();

	[Header ("Boats")]
	public float boatsDuration;
	public List<Level_Boat> boats = new List<Level_Boat> ();

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void LoadLevel (Transform level)
	{
		
	}

	public void StartLevel (int index)
	{
		levelIndex = index;
		levelToStart = transform.GetChild (levelIndex);

		LoadLevel (levelToStart);
	}

	public void StartLevel (Transform l)
	{
		level = l;
		FindLevelIndex ();

		LoadLevel (levelToStart);
	}

	[PropertyOrder (-1)]
	[ButtonGroup ("1")]
	public void StartLevelTest ()
	{
		LoadLevel (levelToStart);
	}

	[PropertyOrder (-1)]
	[ButtonGroup ("1")]
	public void NextLevel ()
	{
		if (levelIndex + 1 >= transform.childCount - 1)
			return;

		StartLevel (levelIndex + 1);
	}

	[PropertyOrder (-1)]
	[ButtonAttribute]
	void RenameLevels ()
	{
		for (int i = 0; i < transform.childCount; i++)
			transform.GetChild (i).name = "Level #" + i;
	}

	void FindLevelIndex ()
	{
		for(int i = 0; i < transform.childCount; i++)
			if(transform.GetChild (i) == level)
			{
				levelIndex = i;
				return;
			}
	}
}
