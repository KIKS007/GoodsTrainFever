using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotNextToChilled_Constraint : Constraint 
{
	public override bool IsRespected ()
	{
		var next = NextContainer ();
		var previous = PreviousContainer ();

		if (next && next.containerType == ContainerType.Cooled || previous && previous.containerType == ContainerType.Cooled)
			return false;
		else
			return true;
	}
}
