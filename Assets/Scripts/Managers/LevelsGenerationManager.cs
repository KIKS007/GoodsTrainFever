using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

public class LevelsGenerationManager : Singleton<LevelsGenerationManager>
{
	[Header ("Duration")]
	public int levelDuration;

	[Header ("Errors")]
	public int errorsAllowed;

	[Header ("Orders")]
	public int ordersCount;

	[Header ("Trains")]
	public int trainsCount;
	public List<Train_LD> selectedTrains = new List<Train_LD> ();

	[Header ("Common Settings")]
	[Range (0, 100)]
	public int extraContainersPercentage = 10;
	[Range (0, 100)]
	public int storageMaxFillingPercentage = 80;
	[Range (0, 100)]
	public int boatMaxFillingPercentage = 80;

	[Header ("Wagons Weight")]
	public Vector2 wagonExtraWeight = new Vector2 ();
	[Range (0, 100)]
	public int wagonInfiniteWeight;

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

	private LevelSettings_LD _currentLevelSettings;
	private Level _generatedLevel;
	public List<Train> _trainsGenerated = new List<Train> ();
	public List<Container> _containersGenerated = new List<Container> ();
	private int _trainFillingTries = 20;

	private bool _trainFilled = false;
	private bool _tryFailed = false;
	private int _triesCount = 0;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	public void GenerateLevel (int levelIndex)
	{
		StartCoroutine (GenerateLevelCoroutine (levelIndex));
	}

	IEnumerator GenerateLevelCoroutine (int levelIndex)
	{
		foreach (var t in _trainsGenerated)
			if(t)
			Destroy (t.gameObject);

		_trainsGenerated.Clear ();

		if (_generatedLevel != null)
			Destroy (_generatedLevel.gameObject);

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

		for(int i = 0; i < _trainsGenerated.Count; i++)
			StartCoroutine (FillTrain (_trainsGenerated [i], selectedTrains [i]));
	}

	void CreateLevelObject (int levelIndex)
	{
		GameObject newLevel = new GameObject ();

		newLevel.name = "Level Generated #" + (levelIndex + 1).ToString ();

		newLevel.transform.SetParent (LevelsManager.Instance.transform);

		_generatedLevel = newLevel.AddComponent<Level> ();

		_generatedLevel.mostOrdersCount = _currentLevelSettings.mostOrdersCount;
		_generatedLevel.leastTrainsCount = _currentLevelSettings.leastTrainsCount;
		_generatedLevel.errorsAllowed = _currentLevelSettings.errorsAllowed;
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
		_trainFilled = false;
		_triesCount = 0;

		//OVERALL TRIES
		do
		{
			//Destroy Previous Try Containers Generated
			foreach(var c in _containersGenerated)
			{
				if(c)
				{
					c.RemoveContainer ();
					Destroy (c.gameObject);
				}
			}

			yield return new WaitForEndOfFrame ();

			_containersGenerated.Clear ();

			//Make Sure Spots Are Cleared
			ClearSpots (train);

			List<Spot> spots = new List<Spot> (train._allSpots);
			_tryFailed = false;

			//Fill With Trains Forced Containers
			if(trainLD.forcedContainers.Count > 0)
				FillForcedContainers (spots, trainLD);

			//Fill With Available Containers
			Fill (spots, _currentLevelSettings.containersAvailable, train);

			_triesCount++;
		}
		while (_triesCount < _trainFillingTries && !_trainFilled);


		if(!_trainFilled)
		{
			Debug.LogError ("Can't Fill Whole Train!", train);
			yield break;
		}
		else
		{
			Debug.Log ("Train Filled!", train);
		}
	}

	void FillForcedContainers (List<Spot> spots, Train_LD trainLD)
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

			_containersGenerated.Add (container);

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

	void Fill (List<Spot> spots, List<Container_Level> containers, Train train)
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

				_containersGenerated.Add (container);

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
