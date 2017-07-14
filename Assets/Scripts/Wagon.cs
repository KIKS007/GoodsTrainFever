using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wagon : Touchable 
{
	[Header ("Train")]
	public Train train;

	void Start ()
	{
		train = transform.GetComponentInParent<Train> ();
	}
}
