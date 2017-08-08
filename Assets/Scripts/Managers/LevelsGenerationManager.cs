using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using System.Linq;

public class LevelsGenerationManager : Singleton<LevelsGenerationManager>
{
	[Header ("States")]
	public bool isGeneratingLevel = false;
	public bool isGeneratingTrains = false;
	public int currentLevel = 0;

	[Header ("Duration")]
	public int levelDuration;

	[Header ("Errors")]
	public int errorsAllowed;

	[Header ("Orders")]
	public int ordersCount;
	public int ordersElementsCountMin = 2;

	[Header ("Trains")]
	public int trainsCount;
	public List<Train_LD> selectedTrains = new List<Train_LD> ();

	[Header ("Common Settings")]
	[Range (0, 100)]
	public int storageMaxFillingPercentage = 80;
	[Range (0, 100)]
	public int boatMaxFillingPercentage = 80;

	[Header ("Wagons Weight")]
	public Vector2 wagonExtraWeight = new Vector2 ();
	[Range (0, 100)]
	public int wagonInfiniteWeightChance;

	[Header ("Containers Weight")]
	public int[] basicContainerWeights = new int[2];
	public int[] cooledContainerWeights = new int[2];
	public int[] tankContainerWeights = new int[2];
	public int[] dangerousContainerWeights = new int[2];

	[Header ("Test")]
	public int levelToGenerateIndex = 0;
	[Button]
	void GenerateTest ()
	{
		GenerateLevel (levelToGenerateIndex);
	}

	private Storage _storage;
	private Boat _boat;

	public List<Train> _trainsGenerated = new List<Train> ();
	public List<Container> _containersGenerated = new List<Container> ();
	public List<Container> _extraContainersGenerated = new List<Container> ();
	public List<Container> _containersToPlace = new List<Container> ();

	private int _containerToPlaceCount;

