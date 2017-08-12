using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using System.Linq;

public class LevelsGenerationManager : Singleton<LevelsGenerationManager>
{
	[Header ("Levels")]
	public List<LevelSettings_LD> levelsSettings = new List<LevelSettings_LD> ();
	public LevelGenerated currentLevelGenerated;

	[Header ("States")]
	public bool isGeneratingLevel = false;
	public bool isGeneratingTrains = false;
	public int currentLevelIndex = 0;

	[Header ("Duration")]
	public int levelDuration;

	[Header ("Errors")]
	public int errorsAllowed;

	[Header ("Orders")]
	public int ordersCount;
	public int ordersElementsCountMin = 2;
	public int ordersElementsCountMax = 8;

	[Header ("Trains")]
	public int trainsCount;
	public List<Train_LD> selectedTrains = new List<Train_LD> ();

	[Header ("Common Settings")]
	[Range (0, 100)]
	public int storageMaxFillingPercentage = 80;
	[Range (0, 100)]
	public int boatMaxFillingPercentage = 80;

	[Header ("Wagons Weight")]
	[MinMaxSlider (-2, 10)]
	public Vector2 wagonExtraWeight = new Vector2 ();
	[Range (0, 100)]
	public int wagonInfiniteWeightChance;

	[Header ("Containers Weight")]
	public int[] basicContainerWeights = new int[2];
	public int[] cooledContainerWeights = new int[2];
	public int[] tankContainerWeights = new int[2];
	public int[] dangerousContainerWeights = new int[2];

	[Header ("Elements Generated")]
	public List<Train> _trainsGenerated = new List<Train> ();
	public List<Boat> _boatsGenerated = new List<Boat> ();

	[Header ("Containers Generated")]
	public List<Container> _containersGenerated = new List<Container> ();
	public List<Container> _extraContainersGenerated = new List<Container> ();
	public List<Container> _forcedContainersGenerated = new List<Container> ();
	public List<Container> _parasitesContainersGenerated = new List<Container> ();


	private List<Container_Level> _containersAvailable = new List<Container_Level> ();
	private List<Container_Level> _forcedContainers = new List<Container_Level> ();
	private List<Container_Level> _parasiteContainers = new List<Container_Level> ();

	private Storage _storage;
	private List<Container> _containersToPlace = new List<Container> ();
	private int _containerToPlaceCount;

	[HideInInspector]
	public LevelSettings_LD _currentLevelSettings;
	private int _trainFillingTries = 20;

	private bool _trainFilled = false;
	private bool _tryFailed = false;
	private int _triesCount = 0;

	private int _storageMaxFilling = 80;
	private int _boatMaxFilling = 80;

	private int _trainsGenerationCount = 0;

	// Use this for initialization
	void Awake () 
	{
		_storage = FindObjectOfType<Storage> ();

		levelsSettings.Clear ();
		levelsSettings = transform.GetComponentsInChildren<LevelSettings_LD> ().ToList ();
	}
	
	public void GenerateLevel (int levelIndex, LevelSettings_LD level)
	{
		_currentLevelSettings = level;

		StartCoroutine (GenerateLevelCoroutine (levelIndex));
	}

