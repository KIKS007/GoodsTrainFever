using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerated : MonoBehaviour 
{
	[Header ("Stars")]
	public int starsEarned = 0;
	public int mostOrdersCount = 0;
	public int leastTrainsCount = 1;

	[Header ("Errors")]
	public int errorsAllowed = 10;

	[Header ("Stars States")]
	public StarState[] starsStates = new StarState[3];

	[Header ("Orders")]
	public List<Order_Level> orders = new List<Order_Level> ();

	[Header ("Storage")]
	public List<Container> storageContainers = new List<Container> ();

	[Header ("Trains")]
	public int trainsDuration;
	public List<Train> rail1Trains = new List<Train> ();
	public List<Train> rail2Trains = new List<Train> ();

	[Header ("Boats")]
	public int boatsDelay;
	public int boatsDuration;
	public List<Boat> boats = new List<Boat> ();
}
