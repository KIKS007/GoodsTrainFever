using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
using DarkTonic.MasterAudio;

public class LevelsManager : Singleton<LevelsManager>
{
	public int levelToStart = 0;
	public bool loadLevelOnStart = false;
	public bool clearLevelOnStart = true;

	[Header ("Level Index")]
	public int levelIndex;
	public int levelsCount;

	[Header ("Levels")]
	public Level currentLevel;
	public Level currentHandmadeLevel;
	public LevelGenerated currentLevelGenerated;

	[Header ("Tutorials")]
	public UnityEvent[] Tutorials;

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
	public Text trainsToSendText;
	//Replace with icon when made
	public GameObject trainsToSendIcon;

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

	private Text CurrentBoatTimer;

	//public	List<Spot> spots = new List<Spot> ();
	//public List<Spot> spotsTemp = new List<Spot> ();

	// Use this for initialization
	void Awake ()
	{
		_storage = FindObjectOfType<Storage> ();
		_boat = FindObjectOfType<Boat> ();

		levelsCount = transform.childCount;

		if (clearLevelOnStart)
			ClearLevel ();

		if (loadLevelOnStart)
			LoadLevel (levelToStart);

		Container.OnContainerMoved += () => DOVirtual.DelayedCall (0.01f, () => CheckConstraints ());
		MenuManager.Instance.OnLevelStart += () => StartCoroutine (LevelDuration ());
		MenuManager.Instance.OnMainMenu += ClearLevel;
		MenuManager.Instance.OnMainMenu += () => _previousRandomColorOffset.Clear ();
		GameManager.Instance.OnLevelEnd += () => TutorialManager.Instance.ForceStop ();
		errorsText.text = "0";
		errorsTextParent.localScale = Vector3.zero;
	}

