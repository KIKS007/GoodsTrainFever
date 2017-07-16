using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum SpotType { Train, Storage, Boat, Road }

public class Spot : Touchable 
{
	[Header ("Spot")]
	public SpotType spotType;
	public bool isOccupied = false;
	public bool isPileSpot;

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
	private bool _canBeSelected = true;
	private int _containersPileCount = 1;
	private Container _parentContainer;

	private float _fadeDuration = 0.2f;

	void Awake () 
	{
		_collider = GetComponent<Collider> ();
		_meshRenderer = GetComponent<MeshRenderer> ();
		_material = _meshRenderer.material;
		_meshFilter = GetComponent<MeshFilter> ();

		Container.OnContainerSelected += OnContainerSelected;
		Container.OnContainerDeselected += OnContainerDeselected;

		Container.OnContainerMoved += ()=> DOVirtual.DelayedCall (0.2f, ()=> IsOccupied ());

		TrainsMovementManager.Instance.OnTrainMovementStart += TrainHasMoved;
		TrainsMovementManager.Instance.OnTrainMovementEnd += TrainStoppedMoving;

		if(transform.GetComponentInParent<Container> () != null)
		{
			isPileSpot = true;
			_parentContainer = transform.GetComponentInParent<Container> ();
		}
	}

	void Start () 
	{
		SetSpotType ();

		IsOccupied (true);

		OnContainerDeselected ();
	}

	void SetSpotType ()
	{
		if(transform.GetComponentInParent<Storage> () != null)
		{
			_containersParent = transform.GetComponentInParent<Storage> ().containersParent;
			spotType = SpotType.Storage;
			return;
		}

		if(transform.GetComponentInParent<Wagon> () != null)
		{
			if(!isPileSpot)
				_wagon = transform.GetComponentInParent<Wagon> ();

			_containersParent = transform.GetComponentInParent<Wagon> ().train.containersParent;
			spotType = SpotType.Train;
			return;
		}
	}

	public void IsOccupied (bool setup = false)
	{
		int containersMask = 1 << LayerMask.NameToLayer ("Containers");

		Vector3 position = transform.position;
		position.y += 0.5f;

		RaycastHit hit;
		Physics.Raycast (transform.position, Vector3.up, out hit, 4f, containersMask, QueryTriggerInteraction.Collide);

		Collider[] colliders = Physics.OverlapBox (position, new Vector3 (0.5f, 0.25f, 0.5f), Quaternion.identity, containersMask, QueryTriggerInteraction.Collide);

		isOccupied = false;

		foreach(var c in colliders)
			isOccupied = true;

		if (hit.collider != null)
		{
			isOccupied = true;
			
			if (setup && IsSameSize (hit.collider.GetComponent<Container> ()))
			{
				container = hit.collider.GetComponent<Container> ();
				hit.collider.GetComponent<Container> ().SetInitialSpot (this);
			}
		}

		if(container != null && container.spotOccupied != null && container.spotOccupied == this)
			isOccupied = true;

		/*if (hit.collider)
		{
			isOccupied = true;

			if (!IsSameSize (hit.collider.GetComponent<Container> ()))
				return;
			
			if (!setup)
				hit.collider.GetComponent<Container> ().SetInitialSpot (this);
		}
		else
			isOccupied = false;*/
	}

	bool CanPileContainer ()
	{
		int containersMask = 1 << LayerMask.NameToLayer ("Containers");

		int belowContainers = 0;

		RaycastHit[] hits = Physics.RaycastAll (transform.position, Vector3.down, 100f, containersMask, QueryTriggerInteraction.Collide);

		foreach (var h in hits)
		{
			if (container && h.collider.gameObject == container.gameObject)
				continue;
			
			belowContainers++;
		}

		if (spotType == SpotType.Train)
		{
			if (belowContainers == 0)
				return true;
			else
				return false;
		}

		else
		{
			if (belowContainers <= _containersPileCount)
				return true;
			else
				return false;
		}
	}

	public void SetContainer (Container container)
	{
		isOccupied = true;
		this.container = container;
	}

	public void RemoveContainer ()
	{
		isOccupied = false;

		container = null;
	}

	public override void OnTouchUpAsButton ()
	{
		base.OnTouchUpAsButton ();

		if (!_canBeSelected)
			return;
		
		ContainersMovementManager.Instance.TakeSpot (this);
	}

	void OnContainerSelected (Container container)
	{
		IsOccupied ();

		if (isOccupied)
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

		float delay = Vector3.Distance (container.transform.position, transform.position) * 0.01f;

		_material.DOFloat (1f, "_HologramOpacity", _fadeDuration).SetDelay (delay);
		_material.DOFloat (1f, "_Opacity", _fadeDuration).SetDelay (delay);
	}

	void OnContainerDeselected (Container container = null)
	{
		_collider.enabled = false;

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
		BoxCollider containerCollider = container._collider as BoxCollider;
		BoxCollider boxCollider = _collider as BoxCollider;

		if (Mathf.Abs (boxCollider.size.x - containerCollider.size.x) < 1f)
			return true;
		else
			return false;
	}

	void TrainHasMoved ()
	{
		_canBeSelected = false;
	}

	void TrainStoppedMoving ()
	{
		StopCoroutine (TrainStoppedMovingDelay ());
		StartCoroutine (TrainStoppedMovingDelay ());
	}

	IEnumerator TrainStoppedMovingDelay ()
	{
		yield return new WaitForSecondsRealtime (0.2f);
		_canBeSelected = true;
	}

	void OnDestroy ()
	{
		if (TrainsMovementManager.applicationIsQuitting)
			return;

		TrainsMovementManager.Instance.OnTrainMovementStart -= TrainHasMoved;
		TrainsMovementManager.Instance.OnTrainMovementEnd -= TrainStoppedMoving;

		Container.OnContainerMoved -= ()=> IsOccupied ();
	}
}
