using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Train : Touchable 
{
	[Header ("States")]
	public bool inTransition = false;

	[Header ("Containers")]
	public Transform containersParent;
	public List<Container> allContainers;

	[Header ("Wagons")]
	public Transform wagonsParent;
	public List<Wagon> wagons = new List<Wagon> ();


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
	}

	void SetupContainersList ()
	{
		allContainers.Clear ();

		var spotsTemp = transform.GetComponentsInChildren<Spot> ().ToList ();
		spotsTemp = spotsTemp.OrderBy (x => Vector3.Distance (transform.position, x.transform.position)).ToList ();

		List<Spot> spots = new List<Spot> ();
		spots.AddRange (spotsTemp);

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

		int containerIndex = 0;

		for(int i = 0; i < spots.Count; i++)
		{
			allContainers.Add (null);

			int index = containerIndex;
			spots [i].OnSpotTaken += (arg) => allContainers [index] = arg;
			spots [i].OnSpotFreed += () => allContainers [index] = null;

			if(spots [i].isDoubleSize)
			{
				//Fill The Second Slot
				spots [i].OnSpotTaken += (arg) => allContainers [index + 1] = arg;
				spots [i].OnSpotFreed += () => allContainers [index + 1] = null;

				//The Two 20 Spots Overlapping the 40
				allContainers.Add (null);

				spots [i]._overlappingSpots [0].OnSpotTaken += (arg) => allContainers [index] = arg;
				spots [i]._overlappingSpots [0].OnSpotFreed += () => allContainers [index] = null;

				spots [i]._overlappingSpots [1].OnSpotTaken += (arg) => allContainers [index + 1] = arg;
				spots [i]._overlappingSpots [1].OnSpotFreed += () => allContainers [index + 1] = null;

				containerIndex++;
			}

			if (spots [i].container)
				allContainers [allContainers.Count - 1] = spots [i].container;
			
			containerIndex++;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public override void OnTouchDown ()
	{
		base.OnTouchDown ();

		if (inTransition)
			return;

		TrainsMovementManager.Instance.selectedTrain = this;
	}
}