	IEnumerator GenerateLevelCoroutine (int levelIndex)
	{
		currentLevelIndex = levelIndex;

		isGeneratingLevel = true;

		ClearLevelGenerated ();

		levelDuration = _currentLevelSettings.levelDuration;
		errorsAllowed = _currentLevelSettings.errorsAllowed;

		ordersCount = Random.Range (_currentLevelSettings.ordersCountMin, _currentLevelSettings.ordersCountMax + 1);
		trainsCount = Random.Range (_currentLevelSettings.trainsCountMin, _currentLevelSettings.trainsCountMax + 1);


		//Copy Containers Level
		_containersAvailable.Clear ();
		_forcedContainers.Clear ();
		_parasiteContainers.Clear ();

		foreach (var c in _currentLevelSettings.containersAvailable)
			_containersAvailable.Add (new Container_Level (c));

		foreach (var c in _currentLevelSettings.forcedContainers)
			_forcedContainers.Add (new Container_Level (c));

		foreach (var c in _currentLevelSettings.parasiteContainers)
			_parasiteContainers.Add (new Container_Level (c));
		

		CreateLevelObject (levelIndex);

		SelectTrains ();

		_trainsGenerated = TrainsMovementManager.Instance.GenerateTrains (selectedTrains);

		yield return new WaitForEndOfFrame ();

		yield return new WaitForEndOfFrame ();

		isGeneratingTrains = true;

		_forcedContainersGenerated.Clear ();

		foreach(var c in _forcedContainers)
		{
			Train randomTrain = _trainsGenerated [Random.Range (0, _trainsGenerated.Count)];

			FillForceContainer (randomTrain._allSpots, c, randomTrain, _containersGenerated);
		}
			
		for(int i = 0; i < _trainsGenerated.Count; i++)
			StartCoroutine (FillTrain (_trainsGenerated [i], selectedTrains [i]));

		yield return new WaitWhile (() => _trainsGenerationCount > 0);

		SetWagonsWeight ();

		isGeneratingTrains = false;

		foreach (var t in _trainsGenerated)
			KeepOrdersContainers (t);

		CreateOrders ();

		ordersCount = currentLevelGenerated.orders.Count;

		_currentLevelSettings.ordersCount = ordersCount;
		currentLevelGenerated.ordersCount = ordersCount;

		ExtraContainers ();

		_containersToPlace.Clear ();
		_containersToPlace.AddRange (_containersGenerated);
		_containersToPlace.AddRange (_extraContainersGenerated);

		_containerToPlaceCount = 0;

		//Get Containers Overall Count
		foreach (var c in _containersToPlace)
		{
			_containerToPlaceCount++;

			if(c.isDoubleSize)
				_containerToPlaceCount++;
		}

		LevelsManager.Instance.EmptyZone (_storage.containersParent);
		BoatsMovementManager.Instance.ClearBoat ();

		yield return new WaitForEndOfFrame ();

		yield return new WaitForEndOfFrame ();

		FillContainerZone (_storage.containersParent, _storage.spotsParent, ContainersMovementManager.Instance.storagePileCount, _storageMaxFilling, _currentLevelSettings.storageFillingPercentage);

		_boatsGenerated.Clear ();

		while(_containersToPlace.Count > 0)
		{
			Boat boat = BoatsMovementManager.Instance.SpawnBoat ();

			_boatsGenerated.Add (boat);

			yield return new WaitForEndOfFrame ();

			FillContainerZone (boat.containersParent, boat.spotsParent, ContainersMovementManager.Instance.boatPileCount, _boatMaxFilling);
		}

		ParasiteContainers ();

		OrdersManager.Instance.containersFromNoOrder.AddRange (_parasitesContainersGenerated);

		SetupTrains ();

		if(_boatsGenerated.Count > 0)
			SetupBoats ();

		isGeneratingLevel = false;
	}

	void ClearLevelGenerated ()
	{
		//Destroy Previous Trains
		foreach (var t in _trainsGenerated)
			if(t)
				Destroy (t.gameObject);

		//Destroy Previous Try Containers Generated
		foreach(var c in _containersGenerated)
		{
			if(c)
			{
				c.RemoveContainer ();
				Destroy (c.gameObject);
			}
		}

		//Destroy Previous Try Extra Containers Generated
		foreach(var c in _extraContainersGenerated)
		{
			if(c)
			{
				c.RemoveContainer ();
				Destroy (c.gameObject);
			}
		}

		_containersGenerated.Clear ();
		_trainsGenerated.Clear ();

		if (currentLevelGenerated != null)
			Destroy (currentLevelGenerated.gameObject);
		
	}

	void CreateLevelObject (int levelIndex)
	{
		GameObject newLevel = new GameObject ();

		newLevel.name = "Level Generated #" + (levelIndex + 1).ToString ();

		newLevel.transform.SetParent (transform);

		currentLevelGenerated = newLevel.AddComponent<LevelGenerated> ();

		currentLevelGenerated.mostOrdersCount = _currentLevelSettings.mostOrdersCount;
		currentLevelGenerated.leastTrainsCount = _currentLevelSettings.leastTrainsCount;
		currentLevelGenerated.errorsAllowed = _currentLevelSettings.errorsAllowed;

		currentLevelGenerated.starsEarned = _currentLevelSettings.starsEarned;
		currentLevelGenerated.starsStates = _currentLevelSettings.starsStates;
	}

