using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTrains : Touchable 
{
	public override void OnTouchDown ()
	{
		base.OnTouchDown ();

		TrainsMovementManager.Instance.ResetTrainsPosition ();
	}
}
