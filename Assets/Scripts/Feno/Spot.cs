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

namespace Feno
{
public class Spot : MonoBehaviour
{

	private bool isFree = true;


	public bool IsFree {
		set {
			isFree = value;
			Refresh ();
		}
		get {
			return isFree;
		}
	}

	public Spot Up, Down;
	private Transform dot;


	public Collider colliderCp;

	private bool isHighlight = false;

	public float SizeHighlight = 5f;
	public float SizeRegular = 1f;
	public float SizePulsating = 2f;
	public float TimePulsating = 2f;
	public float DotEaseTime = .2f;

	public Color ColorRegular = Color.white;
	public Color ColorHighlight = Color.green;

	public string ContainerName;

	void Awake ()
	{
		colliderCp = GetComponent<Collider> ();
		dot = transform.parent.GetComponentInChildren<Dot> ().transform;
	}

	void Start ()
	{
		/*TouchManager.Singleton.OnTouchBegin += TouchBegin;
		TouchManager.Singleton.OnDrag += Drag;
		TouchManager.Singleton.OnTouchEnd += TouchEnd;
		Scan ();

		Camera.main.GetComponent<CameraMotion> ().OnIntroduction += Intro;*/
	}

	void Intro ()
	{
		// PreSpawn Container;
		Debug.Log (ContainerName);
		if (ContainerName != "") {
			foreach (ContainerType type in GameManager.Singleton.Containers) {
				if (type.Name == ContainerName) {
					GameObject cont = Instantiate (GameManager.Singleton.Container, transform.position + Vector3.up * 10, Quaternion.identity) as GameObject;
					cont.GetComponent<MeshRenderer> ().material = type.material;
					float downDelay = (Down != null) ? 1f : 0f;
					cont.transform.localScale = Vector3.zero;
					DOVirtual.DelayedCall (Random.Range (6f, 7f) + downDelay, () => {
						cont.GetComponent<Container> ().SnapTo (this);
						cont.transform.DOScale (Vector3.one, 1f).SetEase (Ease.OutElastic);
					});
					return;
				}
			}
		}
	}

	void Scan ()
	{
		Up = null;
		RaycastHit hit;
		Ray ray = new Ray (transform.position + Vector3.up, Vector3.up * 2f);

		Debug.DrawRay (ray.origin, ray.direction, Color.red, 50f);
		LayerMask mask = 1 << gameObject.layer;

		if (Physics.Raycast (ray, out hit, transform.lossyScale.y, mask)) {
			Up = hit.collider.GetComponent<Spot> ();
			Up.Down = this;
		}

		Refresh ();
	}

	void Refresh ()
	{
		if (Up != null) {
			Up.colliderCp.enabled = !isFree;
		}
	}

	void TouchBegin (Spot spot, Container container)
	{
		if (!isFree || !colliderCp.enabled)
			return;


		// Don't highlight the spot just above the container
		if (IsAbove (container)) {
			return;
		}
		
		dot.DOKill (true);
		dot.GetComponent<SpriteRenderer> ().DOKill (true);
		dot.GetComponent<SpriteRenderer> ().DOColor (ColorRegular, DotEaseTime);
		Pulse ();
//		dot.DOScale (SizeRegular, DotEaseTime).SetEase (Ease.OutElastic).SetDelay (Mathf.Min (.3f, Vector3.Distance (transform.position, container.transform.position) * .1f));
		dot.DOBlendableScaleBy (SizeRegular * Vector3.one, Vector3.Distance (transform.position, container.transform.position) * .2f).SetEase (Ease.OutElastic).SetDelay (Vector3.Distance (transform.position, container.transform.position) * .02f);
	}

	void Drag (Spot spot, Container container)
	{
		if (!CheckAvailability (container))
			return;

		if (spot == null) {
			HighlightOff ();
		} else {
			if (spot.GetInstanceID () == this.GetInstanceID () && !isHighlight) {
				HighlightOn ();
			} else if (spot.GetInstanceID () != this.GetInstanceID () && isHighlight) {
				HighlightOff ();
			}
		}
	}

	void TouchEnd (Spot spot, Container container)
	{
		dot.DOKill (true);
		dot.DOScale (Vector3.zero, DotEaseTime).SetEase (Ease.OutCirc);
		dot.GetComponent<SpriteRenderer> ().DOKill (true);
		dot.GetComponent<SpriteRenderer> ().DOColor (ColorRegular, DotEaseTime);
	}

	void HighlightOn ()
	{
		isHighlight = true;
		dot.GetComponent<SpriteRenderer> ().DOKill (true);

//		dot.DOScale (Vector3.one * SizeHighlight, DotEaseTime).SetEase (Ease.OutCirc);
		dot.GetComponent<SpriteRenderer> ().DOColor (ColorHighlight, DotEaseTime);
	}

	void HighlightOff ()
	{
		isHighlight = false;
		dot.GetComponent<SpriteRenderer> ().DOKill (true);
		dot.GetComponent<SpriteRenderer> ().DOColor (ColorRegular, DotEaseTime);
		//		dot.DOScale (Vector3.one * SizeRegular, DotEaseTime).SetEase (Ease.OutCirc);
	}

	public bool IsAbove (Container container)
	{
		if (container.Spot != null) {
			if (container.Spot.Up != null) {
				if (container.Spot.Up.GetInstanceID () == GetInstanceID ())
					return true;
			}
		}

		return false;
	}

	public bool CheckAvailability (Container container)
	{
		if (!isFree || !colliderCp.enabled || IsAbove (container))
			return false;
		else
			return true;
	}

	void Pulse ()
	{
		dot.transform.DOBlendableScaleBy (Vector3.one * .5f, TimePulsating).SetEase (Ease.InOutCirc).OnComplete (() => {
			dot.transform.DOBlendableScaleBy (-Vector3.one * .5f, TimePulsating).SetEase (Ease.InOutCirc).OnComplete (() => Pulse ());
		});
	}
}
}