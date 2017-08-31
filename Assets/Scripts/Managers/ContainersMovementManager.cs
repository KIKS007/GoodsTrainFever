using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class ContainersMovementManager : Singleton<ContainersMovementManager> 
{
	public Action OnContainerMovement;
	public Action OnContainerMovementEnd;

	public bool containerInMotion = false;

	[Header ("Container Pile Count")]
	public int storagePileCount = 2;
	public int boatPileCount = 1;

	[Header ("Spot Height")]
	public float containerHeight = 2.6f;

	[Header ("Selected Container")]
	public Container selectedContainer = null;

	[Header ("Start Hover")]
	public float startHoverHeight = 2.5f;
	public float startHoverDuration = 0.3f;

	[Header ("Hover")]
	public Ease hoverEase = Ease.OutQuad;
	public float hoverHeight = 1f;
	public float hoverDuration = 1f;

	[Header ("Stop Hover")]
	public float stopHoverDuration = 0.2f;

	[Header ("Take Spot")]
	public float takeSpotDuration = 0.4f;

	[Header ("Spot Distance Factor")]
	public float spotDistanceFactor = 0.02f;

	private float _startHeight;

	void Start ()
	{
		TrainsMovementManager.Instance.OnTrainDeparture += TrainDeparture;
		BoatsMovementManager.Instance.OnBoatDeparture += BoatDeparture;
	}

	public void DeselectContainer ()
	{
		if (selectedContainer)
			selectedContainer.Deselect ();
	}

	void TrainDeparture (Train train)
	{
		if (selectedContainer && selectedContainer.train == train)
			selectedContainer.Deselect ();
	}

	void BoatDeparture ()
	{
		if (selectedContainer && selectedContainer.spotOccupied.spotType == SpotType.Boat)
			selectedContainer.Deselect ();
	}

	public void StartHover (Container container)
	{
		container.transform.DOKill (true);
		_startHeight = container.transform.localPosition.y;

		ScreenshakeManager.Instance.Shake (FeedbackType.StartHover);

		container.transform.DOLocalMoveY (container.transform.localPosition.y + startHoverHeight, startHoverDuration).SetEase (hoverEase).OnComplete (()=> Hover (container));
	}

	void Hover (Container container)
	{
		container.transform.DOLocalMoveY (container.transform.localPosition.y - hoverHeight, hoverDuration).SetEase (hoverEase).OnComplete (() => 
			{
				container.transform.DOLocalMoveY (container.transform.localPosition.y + hoverHeight, hoverDuration).SetEase (hoverEase).OnComplete (()=> Hover (container));
			});
	}

	public void StopHover (Container container)
	{
		container.transform.DOKill ();
		container.transform.DOLocalMoveY (_startHeight, stopHoverDuration).SetEase (Ease.OutBounce, 40, 1);

		ScreenshakeManager.Instance.Shake (FeedbackType.StopHover);
	}

	public void TakeSpot (Spot spot)
	{
		if (selectedContainer == null)
			return;

		Container container = selectedContainer;
		Vector3 targetPosition = spot.transform.position;

		if (spot.isPileSpot && spot._parentContainer.isMoving)
		{
			targetPosition = spot._parentContainer.spotOccupied.transform.position;
			targetPosition.y += containerHeight;
		}

		container.TakeSpot (spot);

		//targetPosition = container.transform.parent.InverseTransformPoint (targetPosition);
		targetPosition = container.transform.parent.InverseTransformPoint (targetPosition);

		container.isMoving = true;

		containerInMotion = true;

		if (OnContainerMovement != null)
			OnContainerMovement ();


		container.transform.DOKill (false);

		Vector3 direction = (targetPosition - container.transform.position).normalized;

		if(VectorApproximatelyEqual (Vector3.right, direction) || VectorApproximatelyEqual (-Vector3.right, direction))
			direction = Vector3.forward * -Mathf.Sign (direction.x);
		else
			direction = Vector3.right * Mathf.Sign (direction.z);

		ScreenshakeManager.Instance.Shake (FeedbackType.StartTakeSpot);

		//360 Rotation
		//container.transform.DORotate (rotationDir * Mathf.Sign (diff.x + diff.y + diff.z) * 360f, .4f, RotateMode.FastBeyond360);
		container.transform.DORotate (direction * 360f, takeSpotDuration, RotateMode.FastBeyond360);

		container.transform.DOLocalMoveX (targetPosition.x, takeSpotDuration).SetEase (Ease.OutCubic);
		container.transform.DOLocalMoveZ (targetPosition.z, takeSpotDuration).SetEase (Ease.OutCubic);
		container.transform.DOLocalMoveY (targetPosition.y + 10f + UnityEngine.Random.Range (-2, 3), takeSpotDuration - 0.1f).SetEase (Ease.OutCubic).OnComplete (() => 
			{
				container.transform.DOLocalMoveY (targetPosition.y, takeSpotDuration + 0.1f).SetEase (Ease.OutBounce, 40, 1);
				container.transform.DOPunchRotation (direction * 10f, takeSpotDuration + 0.1f, 10).SetDelay (.1f).OnStart (() => 
					{
						ParticlesManager.Instance.CreateParticles (FeedbackType.EndTakeSpot, container.transform.position - (Vector3.up * container._collider.bounds.extents.y * 4), 0.1f);
						ScreenshakeManager.Instance.Shake (FeedbackType.EndTakeSpot);

					}).OnComplete (()=> 
						{
							containerInMotion = false;

							container.isMoving = false;

							if(OnContainerMovementEnd != null)
								OnContainerMovementEnd ();
						});
			});
	}

	bool VectorApproximatelyEqual (Vector3 a, Vector3 b)
	{
		return Vector3.SqrMagnitude (a - b) < 1f;
	}
}
