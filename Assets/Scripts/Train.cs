using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : Touchable 
{
	[Header ("Wagons")]
	public List<Wagon> wagons = new List<Wagon> ();
	public Transform wagonsParent;

	[Header ("Containers")]
	public Transform containersParent;

	private Vector3 _mousePosition;
	private Vector3 _mouseDeltaPosition;

	// Use this for initialization
	void Start ()
	{
		wagons.AddRange (transform.GetComponentsInChildren<Wagon> ());
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public override void OnTouchDown ()
	{
		base.OnTouchDown ();

		TrainsMovementManager.Instance.selectedTrain = this;
	}

	public override void OnTouchUp ()
	{
		base.OnTouchUp ();

		if(TrainsMovementManager.Instance.selectedTrain == this)
			TrainsMovementManager.Instance.selectedTrain = null;
	}
}
