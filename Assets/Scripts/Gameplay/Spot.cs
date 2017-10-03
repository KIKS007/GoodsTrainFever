using System.Collections;
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
    public Action<Container> OnSpotFreed;

    [Header("Spot")]
    public SpotType spotType;
    public bool isOccupied = false;

    [Header("Read Only")]
    [ReadOnlyAttribute]
    public bool isDoubleSize = false;
    [ReadOnlyAttribute]
    public bool isPileSpot;
    [ReadOnlyAttribute]
    public bool isPileUp = false;

    [Header("Container")]
    public Container container;

    [Header("Overlapping Spots")]
    public List<Spot> overlappingSpots = new List<Spot>();

    [Header("Subordinate Spot")]
    public bool isSubordinate = false;
    [ShowIf("isSubordinate")]
	public List<Spot> chiefSpots = new List<Spot>();
	public List<Spot> subordinatesSpots = new List<Spot>();

    [HideInInspector]
    public Wagon _wagon;
    [HideInInspector]
    public Transform _containersParent;
    [HideInInspector]
    public Container _parentContainer;
    [HideInInspector]
    public bool _isSpawned = false;
	public int _spotTrainIndex;
    public int _spotWagonIndex;

    private Collider _collider;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private Material _material;
    private float _fadeDuration = 0.2f;
    private Spot _doubleSizeSpotSpawned;
    private float _opacity;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _material = _meshRenderer.material;
        _meshFilter = GetComponent<MeshFilter>();

        _opacity = _material.GetFloat("_Opacity");

        _material.SetFloat("_Opacity", 0f);

        Container.OnContainerSelected += OnContainerSelected;
        Container.OnContainerDeselected += OnContainerDeselected;

        if (transform.GetComponentInParent<Container>() != null)
        {
            isPileSpot = true;
            _parentContainer = transform.GetComponentInParent<Container>();
        }

        SetIsDoubleSize();

        SetIsPileSpot();

        GetOverlappingSpots();

        SetSpotType(true);

        IsOccupied();
    }

    void Start()
    {
		 _meshRenderer.enabled = true;

        SetSpotType();

        if (!_isSpawned)
            OnContainerDeselected();

        OverlappingSpotsOccupied();

        if (ContainersMovementManager.Instance.selectedContainer != null)
            OnContainerSelected(ContainersMovementManager.Instance.selectedContainer);
    }

    public void SetIsDoubleSize()
    {
        _collider = GetComponent<Collider>();
        BoxCollider boxCollider = _collider as BoxCollider;

        if (boxCollider.size.x > 12f)
            isDoubleSize = true;
        else
            isDoubleSize = false;
    }

    public void SetIsPileSpot()
    {
        if (transform.parent.GetComponent<Container>() != null || transform.parent.parent.GetComponent<Container>() != null)
            isPileSpot = true;
        else
            isPileSpot = false;
    }

    public void GetOverlappingSpots()
    {
        int spotsMask = 1 << LayerMask.NameToLayer("Spots");

        Vector3 position = transform.position;
        position.y += 0.5f;

        Collider[] colliders = Physics.OverlapBox(position, new Vector3(0.5f, 0.25f, 0.5f), Quaternion.identity, spotsMask, QueryTriggerInteraction.Collide);

        foreach (var c in colliders)
        {
            Spot spot = c.GetComponent<Spot>();

			if (isSubordinate && spot.isDoubleSize)
				continue;

			if (c != _collider && spot != null && !spot.isSubordinate)
            {
                if (!isDoubleSize && spot.isDoubleSize || isDoubleSize)
                    overlappingSpots.Add(spot);
            }
        }

        overlappingSpots = overlappingSpots.OrderByDescending(x => x.transform.position.x).ToList();

		if (_isSpawned || isSubordinate)
            foreach (var o in overlappingSpots)
                o.overlappingSpots.Add(this);

		if (isSubordinate)
			foreach (var c in chiefSpots)
				c.subordinatesSpots.Add (this);
    }

    public void OverlappingSpotsOccupied()
    {
        if (container == null)
            isOccupied = false;

        foreach (var s in overlappingSpots)
        {
			if (s.container && s.container != null)
			//if (s.container && s.container != null && !isSubordinate)
            {
                if (!isDoubleSize && s.isDoubleSize)
                    isOccupied = true;

                if (isDoubleSize && !s.isDoubleSize)
                    isOccupied = true;
            }

			/*if(s.isOccupied && isSubordinate)
			{
				if (!isDoubleSize && s.isDoubleSize)
					isOccupied = true;

				if (isDoubleSize && !s.isDoubleSize)
					isOccupied = true;
			}*/
        }

		foreach (var s in subordinatesSpots)
		{
			if(s.container && s.container != ContainersMovementManager.Instance.selectedContainer)
				isOccupied = true;
		}
    }

    void SetSpotType(bool onlyType = false)
    {
        if (transform.GetComponentInParent<Storage>() != null)
        {
            spotType = SpotType.Storage;

            if (!onlyType)
                _containersParent = transform.GetComponentInParent<Storage>().containersParent;

            return;
        }

        if (transform.GetComponentInParent<Wagon>() != null)
        {
            spotType = SpotType.Train;

            if (!isPileSpot)
                _wagon = transform.GetComponentInParent<Wagon>();

            if (!onlyType)
                _containersParent = transform.GetComponentInParent<Wagon>().train.containersParent;
            return;
        }

        if (transform.GetComponentInParent<Boat>() != null)
        {
            spotType = SpotType.Boat;

            if (!onlyType)
                _containersParent = transform.GetComponentInParent<Boat>().containersParent;
            return;
        }
    }

    public void IsOccupied()
    {
        int containersMask = 1 << LayerMask.NameToLayer("Containers");

        Vector3 position = transform.position;

        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.up, out hit, 4f, containersMask, QueryTriggerInteraction.Collide);

        isOccupied = false;

        if (hit.collider != null)
        {
            isOccupied = true;

            if (IsSameSize(hit.collider.GetComponent<Container>()))
            {
                container = hit.collider.GetComponent<Container>();
                hit.collider.GetComponent<Container>().SetInitialSpot(this);
            }
        }

        if (container != null && container.spotOccupied != null && container.spotOccupied == this)
            isOccupied = true;
    }

    public void SetInitialContainer(Container c)
    {
        if (!IsSameSize(c))
        {
            Debug.LogWarning("Not Same Size with " + c.name + " !", this);
            return;
        }

        isOccupied = true;

        c.SetInitialSpot(this);
    }

    public bool CanPileContainer()
    {
        int containersMask = 1 << LayerMask.NameToLayer("Containers");

        int belowContainers = 0;

        Vector3 rayPosition = transform.position;
        rayPosition.x -= 0.01f;

        if (isPileSpot && _parentContainer.isMoving)
        {
            rayPosition = _parentContainer.spotOccupied.transform.position;
            rayPosition.y += ContainersMovementManager.Instance.containerHeight;

            belowContainers = 1;
        }

        RaycastHit[] hits = Physics.RaycastAll(rayPosition, Vector3.down, 100f, containersMask, QueryTriggerInteraction.Collide);

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
            int pileCount = spotType == SpotType.Storage ? ContainersMovementManager.Instance.storagePileCount : ContainersMovementManager.Instance.boatPileCount;

            if (belowContainers <= pileCount)
                return true;
            else
                return false;
        }
    }

    public void SetContainer(Container container)
    {
        isOccupied = true;
        this.container = container;

        foreach (var s in overlappingSpots)
            s.OverlappingSpotsOccupied();

        if (OnSpotTaken != null)
            OnSpotTaken(this.container);
    }

    public void RemoveContainer()
    {
        isOccupied = false;

        if (OnSpotFreed != null)
            OnSpotFreed(container);

        container = null;

        foreach (var s in overlappingSpots)
            s.OverlappingSpotsOccupied();
    }

    public override void OnTouchUpAsButton()
    {
        base.OnTouchUpAsButton();

        if (TrainsMovementManager.Instance.selectedTrainHasMoved || TrainsMovementManager.Instance.resetingTrains)
			return;

        if (_wagon && _wagon.train.inTransition && !_wagon.train.waitingDeparture)
            return;

        if (spotType == SpotType.Boat && BoatsMovementManager.Instance.inTransition)
            return;

        ContainersMovementManager.Instance.TakeSpot(this);
    }

    void OnContainerSelected(Container container)
    {
        SpawnDoubleSizeSpot(container);

        OverlappingSpotsOccupied();

        if (isOccupied)
            return;

        if (_wagon && _wagon.train.inTransition && !_wagon.train.waitingDeparture)
            return;

        if (isSubordinate && !AreChiefSpotOccupied())
            return;

        if (!IsSameSize(container))
            return;

        if (!CanPileContainer())
            return;

        /*if (!AreConstraintsRespected (container))
			return;*/

        if (isPileSpot && _parentContainer.selected)
            return;

       //  _meshRenderer.enabled = true;

        _meshFilter.mesh = container._mesh;
        _collider.enabled = true;

        DOTween.Kill(_material);

        float delay = Vector3.Distance(container.transform.position, transform.position) * ContainersMovementManager.Instance.spotDistanceFactor;

		if (_wagon && _wagon.train.inTransition && _wagon.train.waitingDeparture)
			delay = 0;

        _material.DOFloat(_opacity, "_Opacity", _fadeDuration).SetDelay(delay);
    }

    void OnContainerDeselected(Container container = null)
    {
        _collider.enabled = false;

        DOTween.Kill(_material);

        _material.DOFloat(0f, "_HologramOpacity", _fadeDuration);
        _material.DOFloat(0f, "_Opacity", _fadeDuration).OnComplete(() =>
      {
          /*if (_meshRenderer)
              _meshRenderer.enabled = false;*/
      });
    }

    public bool AreConstraintsRespected(Container container)
    {
        if (spotType != SpotType.Train)
            return true;

        if (container.constraints.Count == 0)
            return true;

        foreach (var c in container.constraints)
            if (!c.constraint.IsRespected(this))
            {
                //Debug.Log (_spotTrainIndex, this);
                return false;
            }

        return true;
    }

    public bool IsSameSize(Container container)
    {
        if (isDoubleSize && container.isDoubleSize || !isDoubleSize && !container.isDoubleSize)
            return true;
        else
            return false;
    }

    public void SetIsPileUp(bool piled)
    {
        isPileUp = piled;

        foreach (var s in overlappingSpots)
            s.isPileUp = piled;
    }

    public bool AreChiefSpotOccupied()
    {
		if (!isSubordinate || chiefSpots.Count == 0)
            return false;

		int occupiedCount = 0;

		foreach (var c in chiefSpots)
			if (c.isOccupied && c.container == null || c.isOccupied && c.container == ContainersMovementManager.Instance.selectedContainer)
				occupiedCount++;

		if(occupiedCount == chiefSpots.Count)
			return true;
		else
			return false;
    }

    public Spot SpawnDoubleSizeSpot(Container c, bool selectOnStart = true)
    {
        if (_doubleSizeSpotSpawned && _doubleSizeSpotSpawned.isOccupied)
            return null;

        if (_doubleSizeSpotSpawned)
            Destroy(_doubleSizeSpotSpawned.gameObject);

        if (!c.isDoubleSize)
            return null;

        if (spotType == SpotType.Train)
            return null;

        if (!isDoubleSize)
            return null;

        if (container)
            return null;

        if (isPileUp)
            return null;

        foreach (var s in overlappingSpots)
        {
            if (s.isOccupied == false)
                return null;
            else
            {
                foreach (var pileSpot in s.container._pileSpots)
                    if (pileSpot.isOccupied)
                        return null;
            }
        }

        Vector3 spotPosition = transform.position;
        spotPosition.y += ContainersMovementManager.Instance.containerHeight;

        _doubleSizeSpotSpawned = (Instantiate(GlobalVariables.Instance.spot40SpawnedPrefab, spotPosition, transform.rotation, transform.parent)).GetComponent<Spot>();
        _doubleSizeSpotSpawned._isSpawned = true;

        _doubleSizeSpotSpawned._containersParent = _containersParent;

        _doubleSizeSpotSpawned.overlappingSpots.Clear();

        foreach (var o in overlappingSpots)
            foreach (var p in o.container._pileSpots)
                _doubleSizeSpotSpawned.overlappingSpots.Add(p);

        _doubleSizeSpotSpawned.GetOverlappingSpots();

        if (selectOnStart)
            _doubleSizeSpotSpawned.OnContainerSelected(c);

        return _doubleSizeSpotSpawned;
    }

    void OnDestroy()
    {
        if (TrainsMovementManager.applicationIsQuitting)
            return;

        Container.OnContainerSelected -= OnContainerSelected;
        Container.OnContainerDeselected -= OnContainerDeselected;

        if (_isSpawned)
            foreach (var o in overlappingSpots)
                o.overlappingSpots.Remove(this);
    }
}
