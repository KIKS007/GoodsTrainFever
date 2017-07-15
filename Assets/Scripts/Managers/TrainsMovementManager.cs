using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using DG.Tweening;

public enum HoldState { None, Touched, Holding, SwipingRight, SwipingLeft }

public class TrainsMovementManager : Singleton<TrainsMovementManager>
{
	public Action OnTrainMovementStart;
	public Action OnTrainMovementEnd;

	[Header ("States")]
	public HoldState holdState = HoldState.None;
	public float holdDelay = 0.5f;

	[Header ("Trains")]
	public List<Train> allTrains = new List<Train> ();
	public Train selectedTrain = null;

	[Header ("Touch Settings")]
	public float deltaMouvementThreshold;
	public float deltaMousePositionFactor = 1;
	public float deltaTouchPositionFactor = 1;

	[Header ("Movement")]
	public float movementLerp = 0.1f;
	public float movementDeceleration = 0.9f;

	[Header ("Reset Movement")]
	public bool resetingTrains = false;
	public float resetDuration = 0.5f;
	public Ease resetEase = Ease.OutQuad;

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
		TouchManager.Instance.OnTouchUp += TouchUp;

		if (Application.isEditor)
			TouchManager.Instance.OnTouchHold += TouchHold;
		else
			TouchManager.Instance.OnTouchMoved += TouchHold;


		ContainersMovementManager.Instance.OnContainerMovement += ResetTrainsVelocity;
	}

	// Update is called once per frame
	void FixedUpdate () 
	{
		if (ContainersMovementManager.Instance.containerInMotion)
			return;

		if (resetingTrains)
			return;

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

	public void ResetTrainsPosition ()
	{
		foreach (var t in allTrains)
			t.transform.DOMoveX (t._xInitialPosition, resetDuration).SetEase (resetEase);
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
