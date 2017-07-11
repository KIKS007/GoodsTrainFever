using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Touchable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	protected float _holdDelay = 0.1f;
	protected bool _pointerDown = false;
	protected bool _holding = false;

	// Update is called once per frame
	protected virtual void Update () 
	{
		if (_pointerDown)
			OnTouch ();

		if (_holding)
			OnTouchHold ();
	}

	protected virtual void OnTouchDown ()
	{
		Debug.Log ("OnTouchDown");
	}

	protected virtual void OnTouch ()
	{
		Debug.Log ("OnTouch");
	}

	protected virtual void OnTouchHold ()
	{
		Debug.Log ("OnTouchHold");
	}

	protected virtual void OnTouchUp ()
	{
		Debug.Log ("OnTouchUp");
	}

	public void OnPointerDown (PointerEventData eventData)
	{
		OnTouchDown ();
		StartCoroutine (HoldDelay ());
		_pointerDown = true;
		_holding = false;
	}

	public void OnPointerUp (PointerEventData eventData)
	{
		OnTouchUp ();
		StopCoroutine (HoldDelay ());
		_pointerDown = false;
		_holding = false;
	}

	protected IEnumerator HoldDelay ()
	{
		yield return new WaitForSecondsRealtime (_holdDelay);

		if (_pointerDown)
			_holding = true;
	}
}
