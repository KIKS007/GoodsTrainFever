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

	protected Container NextContainer ()
	{
		int index = _container.train.containers.FindIndex(a => a == _container);

		int nextContainer = _container.isDoubleSize ? index + 2 : index + 1;

		if (nextContainer < _container.train.containers.Count - 1)
			return _container.train.containers [nextContainer];
		else
			return null;
	}

	protected Container PreviousContainer ()
	{
		int index = _container.train.containers.FindIndex(a => a == _container);

		int previousContainer = _container.isDoubleSize ? index - 2 : index - 1;

		if (previousContainer >= 0)
			return _container.train.containers [previousContainer];
		else
			return null;
	}
}
