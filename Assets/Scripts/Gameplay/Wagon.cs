using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WagonType { Fourty, Sixty }

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

		SetWeight ();
	}

	void SetWeight ()
	{
		foreach(var w in GlobalVariables.Instance.wagonsMaxWeight)
			if(w.wagonType == wagonType)
			{
				maxWeight = (int) UnityEngine.Random.Range ((int)w.weightBounds.x, (int)w.weightBounds.y);
				break;
			}
	}

	public void UpdateWeight ()
	{
		currentWeight = 0;

		foreach (var c in containers)
			if(c != null)
				currentWeight += c.weight;
	}
}
