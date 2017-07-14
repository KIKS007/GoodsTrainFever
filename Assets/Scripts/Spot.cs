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

	private Collider _collider;
	private MeshRenderer _meshRenderer;
	private MeshFilter _meshFilter;
	private Material _material;

	private float _fadeDuration = 0.2f;

	// Use this for initialization
	void Start () 
	{
		_collider = GetComponent<Collider> ();
		_meshRenderer = GetComponent<MeshRenderer> ();
		_material = _meshRenderer.material;
		_meshFilter = GetComponent<MeshFilter> ();

		Container.OnContainerSelected += OnContainerSelected;
		Container.OnContainerDeselected += OnContainerDeselected;

		SetSpotType ();

		IsOccupied ();

		OnContainerDeselected ();
	}

	void SetSpotType ()
	{
		if(transform.GetComponentInParent<Wagon> () != null)
		{
			spotType = SpotType.Train;
			return;
		}
	}

	void IsOccupied ()
	{
		int containersMask = 1 << LayerMask.NameToLayer ("Containers");

		Vector3 position = transform.position;
		position.y += 1;

		RaycastHit hit;
		Physics.Raycast (transform.position, Vector3.up, out hit, 2f, containersMask, QueryTriggerInteraction.Collide);

		if (hit.collider)
		{
			hit.collider.GetComponent<Container> ().SetInitialSpot (this);
			isOccupied = true;
		}
		else
			isOccupied = false;
	}

	public override void OnTouchUpAsButton ()
	{
		base.OnTouchUpAsButton ();

		ContainersMovementManager.Instance.TakeSpot (this);
	}

	void OnContainerSelected (Container container)
	{
		if (isOccupied || !IsSameSize (container))
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

		_material.DOFloat (0f, "_HologramOpacity", _fadeDuration);
		_material.DOFloat (0f, "_Opacity", _fadeDuration).OnComplete (()=> _meshRenderer.enabled = false);
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
}
