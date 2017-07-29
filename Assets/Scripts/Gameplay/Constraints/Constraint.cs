using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constraint : MonoBehaviour 
{
	protected Container _container;

	protected virtual void Start ()
	{
		_container = GetComponent<Container> ();
	}

	public virtual bool IsRespected ()
	{
		return true;
	}

	public virtual bool IsRespected (Spot spot)
	{
		return true;
	}

	protected Container NextContainer ()
	{
		int index = _container.train.containers.FindIndex(a => a == _container);

		int nextContainer = _container.isDoubleSize ? index + 2 : index + 1;

		if (nextContainer < _container.train.containers.Count - 1 && _container.train.containers [nextContainer] != _container)
			return _container.train.containers [nextContainer];
		else
			return null;
	}

	protected Container PreviousContainer ()
	{
		int index = _container.train.containers.FindIndex(a => a == _container);

		int previousContainer = index - 1;

		if (previousContainer >= 0 && _container.train.containers [previousContainer] != _container)
			return _container.train.containers [previousContainer];
		else
			return null;
	}

	protected Container NextContainer (Spot spot)
	{
		int nextContainer = _container.isDoubleSize ? spot._spotTrainIndex + 2 : spot._spotTrainIndex + 1;

		if (nextContainer < spot._wagon.train.containers.Count - 1 && spot._wagon.train.containers [nextContainer] != _container)
			return spot._wagon.train.containers [nextContainer];
		else
			return null;
	}

	protected Container PreviousContainer (Spot spot)
	{
		int previousContainer = spot._spotTrainIndex - 1;

		if (previousContainer >= 0 && spot._wagon.train.containers [previousContainer] != _container)
			return spot._wagon.train.containers [previousContainer];
		else
			return null;
	}
}
