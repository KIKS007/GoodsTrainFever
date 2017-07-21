﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;
using Sirenix.OdinInspector;

public enum SpotType { Train, Storage, Boat, Road }

public class Spot : Touchable 
{
	public Action<Container> OnSpotTaken;
	public Action OnSpotFreed;

	[Header ("Spot")]
	public SpotType spotType;
	public bool isOccupied = false;

	[Header ("Read Only")]
	[ReadOnlyAttribute]
	public bool isDoubleSize = false;
	[ReadOnlyAttribute]
	public bool isPileSpot;
	[ReadOnlyAttribute]
	public bool isPileUp = false;

	[Header ("Container")]
	public Container container;

	[HideInInspector]
	public Wagon _wagon;
	[HideInInspector]
	public Transform _containersParent;

	private Collider _collider;
	private MeshRenderer _meshRenderer;
	private MeshFilter _meshFilter;
	private Material _material;
	[HideInInspector]
	public Container _parentContainer;
	public List<Spot> _overlappingSpots = new List<Spot> ();
	[HideInInspector]
	public bool _isSpawned = false;

	private float _fadeDuration = 0.2f;
	private Spot _doubleSizeSpotSpawned;

	void Awake () 
	{
		_collider = GetComponent<Collider> ();
		_meshRenderer = GetComponent<MeshRenderer> ();
		_material = _meshRenderer.material;
		_meshFilter = GetComponent<MeshFilter> ();

		Container.OnContainerSelected += OnContainerSelected;
		Container.OnContainerDeselected += OnContainerDeselected;

		if(transform.GetComponentInParent<Container> () != null)
		{
			isPileSpot = true;
			_parentContainer = transform.GetComponentInParent<Container> ();
		}

		SetIsDoubleSize ();

		SetIsPileSpot ();

		GetOverlappingSpots ();

		SetSpotType (true);
	}

	void Start () 
	{
		SetSpotType ();

		IsOccupied ();

		if(!_isSpawned)
			OnContainerDeselected ();

		OverlappingSpotsOccupied ();
	}

	public void SetIsDoubleSize ()
	{
		_collider = GetComponent<Collider> ();
		BoxCollider boxCollider = _collider as BoxCollider;

		if (boxCollider.size.x > 12f)
			isDoubleSize = true;
		else
			isDoubleSize = false;
	}

	public void SetIsPileSpot ()
	{
		if (transform.parent.GetComponent<Container> () != null)
			isPileSpot = true;
		else
			isPileSpot = false;
	}

	void GetOverlappingSpots ()
	{
		int spotsMask = 1 << LayerMask.NameToLayer ("Spots");

		Vector3 position = transform.position;
		position.y += 0.5f;

		Collider[] colliders = Physics.OverlapBox (position, new Vector3 (0.5f, 0.25f, 0.5f), Quaternion.identity, spotsMask, QueryTriggerInteraction.Collide);

		foreach (var c in colliders)
		{
			Spot spot = c.GetComponent<Spot> ();

			if (c != _collider && spot != null)
			{
				if(!isDoubleSize && spot.isDoubleSize || isDoubleSize)
					_overlappingSpots.Add (spot);
			}
		}

		_overlappingSpots = _overlappingSpots.OrderByDescending (x => x.transform.position.x).ToList ();
	}

	public void OverlappingSpotsOccupied ()
	{
		if(container == null)
			isOccupied = false;

		foreach(var s in _overlappingSpots)
		{
			if (s.isOccupied)
			{
				if(!isDoubleSize && s.isDoubleSize && s.container != null)
					isOccupied = true;

				if(isDoubleSize)
					isOccupied = true;
			}
		}
	}

	void SetSpotType (bool onlyType = false)
	{
		if(transform.GetComponentInParent<Storage> () != null)
		{
			spotType = SpotType.Storage;

			if(!onlyType)
				_containersParent = transform.GetComponentInParent<Storage> ().containersParent;

			return;
		}

		if(transform.GetComponentInParent<Wagon> () != null)
		{
			spotType = SpotType.Train;

			if(!isPileSpot)
				_wagon = transform.GetComponentInParent<Wagon> ();

			if(!onlyType)
				_containersParent = transform.GetComponentInParent<Wagon> ().train.containersParent;
			return;
		}
	}

	public void IsOccupied ()
	{
		int containersMask = 1 << LayerMask.NameToLayer ("Containers");

		Vector3 position = transform.position;
		position.y += 0.5f;

		RaycastHit hit;
		Physics.Raycast (transform.position, Vector3.up, out hit, 4f, containersMask, QueryTriggerInteraction.Collide);

		isOccupied = false;

		if (hit.collider != null)
		{
			isOccupied = true;

			if (IsSameSize (hit.collider.GetComponent<Container> ()))
			{
				container = hit.collider.GetComponent<Container> ();
				hit.collider.GetComponent<Container> ().SetInitialSpot (this);
			}
		}

		if(container != null && container.spotOccupied != null && container.spotOccupied == this)
			isOccupied = true;
	}

