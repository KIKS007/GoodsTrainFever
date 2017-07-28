using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotOnTrainExtremities_Constraint : Constraint
{
	public override bool IsRespected ()
	{
		int index = _container.train.wagons.FindIndex(a => a == _container.wagon);

		if (index == 0 || index == _container.train.wagons.Count - 1)
			return false;
		else
			return true;
	}
}
