﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;

public class Container : Touchable 
{
	public static Action <Container> OnContainerSelected;
	public static Action <Container> OnContainerDeselected;

	[Header ("States")]
	public bool canBeSelected = true;
	public bool selected = false;

	[Header ("Train")]
	public Train train = null;
	public Wagon wagon = null;

	[Header ("Spot")]
	public Spot spotOccupied = null;

	[HideInInspector]
	public Mesh _mesh;
	[HideInInspector]
	public Collider _collider;

	void Start ()
	{
		_mesh = GetComponent<MeshFilter> ().mesh;
		_collider = GetComponent<Collider> ();

		TrainsMovementManager.Instance.OnTrainMovementStart += TrainHasMoved;
		TrainsMovementManager.Instance.OnTrainMovementEnd += TrainStoppedMoving;
	}

	public void SetInitialSpot (Spot spot)
	{
		spotOccupied = spot;

		transform.position = spot.transform.position;

		wagon = spot._wagon;
		train = wagon.train;

		transform.SetParent (train.containersParent);
	}

	public override void OnTouchUpAsButton ()
	{
		base.OnTouchUpAsButton ();

		if (!canBeSelected)
			return;

		if (!selected)
			Select ();
		else
			Deselect ();
	}

	public void Select ()
	{
		if (ContainersMovementManager.Instance.selectedContainer != null)
			ContainersMovementManager.Instance.selectedContainer.Deselect ();
		
		ContainersMovementManager.Instance.selectedContainer = this;

		ContainersMovementManager.Instance.StartHover (this);

		selected = true;

		if (OnContainerSelected != null)
			OnContainerSelected (this);
	}

	public void Deselect ()
	{
		if(ContainersMovementManager.Instance.selectedContainer == this)
			ContainersMovementManager.Instance.selectedContainer = null;

		ContainersMovementManager.Instance.StopHover (this);

		selected = false;

		if (OnContainerDeselected != null)
			OnContainerDeselected (this);
	}

	public void TakeSpot (Spot spot)
	{
		if(ContainersMovementManager.Instance.selectedContainer == this)
			ContainersMovementManager.Instance.selectedContainer = null;

		spotOccupied.isOccupied = false;
		spot.isOccupied = true;

		spotOccupied = spot;

		selected = false;

		wagon = spot._wagon;
		train = wagon.train;

		transform.SetParent (train.containersParent);

		if (OnContainerDeselected != null)
			OnContainerDeselected (this);
	}

	void TrainHasMoved ()
	{
		canBeSelected = false;
	}

	void TrainStoppedMoving ()
	{
		DOVirtual.DelayedCall (0.2f, ()=> canBeSelected = true);
	}

	void OnDestroy ()
	{
		if (TrainsMovementManager.applicationIsQuitting)
			return;
		
		TrainsMovementManager.Instance.OnTrainMovementStart -= TrainHasMoved;
		TrainsMovementManager.Instance.OnTrainMovementEnd -= TrainStoppedMoving;
	}

	[PropertyOrder (-1)]
	[ButtonAttribute ("Take Spot")]
	public void EditorTakeSpot ()
	{
		if(spotOccupied == null)
		{
			Debug.LogWarning ("No Spot!");
			return;
		}

		transform.position = spotOccupied.transform.position;
	}
}