	void SelectTrains ()
	{
		if(_currentLevelSettings.trainsAvailable.Count == 0)
		{
			Debug.LogError ("No Trains_LD!");
			return;
		}

		selectedTrains.Clear ();

		for(int i = 0; i < trainsCount; i++)
			selectedTrains.Add (_currentLevelSettings.trainsAvailable [Random.Range (0, _currentLevelSettings.trainsAvailable.Count)]);
	}

	public Container_Level RandomColor (Container_Level c)
	{
		var container = new Container_Level (c);

		if (c.containerColor != ContainerColor.Random)
			return container;
		
		container.containerColor = (ContainerColor)Random.Range (1, System.Enum.GetValues (typeof(ContainerColor)).Length);

		return container;
	}

	IEnumerator FillTrain (Train train, Train_LD trainLD)
	{
		_trainsGenerationCount++;

		_trainFilled = false;
		_triesCount = 0;

		List<Container> containersSpawned = new List<Container> ();

		//OVERALL TRIES
		do
		{
			//Destroy Previous Try Containers Generated
			foreach(var c in containersSpawned)
			{
				if(c)
				{
					c.RemoveContainer ();
					Destroy (c.gameObject);
				}
			}

			yield return new WaitForEndOfFrame ();

			containersSpawned.Clear ();

			//Make Sure Spots Are Cleared
			ClearSpots (train);

			List<Spot> spots = new List<Spot> (train._allSpots);
			_tryFailed = false;

			//Fill With Available Containers
			Fill (spots, _containersAvailable, train, containersSpawned);

			_triesCount++;
		}
		while (_triesCount < _trainFillingTries && !_trainFilled);


		if(!_trainFilled)
			Debug.LogError ("Can't Fill Whole Train!", train);
		
		else
		{
			
			//Debug.Log ("Train filled after " + (_triesCount).ToString () + " tries!", train);
		}

		_containersGenerated.AddRange (containersSpawned);

		_trainsGenerationCount--;
	}

	void Fill (List<Spot> spots, List<Container_Level> containers, Train train, List<Container> containersList)
	{
		//FOR EACH SPOT
		while (spots.Count > 0 && !_tryFailed)
		{
			List<Container_Level> containersToTest = new List<Container_Level> (containers);

			//Debug.Log ("Spots: " + spots.Count);

			//TEST EACH CONTAINER AVAILABLE
			while(true)
			{
				Container container = null;

				//Choose Random Container Among Those Not Tested
				Container_Level containerLevel = containersToTest [Random.Range (0, containersToTest.Count)];
				Spot spot = null;

				containerLevel = RandomColor (containerLevel);

				//Create Container
				container = LevelsManager.Instance.CreateContainer (containerLevel, GlobalVariables.Instance.extraContainersParent);

				//Test Container Wich Each Spot
				foreach(var s in spots)
				{
					if(s.IsSameSize (container) && container.CheckConstraints (s))
					{
						spot = s;
						spot.SetInitialContainer (container);
						break;
					}
				}

				if(spot != null)
				{
					containersList.Add (container);

					spots.Remove (spot);
					foreach(var o in spot.overlappingSpots)
						spots.Remove (o);

					if(spots.Count == 0)
						_trainFilled = true;

					break;
				}

				//Remove Tested Container
				containersToTest.Remove (containerLevel);

				//Destroy Tested Container
				container.RemoveContainer ();
				Destroy (container.gameObject);

				//If No More Containers To Test Try Failed
				if(containersToTest.Count == 0)
				{
					//Debug.LogError ("Try: " + _triesCount.ToString () + " failed!", train);
					_tryFailed = true;
					break;
				}
			}
		}
	}

