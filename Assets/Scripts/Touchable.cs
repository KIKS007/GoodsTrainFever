using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Touchable : MonoBehaviour
{

	protected float _holdDelay = 1f;
	protected bool _pointerDown = false;

	// Update is called once per frame
	protected virtual void Update () 
	{
		
	}

	protected virtual void OnMouseDown ()
	{
		OnTouchDown ();
		StartCoroutine (HoldDelay ());
		_pointerDown = true;
	}

	//protected virtual void OnMouseUpAsButton ()
	protected virtual void OnMouseUp ()
	{
		OnTouchUp ();
		StopCoroutine (HoldDelay ());
		_pointerDown = false;
	}

	protected virtual void OnMouseDrag ()
	{
		OnTouching ();
	}

	protected IEnumerator HoldDelay ()
	{
		yield return new WaitForSecondsRealtime (_holdDelay);

		if (!_pointerDown)
			yield break;

		OnHold ();

		while(_pointerDown)
		{
			OnHolding ();
			yield return new WaitForEndOfFrame ();
		}
	}

	protected virtual void OnTouchDown ()
	{
		Debug.Log ("OnTouchDown");
	}

	protected virtual void OnTouching ()
	{
		Debug.Log ("OnTouching");
	}

	protected virtual void OnHold ()
	{
		Debug.Log ("OnHold");
	}

	protected virtual void OnHolding ()
	{
		Debug.Log ("OnTouchHold");
	}

	protected virtual void OnTouchUp ()
	{
		Debug.Log ("OnTouchUp");
	}


}
