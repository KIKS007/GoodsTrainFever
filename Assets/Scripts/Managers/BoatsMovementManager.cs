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
	public List<Boat> spawnedBoats = new List<Boat> ();

	[Header ("State")]
	public bool inTransition = false;

	[Header ("Movements")]
	public Ease boatEase = Ease.OutQuad;
	public float boatSpeed = 20f;
	public float arrivingXPosition;
	public float gameXPosition;
	public float departureXPosition;

	[Header ("Prefab")]
	public GameObject boatPrefab;

	public void BoatStart ()
	{
		ClearBoat ();

		inTransition = true;

		boat.DOMoveX (gameXPosition, boatSpeed).SetSpeedBased ().SetEase (boatEase).OnComplete (() => inTransition = false);
		LevelsManager.Instance.SetCurrentBoatTimer (boat.gameObject.GetComponent<Boat> ().TimerText);
	}

	public void BoatStart (Boat boat)
	{
		inTransition = true;

		Vector3 position = boat.transform.position;
		position.x = arrivingXPosition;

		boat.transform.position = position;

		boat.transform.DOMoveX (gameXPosition, boatSpeed).SetSpeedBased ().SetEase (boatEase).OnComplete (() => inTransition = false);
		LevelsManager.Instance.SetCurrentBoatTimer (boat.TimerText);
	}

	public void BoatDeparture ()
	{
		inTransition = true;

		if (OnBoatDeparture != null)
			OnBoatDeparture ();
		boat.DOMoveX (departureXPosition, boatSpeed).SetSpeedBased ().SetEase (boatEase).OnComplete (() => inTransition = false);
	}

	public void BoatDeparture (Boat boat)
	{
		inTransition = true;

		if (OnBoatDeparture != null)
			OnBoatDeparture ();
		boat.transform.DOMoveX (departureXPosition, boatSpeed).SetSpeedBased ().SetEase (boatEase).OnComplete (() => {
			
			inTransition = false;
			Destroy (boat.gameObject);
		});
	}

	public void ClearBoat ()
	{
		Vector3 position = boat.transform.position;
		position.x = arrivingXPosition;

		boat.transform.position = position;

		foreach (var b in spawnedBoats)
			if (b != null)
				Destroy (b.gameObject);

		spawnedBoats.Clear ();
	}

	public Boat SpawnBoat ()
	{
		Vector3 position = boatPrefab.transform.position;
		position.x = arrivingXPosition;

		position.x += 30 * (spawnedBoats.Count + 1);

		Boat boat = (Instantiate (boatPrefab, position, boatPrefab.transform.rotation, GlobalVariables.Instance.gameplayParent)).GetComponent<Boat> ();
		spawnedBoats.Add (boat);

		return boat;
	}
}
