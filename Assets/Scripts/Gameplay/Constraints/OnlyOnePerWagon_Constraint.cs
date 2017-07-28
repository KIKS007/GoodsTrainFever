using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyOnePerWagon_Constraint : Constraint 
{
	public override bool IsRespected ()
	{
		foreach(var c in _container.wagon.containers)
			if(c.containerType == _container.containerType)
				return false;

		return true;
	}
}

