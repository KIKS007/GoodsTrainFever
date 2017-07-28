using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotNextToDangerous_Constraint : Constraint
{
	public override bool IsRespected ()
	{
		var next = NextContainer ();
		var previous = PreviousContainer ();

		if (next && next.containerType == ContainerType.Dangerous || previous && previous.containerType == ContainerType.Dangerous)
			return false;
		else
			return true;
	}
}
