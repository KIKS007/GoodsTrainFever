using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order_UI : MonoBehaviour 
{
	public bool isPrepared = false;

	[Header ("Containers")]
	public List<Container_UI> containers = new List<Container_UI> ();

	public void Setup () 
	{
		CheckContainers ();
	}

	void CheckContainers ()
	{
		bool hasCheck = true;

		do
		{
			hasCheck = true;

			foreach (var c in OrdersManager.Instance.containersFromNoOrder)
			{
				if (ContainerAdded (c))
				{
					OrdersManager.Instance.containersFromNoOrder.Remove (c);
					hasCheck = false;
					break;
				}
			}
		}
		while (!hasCheck);

	}

	public bool ContainerAdded (Container container)
	{
		foreach(var c in containers)
		{
			if (c.neededCount == 0)
				continue;

			if (c.container.containerColor != container.containerColor)
				continue;

			if (c.container.containerType != container.containerType)
				continue;

			if (c.container.isDoubleSize != container.isDoubleSize)
				continue;

			c.ContainerAdded ();

			CheckIsPrepared ();

			return true;
		}

		return false;
	}

	public bool ContainerRemoved (Container container)
	{
		List<Container_UI> containersTemp = new List<Container_UI> (containers);
		containersTemp.Reverse ();

		foreach(var c in containersTemp)
		{
			if (c.preparedCount == 0)
				continue;

			if (c.container.containerColor != container.containerColor)
				continue;

			if (c.container.containerType != container.containerType)
				continue;

			if (c.container.isDoubleSize != container.isDoubleSize)
				continue;

			c.ContainerRemoved ();

			CheckIsPrepared ();

			return true;
		}

		return false;
	}

	void CheckIsPrepared ()
	{
		bool prepared = true;

		foreach (var c in containers)
			if (!c.isPrepared)
			{
				prepared = false;
				break;
			}

		isPrepared = prepared;
	}
}
