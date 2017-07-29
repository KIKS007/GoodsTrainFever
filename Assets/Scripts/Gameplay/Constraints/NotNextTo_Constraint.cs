using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotNextTo_Constraint : Constraint 
{
	public ContainerType notNextToType;

	public override bool IsRespected ()
	{
		var next = NextContainer ();
		var previous = PreviousContainer ();

		if (next && next.containerType == notNextToType || previous && previous.containerType == notNextToType)
			return false;
		else
			return true;
	}

	public override bool IsRespected (Spot spot)
	{
		var next = NextContainer (spot);
		var previous = PreviousContainer (spot);

		if (next && next.containerType == notNextToType || previous && previous.containerType == notNextToType)
			return false;
		else
			return true;
	}
}
