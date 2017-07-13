﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

	private float _startHeight;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartHover (Container container)
	{
		container.transform.DOKill (true);
		_startHeight = container.transform.localPosition.y;

		//ScreenshakeManager.Singleton.Shake (Vector3.up * .3f, 1, 0, .8f);
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

		//ScreenshakeManager.Singleton.Shake (Vector3.left * Mathf.Max (0, (transform.position.y / startHeight) - 1.1f), 5, .1f, .4f);
	}

	public void TakeSpot (Spot spot)
	{
		if (selectedContainer == null)
			return;

		Container container = selectedContainer;

		container.TakeSpot (spot);

		container.transform.DOKill (false);

		Vector3 rotationDir;

		rotationDir = Vector3.forward;
		Vector3 diff = Vector3.Scale (container.transform.position, InverseVector (rotationDir)) - Vector3.Scale (spot.transform.position, InverseVector (rotationDir));

		//ScreenshakeManager.Singleton.Shake (Vector3.up * .5f, 1, 0);

		//360 Rotation
		container.transform.DORotate (rotationDir * Mathf.Sign (diff.x + diff.y + diff.z) * 360f, .4f, RotateMode.FastBeyond360);

		container.transform.DOMoveX (spot.transform.position.x, .4f).SetEase (Ease.OutCubic);
		container.transform.DOMoveZ (spot.transform.position.z, .4f).SetEase (Ease.OutCubic);
		container.transform.DOMoveY (spot.transform.position.y + 10f + Random.Range (-2, 3), .3f).SetEase (Ease.OutCubic).OnComplete (() => 
			{
				container.transform.DOMoveY (spot.transform.position.y, .5f).SetEase (Ease.OutBounce, 40, 1);
				container.transform.DOPunchRotation (rotationDir * Mathf.Sign (diff.x + diff.y + diff.z) * 10f, .5f, 10).SetDelay (.1f).OnStart (() => 
					{
						
						//DOVirtual.DelayedCall (.2f, () => ParticlesManager.Singleton.Create (ParticlesManager.Singleton.FXDropFog, spot.transform.position - (Vector3.up * colliderCp.bounds.extents.y * .5f)));
						
						//ScreenshakeManager.Singleton.Shake (Vector3.up * 2, 5, .1f);
					});
			});
	}

	Vector3 InverseVector (Vector3 vector)
	{
		Vector3 v = vector;
		v.x = Mathf.Abs (v.x - 1);
		v.y = Mathf.Abs (v.y - 1);
		v.z = Mathf.Abs (v.z - 1);
		return v;
	}
}