using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LevelSettings_LD : MonoBehaviour 
{
	[Header ("Duration")]
	public int levelDuration = 60;

	[Header ("Stars")]
	public int mostOrdersCount = 0;
	public int leastTrainsCount = 1;

	[Header ("Errors")]
	public int errorsAllowed = 10;

	[Header ("Orders")]
	public int ordersCountMin = 1;
	public int ordersCountMax = 2;

	[Header ("Trains")]
	public int trainsCountMin = 1;
	public int trainsCountMax = 2;
	public List<Train_LD> trainsAvailable = new List<Train_LD> ();

	[Header ("Containers")]
	public List<Container_Level> containersAvailable = new List<Container_Level> ();

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