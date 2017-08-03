using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine.UI;

public enum ContainerType { Basic, Cooled, Tank, Dangerous };

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

	[Header ("States")]
	public bool selected = false;
	public bool isPileUp = false;
	public bool isMoving = false;

	[Header ("Constraints")]
	public bool allConstraintsRespected = true;
	public List<ContainerConstraint> constraints = new List<ContainerConstraint> ();

	[Header ("Size")]
	[ReadOnlyAttribute]
	public bool isDoubleSize;

	[Header ("Train")]
	public Train train = null;
	public Wagon wagon = null;

	[Header ("Spot")]
	public Spot spotOccupied = null;

	[Header ("Color")]
	public string shaderColorProperty = "_Albedo1";

	[HideInInspector]
	public Mesh _mesh;
	[HideInInspector]
	public Collider _collider;

	[HideInInspector]
	public Spot[] _pileSpots = new Spot[0];
	private Material _material;
	private Text _weightText;
	private Image _weightImage;

	void Awake ()
	{
		_mesh = GetComponent<MeshFilter> ().mesh;
		_collider = GetComponent<Collider> ();
		_pileSpots = transform.GetComponentsInChildren<Spot> ();
		_weightText = transform.GetComponentInChildren<Text> ();
		_weightImage = transform.GetComponentInChildren<Image> ();

		OnContainerMoved += IsPileUp;
		OnContainerMoved += CheckConstraints;
		TouchManager.Instance.OnTouchUpNoTarget += OnTouchUpNoTarget;

		constraints.Clear ();

		foreach (var c in transform.GetComponents<Constraint> ())
		{
			constraints.Add (new ContainerConstraint ());
			constraints [constraints.Count - 1].constraint = c;
		}

		foreach (var p in _pileSpots)
			p.gameObject.SetActive (true);

		SetIsDoubleSize ();

		//SetupColor ();

		//SetWeight ();
	}

	public void Setup (Container_Level container_Level)
	{
		containerType = container_Level.containerType;

		SetupColor (container_Level.containerColor);

		weight = container_Level.containerWeight;

		UpdateWeightText ();

		SetIsDoubleSize ();
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

	void OnTouchUpNoTarget ()
	{
		if (selected)
			Deselect ();
	}

	public void SetInitialSpot (Spot spot)
	{
		spotOccupied = spot;

		transform.position = spot.transform.position;

		if(spot._containersParent != null)
			transform.SetParent (spot._containersParent);

		if(spot.spotType == SpotType.Train)
		{
			wagon = spot._wagon;
			train = wagon.train;
		}

		spot.SetContainer (this);

		//Debug.Log (spotOccupied, this);
	}

	void UpdateWeightText ()
	{
		GlobalVariables globalVariables = FindObjectOfType<GlobalVariables> ();

		_weightText = transform.GetComponentInChildren<Text> ();
		_weightImage = transform.GetComponentInChildren<Image> ();

		_weightText.text = weight.ToString ();

		Color color = new Color ();

		switch (containerColor)
		{
		case ContainerColor.Red:
			color = globalVariables.redColor;
			break;
		case ContainerColor.Blue:
			color = globalVariables.blueColor;
			break;
		case ContainerColor.Yellow:
			color = globalVariables.yellowColor;
			break;
		case ContainerColor.Violet:
			color = globalVariables.violetColor;
			break;
		}

		_weightImage.color = color;
	}

	public override void OnTouchUpAsButton ()
	{
		base.OnTouchUpAsButton ();

		if (TrainsMovementManager.Instance.selectedTrainHasMoved || TrainsMovementManager.Instance.resetingTrains)
			return;

		if (train && train.inTransition)
			return;

		if (spotOccupied.spotType == SpotType.Boat && BoatsMovementManager.Instance.inTransition)
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

		RemoveContainer ();

		spot.SetContainer (this);

		spotOccupied = spot;

		selected = false;

		transform.SetParent (spot._containersParent);

		if(spot.spotType == SpotType.Train)
		{
			wagon = spot._wagon;
			train = wagon.train;
			TrainsMovementManager.Instance.trainContainerInMotion = train;

			CheckConstraints ();
		}
		else
		{
			wagon = null;
			train = null;

			allConstraintsRespected = true;
		}

		SetPileSpot ();

		if (OnContainerMoved != null)
			OnContainerMoved ();
		
		if (OnContainerDeselected != null)
			OnContainerDeselected (this);
	}

	public void RemoveContainer ()
	{
		if(spotOccupied != null)
			spotOccupied.RemoveContainer ();

		spotOccupied = null;
	}

	public void CheckConstraints ()
	{
		if(spotOccupied == null || spotOccupied.spotType != SpotType.Train)
		{
			allConstraintsRespected = true;
			return;
		}

		bool allRespected = true;

		foreach(var c in constraints)
		{
			c.isRespected = c.constraint.IsRespected ();

			if (!c.isRespected)
				allRespected = false;
		}

		allConstraintsRespected = allRespected;
	}

	public bool CheckConstraints (Spot spot)
	{
		if(spotOccupied == null || spotOccupied.spotType != SpotType.Train)
		{
			allConstraintsRespected = true;
			return allConstraintsRespected;
		}

		bool allRespected = true;

		foreach(var c in constraints)
		{
			c.isRespected = c.constraint.IsRespected (spot);

			if (!c.isRespected)
				allRespected = false;
		}

		allConstraintsRespected = allRespected;
		return allRespected;
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
			if(s.isOccupied)
			{
				isPiledUpTemp = true;
				break;
			}
		}

		isPileUp = isPiledUpTemp;

		if (spotOccupied)
			spotOccupied.SetIsPileUp (isPiledUpTemp);
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

		OnContainerMoved -= IsPileUp;
		OnContainerMoved -= CheckConstraints;
		TouchManager.Instance.OnTouchUpNoTarget -= OnTouchUpNoTarget;
	}

	public void SetupColor ()
	{
		GlobalVariables globalVariables = FindObjectOfType<GlobalVariables> ();
		MeshRenderer meshRenderer = GetComponent<MeshRenderer> ();

		meshRenderer.sharedMaterial = new Material (meshRenderer.sharedMaterial);
		_material = meshRenderer.sharedMaterial;

		containerColor = (ContainerColor) UnityEngine.Random.Range (0, (int) Enum.GetNames (typeof(ContainerColor)).Length);

		Color color = new Color ();

		switch (containerColor)
		{
		case ContainerColor.Red:
			color = globalVariables.redColor;
			break;
		case ContainerColor.Blue:
			color = globalVariables.blueColor;
			break;
		case ContainerColor.Yellow:
			color = globalVariables.yellowColor;
			break;
		case ContainerColor.Violet:
			color = globalVariables.violetColor;
			break;
		}

		if(_material.HasProperty (shaderColorProperty))
			_material.SetColor (shaderColorProperty, color);
		else
			_material.SetColor (shaderColorProperty, color);

		UpdateWeightText ();
	}

	public void SetupColor (ContainerColor c)
	{
		GlobalVariables globalVariables = FindObjectOfType<GlobalVariables> ();
		MeshRenderer meshRenderer = GetComponent<MeshRenderer> ();

		meshRenderer.sharedMaterial = new Material (meshRenderer.sharedMaterial);
		_material = meshRenderer.sharedMaterial;

		containerColor = c;

		Color color = new Color ();

		switch (c)
		{
		case ContainerColor.Red:
			color = globalVariables.redColor;
			break;
		case ContainerColor.Blue:
			color = globalVariables.blueColor;
			break;
		case ContainerColor.Yellow:
			color = globalVariables.yellowColor;
			break;
		case ContainerColor.Violet:
			color = globalVariables.violetColor;
			break;
		}

		if(_material.HasProperty ("_Albedo"))
			_material.SetColor ("_Albedo", color);
		else
			_material.SetColor ("_Albedo1", color);

		UpdateWeightText ();
	}

	[PropertyOrder (-1)]
	[ButtonAttribute ("Update Color")]
	void UpdateAllColor ()
	{
		foreach (var c in FindObjectsOfType<Container> ())
			c.SetupColor ();
	}

	[PropertyOrder (0)]
	[ButtonAttribute ("Take Spot")]
	public void EditorTakeSpot ()
	{
		if(spotOccupied == null)
		{
			Debug.LogWarning ("No Spot!");
			return;
		}

		Vector3 position = spotOccupied.transform.position;
		position.y += 0.01f;
		transform.position = position;
	}

	[System.Serializable]
	public class ContainerConstraint
	{
		public bool isRespected = true;
		[SerializeField]
		public Constraint constraint;
	}
}
