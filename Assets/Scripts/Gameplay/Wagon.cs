﻿using System.Collections;
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
	}

	public void UpdateWeight ()
	{
		currentWeight = 0;

		int containersCount = 0;

		for(int i = 0 ; i < containers.Count; i++)
		{
			if(containers [i] != null)
			{
				if (i > 0 && containers [i] == containers [i - 1])
					continue;

				containersCount++;
				currentWeight += containers [i].weight;
			}
		}

		overweight = currentWeight > maxWeight;

		if(maxWeight != 666)
		{
			_weightText.text = currentWeight.ToString ("00") + "/" + maxWeight.ToString ("00");
			_weightImage.color = overweight ? GlobalVariables.Instance.wagonOverweightColor : GlobalVariables.Instance.wagonNormalWeightColor;
		}
		else
		{
			_weightImage.color = GlobalVariables.Instance.wagonNormalWeightColor;
			_weightText.text = "∞";
		}


		train.UpdateWeight ();
	}
}
