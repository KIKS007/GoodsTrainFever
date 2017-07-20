using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;

public enum ContainerType { Basic, Food };

public class Container : Touchable 
{
	public static Action <Container> OnContainerSelected;
	public static Action <Container> OnContainerDeselected;
	public static Action OnContainerMoved;

	[Header ("States")]
	public bool selected = false;
	public bool isPileUp = false;
	public bool isMoving = false;

	[Header ("Size")]
	public bool isDoubleSize = false;

	[Header ("Type")]
	public ContainerType containerType = ContainerType.Basic;

	[Header ("Train")]
	public Train train = null;
	public Wagon wagon = null;

	[Header ("Spot")]
	public Spot spotOccupied = null;

	[HideInInspector]
	public Mesh _mesh;
	[HideInInspector]
	public Collider _collider;

	private Spot[] _pileSpots = new Spot[0];

	void Awake ()
	{
		_mesh = GetComponent<MeshFilter> ().mesh;
		_collider = GetComponent<Collider> ();
		_pileSpots = transform.GetComponentsInChildren<Spot> ();

		TouchManager.Instance.OnTouchUpNoTarget += OnTouchUpNoTarget;
	}

	void OnTouchUpNoTarget ()
	{
		if (selected)
			Deselect ();
	}

	public void SetInitialSpot (Spot spot)
	{
		spotOccupied = spot;

		transform.position = spot.transform.position;
		transform.SetParent (spot._containersParent);

		if(spot.spotType == SpotType.Train)
		{
			wagon = spot._wagon;
			train = wagon.train;
		}
	}

	public override void OnTouchUpAsButton ()
	{
		base.OnTouchUpAsButton ();

		if (TrainsMovementManager.Instance.selectedTrainHasMoved || TrainsMovementManager.Instance.resetingTrains)
			return;

		if (train && train.inTransition)
			return;

		IsPileUp ();

		if (isPileUp)
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

		spotOccupied.RemoveContainer ();
		spot.SetContainer (this);

		spotOccupied = spot;

		selected = false;

		transform.SetParent (spot._containersParent);

		if(spot.spotType == SpotType.Train)
		{
			wagon = spot._wagon;
			train = wagon.train;
			TrainsMovementManager.Instance.trainContainerInMotion = train;
		}
		else
		{
			wagon = null;
			train = null;
		}

		SetPileSpot ();

		if (OnContainerMoved != null)
			OnContainerMoved ();
		
		if (OnContainerDeselected != null)
			OnContainerDeselected (this);
	}

	void SetPileSpot ()
	{
		foreach (var s in _pileSpots)
		{
			s.spotType = spotOccupied.spotType;
			s._containersParent = transform.parent;
		}
	}

	void IsPileUp ()
	{
		bool isPiledUpTemp = false;

		foreach(var s in _pileSpots)
		{
			s.IsOccupied ();

			if(s.isOccupied)
			{
				isPiledUpTemp = true;
				break;
			}
		}

		isPileUp = isPiledUpTemp;
	}
		
	public void OnContainerMovedEvent ()
	{
		if (OnContainerMoved != null)
			OnContainerMoved ();
	}

	void OnDestroy ()
	{
		if (TrainsMovementManager.applicationIsQuitting)
			return;
		
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
