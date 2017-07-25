using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class BoatsMovementManager : Singleton<BoatsMovementManager>
{
	public Action OnBoatDeparture;

	[Header ("Boat")]
	public Transform boat;

	[Header ("State")]
	public bool inTransition = false;

	[Header ("Movements")]
	public Ease boatEase = Ease.OutQuad;
	public float boatSpeed = 20f;
	public float arrivingXPosition;
	public float gameXPosition;
	public float departureXPosition;

	public void BoatSpawn ()
	{
		ClearBoat ();

		inTransition = true;

		boat.DOMoveX (gameXPosition, boatSpeed).SetSpeedBased ().SetEase (boatEase).OnComplete (()=> inTransition = false);
	}

	public void BoatDeparture ()
	{
		inTransition = true;

		if (OnBoatDeparture != null)
			OnBoatDeparture ();

		boat.DOMoveX (departureXPosition, boatSpeed).SetSpeedBased ().SetEase (boatEase).OnComplete (()=> inTransition = false);
	}

	public void ClearBoat ()
	{
		Vector3 position = boat.transform.position;
		position.x = arrivingXPosition;

		boat.transform.position = position;
	}
}