	void FillForceContainer (List<Spot> spots, Container_Level containerLevel, Train train, List<Container> containersList)
	{
		//FOR EACH SPOT
		Container container = null;
		
		//Choose Random Container Among Those Not Tested
		Spot spot = null;
		
		containerLevel = RandomColor (containerLevel);
		
		//Create Container
		container = LevelsManager.Instance.CreateContainer (containerLevel, GlobalVariables.Instance.extraContainersParent);
		
		//Test Container Wich Each Spot
		foreach(var s in spots)
		{
			if(s.IsSameSize (container) && container.CheckConstraints (s) && !s.isOccupied)
			{
				spot = s;
				spot.SetInitialContainer (container);
				break;
			}
		}
		
		if(spot != null)
		{
			containersList.Add (container);

			_forcedContainersGenerated.Add (container);
		}
		else
		{
			//Destroy Tested Container
			container.RemoveContainer ();
			
			Debug.LogWarning ("Can't Fill Forced Container: " + container.name);
			
			Destroy (container.gameObject);
		}
	}

	void SetWagonsWeight ()
	{
		foreach(var t in _trainsGenerated)
		{
			foreach(var w in t.wagons)
			{
				w.UpdateWeight ();

				int maxWeight = w.currentWeight;

				if (Random.Range (0, 100) < wagonInfiniteWeightChance)
					maxWeight = 666;
				else
					maxWeight += Mathf.RoundToInt (Random.Range (wagonExtraWeight.x, wagonExtraWeight.y));

				w.maxWeight = maxWeight;
			}
		}
	}

	void ClearSpots (Train train)
	{
		foreach(var s in train._allSpots)
		{
			s.container = null;
			s.isOccupied = false;
		}
	}

	void KeepOrdersContainers (Train train)
	{
		int fillingPercentage = 100;

		List<Container> containers = new List<Container> ();
		
		foreach(var c in train.containers)
			if(!containers.Contains (c) && c != null && !_forcedContainersGenerated.Contains (c))
				containers.Add (c);
		
		while (fillingPercentage > _currentLevelSettings.ordersFillingPercentage)
		{
			Container removedContainer = containers [Random.Range (0, containers.Count)];
			train.containers [train.containers.FindIndex (x => x == removedContainer)] = null;

			if(removedContainer.isDoubleSize)
				train.containers [train.containers.FindIndex (x => x == removedContainer)] = null;

			containers.Remove (removedContainer);
			_containersGenerated.Remove (removedContainer);

			Destroy (removedContainer.gameObject);

			int containersCount = 0;

			foreach(var c in train.containers)
				if(c != null)
					containersCount++;

			fillingPercentage = Mathf.RoundToInt (((float)containersCount / (float)train.containers.Count) * 100f);
		}

	}

	void CreateOrders ()
	{
		var containersGeneratedTemp = new List<Container> (_containersGenerated);

		currentLevelGenerated.orders.Clear ();

		for (int i = 0; i < ordersCount; i++)
			currentLevelGenerated.orders.Add (new Order_Level ());

		while(containersGeneratedTemp.Count > currentLevelGenerated.orders.Count * ordersElementsCountMax)
		{
			currentLevelGenerated.orders.Add (new Order_Level ());
		}


		foreach(var o in currentLevelGenerated.orders)
		{
			for(int i = 0; i < ordersElementsCountMin; i++)
			{
				if (containersGeneratedTemp.Count == 0)
					return;

				o.levelContainers.Add (new Container_Level ());
				var containerLevel = o.levelContainers [o.levelContainers.Count - 1];

				var c = containersGeneratedTemp [Random.Range (0, containersGeneratedTemp.Count)];
				containersGeneratedTemp.Remove (c);
				
				containerLevel.containerColor = c.containerColor;
				containerLevel.containerType = c.containerType;
				containerLevel.isDoubleSize = c.isDoubleSize;
			}
		}

		foreach(var c in containersGeneratedTemp)
		{
			int randomOrder = Random.Range (0, currentLevelGenerated.orders.Count);

			while(currentLevelGenerated.orders [randomOrder].levelContainers.Count >= ordersElementsCountMax)
				randomOrder = Random.Range (0, currentLevelGenerated.orders.Count);

			currentLevelGenerated.orders [randomOrder].levelContainers.Add (new Container_Level ());

			var containerLevel = currentLevelGenerated.orders [randomOrder].levelContainers [currentLevelGenerated.orders [randomOrder].levelContainers.Count - 1];

			containerLevel.containerColor = c.containerColor;
			containerLevel.containerType = c.containerType;
			containerLevel.isDoubleSize = c.isDoubleSize;
		}
	}

