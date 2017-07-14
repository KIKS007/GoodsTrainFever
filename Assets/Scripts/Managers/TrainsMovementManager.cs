using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum HoldState { None, Touched, Holding, SwipingRight, SwipingLeft }

public class TrainsMovementManager : Singleton<TrainsMovementManager>
{
	public Action OnTrainMovementStart;
	public Action OnTrainMovementEnd;

	[Header ("States")]
	public HoldState holdState = HoldState.None;

	[Header ("Train")]
	public Train selectedTrain = null;

	[Header ("Movement")]
	public float deltaMousePositionFactor = 1;
	public float deltaTouchPositionFactor = 1;
	public float movementLerp = 0.1f;

	private Vector3 _mousePosition;
	private Vector3 _deltaPosition;
	private float _holdDelay = 0.3f;

	// Update is called once per frame
	void Update () 
	{
		if (Application.isEditor)
			MouseHold ();
		else
			TouchHold ();

		if (holdState != HoldState.None && selectedTrain)
			MoveTrain (selectedTrain);
	}

	void TouchHold ()
	{
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);

			switch (touch.phase)
			{
			case TouchPhase.Began:
				
				holdState = HoldState.Touched;

				StartCoroutine (HoldDelay ());

				break;

			case TouchPhase.Moved:

				_deltaPosition = touch.deltaPosition;

				if (_deltaPosition.x < 0)
				{
					if (holdState == HoldState.Holding && OnTrainMovementStart != null)
						OnTrainMovementStart ();
					
					holdState = HoldState.SwipingLeft;
				}
				
				else if(_deltaPosition.x > 0)
				{
					if (holdState == HoldState.Holding && OnTrainMovementStart != null)
						OnTrainMovementStart ();

					holdState = HoldState.SwipingRight;
				}

				else if(holdState == HoldState.Touched)
					holdState = HoldState.Holding;
				
				break;

			case TouchPhase.Ended:

				holdState = HoldState.None;

				if (OnTrainMovementEnd != null)
					OnTrainMovementEnd ();

				break;
			}
		}
	}

	void MouseHold ()
	{
		if(Input.GetMouseButtonDown (0))
		{
			holdState = HoldState.Touched;
			_mousePosition = Input.mousePosition;

			StartCoroutine (HoldDelay ());
		}

		if(Input.GetMouseButtonUp (0))
		{
			holdState = HoldState.None;

			if (OnTrainMovementEnd != null)
				OnTrainMovementEnd ();
		}

		if(Input.GetMouseButton (0))
		{
			_deltaPosition = Input.mousePosition - _mousePosition; 

			if (_deltaPosition.x < 0)
			{
				if (holdState == HoldState.Holding && OnTrainMovementStart != null)
					OnTrainMovementStart ();
				
				holdState = HoldState.SwipingLeft;
			}
			
			else if (_deltaPosition.x > 0)
			{
				if (holdState == HoldState.Holding && OnTrainMovementStart != null)
					OnTrainMovementStart ();
				
				holdState = HoldState.SwipingRight;
			}

			else if(holdState == HoldState.Touched)
				holdState = HoldState.Holding;
			
			_mousePosition = Input.mousePosition;
		}
	}

	IEnumerator HoldDelay ()
	{
		yield return new WaitForSecondsRealtime (_holdDelay);

		if(holdState == HoldState.Touched)
			holdState = HoldState.Holding;
	}

	void MoveTrain (Train train)
	{
		if (train == null || holdState == HoldState.Touched)
			return;

		Vector3 position = train.transform.position;

		if (Application.isEditor)
			position.x += _deltaPosition.x * deltaMousePositionFactor;
		else
			position.x += _deltaPosition.x * deltaTouchPositionFactor;

		train.transform.position = Vector3.Lerp (train.transform.position, position, movementLerp);
	}
}
