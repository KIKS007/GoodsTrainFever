/* 
 * Copyright (C) IronEqual SAS
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * File is not exclusive
 * Written by Feno <feno@ironequal.com>, 2017
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Container : MonoBehaviour
{
	private Spot spot = null;
	private float startHeight = 0f;

	public Spot Spot {
		set {
			if (spot != null)
				spot.IsFree = true;
			spot = value;
			spot.IsFree = false;
		}
		get {
			return spot;
		}
	}


	private Collider colliderCp;
	// Use this for initialization
	void Start ()
	{
		colliderCp = GetComponent<Collider> ();
	}

	public void SnapTo (Spot spot)
	{
		transform.DOKill (false);

		Vector3 rotationDir;

		if ((transform.position - spot.transform.position).z < 1)
			rotationDir = Vector3.forward;
		else
			rotationDir = Vector3.right;

		rotationDir = Vector3.forward;
		Vector3 diff = Vector3.Scale (transform.position, InverseVector (rotationDir)) - Vector3.Scale (spot.transform.position, InverseVector (rotationDir));

		ScreenshakeManager.Singleton.Shake (Vector3.up * .5f, 1, 0);

		transform.DORotate (rotationDir * Mathf.Sign (diff.x + diff.y + diff.z) * 360f, .4f, RotateMode.FastBeyond360);
		transform.DOMoveX (spot.transform.position.x, .4f).SetEase (Ease.OutCubic);
		transform.DOMoveZ (spot.transform.position.z, .4f).SetEase (Ease.OutCubic);
		transform.DOMoveY (spot.transform.position.y + 10f + Random.Range (-2, 3), .3f).SetEase (Ease.OutCubic).OnComplete (() => {
			transform.DOMoveY (spot.transform.position.y, .5f).SetEase (Ease.OutBounce, 40, 1);
			transform.DOPunchRotation (rotationDir * Mathf.Sign (diff.x + diff.y + diff.z) * 10f, .5f, 10).SetDelay (.1f).OnStart (() => {
				DOVirtual.DelayedCall (.2f, () => ParticlesManager.Singleton.Create (ParticlesManager.Singleton.FXDropFog, spot.transform.position - (Vector3.up * colliderCp.bounds.extents.y * .5f)));
				ScreenshakeManager.Singleton.Shake (Vector3.up * 2, 5, .1f);
			});
		});

		Spot = spot;
	}

	Vector3 InverseVector (Vector3 vector)
	{
		Vector3 v = vector;
		v.x = Mathf.Abs (v.x - 1);
		v.y = Mathf.Abs (v.y - 1);
		v.z = Mathf.Abs (v.z - 1);
		return v;
	}

	public bool IsUnder ()
	{
		if (Spot != null) {
			if (Spot.Up != null) {
				if (!Spot.Up.IsFree)
					return true;
			}
		}
		return false;
	}

	public void StartHover ()
	{
		transform.DOKill (true);
		startHeight = transform.localPosition.y;
		ScreenshakeManager.Singleton.Shake (Vector3.up * .3f, 1, 0, .8f);
		transform.DOLocalMoveY (transform.localPosition.y + 2.5f, .3f).SetEase (Ease.InOutCirc).OnComplete (() => {
			transform.DOLocalMoveY (transform.localPosition.y - 1f, 1f).SetEase (Ease.InOutCirc).OnComplete (() => Hover ());
		});
	}

	public void Hover ()
	{
		transform.DOLocalMoveY (transform.localPosition.y + 1f, 1f).SetEase (Ease.InOutCirc).OnComplete (() => {
			transform.DOLocalMoveY (transform.localPosition.y - 1f, 1f).SetEase (Ease.InOutCirc).OnComplete (() => Hover ());
		});
	}

	public void StopHover ()
	{
		transform.DOKill ();
		transform.DOLocalMoveY (startHeight, .2f).SetEase (Ease.OutBounce, 40, 1);
		Debug.Log ((transform.position.y / startHeight) - 1);
		ScreenshakeManager.Singleton.Shake (Vector3.left * Mathf.Max (0, (transform.position.y / startHeight) - 1.1f), 5, .1f, .4f);
	}
}

