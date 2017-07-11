/* 
 * Copyright (C) IronEqual SAS
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * File is not exclusive
 * Written by Feno <feno@ironequal.com>, 2017
 */

/*

TO DO:
 - Seperate Logic & Game Feel




*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class TouchManager : MonoBehaviour
{
	private int j;

	public LayerMask ContainerMask;
	public LayerMask SpotMask;

	private Camera cam;

	public Action<Spot, Container> OnTouchBegin, OnDrag, OnTouchEnd;

	public GameObject holdContainer;

	public static TouchManager Singleton;

	public GameObject ContainerFeedback;

	// --------------------


	void ButtonDown ()
	{
		RaycastFromCamera (ContainerMask, (RaycastHit hit) => {
			Container targetContainer = hit.transform.gameObject.GetComponent<Container> ();

			// Ensure that there isn't a Container above. (Flying Container)
			if (targetContainer.IsUnder ()) {
				return;
			}
				
			holdContainer = hit.transform.gameObject;

			if (OnTouchBegin != null)
				OnTouchBegin (new Spot (), holdContainer.GetComponent<Container> ()); 

			holdContainer.GetComponent<Container> ().StartHover ();
		});
	}

	void Button ()
	{
		RaycastFromCamera (SpotMask, (RaycastHit hit) => {
			Spot spot = null;
			ContainerFeedback.transform.position = Vector3.up * -100;

			if (hit.transform != null) {
				spot = hit.transform.GetComponent<Spot> ();

				if (!spot.CheckAvailability (holdContainer.GetComponent<Container> ()))
					return;
				
				ContainerFeedback.transform.position = spot.transform.position;
			}

			if (OnDrag != null)
				OnDrag (spot, holdContainer.GetComponent<Container> ());
		}, false);

	}

	void ButtonUp ()
	{
		RaycastFromCamera (SpotMask, (RaycastHit hit) => {
			ContainerFeedback.transform.position = Vector3.up * -100;


			Container container = holdContainer.GetComponent<Container> ();
			// If don't hit any Spot
			container.StopHover ();
			if (hit.transform == null) {
				if (OnTouchEnd != null)
					OnTouchEnd (new Spot (), null);
				return;
			}

			Spot spot = hit.transform.GetComponent<Spot> ();


			holdContainer = null;

			if (OnTouchEnd != null)
				OnTouchEnd (spot, null);

			// Ensure that we're not trying to snap to the spot just above. (Flying Container)
			if (spot.IsAbove (container)) {
				return;
			}

			if (!spot.IsFree)
				return;

			container.SnapTo (spot);
		}, false);


	}

	// --------------------

	void Awake ()
	{
		if (TouchManager.Singleton == null) {
			TouchManager.Singleton = this;
		} else {
			Destroy (gameObject);
		}
	}

	// Use this for initialization
	void Start ()
	{
		cam = GetComponent<Camera> ();
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButtonDown (0)) {
			ButtonDown ();
		} else if (Input.GetMouseButton (0) && holdContainer != null) {
			Button ();
		} else if (Input.GetMouseButtonUp (0) && holdContainer != null) {
			ButtonUp ();
		}
	}


	// --------------------


	void RaycastFromCamera (LayerMask mask, Action<RaycastHit> action, bool onlyReturnIfHit = true)
	{
		RaycastHit hit;
		Ray ray = cam.ScreenPointToRay (Input.mousePosition);

		if (Physics.Raycast (ray, out hit, Mathf.Infinity, mask)) {
			if (onlyReturnIfHit)
				action (hit);
		}

		if (!onlyReturnIfHit)
			action (hit);
	}
}