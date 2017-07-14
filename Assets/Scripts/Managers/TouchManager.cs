using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TouchManager : Singleton<TouchManager>
{
	public Action OnTouchDown;
	public Action<Vector3> OnTouchMoved;
	public Action<Vector3> OnTouchHold;
	public Action OnTouchUp;

	private bool _touchDown = false;
	private Vector3 _deltaPosition;
	private Vector3 _mousePosition;

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

				_touchDown = true;
				
				if (OnTouchDown != null)
					OnTouchDown ();

				StartCoroutine (TouchHoldCoroutine ());

				break;

			case TouchPhase.Moved:

				_deltaPosition = touch.deltaPosition;

				if (OnTouchMoved != null)
					OnTouchMoved (_deltaPosition);

				break;

			case TouchPhase.Ended:

				_touchDown = false;
				
				if (OnTouchUp != null)
					OnTouchUp ();

				break;
			}
		}
	}

	void MouseHold ()
	{
		if(Input.GetMouseButtonDown (0))
		{
			_touchDown = true;

			_mousePosition = Input.mousePosition;

			if (OnTouchDown != null)
				OnTouchDown ();
		}
		
		if(Input.GetMouseButtonUp (0))
		{
			_touchDown = false;

			if (OnTouchUp != null)
				OnTouchUp ();
		}

		if(Input.GetMouseButton (0))
		{
			_deltaPosition = Input.mousePosition - _mousePosition; 
			
			if (OnTouchHold != null)
				OnTouchHold (_deltaPosition);
			
			_mousePosition = Input.mousePosition;
		}


	}

	IEnumerator TouchHoldCoroutine ()
	{
		while (_touchDown)
		{
			if (OnTouchHold != null)
				OnTouchHold (_deltaPosition);
			
			yield return new WaitForEndOfFrame ();
		}
	}
}
