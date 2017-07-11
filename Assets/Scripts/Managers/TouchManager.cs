using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum HoldState { None, Holding, SwipingRight, SwipingLeft }

public class TouchManager : Singleton<TouchManager>
{
	[Header ("States")]
	public GameObject target;
	public HoldState holdState = HoldState.None;

	[Header ("Touch Layer")]
	public LayerMask touchRaycastLayer;

	private Vector3 _mousePosition;
	private Vector3 _mouseDeltaPosition;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Application.isEditor)
			MouseHold ();
		else
			TouchHold ();
	}

	void TouchHold ()
	{
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);

			switch (touch.phase)
			{
			case TouchPhase.Began:
				
				holdState = HoldState.Holding;

				break;

			case TouchPhase.Moved:

				if (touch.deltaPosition.x < 0)
					holdState = HoldState.SwipingLeft;
				
				else if(touch.deltaPosition.x > 0)
					holdState = HoldState.SwipingRight;

				else
					holdState = HoldState.Holding;
				
				break;

			case TouchPhase.Ended:

				holdState = HoldState.None;

				break;
			}
		}
	}

	void MouseHold ()
	{
		if(Input.GetMouseButtonDown (0))
		{
			holdState = HoldState.Holding;
			_mousePosition = Input.mousePosition;
		}

		if(Input.GetMouseButtonUp (0))
			holdState = HoldState.None;

		if(Input.GetMouseButton (0))
		{
			_mouseDeltaPosition = Input.mousePosition - _mousePosition; 

			if (_mouseDeltaPosition.x < 0)
				holdState = HoldState.SwipingLeft;
			
			else if (_mouseDeltaPosition.x > 0)
				holdState = HoldState.SwipingRight;

			else
				holdState = HoldState.Holding;
			
			_mousePosition = Input.mousePosition;
		}
	}
}
