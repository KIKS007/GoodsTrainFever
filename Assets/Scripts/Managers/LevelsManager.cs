using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

public class LevelsManager : Singleton<LevelsManager> 
{
	public int levelToStart = 0;
	public bool loadLevelOnStart = false;
	public bool clearLevelOnStart = true;

	[Header ("Level")]
	public int levelIndex;
	public Level currentLevel;
	public int levelsCount;

	[Header ("Errors")]
	public int errorsLocked = 0;
	public int errorsAllowed = 0;
	public int errorsSecondStarAllowed = 0;

	[Header ("Errors Check")]
	public int currentErrors;
	public Text errorsText;
	public Transform errorsTextParent;
	public int nextToGroups = 0;
	public List<SpecialConstraint> specialConstraint = new List<SpecialConstraint> ();

	[Header ("Level Duration")]
	public int levelDuration = 0;

	[Header ("Orders")]
	public bool randomColors = false;
	public List<Order_Level> orders = new List<Order_Level> ();

	[Header ("Storage")]
	public bool spawnDoubleSizeFirst = false;
	public bool spawnAllOrderContainers = true;
	public List<Container_Level> storageContainers = new List<Container_Level> ();

	[Header ("Trains")]
	public int trainsUsed = 0;
	public float waitDurationBetweenTrains = 2f;
	public int trainsToSend;
	public List<Train_Level> rail1Trains = new List<Train_Level> ();
	public List<Train_Level> rail2Trains = new List<Train_Level> ();

	[Header ("Boats")]
	public float boatsDuration;
	public bool lastBoatStay = true;
	public float waitDurationBetweenBoats = 2f;
	public List<Boat_Level> boats = new List<Boat_Level> ();

	[Header ("Containers Prefabs")]
	public GameObject[] basicContainersPrefabs = new GameObject[2];
	public GameObject[] cooledContainersPrefabs = new GameObject[2];
	public GameObject[] tankContainersPrefabs = new GameObject[2];
	public GameObject[] dangerousContainersPrefabs = new GameObject[2];

	[HideInInspector]
	public Storage _storage;
	[HideInInspector]
	public Boat _boat;
	private bool _rail1Occupied = false;
	private bool _rail2Occupied = false;
	private int _randomColorOffset;
	private List<int> _previousRandomColorOffset = new List<int> ();

	//public	List<Spot> spots = new List<Spot> ();
	//public List<Spot> spotsTemp = new List<Spot> ();

	// Use this for initialization
	void Awake () 
	{
		_storage = FindObjectOfType<Storage> ();
		_boat = FindObjectOfType<Boat> ();

		levelsCount = transform.childCount;

		if (clearLevelOnStart)
			ClearLevelSettings ();

		if (loadLevelOnStart)
			LoadLevelSettings (levelToStart);

		Container.OnContainerMoved += ()=> DOVirtual.DelayedCall (0.01f, ()=> CheckConstraints ());

		MenuManager.Instance.OnLevelStart += () => StartCoroutine (LevelDuration ());
		MenuManager.Instance.OnMainMenu += ClearLevelSettings;
		MenuManager.Instance.OnMainMenu += ()=> _previousRandomColorOffset.Clear ();

		errorsText.text = "0";
		errorsTextParent.localScale = Vector3.zero;
	}

	void ClearLevelSettings ()
	{
		StopAllCoroutines ();

		trainsUsed = 0;
		levelDuration = 0;
		trainsToSend = 0;
		currentErrors = 0;
		errorsLocked = 0;

		orders.Clear ();
		storageContainers.Clear ();
		rail1Trains = null;
		rail2Trains = null;
		boatsDuration = 0;
		boats.Clear ();

		_rail1Occupied = false;
		_rail2Occupied = false;


		TrainsMovementManager.Instance.ClearTrains ();

		BoatsMovementManager.Instance.ClearBoat ();

		EmptyZone (_boat.containersParent);

		OrdersManager.Instance.ClearOrders (false);
	}

