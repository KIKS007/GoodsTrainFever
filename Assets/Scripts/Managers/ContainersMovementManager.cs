﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class ContainersMovementManager : Singleton<ContainersMovementManager> 
{
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

	private float _startHeight;


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

		container.TakeSpot (spot);

		container.transform.DOKill (false);

		Vector3 direction = (spot.transform.position - container.transform.position).normalized;

		if(VectorApproximatelyEqual (Vector3.right, direction) || VectorApproximatelyEqual (-Vector3.right, direction))
			direction = Vector3.forward * -Mathf.Sign (direction.x);
		else
			direction = Vector3.right * Mathf.Sign (direction.z);

		ScreenshakeManager.Instance.Shake (FeedbackType.StartTakeSpot);

		//360 Rotation
		//container.transform.DORotate (rotationDir * Mathf.Sign (diff.x + diff.y + diff.z) * 360f, .4f, RotateMode.FastBeyond360);
		container.transform.DORotate (direction * 360f, .4f, RotateMode.FastBeyond360);

		container.transform.DOMoveX (spot.transform.position.x, .4f).SetEase (Ease.OutCubic);
		container.transform.DOMoveZ (spot.transform.position.z, .4f).SetEase (Ease.OutCubic);
		container.transform.DOMoveY (spot.transform.position.y + 10f + UnityEngine.Random.Range (-2, 3), .3f).SetEase (Ease.OutCubic).OnComplete (() => 
			{
				container.transform.DOMoveY (spot.transform.position.y, .5f).SetEase (Ease.OutBounce, 40, 1);
				container.transform.DOPunchRotation (direction * 10f, .5f, 10).SetDelay (.1f).OnStart (() => 
					{
						//DOVirtual.DelayedCall (.2f, () => ParticlesManager.Singleton.Create (ParticlesManager.Singleton.FXDropFog, spot.transform.position - (Vector3.up * colliderCp.bounds.extents.y * .5f)));
						//container.OnContainerMovedEvent ();
						ScreenshakeManager.Instance.Shake (FeedbackType.EndTakeSpot);

					});
			});
	}

	bool VectorApproximatelyEqual (Vector3 a, Vector3 b)
	{
		return Vector3.SqrMagnitude (a - b) < 1f;
	}
}