	private LevelSettings_LD _currentLevelSettings;
	private LevelGenerated _levelGenerated;
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
		_boat = FindObjectOfType<Boat> ();
	}
	
	public void GenerateLevel (int levelIndex)
	{
		StartCoroutine (GenerateLevelCoroutine (levelIndex));
	}

	IEnumerator GenerateLevelCoroutine (int levelIndex)
	{
		currentLevel = levelIndex;

		isGeneratingLevel = true;

		ClearLevelGenerated ();

		if(transform.childCount - 1 < levelIndex)
		{
			Debug.LogError ("Invalid LevelIndex!");
			yield break;
		}

		_currentLevelSettings = transform.GetChild (levelIndex).GetComponent<LevelSettings_LD> ();

		levelDuration = _currentLevelSettings.levelDuration;
		errorsAllowed = _currentLevelSettings.errorsAllowed;

		ordersCount = Random.Range (_currentLevelSettings.ordersCountMin, _currentLevelSettings.ordersCountMax + 1);
		trainsCount = Random.Range (_currentLevelSettings.trainsCountMin, _currentLevelSettings.trainsCountMax + 1);

		CreateLevelObject (levelIndex);

		SelectTrains ();

		_trainsGenerated = TrainsMovementManager.Instance.GenerateTrains (selectedTrains);

		yield return new WaitForEndOfFrame ();

		isGeneratingTrains = true;

		for(int i = 0; i < _trainsGenerated.Count; i++)
			StartCoroutine (FillTrain (_trainsGenerated [i], selectedTrains [i]));

		yield return new WaitWhile (() => _trainsGenerationCount > 0);

		isGeneratingTrains = false;

		foreach (var t in _trainsGenerated)
			KeepOrdersContainers (t);

		CreateOrders ();

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

		FillContainerZone (_storage.containersParent, _storage.spotsParent, ContainersMovementManager.Instance.storagePileCount, _storageMaxFilling, _currentLevelSettings.storageFillingPercentage);

		while(_containersToPlace.Count > 0)
		{
			Boat boat = BoatsMovementManager.Instance.SpawnBoat ();

			yield return new WaitForEndOfFrame ();

			FillContainerZone (boat.containersParent, boat.spotsParent, ContainersMovementManager.Instance.boatPileCount, _boatMaxFilling);
		}

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

		if (_levelGenerated != null)
			Destroy (_levelGenerated.gameObject);
		
	}

	void CreateLevelObject (int levelIndex)
	{
		GameObject newLevel = new GameObject ();

		newLevel.name = "Level Generated #" + (levelIndex + 1).ToString ();

		newLevel.transform.SetParent (transform);

		_levelGenerated = newLevel.AddComponent<LevelGenerated> ();

		_levelGenerated.mostOrdersCount = _currentLevelSettings.mostOrdersCount;
		_levelGenerated.leastTrainsCount = _currentLevelSettings.leastTrainsCount;
		_levelGenerated.errorsAllowed = _currentLevelSettings.errorsAllowed;

		_levelGenerated.starsEarned = _currentLevelSettings.starsEarned;
		_levelGenerated.starsStates = _currentLevelSettings.starsStates;
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

	void SetWeight (Container_Level c)
	{
		switch (c.containerType)
		{
		case ContainerType.Basic:
			if (!c.isDoubleSize)
				c.containerWeight = basicContainerWeights [0];
			else
				c.containerWeight = basicContainerWeights [1];
			break;

		case ContainerType.Cooled:
			
			if (!c.isDoubleSize)
				c.containerWeight = cooledContainerWeights [0];
			else
				c.containerWeight = cooledContainerWeights [1];
			break;

		case ContainerType.Tank:
			
			if (!c.isDoubleSize)
				c.containerWeight = tankContainerWeights [0];
			else
				c.containerWeight = tankContainerWeights [1];
			break;

		case ContainerType.Dangerous:
			
			if (!c.isDoubleSize)
				c.containerWeight = dangerousContainerWeights [0];
			else
				c.containerWeight = dangerousContainerWeights [1];
			break;
		}
	}

	void RandomColor (Container_Level c)
	{
		c.containerColor = (ContainerColor)Random.Range (0, System.Enum.GetValues (typeof(ContainerColor)).Length);
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

			//Fill With Trains Forced Containers
			if(trainLD.forcedContainers.Count > 0)
				FillForcedContainers (spots, trainLD, containersSpawned);

			//Fill With Available Containers
			Fill (spots, _currentLevelSettings.containersAvailable, train, containersSpawned);

			_triesCount++;
		}
		while (_triesCount < _trainFillingTries && !_trainFilled);


		if(!_trainFilled)
			Debug.LogError ("Can't Fill Whole Train!", train);
		
		else
			Debug.Log ("Train filled after " + (_triesCount).ToString () + " tries!", train);

		_containersGenerated.AddRange (containersSpawned);

		_trainsGenerationCount--;
	}

	void FillForcedContainers (List<Spot> spots, Train_LD trainLD, List<Container> containersList)
	{
		foreach(var c in trainLD.forcedContainers)
		{
			Container container = null;

			//Choose Random Container Among Those Not Tested
			Container_Level containerLevel = c;

			SetWeight (containerLevel);
			RandomColor (containerLevel);

			//Create Container
			container = LevelsManager.Instance.CreateContainer (containerLevel, GlobalVariables.Instance.gameplayParent);

			containersList.Add (container);

			Spot spot = spots [Random.Range (0, spots.Count)];

			do
			{
				spot = spots [Random.Range (0, spots.Count)];
			}
			while(!spot.IsSameSize (container));

			spot.SetInitialContainer (container);

			spots.Remove (spot);
			foreach(var o in spot.overlappingSpots)
				spots.Remove (o);
		}
	}

	void Fill (List<Spot> spots, List<Container_Level> containers, Train train, List<Container> containersList)
	{
		//FOR EACH SPOT
		do
		{
			List<Container_Level> containersToTest = new List<Container_Level> (containers);

			//Debug.Log ("Spots: " + spots.Count);

			//TEST EACH CONTAINER AVAILABLE
			do
			{
				Container container = null;

				//Choose Random Container Among Those Not Tested
				Container_Level containerLevel = containersToTest [Random.Range (0, containersToTest.Count)];
				Spot spot = null;

				SetWeight (containerLevel);
				RandomColor (containerLevel);

				//Create Container
				container = LevelsManager.Instance.CreateContainer (containerLevel, GlobalVariables.Instance.gameplayParent);

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
			while(true);

		}
		while (spots.Count > 0 && !_tryFailed);
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

		while (fillingPercentage > _currentLevelSettings.ordersFillingPercentage)
		{
			List<Container> containers = new List<Container> ();

			foreach(var c in train.containers)
				if(!containers.Contains (c) && c != null)
					containers.Add (c);

			Container removedContainer = containers [Random.Range (0, containers.Count)];
			train.containers [train.containers.FindIndex (x => x == removedContainer)] = null;

			if(removedContainer.isDoubleSize)
				train.containers [train.containers.FindIndex (x => x == removedContainer)] = null;

			_containersGenerated.Remove (removedContainer);

			Destroy (removedContainer.gameObject);

			int containersCount = 0;

			foreach(var c in train.containers)
				if(c != null)
					containersCount++;

			fillingPercentage = (int) (((float)containersCount / (float)train.containers.Count) * 100f);
		}

	}

	void CreateOrders ()
	{
		var containersGeneratedTemp = new List<Container> (_containersGenerated);

		_levelGenerated.orders.Clear ();

		for (int i = 0; i < ordersCount; i++)
			_levelGenerated.orders.Add (new Order_Level ());

		foreach(var o in _levelGenerated.orders)
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
				containerLevel.containerCount = 1;
				containerLevel.containerWeight = c.weight;
				containerLevel.isDoubleSize = c.isDoubleSize;
			}
		}

		foreach(var c in containersGeneratedTemp)
		{
			int randomOrder = Random.Range (0, _levelGenerated.orders.Count);
			_levelGenerated.orders [randomOrder].levelContainers.Add (new Container_Level ());

			var containerLevel = _levelGenerated.orders [randomOrder].levelContainers [_levelGenerated.orders [randomOrder].levelContainers.Count - 1];

			containerLevel.containerColor = c.containerColor;
			containerLevel.containerType = c.containerType;
			containerLevel.containerCount = 1;
			containerLevel.containerWeight = c.weight;
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
				SetWeight (generatedContainerLevel);

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
			if (!s.isPileSpot && !s._isSpawned)
				spots.Add (s);

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
					FillContainerZone (containersParent, spotsParent, pileCount, zoneMaxFilling, percentageToFill, true);
				
				return;
			}

			//Fill Succeeded
			else
			{
				containersPlacedCount++;
				
				if(container.isDoubleSize)
					containersPlacedCount++;
				
				containersPercentage = (int) (((float)containersPlacedCount / (float)_containerToPlaceCount) * 100f);
				zoneFillingPercentage = (int) (((float)containersPlacedCount / (float)zoneInitialSpotsCount) * 100f);
				
				containersToPlace.Remove (container);
			}
		}

		//Debug.Log ("ContainersPercentage: " + containersPercentage + "% && zoneFillingPercentage: " + zoneFillingPercentage + "%", containersParent.parent);

		//Update Containers To Place
		_containersToPlace.Clear ();
		_containersToPlace.AddRange (containersToPlace);
	}

	bool FillContainer (List<Spot> spots, Container container, bool forceSpawnDoubleFirst = false)
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

	Container_Level RandomContainerLevel ()
	{
		Container_Level containerLevel = new Container_Level (); 

		RandomColor (containerLevel);
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