	public void LoadLevelSettings (int index)
	{
		if(index > transform.childCount - 1)
		{
			Debug.LogError ("Invalid Level!");
			return;
		}

		levelIndex = index;

		ClearLevelSettings ();

		if (randomColors)
		{
			do 
			{
				_randomColorOffset = Random.Range (0, 4);

			} while (_previousRandomColorOffset.Contains (_randomColorOffset));

			_previousRandomColorOffset.Add (_randomColorOffset);

			if (_previousRandomColorOffset.Count > 3)
				_previousRandomColorOffset.RemoveAt (0);

		}
		else
			_randomColorOffset = 0;
		
		Level level = transform.GetChild (index).GetComponent<Level> ();

		currentLevel = level;

		spawnAllOrderContainers = level.spawnAllOrderContainers;
		rail1Trains = level.rail1Trains;
		rail2Trains = level.rail2Trains;
		boatsDuration = level.boatsDuration;
		lastBoatStay = level.lastBoatStay;
		errorsAllowed = level.errorsAllowed;

		orders.Clear ();
		storageContainers.Clear ();
		boats.Clear ();

		foreach (var o in level.orders)
			orders.Add (new Order_Level (o));

		foreach (var c in level.storageContainers)
			storageContainers.Add (new Container_Level (c));

		foreach (var b in level.boats)
			boats.Add (new Boat_Level (b));
		

		errorsSecondStarAllowed = (int)(errorsAllowed * 0.5f);

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
		{
			_rail1Occupied = true;
			trainsToSend += rail1Trains.Count;
			StartCoroutine (SpawnTrains (rail1Trains, TrainsMovementManager.Instance.rail1));
		}

		if (rail2Trains.Count > 0)
		{
			_rail2Occupied = true;
			trainsToSend += rail2Trains.Count;
			StartCoroutine (SpawnTrains (rail2Trains, TrainsMovementManager.Instance.rail2));
		}

		//Boats
		if (boats.Count > 0)
			StartCoroutine (SpawnBoats ());
	}

	void RandomColors (List<Container_Level> containers)
	{
		//Debug.Log ("randomOffset : " + _randomColorOffset);

		foreach(var c in containers)
		{
			int color = (int)c.containerColor;

			c.containerColor = new ContainerColor ();
			c.containerColor = (ContainerColor)color;

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
		yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

		yield return new WaitForSeconds (order.delay);

		OrdersManager.Instance.AddOrder (order);
	}

	IEnumerator SpawnTrains (List<Train_Level> train_Level, Rail rail)
	{
		yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);
	
		for(int i = 0; i < train_Level.Count; i++)
		{
			Train train = TrainsMovementManager.Instance.SpawnTrain (rail, train_Level [i]);

			yield return new WaitWhile (()=> train.inTransition);

			yield return new WaitWhile (()=> train.waitingDeparture);

			CheckConstraints (train);
			CheckConstraints ();

			trainsUsed++;
			trainsToSend--;
			OrdersManager.Instance.TrainDeparture (train.containers);

			if(errorsLocked > errorsAllowed)
			{
				LevelEnd (LevelEndType.Errors);
				yield break;
			}

			if (trainsToSend == 0)
				GameManager.Instance.gameState = GameState.End;
			
			if (OrdersManager.Instance.allOrdersSent)
			{
				LevelEnd (LevelEndType.Orders);
				yield break;
			}

			yield return new WaitUntil (()=> train == null);

			if(i != train_Level.Count - 1)
				yield return new WaitForSeconds (waitDurationBetweenTrains);
			else
			{
				if (rail == TrainsMovementManager.Instance.rail1)
				{
					_rail1Occupied = false;

					if (_rail2Occupied == false)
					{
						LevelEnd (LevelEndType.Trains);
						yield break;
					}
				}
				else
				{
					_rail2Occupied = false;

					if (_rail1Occupied == false)
					{
						LevelEnd (LevelEndType.Trains);
						yield break;
					}
				}
			}
		}

		if (OrdersManager.Instance.allOrdersSent)
		{
			LevelEnd (LevelEndType.Orders);
			yield break;
		}
	}

