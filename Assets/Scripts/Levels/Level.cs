using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Level : MonoBehaviour 
{
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
}

[System.Serializable]
public class Level_Order
{
	public float delay;

	public List<Level_Container> levelContainers = new List<Level_Container> ();
}

[System.Serializable]
public class Level_Train
{
	public int trainDuration;
	public List<Level_Wagon> wagons;
}

[System.Serializable]
public class Level_Boat
{
	public bool overrideDuration = false;
	[ShowIfAttribute ("overrideDuration")]
	public float duration;

	public List<Level_Container> boatContainers = new List<Level_Container> ();
}

[System.Serializable]
public class Level_Container
{
	public ContainerType containerType;
	public ContainerColor containerColor = ContainerColor.Random;
	public bool isDoubleSize = false;
	public int containerWeight;
	public int containerCount = 1;
}

[System.Serializable]
public class Level_Wagon
{
	public WagonType wagonType = WagonType.Fourty;
	public int wagonMaxWeight;
}

