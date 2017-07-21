using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;

public enum ContainerType { Basic, Food, Metal, Dangerous };

public enum ContainerColor { Red, Blue, Yellow, Violet }

public class Container : Touchable 
{
	public static Action <Container> OnContainerSelected;
	public static Action <Container> OnContainerDeselected;
	public static Action OnContainerMoved;

	[Header ("Container Type")]
	public ContainerType containerType = ContainerType.Basic;
	public ContainerColor containerColor;
	public int weight = 0;
	public bool isDoubleSize = false;

	[Header ("States")]
	public bool selected = false;
	public bool isPileUp = false;
	public bool isMoving = false;

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
	private Material _material;

	void Awake ()
	{
		_mesh = GetComponent<MeshFilter> ().mesh;
		_collider = GetComponent<Collider> ();
		_pileSpots = transform.GetComponentsInChildren<Spot> ();

		foreach (var p in _pileSpots)
			p.gameObject.SetActive (true);

		SetWeight ();

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

	void SetWeight ()
	{
		foreach(var w in GlobalVariables.Instance.containersWeight)
			if(w.containerType == containerType)
			{
				weight = (int) UnityEngine.Random.Range ((int)w.weightBounds.x, (int)w.weightBounds.y);
				break;
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

		TouchManager.Instance.OnTouchUpNoTarget -= OnTouchUpNoTarget;
	}

	public void SetupColor ()
	{
		GlobalVariables gameManager = FindObjectOfType<GlobalVariables> ();
		MeshRenderer meshRenderer = GetComponent<MeshRenderer> ();

		meshRenderer.sharedMaterial = new Material (meshRenderer.sharedMaterial);
		_material = meshRenderer.sharedMaterial;

		containerColor = (ContainerColor) UnityEngine.Random.Range (0, (int) Enum.GetNames (typeof(ContainerColor)).Length);

		Color color = new Color ();

		switch (containerColor)
		{
		case ContainerColor.Red:
			color = gameManager.redColor;
			break;
		case ContainerColor.Blue:
			color = gameManager.blueColor;
			break;
		case ContainerColor.Yellow:
			color = gameManager.yellowColor;
			break;
		case ContainerColor.Violet:
			color = gameManager.violetColor;
			break;
		}

		if(_material.HasProperty ("_Albedo"))
			_material.SetColor ("_Albedo", color);
		else
			_material.SetColor ("_Albedo1", color);
	}

	public void SetupColor (ContainerColor c)
	{
		GlobalVariables gameManager = FindObjectOfType<GlobalVariables> ();
		MeshRenderer meshRenderer = GetComponent<MeshRenderer> ();

		meshRenderer.sharedMaterial = new Material (meshRenderer.sharedMaterial);
		_material = meshRenderer.sharedMaterial;

		containerColor = c;

		Color color = new Color ();

		switch (c)
		{
		case ContainerColor.Red:
			color = gameManager.redColor;
			break;
		case ContainerColor.Blue:
			color = gameManager.blueColor;
			break;
		case ContainerColor.Yellow:
			color = gameManager.yellowColor;
			break;
		case ContainerColor.Violet:
			color = gameManager.violetColor;
			break;
		}

		if(_material.HasProperty ("_Albedo"))
			_material.SetColor ("_Albedo", color);
		else
			_material.SetColor ("_Albedo1", color);
	}

	[PropertyOrder (-1)]
	[ButtonAttribute ("Update Color")]
	void UpdateAllColor ()
	{
		foreach (var c in FindObjectsOfType<Container> ())
			c.SetupColor ();
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
