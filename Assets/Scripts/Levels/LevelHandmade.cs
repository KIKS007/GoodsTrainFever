using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelHandmade : Level
{
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
