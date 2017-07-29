﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitedPerWagon_Constraint : Constraint 
{
	public int limitedTo = 1;

	public override bool IsRespected ()
	{
		int counts = 0;

		foreach (var c in _container.wagon.containers)
			if (c != null && c != _container && c.containerType == _container.containerType)
				counts++;

		if(counts < limitedTo)
			return true;
		else
			return false;
	}

	public override bool IsRespected (Spot spot)
	{
		int counts = 0;

		foreach(var c in spot._wagon.containers)
			if(c != null && c != _container && c.containerType == _container.containerType)
				counts++;

		if(counts < limitedTo)
			return true;
		else
			return false;
	}
}
