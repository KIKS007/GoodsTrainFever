using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Train : Touchable 
{
	public static Action<Container> OnContainerAdded;
	public static Action<Container> OnContainerRemoved;

	[Header ("States")]
	public bool inTransition = false;
	public bool waitingDeparture = true;

	[Header ("Length")]
	public float trainLength;

	[Header ("Containers")]
	public Transform containersParent;
	public List<Container> containers;

	[Header ("Wagons")]
	public Transform wagonsParent;
	public List<Wagon> wagons = new List<Wagon> ();

	[Header ("Weight")]
	public int maxWeight = 0;
	public int currentWeight;

	private Vector3 _mouseDeltaPosition;
	private Vector3 _mousePosition;

	// Use this for initialization
	void Awake ()
	{
		wagons.AddRange (transform.GetComponentsInChildren<Wagon> ());
	}

	void Start ()
	{
		SetupContainersList ();

		foreach (var w in wagons)
			maxWeight += w.maxWeight;
	}

	void SetupContainersList ()
	{
		containers.Clear ();

		var spotsTemp = transform.GetComponentsInChildren<Spot> ().ToList ();
		spotsTemp = spotsTemp.OrderBy (x => Vector3.Distance (transform.position, x.transform.position)).ToList ();

		List<Spot> spots = new List<Spot> ();
		spots.AddRange (spotsTemp);

		//Sort Spots
		foreach(var s in spotsTemp)
		{
			if(s.isDoubleSize)
			{
				foreach (var o in s._overlappingSpots)
					spots.Remove (o);
			}

			else if(s.isPileSpot)
				spots.Remove (s);
		}

		//Setup Spots Events
		for(int i = 0; i < spots.Count; i++)
		{
			//New Slot
			AddContainerSlot (spots [i]);

			//First Slot
			AddSpotEvents (spots [i]);

			if(spots [i].isDoubleSize)
			{
				//First Overlapping 20
				AddSpotEvents (spots [i]._overlappingSpots [0]);

				//New Slot
				AddContainerSlot (spots [i]);

				//Fill The Second 40 Slot
				AddSpotEvents (spots [i]);

				//Second Overlapping 20
				AddSpotEvents (spots [i]._overlappingSpots [1]);
			}
		}

		/*for(int i = 0; i < spots.Count; i++)
		{
			containers.Add (null);

			int index = containerIndex;
			spots [i].OnSpotTaken += (arg) => containers [index] = arg;
			spots [i].OnSpotFreed += () => containers [index] = null;

			if(spots [i].isDoubleSize)
			{
				//The Two 20 Spots Overlapping the 40
				spots [i]._overlappingSpots [0].OnSpotTaken += (arg) => containers [index] = arg;
				spots [i]._overlappingSpots [0].OnSpotFreed += () => containers [index] = null;

				containers.Add (null);

				//Fill The Second 40 Slot
				spots [i].OnSpotTaken += (arg) => containers [index + 1] = arg;
				spots [i].OnSpotFreed += () => containers [index + 1] = null;


				spots [i]._overlappingSpots [1].OnSpotTaken += (arg) => containers [index + 1] = arg;
				spots [i]._overlappingSpots [1].OnSpotFreed += () => containers [index + 1] = null;

				containerIndex++;
			}

			if (spots [i].container)
				containers [containers.Count - 1] = spots [i].container;

			containerIndex++;
		}*/

		foreach (var w in wagons)
			w.UpdateWeight ();
	}

	void AddContainerSlot (Spot spot)
	{
		containers.Add (null);

		spot._wagon.containers.Add (null);
	}

	void AddSpotEvents (Spot spot)
	{
		int trainContainersIndex = containers.Count - 1;
		int wagonContainersIndex = spot._wagon.containers.Count - 1;

		//Update Train Containers
		spot.OnSpotTaken += (arg) => containers [trainContainersIndex] = arg;
		spot.OnSpotFreed += (arg) => containers [trainContainersIndex] = null;

		//Update Wagon Containers
		spot.OnSpotTaken += (arg) => spot._wagon.containers [wagonContainersIndex] = arg;
		spot.OnSpotFreed += (arg) => spot._wagon.containers [wagonContainersIndex] = null;

		//Update Weight
		spot.OnSpotTaken += (arg) => spot._wagon.UpdateWeight ();
		spot.OnSpotFreed += (arg) => spot._wagon.UpdateWeight ();

		if(spot.container)
		{
			containers [trainContainersIndex] = spot.container;
			spot._wagon.containers [wagonContainersIndex] = spot.container;
		}

		//Update Train Events
		spot.OnSpotTaken += (arg) => OnContainerAdded (arg);
		spot.OnSpotFreed += (arg) => OnContainerRemoved (arg);
	}

	public void UpdateWeight ()
	{
		currentWeight = 0;

		foreach (var w in wagons)
			currentWeight += w.currentWeight;
	}

	public override void OnTouchDown ()
	{
		base.OnTouchDown ();

		if (inTransition)
			return;

		TrainsMovementManager.Instance.selectedTrain = this;
	}
}
