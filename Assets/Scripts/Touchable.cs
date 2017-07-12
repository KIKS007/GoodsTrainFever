using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public enum TouchableState { None, Touched, Hold }

public class Touchable : MonoBehaviour
{
	[Header ("Touch States")]
	[InfoBox ("This touchable element doesn't have a collider!", InfoMessageType.Warning, "HasntCollider")]
	public TouchableState touchableState = TouchableState.None;

	bool HasntCollider ()
	{
		if (GetComponent<Collider> () != null)
			return false;
		else
			return true;
	}

	protected float _holdDelay = 0.3f;
	protected bool _pointerDown = false;

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
		touchableState = TouchableState.Touched;

		Debug.Log ("OnTouchDown");
	}

	protected virtual void OnTouching ()
	{
		Debug.Log ("OnTouching");
	}

	protected virtual void OnHold ()
	{
		touchableState = TouchableState.Hold;

		Debug.Log ("OnHold");
	}

	protected virtual void OnHolding ()
	{
		Debug.Log ("OnTouchHold");
	}

	protected virtual void OnTouchUp ()
	{
		touchableState = TouchableState.None;

		Debug.Log ("OnTouchUp");
	}


}
