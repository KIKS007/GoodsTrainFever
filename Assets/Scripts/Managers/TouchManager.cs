﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TouchManager : Singleton<TouchManager>
{
	public Action OnTouchDown;
	public Action<Vector3> OnTouchMoved;
	public Action<Vector3> OnTouchHold;
	public Action OnTouchUp;
	public Action OnTouchUpNoTarget;

	public bool useRaycast = false;
	public bool isTouchingTouchable = false;

	private bool _touchDown = false;
	private Vector3 _deltaPosition;
	private Vector3 _mousePosition;
	private Camera _camera;

	void Start ()
	{
		//Application.targetFrameRate = 30;

		_camera = FindObjectOfType<Camera> ();
	}

	// Update is called once per frame
	void Update () 
	{
		#if UNITY_EDITOR
		if(Application.isEditor && !UnityEditor.EditorApplication.isRemoteConnected)
			MouseHold ();
		else
			TouchHold ();
		#else
		TouchHold ();
		#endif
	}

	void TouchHold ()
	{
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch (0);

			_deltaPosition = touch.deltaPosition;

			switch (touch.phase)
			{
			case TouchPhase.Began:
				
				_touchDown = true;
				
				if(useRaycast)
				{
					Touchable touchable = RaycastTouchable (touch.position);
					if (touchable != null)
						touchable.OnTouchDown ();

				}
				
				if (OnTouchDown != null)
					OnTouchDown ();
				
				StartCoroutine (TouchHoldCoroutine ());
				
				break;
				
			case TouchPhase.Moved:
				
				if (OnTouchMoved != null)
					OnTouchMoved (_deltaPosition);
				
				break;
				
			case TouchPhase.Ended:
				
				_touchDown = false;
				
				if(useRaycast)
				{
					Touchable touchable = RaycastTouchable (touch.position);
					if (touchable != null)
						touchable.OnTouchUpAsButton ();
				}
				
				if (OnTouchUpNoTarget != null && !isTouchingTouchable)
					OnTouchUpNoTarget ();
				
				if (OnTouchUp != null)
					OnTouchUp ();
				
				isTouchingTouchable = false;
				
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

			if(useRaycast)
			{
				Touchable touchable = RaycastTouchable (_mousePosition);
				if (touchable != null)
					touchable.OnTouchDown ();
			}

			if (OnTouchDown != null)
				OnTouchDown ();
		}
		
		else if(Input.GetMouseButtonUp (0))
		{
			_touchDown = false;

			if(useRaycast)
			{
				Touchable touchable = RaycastTouchable (_mousePosition);
				if (touchable != null)
					touchable.OnTouchUpAsButton ();
			}

			if (OnTouchUpNoTarget != null && !isTouchingTouchable)
				OnTouchUpNoTarget ();

			if (OnTouchUp != null)
				OnTouchUp ();
			
			isTouchingTouchable = false;
		}

		else if(Input.GetMouseButton (0))
		{
			_deltaPosition = Input.mousePosition - _mousePosition; 
			
			if (OnTouchHold != null)
				OnTouchHold (_deltaPosition);
			
			_mousePosition = Input.mousePosition;
		}

	}

	Touchable RaycastTouchable (Vector3 position, LayerMask mask)
	{
		RaycastHit hit;
		Ray ray = _camera.ScreenPointToRay (position);

		if (Physics.Raycast (ray, out hit, Mathf.Infinity, mask)) 
		{
			Touchable touchable = hit.collider.GetComponent<Touchable>();

			if (touchable == null && hit.rigidbody)
				touchable = hit.rigidbody.gameObject.GetComponent<Touchable>();
			
			if (touchable != null)
				return touchable;
			else
				return null;
		}
		else
			return null;
	}

	Touchable RaycastTouchable (Vector3 position)
	{
		RaycastHit hit;
		Ray ray = _camera.ScreenPointToRay (position);

		if (Physics.Raycast (ray, out hit, Mathf.Infinity)) 
		{
			Touchable touchable = hit.collider.GetComponent<Touchable>();

			if (touchable == null && hit.rigidbody)
				touchable = hit.rigidbody.gameObject.GetComponent<Touchable>();

			if (touchable != null)
				return touchable;
			else
				return null;
		}
		else
			return null;
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
