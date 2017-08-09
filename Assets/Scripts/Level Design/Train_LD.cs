using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train_LD : ScriptableObject
{
	public List<Wagon_Level> wagons;
	public List<Container_Level> forcedContainers = new List<Container_Level> ();
	public List<Container_Level> parasiteContainers = new List<Container_Level> ();
}
