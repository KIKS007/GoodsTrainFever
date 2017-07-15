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

	[HideInInspector]
	public float _xInitialPosition;

	private Vector3 _mouseDeltaPosition;
	private Vector3 _mousePosition;

	// Use this for initialization
	void Awake ()
	{
		_xInitialPosition = transform.position.x;

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
}