	bool CanPileContainer ()
	{
		int containersMask = 1 << LayerMask.NameToLayer ("Containers");

		int belowContainers = 0;

		Vector3 rayPosition = transform.position;
		rayPosition.x -= 0.01f;

		if (isPileSpot && _parentContainer.isMoving)
		{
			rayPosition = _parentContainer.spotOccupied.transform.position;
			rayPosition.y += ContainersMovementManager.Instance.containerHeight;

			belowContainers = 1;
		}

		RaycastHit[] hits = Physics.RaycastAll (rayPosition, Vector3.down, 100f, containersMask, QueryTriggerInteraction.Collide);

		foreach (var h in hits)
		{
			if (container && h.collider.gameObject == container.gameObject)
				continue;

			if (isPileSpot && _parentContainer.isMoving && h.collider.gameObject == _parentContainer.gameObject)
			{
				//Debug.Log ("--- moins moins");
				belowContainers--;
			}
			
			belowContainers++;
		}

		if (spotType == SpotType.Train)
		{
			if (belowContainers == 0 && !isPileSpot)
				return true;
			else
				return false;
		}

		else
		{
			if (belowContainers <= ContainersMovementManager.Instance.containersPileCount)
				return true;
			else
				return false;
		}
	}

	public void SetContainer (Container container)
	{
		isOccupied = true;
		this.container = container;

		foreach (var s in _overlappingSpots)
			s.OverlappingSpotsOccupied ();

		if (OnSpotTaken != null)
			OnSpotTaken (this.container);
	}

	public void RemoveContainer ()
	{
		isOccupied = false;

		container = null;

		foreach (var s in _overlappingSpots)
			s.OverlappingSpotsOccupied ();

		if (OnSpotFreed != null)
			OnSpotFreed ();
	}

	public override void OnTouchUpAsButton ()
	{
		base.OnTouchUpAsButton ();

		if (TrainsMovementManager.Instance.selectedTrainHasMoved || TrainsMovementManager.Instance.resetingTrains)
			return;

		if (_wagon && _wagon.train.inTransition)
			return;

		ContainersMovementManager.Instance.TakeSpot (this);
	}

	void OnContainerSelected (Container container)
	{
		SpawnDoubleSizeSpot (container);

		OverlappingSpotsOccupied ();

		if (isOccupied)
			return;

		if (_wagon && _wagon.train.inTransition)
			return;
		
		if (!IsSameSize (container))
			return;

		if (!CanPileContainer ())
			return;

		if (isPileSpot && _parentContainer.selected)
			return;

		_meshRenderer.enabled = true;
			
		_meshFilter.mesh = container._mesh;
		_collider.enabled = true;

		DOTween.Kill (_material);

		float delay = Vector3.Distance (container.transform.position, transform.position) * 0.02f;

		_material.DOFloat (1f, "_HologramOpacity", _fadeDuration).SetDelay (delay);
		_material.DOFloat (1f, "_Opacity", _fadeDuration).SetDelay (delay);
	}

	void OnContainerDeselected (Container container = null)
	{
		_collider.enabled = false;

		DOTween.Kill (_material);

		if(container != this.container)
		{
			_material.DOFloat (0f, "_HologramOpacity", _fadeDuration);
			_material.DOFloat (0f, "_Opacity", _fadeDuration).OnComplete (()=> _meshRenderer.enabled = false);
		}
		else
		{
			_material.DOFloat (0f, "_HologramOpacity", _fadeDuration);
			_material.DOFloat (0f, "_Opacity", _fadeDuration).OnComplete (()=> _meshRenderer.enabled = false);
		}
	}

	bool IsSameSize (Container container)
	{
		if(isDoubleSize && container.isDoubleSize || !isDoubleSize && !container.isDoubleSize)
			return true;
		else
			return false;
	}

	public void SetIsPileUp (bool piled)
	{
		isPileUp = piled;

		foreach (var s in _overlappingSpots)
			s.isPileUp = piled;
	}

	void SpawnDoubleSizeSpot (Container c)
	{
		if (_doubleSizeSpotSpawned && _doubleSizeSpotSpawned.isOccupied)
			return;

		if (_doubleSizeSpotSpawned)
			Destroy (_doubleSizeSpotSpawned.gameObject);

		if (!c.isDoubleSize)
			return;
		
		if (spotType == SpotType.Train)
			return;

		if (!isDoubleSize)
			return;
		
		if (container)
			return;

		if (isPileUp)
			return;

		foreach (var s in _overlappingSpots)
			if (s.isOccupied == false)
				return;

		Vector3 spotPosition = transform.position;
		spotPosition.y += ContainersMovementManager.Instance.containerHeight;

		_doubleSizeSpotSpawned = (Instantiate (GlobalVariables.Instance.spot40Prefab, spotPosition, transform.rotation, transform.parent)).GetComponent<Spot> ();
		_doubleSizeSpotSpawned._isSpawned = true;
		_doubleSizeSpotSpawned.OnContainerSelected (c);
	}

	void OnDestroy ()
	{
		if (TrainsMovementManager.applicationIsQuitting)
			return;

		Container.OnContainerSelected -= OnContainerSelected;
		Container.OnContainerDeselected -= OnContainerDeselected;
	}
}
