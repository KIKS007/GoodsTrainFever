using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public enum HoldState
{
	None,
	Touched,
	Holding,
	SwipingRight,
	SwipingLeft

}

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
	public Text rail1Text;
	public Text rail2Text;
	public GameObject Train1Timer;
	public GameObject Train2Timer;

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

	[Header ("Fast Forward")]
	public Button fastForwardButton;
	public Ease fastForwardEase;
	public float fastForwardTransitionDuration;
	public float fastForwardValue;

	[Header ("Train Spawn")]
	public float xArrivingPosition;
	public float xDeparturePosition1;
	public float xDeparturePosition2;
	public float arrivingSpeed = 0.5f;
	public float arrivingDelay = 0;

	[Header ("Fast Sending")]
	public float trainSendingSpeed;
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

	[Header ("Train Arrow")]
	public float trainsVisibleXPosition;
	public Image train1Arrow;
	public Image train2Arrow;

	private Vector3 _deltaPosition;
	private Dictionary<Train, float> _trainsVelocity = new Dictionary<Train, float> ();
	private List<System.Collections.IEnumerator> _trainsDurationCoroutines = new List<System.Collections.IEnumerator> ();

	void Start ()
	{
		Train1Timer.GetComponentInChildren<CanvasGroup> ().alpha = 0;
		Train2Timer.GetComponentInChildren<CanvasGroup> ().alpha = 0;
		Train1Timer.GetComponent<Timer_UI> ().Hide (0);
		Train2Timer.GetComponent<Timer_UI> ().Hide (0);

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

		GameManager.Instance.OnMenu += () => {
			fastForwardButton.interactable = false;

			if (Time.timeScale != 1)
				FastForward (false);
		};

		train1Arrow.gameObject.SetActive (true);
		train2Arrow.gameObject.SetActive (true);

		train1Arrow.DOFade (0, 0);
		train2Arrow.DOFade (0, 0);

		ContainersMovementManager.Instance.OnContainerMovement += ResetTrainsVelocity;
		ContainersMovementManager.Instance.OnContainerMovementEnd += () => trainContainerInMotion = null;

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

	void Update ()
	{
		if (rail1.train && rail1.train.inTransition || rail2.train && rail2.train.inTransition || GameManager.Instance.gameState != GameState.Playing) {
			if (fastForwardButton.interactable)
				fastForwardButton.interactable = false;
		} else {
			if (!fastForwardButton.interactable)
				fastForwardButton.interactable = true;
		}
	}

	void FixedUpdate ()
	{
		if (resetingTrains)
			return;

		SetTrainsVelocity ();

		if (TouchManager.Instance.isTouchingUI)
			return;

		if (holdState != HoldState.None) {
			//if(selectedTrain && trainContainerInMotion != selectedTrain)
			if (selectedTrain)
				MoveTrain (selectedTrain);
			else {
				foreach (var t in allTrains)
                    //if(trainContainerInMotion != t)
                    MoveTrain (t);
			}
		}
	}

	void SetTrainsVelocity ()
	{
		foreach (var t in allTrains) {
			if (t == null)
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

			if (Mathf.Abs (_deltaPosition.x) > trainMovementThreshold) {
				if (selectedTrain && t == selectedTrain || selectedTrain == null)
					selectedTrainHasMoved = true;
			}

			TrainArrow (t);
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

		foreach (var t in allTrains) {
			if (t == null)
				continue;

			if (t.inTransition)
				continue;

			if (t.transform.position.z > -5)
				t.transform.DOMoveX (xDeparturePosition1, resetDuration).SetEase (resetEase).OnComplete (() => resetingTrains = false);
			else
				t.transform.DOMoveX (xDeparturePosition2, resetDuration).SetEase (resetEase).OnComplete (() => resetingTrains = false);
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

		if (_deltaPosition.x < 0) {
			holdState = HoldState.SwipingLeft;
		} else if (_deltaPosition.x > 0) {
			holdState = HoldState.SwipingRight;
		} else if (holdState != HoldState.Touched) {
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
		yield return new WaitForSeconds (holdDelay);

		if (holdState == HoldState.Touched)
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
		if (Application.isEditor && !UnityEditor.EditorApplication.isRemoteConnected) {
			position.x += _deltaPosition.x * deltaMousePositionFactor;

			_trainsVelocity [train] = _deltaPosition.x * deltaMousePositionFactor;
		} else {
			position.x += _deltaPosition.x * deltaTouchPositionFactor;

			_trainsVelocity [train] = _deltaPosition.x * deltaTouchPositionFactor;
		}
#else
			position.x += _deltaPosition.x * deltaTouchPositionFactor;

			_trainsVelocity [train] = _deltaPosition.x * deltaTouchPositionFactor;
#endif

		if (Mathf.Abs (_deltaPosition.x) > trainMovementThreshold) {
			if (selectedTrain && train == selectedTrain)
				selectedTrainHasMoved = true;
		}

		train.transform.Translate (position * Time.fixedDeltaTime);

	}

	public Train SpawnTrain (Rail rail, Train_Level train_Level)
	{
		if (rail.train != null) {
			Debug.LogWarning ("Rail has train!", this);
			return null;
		}

		Vector3 position = rail.transform.position;
		position.y = trainPrefab.transform.position.y;
		position.x = xArrivingPosition;

		if (rail == rail2)
			position.x += xDeparturePosition2 - xDeparturePosition1;

		GameObject train = Instantiate (trainPrefab, position, trainPrefab.transform.rotation, GlobalVariables.Instance.gameplayParent);

		Train trainScript = train.GetComponent<Train> ();
		trainScript.inTransition = true;

		float trainLength = 0;
		float previousWagonLength = 0;
		float wagonLength = 0;

		Vector3 wagonPosition = position;

		for (int i = 0; i < train_Level.wagons.Count; i++) {
			GameObject prefab = wagonFourtyPrefab;
			float length = 0;

			switch (train_Level.wagons [i].wagonType) {
			case WagonType.Fourty:
				prefab = wagonFourtyPrefab;
				wagonLength = wagonFourtyLength;
				break;
			case WagonType.Sixty:
				prefab = wagonSixtyPrefab;
				wagonLength = wagonSixtyLength;
				break;
			case WagonType.Eighty:
				prefab = wagonEightyPrefab;
				wagonLength = wagonEightyLength;
				break;
			}

			length = wagonLength * 0.5f + previousWagonLength * 0.5f;

			wagonPosition.x -= length;

			GameObject wagon = Instantiate (prefab, wagonPosition, prefab.transform.rotation, trainScript.wagonsParent);

			Wagon wagonScript = wagon.GetComponent<Wagon> ();
			trainScript.wagons.Add (wagonScript);

			wagonScript.maxWeight = train_Level.wagons [i].wagonMaxWeight;


			trainLength += wagonLength;
			previousWagonLength = wagonLength;
		}

		trainLength += locomotiveLength;
		trainScript.trainLength = trainLength;

		trainScript.SetupTrain ();

		rail.train = trainScript;
		TrainsMovementManager.Instance.AddTrain (trainScript);

		if (rail == rail1)
			rail1Text.text = "";
		else
			rail2Text.text = "";

		float departurePosition = rail == rail1 ? xDeparturePosition1 : xDeparturePosition2;

		train.transform.DOMoveX (departurePosition, arrivingSpeed).SetEase (trainMovementEase).SetDelay (arrivingDelay).OnComplete (() => OnTrainArrived (rail, trainScript)).SetSpeedBased ();

		_trainsDurationCoroutines.Add (TrainDuration (rail, train_Level.trainDuration));

		StartCoroutine (_trainsDurationCoroutines [_trainsDurationCoroutines.Count - 1]);

		if (train_Level.parasiteContainers.Count > 0)
			DOVirtual.DelayedCall (0.1f, () => TrainParasiteContainers (trainScript, train_Level));

		return trainScript;
	}

	void TrainParasiteContainers (Train train, Train_Level train_Level)
	{
		foreach (var s in train._allSpots) {
			s.isOccupied = false;
			s.container = null;
		}

		foreach (var c in train_Level.parasiteContainers) {
			var containerLevel = LevelsGenerationManager.Instance.RandomColor (c);

			var container = LevelsManager.Instance.CreateContainer (containerLevel, GlobalVariables.Instance.extraContainersParent);
			bool fillSucess = false;

			int spotsTaken = 0;

			foreach (var s in train._allSpots)
				if (s.isOccupied)
					spotsTaken++;

			if (spotsTaken == train._allSpots.Count)
				continue;

			var spots = new List<Spot> (train._allSpots);

			fillSucess = LevelsGenerationManager.Instance.FillContainer (spots, container);

			if (!fillSucess) {
				Destroy (container.gameObject);
				Debug.LogWarning ("Can't Place All Parasite Containers!");
				break;
			}
		}
	}

	public Train SpawnTrain (Rail rail, Train train, int duration)
	{
		if (rail.train != null) {
			Debug.LogWarning ("Rail has train!", this);
			return null;
		}

		Vector3 position = rail.transform.position;
		position.y = trainPrefab.transform.position.y;
		position.x = xArrivingPosition;

		if (rail == rail2)
			position.x += xDeparturePosition2 - xDeparturePosition1;

		train.transform.position = position;

		Train trainScript = train.GetComponent<Train> ();
		trainScript.inTransition = true;

		rail.train = trainScript;
		TrainsMovementManager.Instance.AddTrain (trainScript);

		if (rail == rail1)
			rail1Text.text = "";
		else
			rail2Text.text = "";

		float departurePosition = rail == rail1 ? xDeparturePosition1 : xDeparturePosition2;

		train.transform.DOMoveX (departurePosition, arrivingSpeed).SetEase (trainMovementEase).SetDelay (arrivingDelay).OnComplete (() => OnTrainArrived (rail, trainScript)).SetSpeedBased ();

		_trainsDurationCoroutines.Add (TrainDuration (rail, duration));

		StartCoroutine (_trainsDurationCoroutines [_trainsDurationCoroutines.Count - 1]);

		return trainScript;
	}

	public List<Train> GenerateTrains (List<Train_LD> trains)
	{
		List<Train> trainsGenerated = new List<Train> ();

		Vector3 position = new Vector3 ();
		position.x = xArrivingPosition;

		foreach (Train_LD train_Level in trains) {
			position.y = trainPrefab.transform.position.y;

			GameObject train = Instantiate (trainPrefab, position, trainPrefab.transform.rotation, GlobalVariables.Instance.gameplayParent);

			Train trainScript = train.GetComponent<Train> ();
			trainsGenerated.Add (trainScript);

			trainScript.inTransition = true;

			float trainLength = 0;
			float previousWagonLength = 0;
			float wagonLength = 0;

			Vector3 wagonPosition = position;

			for (int i = 0; i < train_Level.wagons.Count; i++) {
				GameObject prefab = wagonFourtyPrefab;
				float length = 0;

				switch (train_Level.wagons [i].wagonType) {
				case WagonType.Fourty:
					prefab = wagonFourtyPrefab;
					wagonLength = wagonFourtyLength;
					break;
				case WagonType.Sixty:
					prefab = wagonSixtyPrefab;
					wagonLength = wagonSixtyLength;
					break;
				case WagonType.Eighty:
					prefab = wagonEightyPrefab;
					wagonLength = wagonEightyLength;
					break;
				}

				length = wagonLength * 0.5f + previousWagonLength * 0.5f;

				wagonPosition.x -= length;

				GameObject wagon = Instantiate (prefab, wagonPosition, prefab.transform.rotation, trainScript.wagonsParent);

				Wagon wagonScript = wagon.GetComponent<Wagon> ();
				trainScript.wagons.Add (wagonScript);

				trainLength += wagonLength;
				previousWagonLength = wagonLength;
			}


			trainLength += locomotiveLength;
			trainScript.trainLength = trainLength;

			position.x -= trainLength;
		}

		foreach (var t in trainsGenerated)
			t.SetupTrain ();

		return trainsGenerated;
	}

	void OnTrainArrived (Rail rail, Train trainScript)
	{
		trainScript.inTransition = false;

		TrainArrow (rail.train);
	}

	IEnumerator TrainDuration (Rail rail, int duration)
	{
		float time = Mathf.Round (Time.time) + 1f;

		rail.train.duration = duration;

		yield return new WaitUntil (() => Time.time >= time);

		yield return new WaitWhile (() => rail.train.inTransition);

		Text trainText = null;

		if (rail == rail1) {
			Train1Timer.GetComponent<Timer_UI> ().Show (0.3f);

			Train1Timer.GetComponentInChildren<CanvasGroup> ().DOKill ();
			Train1Timer.GetComponentInChildren<CanvasGroup> ().DOFade (1, 0.2f);
			trainText = rail1Text;

		} else {
			Train2Timer.GetComponent<Timer_UI> ().Show (0.3f);

			Train2Timer.GetComponentInChildren<CanvasGroup> ().DOKill ();
			Train2Timer.GetComponentInChildren<CanvasGroup> ().DOFade (1, 0.2f);
			trainText = rail2Text;
		}

		trainText.text = duration.ToString ();

		while (duration > 0) {
			yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

			yield return new WaitForSeconds (1);


			if (rail.train == null || !rail.train.waitingDeparture) {
				yield break;

			}

			yield return new WaitWhile (() => GameManager.Instance.gameState != GameState.Playing);

			duration--;
			trainText.text = duration.ToString ();

			if (duration < 4)
			if (rail == rail1)
				Train1Timer.GetComponent<Timer_UI> ().Vibrate (duration);
			else
				Train2Timer.GetComponent<Timer_UI> ().Vibrate (duration);
		}

		if (rail.train != null && !rail.train.inTransition)
			SendTrain (rail);
	}

	public void SendTrain (Rail rail)
	{
		if (rail == rail1) {
			Train1Timer.GetComponentInChildren<CanvasGroup> ().DOKill ();
			Train1Timer.GetComponentInChildren<CanvasGroup> ().DOFade (0, 0.6f).SetEase (Ease.Linear).OnComplete (() => {
				Train1Timer.GetComponent<Timer_UI> ().Hide (1);
			});


		} else {
			Train2Timer.GetComponentInChildren<CanvasGroup> ().DOKill ();
			Train2Timer.GetComponentInChildren<CanvasGroup> ().DOFade (0, 0.6f).SetEase (Ease.Linear).OnComplete (() => {
				Train2Timer.GetComponent<Timer_UI> ().Hide (1);
			});

		}
		StartCoroutine (SendTrainCoroutine (rail));
	}

	IEnumerator SendTrainCoroutine (Rail rail)
	{
		if (rail.train == null)
			yield break;

		rail.train.inTransition = true;
		rail.train.waitingDeparture = false;

		fastForwardButton.interactable = false;

		TrainArrow (rail.train);

		if (Time.timeScale != 1) {
			StopFastForward ();
			yield return new WaitWhile (() => DOTween.IsTweening ("FastForward"));
		}

		if (OnTrainDeparture != null)
			OnTrainDeparture (rail.train);

		if (rail == rail1)
			rail1Text.text = "!";
		else
			rail2Text.text = "!";

		if (trainContainerInMotion == rail.train)
			yield return new WaitUntil (() => trainContainerInMotion != rail.train);

		float xPosition = xDeparturePosition1 + rail.train.trainLength + offsetLength;

		rail.train.transform.DOMoveX (xPosition, trainSendingSpeed).SetEase (trainMovementEase).SetSpeedBased ().OnComplete (() => {
			RemoveTrain (rail.train);
			Destroy (rail.train.gameObject);

		});

		yield return 0;
	}

	public void FastForward (bool fastForward)
	{
		if (!fastForwardButton.interactable)
			return;

		if (fastForward) {
			DOTween.Kill ("FastForward");
			DOTween.To (() => Time.timeScale, x => Time.timeScale = x, fastForwardValue, fastForwardTransitionDuration).SetEase (fastForwardEase).SetId ("FastForward").SetUpdate (true);
		} else {
			DOTween.Kill ("FastForward");
			DOTween.To (() => Time.timeScale, x => Time.timeScale = x, 1, fastForwardTransitionDuration).SetEase (fastForwardEase).SetId ("FastForward").SetUpdate (true);
		}
	}

	public void StopFastForward ()
	{
		DOTween.Kill ("FastForward");
		DOTween.To (() => Time.timeScale, x => Time.timeScale = x, 1, fastForwardTransitionDuration).SetEase (fastForwardEase).SetId ("FastForward").SetUpdate (true);
	}

	public void ClearTrains ()
	{
		ClearTrainsDuration ();
		Train1Timer.GetComponentInChildren<CanvasGroup> ().alpha = 0;
		Train2Timer.GetComponentInChildren<CanvasGroup> ().alpha = 0;
		Train2Timer.GetComponent<Timer_UI> ().Hide (0.2f);
		Train1Timer.GetComponent<Timer_UI> ().Hide (0.2f);
		if (rail1.train) {
			GameObject t = rail1.train.gameObject;
			RemoveTrain (rail1.train);
			rail1.train = null;
			Destroy (t);
		}

		if (rail2.train) {
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

	public void TrainArrow (Train train)
	{
		Image trainArrow = null;
		float xPosition = trainsVisibleXPosition;

		if (train == rail1.train)
			trainArrow = train1Arrow;
		else {
			trainArrow = train2Arrow;
			xPosition += xDeparturePosition2 - xDeparturePosition1;
		}

		if (train.inTransition && !train.waitingDeparture) {
			if (trainArrow.color.a == 1)
				trainArrow.DOFade (0, MenuManager.Instance.menuAnimationDuration);

			return;
		}

		if (train.transform.position.x - train.trainLength < xPosition) {
			if (trainArrow.color.a == 0)
				trainArrow.DOFade (1, MenuManager.Instance.menuAnimationDuration);

			return;
		} else {
			if (trainArrow.color.a == 1)
				trainArrow.DOFade (0, MenuManager.Instance.menuAnimationDuration);

			return;
		}
	}
}
