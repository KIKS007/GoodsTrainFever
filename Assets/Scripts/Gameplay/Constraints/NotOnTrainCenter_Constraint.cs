﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotOnTrainCenter_Constraint : Constraint
{
	public override bool IsRespected ()
	{
		int index = _container.train.wagons.FindIndex(a => a == _container.wagon);

		if (index == 0 || index == _container.train.wagons.Count - 1)
			return true;
		else
			return false;
	}

	public override bool IsRespected (Spot spot)
	{
		int index = spot._wagon.train.wagons.FindIndex(a => a == spot._wagon);

		if (index == 0 || index == spot._wagon.train.wagons.Count - 1)
			return true;
		else
			return false;
	}
}