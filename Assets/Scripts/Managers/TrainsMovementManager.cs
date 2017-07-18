using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using DG.Tweening;
using UnityEngine.UI;

public enum HoldState { None, Touched, Holding, SwipingRight, SwipingLeft }

public class TrainsMovementManager : Singleton<TrainsMovementManager>
{
	[Header ("States")]
	public HoldState holdState = HoldState.None;
	public float holdDelay = 0.5f;

	[Header ("Trains")]
	public List<Train> allTrains = new List<Train> ();
	public Train selectedTrain = null;
	public bool selectedTrainHasMoved = false;
	public Train trainContainerInMotion;

	[Header ("Touch Settings")]
	public float touchMovementThreshold = 2f;
	public float trainMovementThreshold = 4f;
	public float deltaMousePositionFactor = 1;
	public float deltaTouchPositionFactor = 1;

	[Header ("Movement")]
	public float movementLerp = 0.1f;
	public float movementMaxVelocity = 5f;
	public float movementDeceleration = 0.9f;

	public float trainZeroVelocity;

	[Header ("Reset Movement")]
	public bool resetingTrains = false;
	public float resetDuration = 0.5f;
	public Ease resetEase = Ease.OutQuad;

	[Header ("Train Values")]
	public InputField touchMovementThresholdInput;
	public InputField deltaTouchPositionFactorInput;
	public InputField movementMaxVelocityInput;
	public InputField movementDecelerationInput;

	public Vector3 _deltaPosition;
	private Dictionary<Train, float> _trainsVelocity = new Dictionary<Train, float> ();

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

	// Update is called once per frame
	void FixedUpdate () 
	{
		//Debug.Log (_deltaPosition.x);

		if (resetingTrains)
			return;
		
		trainZeroVelocity = _trainsVelocity [allTrains [0]];

		SetTrainsVelocity ();
		if (holdState == HoldState.None)
		{
		}

		else
		{
			if(selectedTrain && trainContainerInMotion != selectedTrain)
				MoveTrain (selectedTrain);
			
			else if(!TouchManager.Instance.isTouchingTouchable)
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
			if (Mathf.Abs (_trainsVelocity [t]) > movementMaxVelocity)
				_trainsVelocity [t] = movementMaxVelocity * Mathf.Sign (_trainsVelocity [t]);

			_trainsVelocity [t] *= movementDeceleration;

			Vector3 position = t.transform.position;
			position = new Vector3 ();
			position.x += _trainsVelocity [t];

			//t.transform.position = Vector3.Lerp (t.transform.position, position, movementLerp);

			if (holdState == HoldState.None || holdState == HoldState.Touched || holdState == HoldState.Holding)
				t.transform.Translate (position * Time.fixedDeltaTime);

			if (holdState == HoldState.None || holdState == HoldState.Touched)
				continue;
			
			if (Mathf.Abs (_deltaPosition.x) > trainMovementThreshold)
			{
				if (selectedTrain && t == selectedTrain)
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
			t.transform.DOMoveX (t._xInitialPosition, resetDuration).SetEase (resetEase).OnComplete (()=> resetingTrains = false);
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

		//train.transform.position = Vector3.Lerp (train.transform.position, position, movementLerp);
	}
}
