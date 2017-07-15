using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public enum TouchableState { None, Hold }

public class Touchable : MonoBehaviour
{
	public static bool TouchingTouchable = false;

	[Header ("Touch")]
	public TouchableState touchableState = TouchableState.None;

	protected bool _pointerDown = false;

	protected void OnMouseDown ()
	{
		TouchingTouchable = true;

		OnTouchDown ();
		_pointerDown = true;
	}

	protected void OnMouseUp ()
	{
		OnTouchUp ();
		_pointerDown = false;
	}

	protected void OnMouseUpAsButton ()
	{
		OnTouchUpAsButton ();
		_pointerDown = false;
	}

	public virtual void OnTouchDown ()
	{
		if(transform.parent != null && transform.parent.GetComponentInParent<Touchable> () != null)
			transform.parent.GetComponentInParent<Touchable> ().OnTouchDown ();

		StartCoroutine (Holding ());
	}

	IEnumerator Holding ()
	{
		while (touchableState != TouchableState.None)
		{
			OnHold ();
			yield return new WaitForEndOfFrame ();
		}
	}

	public virtual void OnHold ()
	{
		if(transform.parent != null && transform.parent.GetComponentInParent<Train> () != null)
			transform.parent.GetComponentInParent<Train> ().OnHold ();

		touchableState = TouchableState.Hold;
	}

	public virtual void OnTouchUp ()
	{
		if(transform.parent != null && transform.parent.GetComponentInParent<Train> () != null)
			transform.parent.GetComponentInParent<Train> ().OnTouchUp ();
		
		touchableState = TouchableState.None;
	}

	public virtual void OnTouchUpAsButton ()
	{
		if(transform.parent != null && transform.parent.GetComponentInParent<Train> () != null)
			transform.parent.GetComponentInParent<Train> ().OnTouchUpAsButton ();

		touchableState = TouchableState.None;
	}
}
