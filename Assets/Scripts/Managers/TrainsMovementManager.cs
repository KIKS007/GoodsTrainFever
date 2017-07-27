using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public enum HoldState { None, Touched, Holding, SwipingRight, SwipingLeft }

public class TrainsMovementManager : Singleton<TrainsMovementManager>
{
	public Action<Train> OnTrainDeparture;

	[Header ("Trains")]
	public List<Train> allTrains = new List<Train> ();
	public Train selectedTrain = null;
	public bool selectedTrainHasMoved = false;
	public Train trainContainerInMotion;

	[Header ("Rails")]
	public Rail rail1;
	public Rail rail2;

	[Header ("Train Buttons")]
	public Button rail1Button;
	public Button rail2Button;
	public Text rail1Text;
	public Text rail2Text;

	[Header ("Hold")]
	public HoldState holdState = HoldState.None;
	public float holdDelay = 0.5f;

	[Header ("Touch Settings")]
	public float touchMovementThreshold = 2f;
	public float trainMovementThreshold = 4f;
	public float deltaMousePositionFactor = 1;
	public float deltaTouchPositionFactor = 1;

	[Header ("Hold Movement")]
	public float movementMaxVelocity = 5f;
	public float movementDeceleration = 0.9f;

	[Header ("Reset Movement")]
	public bool resetingTrains = false;
	public float resetDuration = 0.5f;
	public Ease resetEase = Ease.OutQuad;

	[Header ("Train Spawn")]
	public float xArrivingPosition;
	public float xDeparturePosition1;
	public float xDeparturePosition2;
	public float arrivingSpeed = 0.5f;
	public float arrivingDelay = 0;

	[Header ("Fast Forward")]
	public float fastForwardSpeed;
	public Ease trainMovementEase = Ease.OutQuad;

	[Header ("Train Length")]
	public float wagonFourtyLength;
	public float wagonSixtyLength;
	public float wagonEightyLength;
	public float locomotiveLength = 10f;
	public float offsetLength = 10f;

	[Header ("Prefabs")]
	public GameObject trainPrefab;
	public GameObject wagonFourtyPrefab;
	public GameObject wagonSixtyPrefab;
	public GameObject wagonEightyPrefab;

	[Header ("Train Values")]
	public InputField touchMovementThresholdInput;
	public InputField deltaTouchPositionFactorInput;
	public InputField movementMaxVelocityInput;
	public InputField movementDecelerationInput;

	private Vector3 _deltaPosition;
	private Dictionary<Train, float> _trainsVelocity = new Dictionary<Train, float> ();
	private List<System.Collections.IEnumerator> _trainsDurationCoroutines = new List<System.Collections.IEnumerator> ();

	void Start ()
	{
		allTrains = FindObjectsOfType<Train> ().ToList ();
		_trainsVelocity.Clear ();

		foreach (var t in allTrains)
			_trainsVelocity.Add (t, 0);

		TouchManager.Instance.OnTouchDown += TouchDown;
		TouchManager.Instance.OnTouchUp += TouchUp;

		TouchManager.Instance.OnTouchHold += TouchHold;
		/*if (Application.isEditor)
		else
			TouchManager.Instance.OnTouchMoved += TouchHold;*/

		ContainersMovementManager.Instance.OnContainerMovement += ResetTrainsVelocity;
		ContainersMovementManager.Instance.OnContainerMovementEnd += ()=> trainContainerInMotion = null;

		touchMovementThresholdInput.text = touchMovementThreshold.ToString ();
		deltaTouchPositionFactorInput.text = deltaTouchPositionFactor.ToString ();
		movementMaxVelocityInput.text = movementMaxVelocity.ToString ();
		movementDecelerationInput.text = movementDeceleration.ToString ();

		touchMovementThresholdInput.onValueChanged.AddListener ((string arg0) => touchMovementThreshold = float.Parse (arg0));
		deltaTouchPositionFactorInput.onValueChanged.AddListener ((string arg0) => deltaTouchPositionFactor = float.Parse (arg0));
		movementMaxVelocityInput.onValueChanged.AddListener ((string arg0) => movementMaxVelocity = float.Parse (arg0));
		movementDecelerationInput.onValueChanged.AddListener ((string arg0) => movementDeceleration = float.Parse (arg0));
	}

	public void AddTrain (Train train)
	{
		allTrains.Add (train);
		_trainsVelocity.Add (train, 0);
	}

	public void RemoveTrain (Train train)
	{
		allTrains.Remove (train);
		_trainsVelocity.Remove (train);
	}

