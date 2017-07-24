using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

public class LevelsManager : Singleton<LevelsManager> 
{
	public Transform levelToStart;

	[Header ("Level")]
	public int levelIndex;
	public Transform level;

	[Header ("Orders")]
	public bool randomColors = false;
	public List<Order_Level> orders = new List<Order_Level> ();

	[Header ("Storage")]
	public bool spawnDoubleSizeFirst = false;
	public bool spawnAllOrderContainers = true;
	public List<Container_Level> storageContainers = new List<Container_Level> ();

	[Header ("Trains")]
	public float waitDurationBetweenTrains = 2f;
	public List<Train_Level> rail1Trains = new List<Train_Level> ();
	public List<Train_Level> rail2Trains = new List<Train_Level> ();

	[Header ("Boats")]
	public float boatsDuration;
	public List<Boat_Level> boats = new List<Boat_Level> ();

	[Header ("Containers Prefabs")]
	public GameObject[] basicContainersPrefabs = new GameObject[2];
	public GameObject[] cooledContainersPrefabs = new GameObject[2];
	public GameObject[] tankContainersPrefabs = new GameObject[2];
	public GameObject[] dangerousContainersPrefabs = new GameObject[2];

	private Storage _storage;
	private Boat _boat;
	private int _randomColorOffset;

	//public	List<Spot> spots = new List<Spot> ();
	//public List<Spot> spotsTemp = new List<Spot> ();

	// Use this for initialization
	void Start () 
	{
		_storage = FindObjectOfType<Storage> ();
		_boat = FindObjectOfType<Boat> ();
	}

	void ClearLevelSettings ()
	{
		orders.Clear ();
		storageContainers.Clear ();
		rail1Trains = null;
		rail2Trains = null;
		boatsDuration = 0;
		boats.Clear ();

		TrainsMovementManager.Instance.ClearTrains ();

		OrdersManager.Instance.ClearOrders (false);
	}

	public void LoadLevelSettings (Transform l)
	{
		if(l == null)
		{
			Debug.LogError ("Invalid Level!");
			return;
		}

		ClearLevelSettings ();

		if (randomColors)
			_randomColorOffset = Random.Range (0, 4);
		else
			_randomColorOffset = 0;
		
		Level level = l.GetComponent<Level> ();

		orders.AddRange (level.orders);
		spawnAllOrderContainers = level.spawnAllOrderContainers;
		storageContainers.AddRange (level.storageContainers);
		rail1Trains = level.rail1Trains;
		rail2Trains = level.rail2Trains;
		boatsDuration = level.boatsDuration;
		boats.AddRange (level.boats);

		foreach(var o in orders)
			RandomColors (o.levelContainers);

		if (storageContainers.Count != 0)
			RandomColors (storageContainers);

		foreach (var b in boats)
			RandomColors (b.boatContainers);

		//Storage
		List<Container_Level> containers = new List<Container_Level> ();

		//Get Containers To Spawn
		if (spawnAllOrderContainers)
			foreach (var o in orders)
				containers.AddRange (o.levelContainers);
		else
			containers = storageContainers;
		
		StartCoroutine (FillContainerZone (containers, _storage.transform, _storage.containersParent));

		//Orders
		foreach (var o in orders)
			StartCoroutine (AddOrder (o));

		//Trains
		if (rail1Trains.Count > 0)
			StartCoroutine (SpawnTrain (rail1Trains, TrainsMovementManager.Instance.rail1));

		if (rail2Trains.Count > 0)
			StartCoroutine (SpawnTrain (rail2Trains, TrainsMovementManager.Instance.rail2));
	}

	void RandomColors (List<Container_Level> containers)
	{
		Debug.Log ("randomOffset : " + _randomColorOffset);

		foreach(var c in containers)
		{
			int color = (int)c.containerColor;

			for(int i = 0; i < _randomColorOffset; i++)
			{
				color++;

				if(color == 4)
					color = 0;
			}

			c.containerColor = (ContainerColor)color;
		}
	}

	IEnumerator AddOrder (Order_Level order)
	{
		yield return new WaitForSecondsRealtime (order.delay);

		OrdersManager.Instance.AddOrder (order);
	}

	IEnumerator SpawnTrain (List<Train_Level> train_Level, Rail rail)
	{
		foreach(var t in train_Level)
		{
			Train train = TrainsMovementManager.Instance.SpawnTrain (rail, t);

			yield return new WaitWhile (()=> train.inTransition);

			yield return new WaitWhile (()=> train.waitingDeparture);

			yield return new WaitUntil (()=> train == null);

			yield return new WaitForSecondsRealtime (waitDurationBetweenTrains);
		}
	}

	void EmptyZone (Transform parent)
	{
		foreach (Transform c in parent)
		{
			var container = c.GetComponent<Container> ();

			if (container != null)
				container.RemoveContainer ();
			
			Destroy (c.gameObject);
		}
	}

