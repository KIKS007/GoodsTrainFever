using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;

public class Container : Touchable 
{
	public static Action <Container> OnContainerSelected;
	public static Action <Container> OnContainerDeselected;
	public static Action OnContainerMoved;

	[Header ("States")]
	public bool canBeSelected = true;
	public bool selected = false;
	public bool isPileUp = false;

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

		TrainsMovementManager.Instance.OnTrainMovementStart += TrainHasMoved;
		TrainsMovementManager.Instance.OnTrainMovementEnd += TrainStoppedMoving;
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

		IsPileUp ();

		if (isPileUp)
			return;

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

		spotOccupied.RemoveContainer ();
		spot.SetContainer (this);

		spotOccupied = spot;

		selected = false;

		transform.SetParent (spot._containersParent);

		if(spot.spotType == SpotType.Train)
		{
			wagon = spot._wagon;
			train = wagon.train;
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

	void TrainHasMoved ()
	{
		canBeSelected = false;
	}

	void TrainStoppedMoving ()
	{
		StopCoroutine (TrainStoppedMovingDelay ());
		StartCoroutine (TrainStoppedMovingDelay ());
	}

	IEnumerator TrainStoppedMovingDelay ()
	{
		yield return new WaitForSecondsRealtime (0.01f);
		canBeSelected = true;
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