	IEnumerator SpawnBoats ()
	{
		yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);
	
		foreach(var b in boats)
		{
			StartCoroutine (FillContainerZone (b.boatContainers, _boat.transform, _boat.containersParent));

			if (b.delay > 0)
				yield return new WaitForSeconds (b.delay);

			BoatsMovementManager.Instance.BoatStart ();

			yield return new WaitWhile (() => BoatsMovementManager.Instance.inTransition);

			float boatDuration = b.overrideDuration & b.duration > 0 ? b.duration : boatsDuration;

			yield return new WaitForSeconds (boatDuration);

			BoatsMovementManager.Instance.BoatDeparture ();

			yield return new WaitWhile (() => BoatsMovementManager.Instance.inTransition);

			yield return new WaitForSeconds (waitDurationBetweenBoats);
		}
	}

	public void EmptyZone (Transform parent, bool destroyContainers = true)
	{
		foreach (Transform c in parent)
		{
			var container = c.GetComponent<Container> ();

			if (container != null)
				container.RemoveContainer ();

			if(destroyContainers)
				Destroy (c.gameObject);
		}
	}

	[Button]
	void FillStorageZonetest ()
	{
		StartCoroutine (FillContainerZone (storageContainers, _storage.transform, _storage.containersParent));
	}

	IEnumerator FillContainerZone (List<Container_Level> containers_Base, Transform zoneParent, Transform containersParent, bool forceSpawnDoubleFirst = false)
	{
		EmptyZone (containersParent);

		yield return new WaitForEndOfFrame ();

		//Sort Containers_Levels
		List<Container_Level> containers_Levels = new List<Container_Level> ();

		if(spawnDoubleSizeFirst || forceSpawnDoubleFirst)
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

		foreach (var s in spotsArray)
			s.isOccupied = false;

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

					if(!forceSpawnDoubleFirst)
						StartCoroutine (FillContainerZone (containers_Base, zoneParent, containersParent, true));
					else
						Debug.LogError ("Too many containers to spawn!", this);
					
					yield break;
				}
				
				//Take Spot
				Spot spotTaken = spotsTemp [Random.Range (0, spotsTemp.Count)];
				
				spots.Remove (spotTaken);
				spots.AddRange (container._pileSpots);
				
				spotTaken.SetInitialContainer (container);
			}
		}
	}

	public Container CreateContainer (Container_Level container_Level, Transform parent)
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

	public void CheckConstraints (Train checkedTrain = null)
	{
		currentErrors = 0;
		nextToGroups = 0;

		foreach (var c in specialConstraint)
		{
			c.count = 0;
			c.groupCount = 0;
		}

		if(checkedTrain == null)
		{
			if (TrainsMovementManager.Instance.rail1.train && TrainsMovementManager.Instance.rail1.train.waitingDeparture)
				CheckTrainConstraints (TrainsMovementManager.Instance.rail1.train);
			
			if (TrainsMovementManager.Instance.rail2.train && TrainsMovementManager.Instance.rail2.train.waitingDeparture)
				CheckTrainConstraints (TrainsMovementManager.Instance.rail2.train);
		}
		else
			CheckTrainConstraints (checkedTrain);

		foreach (var c in specialConstraint)
			if (c.count - 1 > 0)
				currentErrors += c.count - 1;

		if (nextToGroups > 1)
			currentErrors -= nextToGroups - 1;

		if (checkedTrain != null)
			errorsLocked += currentErrors;
		else
		{
			errorsText.text = currentErrors.ToString ();
			
			if (currentErrors == 0)
				errorsTextParent.DOScale (0, MenuManager.Instance.menuAnimationDuration).SetEase (MenuManager.Instance.menuEase);
			else
				errorsTextParent.DOScale (1, MenuManager.Instance.menuAnimationDuration).SetEase (MenuManager.Instance.menuEase);
		}
	}

	void CheckTrainConstraints (Train train)
	{
		Container previousContainer = null;
		int nextToPreviousType = 0;

		foreach(var container in train.containers)
		{
			bool hasNextToNotRespected = false;

			if(container != null && previousContainer != container)
			{
				foreach(var constraint in container.constraints)
				{
					if (!constraint.isRespected)
					{
						bool special = false;
						
						if (ConstraintType.NotNextTo_Constraint.ToString () == constraint.constraint.GetType ().ToString ())
							hasNextToNotRespected = true;
						
						foreach(var c in specialConstraint)
						{
							if(c.constraintType.ToString () == constraint.constraint.GetType ().ToString ())
							{
								c.count++;
								special = true;
								
								break;
							}
						}
						
						if(!special)
							currentErrors++;
						
					}
				}
			}

			//Next To Groups
			if(container == null || container != null && previousContainer != container)
			{
				if (hasNextToNotRespected)
				{
					if (nextToPreviousType == 0)
						nextToPreviousType = 1;
					
					else if (nextToPreviousType == 1)
						nextToPreviousType = 2;
				}
				else 
				{
					if(nextToPreviousType == 2)
					{
						nextToPreviousType = 0;
						nextToGroups++;
					}

					else if (nextToPreviousType == 1)
						nextToPreviousType = 0;
				}

				if(container != null && container.spotOccupied._spotTrainIndex == train.containers.Count - 1
					|| container != null && container.isDoubleSize && container.spotOccupied._spotTrainIndex == train.containers.Count - 2)
				{
					if(nextToPreviousType == 1 || nextToPreviousType == 2)
						nextToGroups++;
				}
			}

			previousContainer = container;
		}

		//Wagons Overweight
		foreach(var w in train.wagons)
		{
			if(w.overweight)
				currentErrors++;
		}
	}

	public void OrderSent (Order_Level orderLevel)
	{
		foreach(var o in orders)
		{
			if(o == orderLevel)
			{
				o.isPrepared = true;
				return;
			}
		}
	}

	public void LevelEnd (LevelEndType levelEndType)
	{
		if (GameManager.Instance.gameState == GameState.Menu)
			return;
		
		ScoreManager.Instance.UnlockStars (OrdersManager.Instance.ordersSentCount, trainsUsed, levelIndex);

		GameManager.Instance.LevelEnd (levelEndType);

		MenuManager.Instance.EndLevel ();
	}

	IEnumerator LevelDuration ()
	{
		levelDuration = 0;

		yield return new WaitUntil (() => GameManager.Instance.gameState == GameState.Playing);

		do
		{
			yield return new WaitWhile (() => GameManager.Instance.gameState == GameState.Pause);

			yield return new WaitForSecondsRealtime (1f);
			
			levelDuration++;
		}
		while (GameManager.Instance.gameState == GameState.Playing);
	}

	#region Level Start	
	[ButtonGroup ("1", -1)]
	public void LoadLevel ()
	{
		LoadLevelSettings (levelToStart);
	}

	[ButtonGroup ("1", -1)]
	public void NextLevel ()
	{
		if (levelIndex + 1 >= transform.childCount)
		{
			Debug.LogWarning ("Invalid Level Index!");
			return;
		}

		LoadLevelSettings (levelIndex + 1);
	}
	#endregion

	#region Other
	[PropertyOrder (-1)]
	[ButtonAttribute]
	void RenameLevels ()
	{
		for (int i = 0; i < transform.childCount; i++)
			transform.GetChild (i).name = "Level #" + (i + 1).ToString ();
	}
	#endregion

	[System.Serializable]
	public class SpecialConstraint
	{
		public ConstraintType constraintType;
		public int groupCount = 0;
		public int count = 0;
	}
}