	[Button]
	void FillStorageZonetest ()
	{
		StartCoroutine (FillContainerZone (storageContainers, _storage.transform, _storage.containersParent));
	}

	IEnumerator FillContainerZone (List<Container_Level> containers_Base, Transform zoneParent, Transform containersParent)
	{
		EmptyZone (containersParent);

		yield return new WaitForEndOfFrame ();

		//Sort Containers_Levels
		List<Container_Level> containers_Levels = new List<Container_Level> ();

		if(spawnDoubleSizeFirst)
		{
			foreach (var c in containers_Base)
				if (c.isDoubleSize)
					containers_Levels.Add (c);

			foreach (var c in containers_Base)
				if (!c.isDoubleSize)
					containers_Levels.Add (c);
		}
		else
			containers_Levels.AddRange (containers_Base);


		List<Spot> spots = new List<Spot> ();
		List<Spot> spotsTemp = new List<Spot> ();

		var spotsArray = zoneParent.GetComponentsInChildren<Spot> ().ToList ();

		//Get & Sort Spots
		foreach (var s in spotsArray)
			if (!s.isPileSpot && !s._isSpawned)
				spots.Add (s);

		//Spawn Containers & Assign Spot
		foreach(var containterLevel in containers_Levels)
		{
			if (containterLevel.containerCount == 0)
				containterLevel.containerCount = 1;

			for(int i = 0; i < containterLevel.containerCount; i++)
			{
				Container container = CreateContainer (containterLevel, containersParent);
				
				spotsTemp.Clear ();
				spotsTemp.AddRange (spots);
				
				//Add Spawned Spots
				if(containterLevel.isDoubleSize)
				{
					foreach(var s in spots)
					{
						if(s.isDoubleSize)
						{
							Spot spotSpawned = s.SpawnDoubleSizeSpot (container, false);
							
							if (spotSpawned != null)
								spotsTemp.Add (spotSpawned);
						}
					}
				}
				
				//Remove Invalid Spots
				foreach(var s in spots)
				{
					if (s.isOccupied || !s.IsSameSize (container) || !s.CanPileContainer () || s == null)
						spotsTemp.Remove (s);
				}
				
				if(spotsTemp.Count == 0)
				{
					Debug.LogError ("No more free spots!", this);
					break;
				}
				
				//Take Spot
				Spot spotTaken = spotsTemp [Random.Range (0, spotsTemp.Count)];
				
				spots.Remove (spotTaken);
				spots.AddRange (container._pileSpots);
				
				spotTaken.SetInitialContainer (container);
			}
		}
	}

	Container CreateContainer (Container_Level container_Level, Transform parent)
	{
		GameObject prefab = basicContainersPrefabs [0];

		switch (container_Level.containerType)
		{
		case ContainerType.Basic:
			prefab = container_Level.isDoubleSize ? basicContainersPrefabs [1] : basicContainersPrefabs [0];
			break;
		case ContainerType.Cooled:
			prefab = container_Level.isDoubleSize ? cooledContainersPrefabs [1] : cooledContainersPrefabs [0];
			break;
		case ContainerType.Tank:
			prefab = container_Level.isDoubleSize ? tankContainersPrefabs [1] : tankContainersPrefabs [0];
			break;
		case ContainerType.Dangerous:
			prefab = container_Level.isDoubleSize ? dangerousContainersPrefabs [1] : dangerousContainersPrefabs [0];
			break;
		}
			
			
		Container container = (Instantiate (prefab, parent.position, Quaternion.identity, parent)).GetComponent<Container> ();

		container.Setup (container_Level);

		return container;
	}

	#region Level Start
	public void StartLevel (int index)
	{
		levelIndex = index;
		levelToStart = transform.GetChild (levelIndex);

		LoadLevelSettings (levelToStart);
	}

	public void StartLevel (Transform l)
	{
		level = l;
		FindLevelIndex ();

		LoadLevelSettings (levelToStart);
	}

	[ButtonGroup ("1", -1)]
	public void StartLevelTest ()
	{
		if (levelToStart == null)
			return;

		LoadLevelSettings (levelToStart);
	}

	[ButtonGroup ("1", -1)]
	public void NextLevel ()
	{
		if (levelIndex + 1 >= transform.childCount - 1)
			return;

		StartLevel (levelIndex + 1);
	}
	#endregion

	#region Other
	[PropertyOrder (-1)]
	[ButtonAttribute]
	void RenameLevels ()
	{
		for (int i = 0; i < transform.childCount; i++)
			transform.GetChild (i).name = "Level #" + i;
	}

	void FindLevelIndex ()
	{
		for(int i = 0; i < transform.childCount; i++)
			if(transform.GetChild (i) == level)
			{
				levelIndex = i;
				return;
			}
	}
	#endregion
}
