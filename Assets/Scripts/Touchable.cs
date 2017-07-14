using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public enum TouchableState { None, Touched, Hold }

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
	public bool letPassHolding = false;
	[ShowIf ("letPassEvents")]
	public bool letPassTouchUp = false;
	[ShowIf ("letPassEvents")]
	public bool letPassTouchUpAsButton = false;

	protected float _holdDelay = 0.3f;
	protected bool _pointerDown = false;

	protected void OnMouseDown ()
	{
		OnTouchDown ();
		StartCoroutine (HoldDelay ());
		_pointerDown = true;
	}

	protected void OnMouseUp ()
	{
		OnTouchUp ();
		StopCoroutine (HoldDelay ());
		_pointerDown = false;
	}

	protected void OnMouseUpAsButton ()
	{
		OnTouchUpAsButton ();
		StopCoroutine (HoldDelay ());
		_pointerDown = false;
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

	public virtual void OnTouchDown ()
	{
		if(letPassTouchDown)
		{
			if(transform.parent.GetComponentInParent<Touchable> () != null)
				transform.parent.GetComponentInParent<Touchable> ().OnTouchDown ();
			
			return;
		}

		touchableState = TouchableState.Touched;
	}

	public virtual void OnHold ()
	{
		if(letPassHold)
		{
			if(transform.parent.GetComponentInParent<Touchable> () != null)
				transform.parent.GetComponentInParent<Touchable> ().OnHold ();

			return;
		}

		touchableState = TouchableState.Hold;
	}

	public virtual void OnHolding ()
	{
		if(letPassHolding)
		{
			if(transform.parent.GetComponentInParent<Touchable> () != null)
				transform.parent.GetComponentInParent<Touchable> ().OnHolding ();

			return;
		}
	}

	public virtual void OnTouchUp ()
	{
		if(letPassTouchUp)
		{
			if(transform.parent.GetComponentInParent<Touchable> () != null)
				transform.parent.GetComponentInParent<Touchable> ().OnTouchUp ();

			return;
		}
		
		touchableState = TouchableState.None;
	}

	public virtual void OnTouchUpAsButton ()
	{
		if(letPassTouchUpAsButton)
		{
			if(transform.parent.GetComponentInParent<Touchable> () != null)
				transform.parent.GetComponentInParent<Touchable> ().OnTouchUpAsButton ();

			return;
		}

		touchableState = TouchableState.None;
	}
}
