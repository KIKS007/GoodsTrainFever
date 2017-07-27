using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum StarState { Locked, Unlocked, Saved }

public class Level : MonoBehaviour 
{
	[Header ("Stars")]
	public int starsEarned = 0;
	public int mostOrdersCount = 0;
	public int leastTrainsCount = 0;

	[Header ("Stars States")]
	public StarState[] starsStates = new StarState[3];

	[Header ("Orders")]
	public List<Order_Level> orders = new List<Order_Level> ();

	[Header ("Storage")]
	public bool spawnAllOrderContainers = true;
	public List<Container_Level> storageContainers = new List<Container_Level> ();

	[Header ("Trains")]
	public List<Train_Level> rail1Trains = new List<Train_Level> ();
	public List<Train_Level> rail2Trains = new List<Train_Level> ();

	[Header ("Boats")]
	public float boatsDuration;
	public bool lastBoatStay = true;
	public List<Boat_Level> boats = new List<Boat_Level> ();
}

[System.Serializable]
public class Order_Level
{
	[ReadOnly]
	public bool isPrepared = false;

	public float delay;
	public List<Container_Level> levelContainers = new List<Container_Level> ();
}

[System.Serializable]
public class Train_Level
{
	public int trainDuration;
	public List<Wagon_Level> wagons;
}

[System.Serializable]
public class Boat_Level
{
	public float delay = 0;

	public bool overrideDuration = false;
	[ShowIfAttribute ("overrideDuration")]
	public float duration;

	public List<Container_Level> boatContainers = new List<Container_Level> ();
}

[System.Serializable]
public class Container_Level
{
	public ContainerType containerType;
	public ContainerColor containerColor;
	public bool isDoubleSize = false;
	public int containerWeight;
	public int containerCount = 1;
}

[System.Serializable]
public class Wagon_Level
{
	public WagonType wagonType = WagonType.Fourty;
	public int wagonMaxWeight;
}

