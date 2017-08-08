using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerated : Level 
{
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