	void ClearLevel ()
	{
		StopAllCoroutines ();

		trainsUsed = 0;
		levelDuration = 0;
		currentErrors = 0;
		errorsLocked = 0;

		UpdateTrainSendCount (0);

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

	public void LoadLevel (int index)
	{
		if (index > transform.childCount - 1) {
			Debug.LogError ("Invalid Level!");
			return;
		}

		MenuManager.Instance.menulevels.SaveMenuPos ();

		KillBoatCountdown ();

		//TUTORIAL LAUNCH
		if (index < Tutorials.Count ())
			Tutorials [index].Invoke ();

		ClearLevel ();

		if (transform.GetChild (index).GetComponent<LevelHandmade> () != null)
			LoadLevelHandmade (index);
		else
			LoadGeneratedLevel (index);
	}

	public void LoadLevelHandmade (int index)
	{
		levelIndex = index;

		if (randomColors) {
			do {
				_randomColorOffset = Random.Range (0, 4);

			} while (_previousRandomColorOffset.Contains (_randomColorOffset));

			_previousRandomColorOffset.Add (_randomColorOffset);

			if (_previousRandomColorOffset.Count > 3)
				_previousRandomColorOffset.RemoveAt (0);

		} else
			_randomColorOffset = 0;

		LevelHandmade level = transform.GetChild (index).GetComponent<LevelHandmade> ();

		currentHandmadeLevel = level;
		currentLevel = currentHandmadeLevel;

		currentLevelGenerated = null;

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


		errorsSecondStarAllowed = Mathf.RoundToInt (errorsAllowed * 0.5f);

		foreach (var o in orders)
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

		containers.AddRange (storageContainers);

		StartCoroutine (FillContainerZone (containers, _storage.transform, _storage.containersParent));

		//Orders
		foreach (var o in orders)
			StartCoroutine (AddOrder (o));

		//Trains
		if (rail1Trains.Count > 0) {
			_rail1Occupied = true;

			UpdateTrainSendCount (trainsToSend + rail1Trains.Count);
			StartCoroutine (SpawnTrains (rail1Trains, TrainsMovementManager.Instance.rail1));
		}

		if (rail2Trains.Count > 0) {
			_rail2Occupied = true;

			UpdateTrainSendCount (trainsToSend + rail2Trains.Count);
			StartCoroutine (SpawnTrains (rail2Trains, TrainsMovementManager.Instance.rail2));
		}

		//Boats
		if (boats.Count > 0)
			StartCoroutine (SpawnBoats ());
	}

	public void LoadGeneratedLevel (int index)
	{
		StartCoroutine (LoadGeneratedLevelCoroutine (index));
	}

	IEnumerator LoadGeneratedLevelCoroutine (int index)
	{
		if (index > transform.childCount - 1) {
			Debug.LogError ("Invalid Level!");
			yield break;
		}

		levelIndex = index;

		currentHandmadeLevel = null;


		LevelSettings_LD levelSettings = transform.GetChild (index).GetComponent<LevelSettings_LD> ();

		LevelsGenerationManager.Instance.GenerateLevel (index, levelSettings);

		yield return new WaitWhile (() => LevelsGenerationManager.Instance.isGeneratingLevel);

		currentLevelGenerated = LevelsGenerationManager.Instance.currentLevelGenerated;
		currentLevel = currentLevelGenerated;

		boatsDuration = currentLevelGenerated.boatsDuration;
		errorsAllowed = currentLevelGenerated.errorsAllowed;

		orders.Clear ();
		storageContainers.Clear ();
		boats.Clear ();

		foreach (var o in currentLevelGenerated.orders)
			orders.Add (new Order_Level (o));

		errorsSecondStarAllowed = Mathf.RoundToInt (errorsAllowed * 0.5f);

		//Orders
		foreach (var o in orders)
			StartCoroutine (AddOrder (o));

		int delay = Random.Range (1, 3);

		//Trains
		if (currentLevelGenerated.rail1Trains.Count > 0) {
			_rail1Occupied = true;

			UpdateTrainSendCount (trainsToSend + currentLevelGenerated.rail1Trains.Count);

			bool trainDelayed = false;
			if (currentLevelGenerated.rail2Trains.Count > 0 && delay == 1)
				trainDelayed = true;

			StartCoroutine (SpawnTrains (currentLevelGenerated.rail1Trains, TrainsMovementManager.Instance.rail1, currentLevelGenerated.trainsDuration, trainDelayed));
		}

		if (currentLevelGenerated.rail2Trains.Count > 0) {
			_rail2Occupied = true;

			UpdateTrainSendCount (trainsToSend + currentLevelGenerated.rail2Trains.Count);

			bool trainDelayed = false;
			if (currentLevelGenerated.rail2Trains.Count > 0 && delay == 2)
				trainDelayed = true;

			StartCoroutine (SpawnTrains (currentLevelGenerated.rail2Trains, TrainsMovementManager.Instance.rail2, currentLevelGenerated.trainsDuration, trainDelayed));
		}

		//Boats
		if (currentLevelGenerated.boats.Count > 0)
			StartCoroutine (SpawnBoats (currentLevelGenerated.boats, currentLevelGenerated.boatsDelay, currentLevelGenerated.boatsDuration));
	}

	void RandomColors (List<Container_Level> containers)
	{
		//Debug.Log ("randomOffset : " + _randomColorOffset);

		for (int i = 0; i < containers.Count; i++)
			containers [i] = LevelsGenerationManager.Instance.RandomColor (containers [i]);

		/*	foreach(var c in containers)
		{
			if (c.containerColor != ContainerColor.Random)
				continue;

			int color = (int)c.containerColor;

			c.containerColor = new ContainerColor ();
			c.containerColor = (ContainerColor)color;

			for(int i = 0; i < _randomColorOffset; i++)
			{
				color++;

				if(color == 5)
					color = 1;
			}

			c.containerColor = (ContainerColor)color;
		}*/
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

		for (int i = 0; i < train_Level.Count; i++) {
			Train train = TrainsMovementManager.Instance.SpawnTrain (rail, train_Level [i]);

			MasterAudio.PlaySoundAndForget ("SFX_TrainIn");

			yield return new WaitWhile (() => train.inTransition);


			yield return new WaitWhile (() => train.waitingDeparture);

			CheckConstraints (train);
			CheckConstraints ();

			trainsUsed++;


			UpdateTrainSendCount (trainsToSend - 1);

			OrdersManager.Instance.TrainDeparture (train.containers);

			if (errorsLocked > errorsAllowed) {
				LevelEnd (LevelEndType.Errors);
				yield break;
			}

			if (trainsToSend == 0)
				GameManager.Instance.EndLevel ();

			if (OrdersManager.Instance.allOrdersSent) {
				LevelEnd (LevelEndType.Orders);
				yield break;
			}

			yield return new WaitUntil (() => train == null);

			if (i != train_Level.Count - 1)
				yield return new WaitForSeconds (waitDurationBetweenTrains);
			else {
				if (rail == TrainsMovementManager.Instance.rail1) {
					_rail1Occupied = false;

					if (_rail2Occupied == false) {
						LevelEnd (LevelEndType.Trains);
						yield break;
					}
				} else {
					_rail2Occupied = false;

					if (_rail1Occupied == false) {
						LevelEnd (LevelEndType.Trains);
						yield break;
					}
				}
			}
		}

		if (OrdersManager.Instance.allOrdersSent) {
			LevelEnd (LevelEndType.Orders);
			yield break;
		}
	}

	IEnumerator SpawnTrains (List<Train> trains, Rail rail, int trainsDuration, bool firstTrainDelay = false)
	{
		yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

		for (int i = 0; i < trains.Count; i++) {
			Train train = trains [i];

			if (firstTrainDelay) {
				firstTrainDelay = false;
				yield return new WaitForSeconds (Random.Range (LevelsGenerationManager.Instance._currentLevelSettings.firstTrainDelay.x, LevelsGenerationManager.Instance._currentLevelSettings.firstTrainDelay.y));
			}

			TrainsMovementManager.Instance.SpawnTrain (rail, train, trainsDuration);

			yield return new WaitWhile (() => train.inTransition);
			CheckConstraints ();


			yield return new WaitWhile (() => train.waitingDeparture);

			CheckConstraints (train);
			CheckConstraints ();

			trainsUsed++;

			UpdateTrainSendCount (trainsToSend - 1);

			OrdersManager.Instance.TrainDeparture (train.containers);

			if (errorsLocked > errorsAllowed) {
				LevelEnd (LevelEndType.Errors);
				yield break;
			}

			if (trainsToSend == 0)
				GameManager.Instance.EndLevel ();

			if (OrdersManager.Instance.allOrdersSent) {
				LevelEnd (LevelEndType.Orders);
				yield break;
			}

			yield return new WaitUntil (() => train == null);

			if (i != trains.Count - 1)
				yield return new WaitForSeconds (waitDurationBetweenTrains);
			else {
				if (rail == TrainsMovementManager.Instance.rail1) {
					_rail1Occupied = false;

					if (_rail2Occupied == false) {
						LevelEnd (LevelEndType.Trains);
						yield break;
					}
				} else {
					_rail2Occupied = false;

					if (_rail1Occupied == false) {
						LevelEnd (LevelEndType.Trains);
						yield break;
					}
				}
			}
		}

		if (OrdersManager.Instance.allOrdersSent) {
			LevelEnd (LevelEndType.Orders);
			yield break;
		}
	}

	public void SetCurrentBoatTimer (Text t)
	{
		CurrentBoatTimer = t;
	}

	IEnumerator SpawnBoats ()
	{
		yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

		foreach (var b in boats) {
			//Debug.Log (_boat.transform.position.y);
			StartCoroutine (FillContainerZone (b.boatContainers, _boat.transform, _boat.containersParent));

			if (b.delay > 0)
				yield return new WaitForSeconds (b.delay);

			BoatsMovementManager.Instance.BoatStart ();

			yield return new WaitWhile (() => BoatsMovementManager.Instance.inTransition);

			float boatDuration = b.overrideDuration & b.duration > 0 ? b.duration : boatsDuration;
			DepartureCountDown (boatDuration);
			yield return new WaitForSeconds (boatDuration + 5);

			BoatsMovementManager.Instance.BoatDeparture ();
			MasterAudio.PlaySound ("SFX_BoatOut");
			yield return new WaitWhile (() => BoatsMovementManager.Instance.inTransition);

			yield return new WaitForSeconds (waitDurationBetweenBoats);
		}
	}

	public void KillBoatCountdown ()
	{
		this.transform.DOKill ();
		if (CurrentBoatTimer != null)
			CurrentBoatTimer.text = "--";
	}

	private void DepartureCountDown (float value)
	{
		this.transform.DOKill ();
		if (value > 0) {
			if (value < 4) {
				MasterAudio.PlaySound ("SFX_BoatTimeOut");
			}


			CurrentBoatTimer.text = value.ToString ();
			this.transform.DOMove (this.transform.position + new Vector3 (Random.Range (-5, 5), Random.Range (-5, 5), Random.Range (-5, 5)), 1).OnComplete (() => {
				DepartureCountDown (value - 1);
			});

		} else {
			CurrentBoatTimer.text = "!";
		}
	}

	IEnumerator SpawnBoats (List<Boat> boats, float delay, float boatDuration)
	{
		yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

		foreach (var b in boats) {
			float duration = boatDuration;

			if (delay > 0)
				yield return new WaitForSeconds (delay);

			BoatsMovementManager.Instance.BoatStart (b);

			yield return new WaitWhile (() => BoatsMovementManager.Instance.inTransition);

			CurrentBoatTimer.text = duration.ToString ();

			while (duration > 0) {

				yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

				yield return new WaitForSeconds (1);

				yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

				duration--;
				CurrentBoatTimer.text = duration.ToString ();

				if (duration < 4) {
					MasterAudio.PlaySound ("SFX_BoatTimeOut");
				}
			}
			CurrentBoatTimer.text = "!";

			yield return new WaitForSeconds (5);
			BoatsMovementManager.Instance.BoatDeparture (b);
			MasterAudio.PlaySound ("SFX_BoatOut");

			yield return new WaitWhile (() => BoatsMovementManager.Instance.inTransition);

			yield return new WaitForSeconds (waitDurationBetweenBoats);
		}
	}

	public void EmptyZone (Transform parent, bool destroyContainers = true)
	{
		foreach (Transform c in parent) {
			var container = c.GetComponent<Container> ();

			if (container != null)
				container.RemoveContainer ();

			if (destroyContainers)
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

		if (spawnDoubleSizeFirst || forceSpawnDoubleFirst) {
			foreach (var c in containers_Base)
				if (c.isDoubleSize)
					containers_Levels.Add (c);

			foreach (var c in containers_Base)
				if (!c.isDoubleSize)
					containers_Levels.Add (c);
		} else
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
		foreach (var containterLevel in containers_Levels) {
			Container container = CreateContainer (containterLevel, containersParent);

			spotsTemp.Clear ();
			spotsTemp.AddRange (spots);

			//Add Spawned Spots
			if (containterLevel.isDoubleSize) {
				foreach (var s in spots) {
					if (s.isDoubleSize) {
						Spot spotSpawned = s.SpawnDoubleSizeSpot (container, false);

						if (spotSpawned != null)
							spotsTemp.Add (spotSpawned);
					}
				}
			}

			//Remove Invalid Spots
			foreach (var s in spots) {
				if (s.isOccupied || !s.IsSameSize (container) || !s.CanPileContainer () || s == null)
					spotsTemp.Remove (s);
			}

			if (spotsTemp.Count == 0) {
				//Debug.LogError ("No more free spots!", this);

				if (!forceSpawnDoubleFirst)
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

	public Container CreateContainer (Container_Level container_Level, Transform parent)
	{
		GameObject prefab = basicContainersPrefabs [0];

		switch (container_Level.containerType) {
		case ContainerType.Basique:
			prefab = container_Level.isDoubleSize ? basicContainersPrefabs [1] : basicContainersPrefabs [0];
			break;
		case ContainerType.Réfrigéré:
			prefab = container_Level.isDoubleSize ? cooledContainersPrefabs [1] : cooledContainersPrefabs [0];
			break;
		case ContainerType.Citerne:
			prefab = container_Level.isDoubleSize ? tankContainersPrefabs [1] : tankContainersPrefabs [0];
			break;
		case ContainerType.Dangereux:
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

		foreach (var c in specialConstraint) {
			c.count = 0;
			c.groupCount = 0;
		}

		if (checkedTrain == null) {
			if (TrainsMovementManager.Instance.rail1.train && TrainsMovementManager.Instance.rail1.train.waitingDeparture)
				CheckTrainConstraints (TrainsMovementManager.Instance.rail1.train);

			if (TrainsMovementManager.Instance.rail2.train && TrainsMovementManager.Instance.rail2.train.waitingDeparture)
				CheckTrainConstraints (TrainsMovementManager.Instance.rail2.train);
		} else
			CheckTrainConstraints (checkedTrain);

		foreach (var c in specialConstraint)
			if (c.count - 1 > 0)
				currentErrors += c.count - 1;

		if (nextToGroups > 1)
			currentErrors -= nextToGroups - 1;

		if (checkedTrain != null)
			errorsLocked += currentErrors;
		else {
			errorsText.text = currentErrors.ToString ();

			if (currentErrors == 0)
				errorsTextParent.DOScale (0, MenuManager.Instance.menuAnimationDuration).SetEase (MenuManager.Instance.menuEase);
			else {
				errorsTextParent.DOScale (1, MenuManager.Instance.menuAnimationDuration).SetEase (MenuManager.Instance.menuEase);
			}
		}
	}

	void CheckTrainConstraints (Train train)
	{
		Container previousContainer = null;
		int nextToPreviousType = 0;

		foreach (var container in train.containers) {
			bool hasNextToNotRespected = false;

			if (container != null && previousContainer != container) {
				foreach (var constraint in container.constraints) {
					if (!constraint.isRespected) {
						bool special = false;
						if (ConstraintType.NotNextTo_Constraint.ToString () == constraint.constraint.GetType ().ToString ())
							hasNextToNotRespected = true;

						foreach (var c in specialConstraint) {
							if (c.constraintType.ToString () == constraint.constraint.GetType ().ToString ()) {
								c.count++;
								special = true;

								break;
							}
						}

						if (!special) {
							currentErrors++;
							container.UpdateErrorDisplay ();
							container.CheckConstraints ();
						}

					}
				}
			}

			//Next To Groups
			if (container == null || container != null && previousContainer != container) {
				if (hasNextToNotRespected) {
					if (nextToPreviousType == 0)
						nextToPreviousType = 1;
					else if (nextToPreviousType == 1)
						nextToPreviousType = 2;
				} else {
					if (nextToPreviousType == 2) {
						nextToPreviousType = 0;
						nextToGroups++;
					} else if (nextToPreviousType == 1)
						nextToPreviousType = 0;
				}

				if (container != null && container.spotOccupied._spotTrainIndex == train.containers.Count - 1
				    || container != null && container.isDoubleSize && container.spotOccupied._spotTrainIndex == train.containers.Count - 2) {
					if (nextToPreviousType == 1 || nextToPreviousType == 2)
						nextToGroups++;
				}
			}

			previousContainer = container;
		}
	}

	public void OrderSent (Order_Level orderLevel)
	{
		foreach (var o in orders) {
			if (o == orderLevel) {
				o.isPrepared = true;
				return;
			}
		}
	}

	public void LevelEnd (LevelEndType levelEndType)
	{
		if (GameManager.Instance.gameState == GameState.Menu)
			return;

		MasterAudio.PlaySound ("SFX_EndLevel");

		TutorialManager.Instance.HideVisualFeedback ();

		ScoreManager.Instance.UnlockStars (OrdersManager.Instance.ordersSentCount, trainsUsed, levelIndex);

		GameManager.Instance.LevelEnd (levelEndType);

		MenuManager.Instance.EndLevel ();
	}

	IEnumerator LevelDuration ()
	{
		levelDuration = 0;

		yield return new WaitUntil (() => GameManager.Instance.gameState == GameState.Playing);

		do {
			if (GameManager.Instance.gameState == GameState.Pause)
				yield return new WaitWhile (() => GameManager.Instance.gameState == GameState.Pause);

			yield return new WaitForSecondsRealtime (1f);

			levelDuration++;
		} while (GameManager.Instance.gameState == GameState.Playing);
	}

	void UpdateTrainSendCount (int count)
	{

		trainsToSend = count;
		trainsToSendText.text = trainsToSend.ToString ();

		if (trainsToSend <= 0) {
			trainsToSendIcon.SetActive (false);
			trainsToSendText.gameObject.SetActive (false);
		} else {
			trainsToSendIcon.SetActive (true);
			trainsToSendText.gameObject.SetActive (true);
		}
		/*if (tmpTRAIN <= 0) {
			trainsToSendIcon.SetActive (false);
			trainsToSendText.gameObject.SetActive (false);
		} else {
			trainsToSendIcon.SetActive (true);
			trainsToSendText.gameObject.SetActive (true);
			trainsToSendText.text = tmpTRAIN.ToString ();

		}*/
	}



	#region Level Start

	[ButtonGroup ("1", -1)]
	public void LoadLevel ()
	{
		LoadLevel (levelToStart);
	}

	public void NextLevel ()
	{
		if (levelIndex + 1 >= transform.childCount) {
			Debug.LogWarning ("Invalid Level Index!");
			return;
		}

		LoadLevel (levelIndex + 1);
	}

	#endregion

	#region Other

	[PropertyOrder (-1)]
	[ButtonAttribute]
	void RenameLevels ()
	{
		for (int i = 0; i < transform.childCount; i++)
			if (transform.GetChild (i).GetComponent<LevelHandmade> () != null)
				transform.GetChild (i).name = "Level #" + (i + 1).ToString ();
			else
				transform.GetChild (i).name = "Level Settings #" + (i + 1).ToString ();
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