	// Update is called once per frame
	void FixedUpdate () 
	{
		if (resetingTrains)
			return;

		if (TouchManager.Instance.isTouchingUI)
			return;
		
		SetTrainsVelocity ();

		if (holdState != HoldState.None)
		{
			if(selectedTrain && trainContainerInMotion != selectedTrain)
				MoveTrain (selectedTrain);
			
			else
			{
				foreach (var t in allTrains)
					if(trainContainerInMotion != t)
						MoveTrain (t);
			}
		}
	}

	void SetTrainsVelocity ()
	{
		foreach(var t in allTrains)
		{
			if(t == null)
				continue;

			if (t.inTransition)
				continue;

			if (Mathf.Abs (_trainsVelocity [t]) > movementMaxVelocity)
				_trainsVelocity [t] = movementMaxVelocity * Mathf.Sign (_trainsVelocity [t]);

			_trainsVelocity [t] *= movementDeceleration;

			Vector3 position = t.transform.position;
			position = new Vector3 ();
			position.x += _trainsVelocity [t];

			if (holdState == HoldState.None || holdState == HoldState.Touched || holdState == HoldState.Holding)
				t.transform.Translate (position * Time.fixedDeltaTime);

			if (Mathf.Abs (_deltaPosition.x) > trainMovementThreshold)
			{
				if (selectedTrain && t == selectedTrain || selectedTrain == null)
					selectedTrainHasMoved = true;
			}
		}
	}

	void ResetTrainsVelocity ()
	{
		foreach (var t in allTrains)
			_trainsVelocity [t] = 0;
	}

	public void ResetTrainsPosition ()
	{
		if (ContainersMovementManager.Instance.containerInMotion)
			return;

		resetingTrains = true;

		ResetTrainsVelocity ();

		foreach (var t in allTrains)
		{
			if (t == null)
				continue;

			if (t.inTransition)
				continue;
			
			if(t.transform.position.z > -5)
				t.transform.DOMoveX (xDeparturePosition1, resetDuration).SetEase (resetEase).OnComplete (()=> resetingTrains = false);
			else
				t.transform.DOMoveX (xDeparturePosition2, resetDuration).SetEase (resetEase).OnComplete (()=> resetingTrains = false);
		}
	}

	void TouchDown ()
	{
		StopCoroutine (HoldDelay ());
		StartCoroutine (HoldDelay ());

		holdState = HoldState.Touched;
	}

	void TouchHold (Vector3 deltaPosition)
	{
		_deltaPosition = deltaPosition;

		if (_deltaPosition.x < 0)
		{
			holdState = HoldState.SwipingLeft;
		}

		else if(_deltaPosition.x > 0)
		{
			holdState = HoldState.SwipingRight;
		}

		else if(holdState != HoldState.Touched)
		{
			holdState = HoldState.Holding;

			//ResetTrainsVelocity ();
		}
	}

	void TouchUp ()
	{
		holdState = HoldState.None;

		if (selectedTrain)
			selectedTrain = null;

		selectedTrainHasMoved = false;
		_deltaPosition = Vector3.zero;
	}

	IEnumerator HoldDelay ()
	{
		yield return new WaitForSecondsRealtime (holdDelay);

		if(holdState == HoldState.Touched)
			holdState = HoldState.Holding;
	}

	void MoveTrain (Train train)
	{
		if (train.inTransition)
			return;

		if (train == null || holdState == HoldState.Touched)
			return;

		if (Mathf.Abs (_deltaPosition.x) < touchMovementThreshold)
			return;

		Vector3 position = train.transform.position;

		position = new Vector3 ();

		#if UNITY_EDITOR
		if(Application.isEditor && !UnityEditor.EditorApplication.isRemoteConnected)
		{
			position.x += _deltaPosition.x * deltaMousePositionFactor;

			_trainsVelocity [train] = _deltaPosition.x * deltaMousePositionFactor;
		}
		else
		{
			position.x += _deltaPosition.x * deltaTouchPositionFactor;

			_trainsVelocity [train] = _deltaPosition.x * deltaTouchPositionFactor;
		}
		#else
			position.x += _deltaPosition.x * deltaTouchPositionFactor;

			_trainsVelocity [train] = _deltaPosition.x * deltaTouchPositionFactor;
		#endif

		if (Mathf.Abs (_deltaPosition.x) > trainMovementThreshold)
		{
			if (selectedTrain && train == selectedTrain)
				selectedTrainHasMoved = true;
		}

		train.transform.Translate (position * Time.fixedDeltaTime);
	}

