using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public enum TouchableState { None, Hold }

public class Touchable : MonoBehaviour
{
	[Header ("Touch")]
	public TouchableState touchableState = TouchableState.None;
	public bool letPassEvents = false;

	[ShowIf ("letPassEvents")]
	public bool letPassTouchDown = false;
	[ShowIf ("letPassEvents")]
	public bool letPassHold = false;
	[ShowIf ("letPassEvents")]
	public bool letPassTouchUp = false;
	[ShowIf ("letPassEvents")]
	public bool letPassTouchUpAsButton = false;

	protected bool _pointerDown = false;

	protected void OnMouseDown ()
	{
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
		if(letPassTouchDown)
		{
			if(transform.parent != null && transform.parent.GetComponentInParent<Touchable> () != null)
				transform.parent.GetComponentInParent<Touchable> ().OnTouchDown ();
		}

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
		if(letPassHold)
		{
			if(transform.parent != null && transform.parent.GetComponentInParent<Touchable> () != null)
				transform.parent.GetComponentInParent<Touchable> ().OnHold ();
		}

		touchableState = TouchableState.Hold;
	}

	public virtual void OnTouchUp ()
	{
		if(letPassTouchUp)
		{
			if(transform.parent != null && transform.parent.GetComponentInParent<Touchable> () != null)
				transform.parent.GetComponentInParent<Touchable> ().OnTouchUp ();
		}
		
		touchableState = TouchableState.None;
	}

	public virtual void OnTouchUpAsButton ()
	{
		if(letPassTouchUpAsButton)
		{
			if(transform.parent != null && transform.parent.GetComponentInParent<Touchable> () != null)
				transform.parent.GetComponentInParent<Touchable> ().OnTouchUpAsButton ();
		}

		touchableState = TouchableState.None;
	}
}
