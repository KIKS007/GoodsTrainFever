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
	public List<Order_Level> orders = new List<Order_Level> ();

	[Header ("Storage")]
	public List<Container_Level> storageContainers = new List<Container_Level> ();

	[Header ("Trains")]
	public List<Train_Level> rail1Trains = new List<Train_Level> ();
	public List<Train_Level> rail2Trains = new List<Train_Level> ();

	[Header ("Boats")]
	public float boatsDuration;
	public List<Boat_Level> boats = new List<Boat_Level> ();

	// Use this for initialization
	void Start () 
	{
		
	}

	public void LoadLevel (Transform l)
	{
		if(l == null)
		{
			Debug.LogError ("Invalid Level!");
			return;
		}

		Level level = l.GetComponent<Level> ();

		orders = level.orders;
		storageContainers = level.storageContainers;
		rail1Trains = level.rail1Trains;
		rail2Trains = level.rail2Trains;
		boatsDuration = level.boatsDuration;
		boats = level.boats;
	}

	IEnumerator AddOrder (Order_Level order)
	{
		yield return new WaitForSecondsRealtime (order.delay);
	}

	#region Level Start
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

	[ButtonGroup ("1", -1)]
	public void StartLevelTest ()
	{
		if (levelToStart == null)
			return;

		LoadLevel (levelToStart);
	}

	[ButtonGroup ("1", -1)]
	public void NextLevel ()
	{
		if (levelIndex + 1 >= transform.childCount - 1)
			return;

		StartLevel (levelIndex + 1);
	}
	#endregion

	#region Other
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
	#endregion
}
