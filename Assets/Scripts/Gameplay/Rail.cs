using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rail : Touchable 
{
	public Train train;

	public void SetTrain (Train t)
	{
		train = t;
	}

	public override void OnTouchDown ()
	{
		base.OnTouchDown ();

		train.OnTouchDown ();
	}
}
