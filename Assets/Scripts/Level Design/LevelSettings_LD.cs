using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelSettings_LD : Level 
{
	[Header ("Duration")]
	public int levelDuration = 60;

	[Header ("Orders")]
	public int ordersCountMin = 1;
	public int ordersCountMax = 2;

	[Header ("Trains")]
	public int trainsCountMin = 1;
	public int trainsCountMax = 2;
	public List<Train_LD> trainsAvailable = new List<Train_LD> ();

	[Header ("Boats")]
	public int boatsDelay;

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