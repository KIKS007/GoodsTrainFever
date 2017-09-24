using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum StarState { Locked, Unlocked, Saved, ErrorLocked }

public class Level : MonoBehaviour 
{
	void DifficultyChanged ()
	{
		FindObjectOfType<LevelsManager> ().RenameLevels ();
	}

	[Range (1, 20)]
	[OnValueChanged ("DifficultyChanged")]
	public int difficulty = 1;

	[Header ("Stars")]
	public int starsEarned = 0;

	[Header ("Errors")]
	public int errorsAllowed = 10;

	[Header ("Stars States")]
	public StarState[] starsStates = new StarState[3];

	[Header ("Orders")]
	[ReadOnly]
	public int ordersCount;
}

[System.Serializable]
public class Order_Level
{
	[ReadOnly]
	public bool isPrepared = false;

	public float delay;
	public List<Container_Level> levelContainers = new List<Container_Level> ();

	public Order_Level ()
	{

	}

	public Order_Level (Order_Level original)
	{
		isPrepared = original.isPrepared;
		delay = original.delay;

		levelContainers.Clear ();

		foreach (var c in original.levelContainers)
			levelContainers.Add (new Container_Level (c));
	}
}

[System.Serializable]
public class Train_Level
{
	public int trainDuration;
	public List<Wagon_Level> wagons;
	public List<Container_Level> parasiteContainers = new List<Container_Level> ();
}

[System.Serializable]
public class Boat_Level
{
	public float delay = 0;

	public bool overrideDuration = false;
	[ShowIfAttribute ("overrideDuration")]
	public float duration;

	public List<Container_Level> boatContainers = new List<Container_Level> ();

	public Boat_Level ()
	{

	}

	public Boat_Level (Boat_Level original)
	{
		delay = original.delay;
		overrideDuration = original.overrideDuration;
		duration = original.duration;

		boatContainers.Clear ();

		foreach (var c in original.boatContainers)
			boatContainers.Add (new Container_Level (c));
	}
}

[System.Serializable]
public class Container_Level
{
	public ContainerType containerType;
	public ContainerColor containerColor;
	public bool isDoubleSize = false;

	public Container_Level ()
	{
		
	}

	public Container_Level (Container_Level original)
	{
		containerType = original.containerType;
		containerColor = original.containerColor;
		isDoubleSize = original.isDoubleSize;
	}

}

[System.Serializable]
public class Wagon_Level
{
	public WagonType wagonType = WagonType.Fourty;
	public int wagonMaxWeight = 10;
}