	void ExtraContainers ()
	{
		foreach (var e in _extraContainersGenerated)
			if(e != null)
			Destroy (e.gameObject);

		_extraContainersGenerated.Clear ();

		for(int i = 0; i < _currentLevelSettings.extraContainersCount; i++)
		{
			bool validContainer = true;
			Container_Level generatedContainerLevel = null; 


			do
			{
				validContainer = true;

				generatedContainerLevel = RandomContainerLevel (); 

				foreach(var c in _containersGenerated)
				{
					if(c.containerColor != generatedContainerLevel.containerColor)
						continue;

					if(c.containerType != generatedContainerLevel.containerType)
						continue;

					if(c.isDoubleSize != generatedContainerLevel.isDoubleSize)
						continue;

					validContainer = false;
				}

			}
			while(!validContainer);

			Container container = LevelsManager.Instance.CreateContainer (generatedContainerLevel, GlobalVariables.Instance.extraContainersParent);

			container.transform.position = new Vector3 (10 * i, -50, 100);

			_extraContainersGenerated.Add (container);
		}
	}

	void FillContainerZone (Transform containersParent, Transform spotsParent, int pileCount, int zoneMaxFilling, int percentageToFill = 100, bool forceSpawnDoubleFirst = false)
	{
		//Reset Containers
		foreach (var c in containersParent.GetComponentsInChildren<Container> ())
			c.RemoveContainer ();

		//Get Spots
		var spotsArray = spotsParent.GetComponentsInChildren<Spot> ().ToList ();
		List<Spot> spots = new List<Spot> ();

		//Get & Sort Spots
		foreach (var s in spotsArray)
		{
			//Debug.Log (s.isOccupied, s);

			if (!s.isPileSpot && !s._isSpawned)
				spots.Add (s);
		}

		//Free Spots
		foreach (var s in spots)
		{
			s.isOccupied = false;
			s.container = null;
		}

		int containersPercentage = 0;
		int containersPlacedCount = 0;
		
		int zoneFillingPercentage = 0;
		int zoneInitialSpotsCount = 0;

		//Debug.Log ("containersToPlace: " + _containersToPlace.Count, containersParent.parent);

		//Sort Containers_Levels
		List<Container> containersToPlace = new List<Container> ();
		
		if(forceSpawnDoubleFirst)
		{
			foreach (var c in _containersToPlace)
				if (c.isDoubleSize)
					containersToPlace.Add (c);
			
			foreach (var c in _containersToPlace)
				if (!c.isDoubleSize)
					containersToPlace.Add (c);
		}
		else
			containersToPlace.AddRange (_containersToPlace);

		//Get Zone Spots Count
		foreach (var s in spots)
			if (!s.isDoubleSize && !s.isPileSpot && !s._isSpawned)
				zoneInitialSpotsCount++;

		zoneInitialSpotsCount *= (pileCount + 1);

		//Debug.Log ("zoneInitialSpotsCount: " + zoneInitialSpotsCount, containersParent.parent);

		while (containersPercentage < percentageToFill && zoneFillingPercentage <= zoneMaxFilling && containersToPlace.Count > 0)
		{
			Container container = containersToPlace [Random.Range (0, containersToPlace.Count)];

			bool containerPlaced = FillContainer (spots, container, forceSpawnDoubleFirst);

			//Fill Failed
			if(!containerPlaced)
			{
				if(!forceSpawnDoubleFirst)
				{
					FillContainerZone (containersParent, spotsParent, pileCount, zoneMaxFilling, percentageToFill, true);
					return;
				}

				break;
			}

			//Fill Succeeded
			else
			{
				containersPlacedCount++;
				
				if(container.isDoubleSize)
					containersPlacedCount++;
				
				containersPercentage = Mathf.RoundToInt (((float)containersPlacedCount / (float)_containerToPlaceCount) * 100f);
				zoneFillingPercentage = Mathf.RoundToInt (((float)containersPlacedCount / (float)zoneInitialSpotsCount) * 100f);
				
				containersToPlace.Remove (container);
			}
		}

		//Debug.Log ("ContainersPercentage: " + containersPercentage + "% && zoneFillingPercentage: " + zoneFillingPercentage + "%", containersParent.parent);

		//Update Containers To Place
		_containersToPlace.Clear ();
		_containersToPlace.AddRange (containersToPlace);
	}

