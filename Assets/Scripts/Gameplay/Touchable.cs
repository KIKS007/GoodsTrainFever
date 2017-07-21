using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class Touchable : MonoBehaviour
{
	protected bool _pointerDown = false;

	public virtual void OnTouchDown ()
	{
		TouchManager.Instance.isTouchingTouchable = true;
		_pointerDown = true;

		if(transform.parent != null && transform.parent.GetComponentInParent<Touchable> () != null)
			transform.parent.GetComponentInParent<Touchable> ().OnTouchDown ();
	}

	public virtual void OnTouchUpAsButton ()
	{
		if(transform.parent != null && transform.parent.GetComponentInParent<Train> () != null)
			transform.parent.GetComponentInParent<Train> ().OnTouchUpAsButton ();

		_pointerDown = false;
	}
}
