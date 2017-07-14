using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum HoldState { None, Touched, Holding, SwipingRight, SwipingLeft }

public class TrainsMovementManager : Singleton<TrainsMovementManager>
{
	public Action OnTrainMovementStart;
	public Action OnTrainMovementEnd;

	[Header ("States")]
	public HoldState holdState = HoldState.None;
	private float holdDelay = 0.2f;

	[Header ("Trains")]
	public List<Train> allTrains = new List<Train> ();
	public Train selectedTrain = null;

	[Header ("Movement")]
	public float deltaMouvementThreshold;
	public float deltaMousePositionFactor = 1;
	public float deltaTouchPositionFactor = 1;
	public float movementLerp = 0.1f;
	public float movementDeceleration = 0.9f;

	private Vector3 _mousePosition;
	private Vector3 _deltaPosition;
	private Dictionary<Train, float> _trainsVelocity = new Dictionary<Train, float> ();

	void Start ()
	{
		allTrains = FindObjectsOfType<Train> ().ToList ();
		_trainsVelocity.Clear ();

		foreach (var t in allTrains)
			_trainsVelocity.Add (t, 0);

		TouchManager.Instance.OnTouchDown += TouchDown;
		TouchManager.Instance.OnTouchHold += TouchHold;
		TouchManager.Instance.OnTouchUp += TouchUp;
	}

	// Update is called once per frame
	void Update () 
	{
		if (holdState != HoldState.None)
		{
			if(selectedTrain)
				MoveTrain (selectedTrain);
			else
			{
				foreach (var t in allTrains)
					MoveTrain (t);
			}
		}
		else
			SetTrainsVelocity ();
	}

	void SetTrainsVelocity ()
	{
		foreach(var t in allTrains)
		{
			_trainsVelocity [t] *= movementDeceleration;
			Vector3 position = t.transform.position;
			position.x += _trainsVelocity [t];

			t.transform.position = Vector3.Lerp (t.transform.position, position, movementLerp);
		}
	}

	void ResetTrainsVelocity ()
	{
		if (selectedTrain)
			_trainsVelocity [selectedTrain] = 0;
		else
		{
			foreach (var t in allTrains)
				_trainsVelocity [t] = 0;
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
			if (holdState == HoldState.Touched && OnTrainMovementStart != null)
				OnTrainMovementStart ();

			holdState = HoldState.SwipingLeft;
		}

		else if(_deltaPosition.x > 0)
		{
			if (holdState == HoldState.Touched && OnTrainMovementStart != null)
				OnTrainMovementStart ();

			holdState = HoldState.SwipingRight;
		}

		else if(holdState != HoldState.Touched)
		{
			holdState = HoldState.Holding;
			ResetTrainsVelocity ();
		}
	}

	void TouchUp ()
	{
		holdState = HoldState.None;

		if (OnTrainMovementEnd != null)
			OnTrainMovementEnd ();

		if (selectedTrain)
			selectedTrain = null;
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

		Vector3 position = train.transform.position;

		if (Mathf.Abs (_deltaPosition.x) < deltaMouvementThreshold)
			return;

		if (Application.isEditor)
		{
			position.x += _deltaPosition.x * deltaMousePositionFactor;
			_trainsVelocity [train] = _deltaPosition.x * deltaMousePositionFactor;
			
		}
		else
		{
			position.x += _deltaPosition.x * deltaTouchPositionFactor;
			_trainsVelocity [train] = _deltaPosition.x * deltaTouchPositionFactor;
		}

		train.transform.position = Vector3.Lerp (train.transform.position, position, movementLerp);
	}
}