	public Train SpawnTrain (Rail rail, Train_Level train_Level, bool waitOtherTrain = false)
	{
		if (rail.train != null)
		{
			Debug.LogWarning ("Rail has train!", this);
			return null;
		}

		Vector3 position = rail.transform.position;
		position.y = trainPrefab.transform.position.y;
		position.x = xArrivingPosition;

		GameObject train = Instantiate (trainPrefab, position, trainPrefab.transform.rotation, GlobalVariables.Instance.gameplayParent);

		Train trainScript = train.GetComponent<Train> ();
		trainScript.inTransition = true;

		float trainLength = locomotiveLength;

		Vector3 wagonPosition = position;
		wagonPosition.x -= locomotiveLength;

		foreach(var w in train_Level.wagons)
		{
			GameObject prefab = wagonFourtyPrefab;
			float wagonLength = 0;

			switch (w.wagonType)
			{
			case WagonType.Fourty:
				prefab = wagonFourtyPrefab;
				wagonLength = wagonFourtyLength;
				break;
			case WagonType.Sixty:
				prefab = wagonSixtyPrefab;
				wagonLength = wagonSixtyLength;
				break;
			case WagonType.Eighty:
				prefab = wagonSixtyPrefab;
				wagonLength = wagonEightyLength;
				break;
			}

			trainLength += wagonLength;
			wagonPosition.x -= wagonLength;

			GameObject wagon = Instantiate (prefab, wagonPosition, prefab.transform.rotation, trainScript.wagonsParent);
			Wagon wagonScript = wagon.GetComponent<Wagon> ();
			trainScript.wagons.Add (wagonScript);

			wagonScript.maxWeight = w.wagonMaxWeight;
		}

		trainScript.trainLength = trainLength;

		rail.train = trainScript;
		TrainsMovementManager.Instance.AddTrain (trainScript);

		if(rail == rail1)
			rail1Text.text = "";
		else
			rail2Text.text = "";

		float departurePosition = rail == rail1 ? xDeparturePosition1 : xDeparturePosition2;

		train.transform.DOMoveX (departurePosition, arrivingSpeed).SetEase (trainMovementEase).SetDelay (arrivingDelay).OnComplete (()=> trainScript.inTransition = false).SetSpeedBased ();

		_trainsDurationCoroutines.Add ( TrainDuration (rail, train_Level.trainDuration, waitOtherTrain) );

		StartCoroutine (_trainsDurationCoroutines [_trainsDurationCoroutines.Count - 1]);

		return trainScript;
	}

	IEnumerator TrainDuration (Rail rail, int duration, bool waitOtherTrain = false)
	{
		float time = Mathf.Round (Time.time) + 1.5f;

		yield return new WaitUntil (() => Time.time >= time);

		yield return new WaitWhile (() => rail.train.inTransition);

		Text trainText = null;
		Button trainButton = null;

		if (rail == rail1)
		{
			trainButton = rail1Button;
			trainText = rail1Text;
		}
		else
		{
			trainButton = rail2Button;
			trainText = rail2Text;
		}

		if (!trainButton.interactable)
			trainButton.interactable = true;

		trainText.text = duration.ToString ();

		do
		{
			yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

			yield return new WaitForSecondsRealtime (1f);

			if(rail.train == null || !rail.train.waitingDeparture)
				yield break;

			yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

			duration--;
			trainText.text = duration.ToString ();
		}
		while (duration > 0);

		if(rail.train != null && !rail.train.inTransition)
		{
			trainButton.interactable = false;
			FastForwardTrain (rail);
		}
	}

	public void FastForwardTrain (Rail rail)
	{
		StartCoroutine (FastForwardTrainCoroutine (rail));
	}

	IEnumerator FastForwardTrainCoroutine (Rail rail)
	{
		if (rail.train == null)
			yield break;

		rail.train.inTransition = true;
		rail.train.waitingDeparture = false;

		if (OnTrainDeparture != null)
			OnTrainDeparture (rail.train);

		if(rail == rail1)
			rail1Text.text = "Sent";
		else
			rail2Text.text = "Sent";

		if (trainContainerInMotion == rail.train)
			yield return new WaitUntil (()=> trainContainerInMotion != rail.train);

		float xPosition = xDeparturePosition1 + rail.train.trainLength + offsetLength;

		rail.train.transform.DOMoveX (xPosition, fastForwardSpeed).SetEase (trainMovementEase).SetSpeedBased ().OnComplete (()=> 
			{
				RemoveTrain (rail.train);
				Destroy (rail.train.gameObject);
			});

		yield return 0;
	}

	public void ClearTrains ()
	{
		ClearTrainsDuration ();

		if(rail1.train)
		{
			GameObject t = rail1.train.gameObject;
			RemoveTrain (rail1.train);
			rail1.train = null;
			Destroy (t);
		}

		if(rail2.train)
		{
			GameObject t = rail2.train.gameObject;
			RemoveTrain (rail2.train);
			rail2.train = null;
			Destroy (t);
		}
	}

	public void ClearTrainsDuration ()
	{
		foreach (var c in _trainsDurationCoroutines)
			StopCoroutine (c);

		_trainsDurationCoroutines.Clear ();
	}
}
