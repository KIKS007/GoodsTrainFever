using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelSettings_LD : MonoBehaviour 
{
	[Header ("Duration")]
	public int levelDuration = 60;

	[Header ("Stars")]
	public int starsEarned = 0;
	public int mostOrdersCount = 0;
	public int leastTrainsCount = 1;

	[Header ("Errors")]
	public int errorsAllowed = 10;

	[Header ("Stars States")]
	public StarState[] starsStates = new StarState[3];

	[Header ("Orders")]
	public int ordersCountMin = 1;
	public int ordersCountMax = 2;

	[Header ("Trains")]
	public int trainsCountMin = 1;
	public int trainsCountMax = 2;
	public List<Train_LD> trainsAvailable = new List<Train_LD> ();

	[Header ("Containers")]
	public List<Container_Level> containersAvailable = new List<Container_Level> ();

	[Header ("Extra Containers")]
	public int extraContainersCount = 5;

	[Header ("Orders Filling Percentage")]
	[Range (0, 100)]
	public int ordersFillingPercentage = 70;

	[Header ("Storage Filling Percentage")]
	[Range (0, 100)]
	public int storageFillingPercentage = 100;

}

[System.Serializable]
public class Container_LD
{
	public ContainerType containerType;
	public bool isDoubleSize = false;
	
	public Container_LD ()
	{
		
	}
	
	public Container_LD (Container_LD original)
	{
		containerType = original.containerType;
		isDoubleSize = original.isDoubleSize;
	}
	
}