using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
	public bool overweight = false;

	[Header ("Containers")]
	public List<Container> containers = new List<Container> ();

	private Text _weightText;
	private Image _weightImage;

	void Awake ()
	{
		train = transform.GetComponentInParent<Train> ();
		_weightText = transform.GetComponentInChildren <Text> ();
		_weightImage = transform.GetComponentInChildren <Image> ();

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

		for(int i = 0 ; i < containers.Count; i++)
		{
			if(containers [i] != null)
			{
				if (i > 0 && containers [i] == containers [i - 1])
					continue;

				currentWeight += containers [i].weight;
			}
		}

		overweight = currentWeight > maxWeight;

		_weightText.text = currentWeight.ToString ("00") + "/" + maxWeight.ToString ("00");
		_weightImage.color = overweight ? GlobalVariables.Instance.wagonOverweightColor : GlobalVariables.Instance.wagonNormalWeightColor;
	}
}
