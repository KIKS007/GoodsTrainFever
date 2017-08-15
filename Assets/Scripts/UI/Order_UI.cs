﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order_UI : MonoBehaviour 
{
	public bool isPrepared = false;
	public bool isSent = false;

	[Header ("Order")]
	public Order_Level orderLevel = new Order_Level ();

	[Header ("Containers")]
	public List<Container_UI> containers = new List<Container_UI> ();


	public void Setup () 
	{
		CheckContainers ();
	}

	public void OrderSent ()
	{
		foreach(var c in containers)
		{
			if(!c.isSent)
				c.ContainerSent ();
		}
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

	public bool ContainerSent (Container container)
	{
		foreach(var c in containers)
		{
			if (c.isSent)
				continue;

			if (c.container != container)
				continue;

			c.ContainerSent ();

			UpdateStates ();

			return true;
		}

		UpdateStates ();

		return false;
	}

	public bool ContainerAdded (Container container)
	{
		foreach(var c in containers)
		{
			if (c.isSent)
				continue;

			if (c.neededCount == 0)
				continue;

			if (c.containerLevel.containerColor != container.containerColor)
				continue;

			if (c.containerLevel.containerType != container.containerType)
				continue;

			if (c.containerLevel.isDoubleSize != container.isDoubleSize)
				continue;

			c.ContainerAdded (container);

			UpdateStates ();

			return true;
		}

		return false;
	}

	public bool ContainerSelected (Container container)
	{
		foreach(var c in containers)
		{
			if(c.container != null && c.container == container)
			{
				c.ContainerSelected ();

				return true;
			}
		}

		foreach(var c in containers)
		{
			if (c.isSent)
				continue;

			if (c.neededCount == 0)
				continue;

			if (c.containerLevel.containerColor != container.containerColor)
				continue;

			if (c.containerLevel.containerType != container.containerType)
				continue;

			if (c.containerLevel.isDoubleSize != container.isDoubleSize)
				continue;

			c.ContainerSelected ();

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
			if (c.isSent)
				continue;

			if (c.container != container)
				continue;
			
			c.ContainerRemoved ();
			
			UpdateStates ();
			
			return true;
		}

		return false;
	}

	void UpdateStates ()
	{
		bool prepared = true;

		foreach (var c in containers)
			if (!c.isPrepared)
			{
				prepared = false;
				break;
			}

		isPrepared = prepared;

		bool sent = true;

		foreach (var c in containers)
			if (!c.isSent)
			{
				sent = false;
				break;
			}

		isSent = sent;
	}
}
