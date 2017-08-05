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
	[HideInInspector]
	public List<Spot> _allSpots = new List<Spot> ();

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
		_allSpots.Clear ();

		_allSpots = transform.GetComponentsInChildren<Spot> ().ToList ();
		_allSpots = _allSpots.OrderBy (x => Vector3.Distance (transform.position, x.transform.position)).ToList ();

		List<Spot> spots = new List<Spot> ();
		spots.AddRange (_allSpots);

		//Sort Spots
		foreach(var s in _allSpots)
		{
			if(s.isDoubleSize)
			{
				foreach (var o in s.overlappingSpots)
					spots.Remove (o);
			}

			else if(s.isPileSpot)
				spots.Remove (s);
		}

		int spotIndex = 0;

		//Setup Spots Events
		for(int i = 0; i < spots.Count; i++)
		{
			//New Slot
			AddContainerSlot (spots [i]);

			//First Slot
			AddSpotEvents (spots [i]);

			spots [i]._spotTrainIndex = spotIndex;

			if(spots [i].isDoubleSize)
			{
				//First Overlapping 20
				AddSpotEvents (spots [i].overlappingSpots [0]);

				//New Slot
				AddContainerSlot (spots [i]);

				spots [i].overlappingSpots [0]._spotTrainIndex = spotIndex;

				spotIndex++;

				//Fill The Second 40 Slot
				AddSpotEvents (spots [i], true);

				//Second Overlapping 20
				AddSpotEvents (spots [i].overlappingSpots [1]);

				spots [i].overlappingSpots [1]._spotTrainIndex = spotIndex;
			}

			spotIndex++;
		}

		foreach (var w in wagons)
			w.UpdateWeight ();
	}

	void AddContainerSlot (Spot spot)
	{
		containers.Add (null);

		spot._wagon.containers.Add (null);
	}

	void AddSpotEvents (Spot spot, bool secondDoubleSize = false)
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
		if(!secondDoubleSize)
		{
			spot.OnSpotTaken += (arg) => spot._wagon.UpdateWeight ();
			spot.OnSpotFreed += (arg) => spot._wagon.UpdateWeight ();
		}

		if(spot.container)
		{
			containers [trainContainersIndex] = spot.container;
			spot._wagon.containers [wagonContainersIndex] = spot.container;
		}

		//Update Train Events
		if(!secondDoubleSize)
		{	
			spot.OnSpotTaken += (arg) => OnContainerAdded (arg);
			spot.OnSpotFreed += (arg) => OnContainerRemoved (arg);
		}
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