	public bool FillContainer (List<Spot> spots, Container container, bool forceSpawnDoubleFirst = false)
	{
		List<Spot> spotsTemp = new List<Spot> (spots);

		//Add Spawned Spots
		if(container.isDoubleSize)
		{
			foreach(var s in spots)
			{
				if(s.isDoubleSize)
				{
					Spot spotSpawned = s.SpawnDoubleSizeSpot (container, false);
					
					if (spotSpawned != null)
					{
						spotsTemp.Add (spotSpawned);
						//Debug.Log ("Spawned !!!");
					}
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
			if(!forceSpawnDoubleFirst)
				Debug.LogWarning ("No more free spots retrying ...", container);
			else
				Debug.LogWarning ("No spots!", container);
			
			return false;
		}
		
		//Take Spot
		Spot spotTaken = spotsTemp [Random.Range (0, spotsTemp.Count)];
		
		spots.Remove (spotTaken);
		spots.AddRange (container._pileSpots);

		if (container.spotOccupied)
			container.RemoveContainer ();

		spotTaken.SetInitialContainer (container);

		return true;
	}

	void ParasiteContainers ()
	{
		_parasitesContainersGenerated.Clear ();

		foreach(var t in _trainsGenerated)
		{
			//Debug.Log (t._allSpots.Count, t);
			foreach(var s in t._allSpots)
			{
				s.isOccupied = false;
				s.container = null;
			}
		}

		foreach(var c in _parasiteContainers)
		{
			var containerLevel = RandomColor (c);

			var container = LevelsManager.Instance.CreateContainer (containerLevel, GlobalVariables.Instance.extraContainersParent);
			bool fillSucess = false;

			foreach(var t in _trainsGenerated)
			{
				int spotsTaken = 0;
				foreach (var s in t._allSpots)
					if (s.isOccupied)
						spotsTaken++;

				if (spotsTaken == t._allSpots.Count)
					continue;

				var spots = new List<Spot> (t._allSpots);

				fillSucess = FillContainer (spots, container);
				
				if (!fillSucess)
					continue;
				else
				{
					_parasitesContainersGenerated.Add (container);

					break;
				}
			}

			if(!fillSucess)
			{
				Destroy (container.gameObject);
				Debug.LogWarning ("Can't Place All Parasite Containers!");
			}
		}
	}

	void SetupTrains ()
	{
		if(_trainsGenerated.Count % 2 == 0)
			currentLevelGenerated.trainsDuration = (int)(_currentLevelSettings.levelDuration / (_trainsGenerated.Count / 2));
		else
			currentLevelGenerated.trainsDuration = (int)(_currentLevelSettings.levelDuration / (_trainsGenerated.Count / 2 + 1));

		bool fillRail1 = true;

		foreach(var t in _trainsGenerated)
		{
			if (fillRail1)
				currentLevelGenerated.rail1Trains.Add (t);
			else
				currentLevelGenerated.rail2Trains.Add (t);

			fillRail1 = !fillRail1;
		}
	}

	void SetupBoats ()
	{
		currentLevelGenerated.boatsDuration = (int)((_currentLevelSettings.levelDuration - _currentLevelSettings.boatsDelay) / _boatsGenerated.Count);

		currentLevelGenerated.boatsDelay = _currentLevelSettings.boatsDelay;

		currentLevelGenerated.boats = _boatsGenerated;
	}

	Container_Level RandomContainerLevel ()
	{
		Container_Level containerLevel = new Container_Level (); 

		containerLevel = RandomColor (containerLevel);
		containerLevel.containerType = (ContainerType)Random.Range (0, System.Enum.GetValues (typeof(ContainerType)).Length);
		containerLevel.isDoubleSize = Random.Range (1, 3) == 1 ? false : true;

		return containerLevel;
	}

	#region Other
	[PropertyOrder (-1)]
	[ButtonAttribute]
	void RenameLevels ()
	{
		for (int i = 0; i < transform.childCount; i++)
			transform.GetChild (i).name = "Level Settings #" + (i + 1).ToString ();
	}
	#endregion
}
