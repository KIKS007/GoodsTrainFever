using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WagonType { Fourty, Sixty, Eighty }

public class Wagon : Touchable 
{
	[Header ("Wagon")]
	public WagonType wagonType = WagonType.Fourty;

	[Header ("Train")]
	public Train train;

	[Header ("Weight")]
	public int maxWeight;
	public int currentWeight;

	[Header ("Containers")]
	public List<Container> containers = new List<Container> ();

	void Awake ()
	{
		train = transform.GetComponentInParent<Train> ();
	}
}
