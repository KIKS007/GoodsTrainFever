using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class OrdersManager : Singleton<OrdersManager> 
{
	[Header ("Train Spawn")]
	public float xArrivingPosition;
	public int wagonsCount = 2;
	[Range (0, 101)]
	public int doubleSizeWagonChance = 50;

	[Header ("Length")]
	public float wagonLength = 10f;
	public float locomotiveLength = 10f;
	public float offsetLength = 10f;

	[Header ("Prefabs")]
	public GameObject trainPrefab;
	public GameObject wagonPrefab;
	public GameObject wagonDoublePrefab;

	[Header ("Fast Forward")]
	public float fastForwardDuration;
	public Ease fastForwardEase = Ease.OutQuad;
	public float xDeparturePosition;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void SpawnTrain (Rail rail)
	{
		Vector3 position = rail.transform.position;
		position.y = trainPrefab.transform.position.y;
		position.x = xArrivingPosition;

		GameObject train = Instantiate (trainPrefab, position, trainPrefab.transform.rotation);

		Train trainScript = train.GetComponent<Train> ();

		for(int i = 0; i < wagonsCount; i++)
		{
			GameObject prefab = wagonPrefab;

			if (Random.Range (0, 100) < doubleSizeWagonChance)
				prefab = wagonDoublePrefab;

			Vector3 wagonPosition = position;
			wagonPosition.x -= locomotiveLength;
			wagonPosition.x -= wagonLength * i;

			GameObject wagon = Instantiate (prefab, wagonPosition, prefab.transform.rotation, trainScript.wagonsParent);

			trainScript.wagons.Add (wagon.GetComponent<Wagon> ());
		}

		rail.train = trainScript;
		TrainsMovementManager.Instance.AddTrain (trainScript);
	}

	public void FastForwardTrain (Train train)
	{
		train.departed = true;

		float xPosition = xDeparturePosition + train.wagons.Count * wagonLength + locomotiveLength + offsetLength;

		train.transform.DOMoveX (xPosition, fastForwardDuration).SetEase (fastForwardEase).OnComplete (()=> 
		{
				TrainsMovementManager.Instance.RemoveTrain (train);
				Destroy (train.gameObject);
		});
	}

	public Train trainTest;
	[ButtonAttribute ("Fast Forward Train")]
	public void FastForwardTrainTest ()
	{
		FastForwardTrain (trainTest);
	}

	public Rail railTest;
	[ButtonAttribute ("Spawn Train")]
	public void SpawnTrainTest ()
	{
		SpawnTrain (railTest);
	}
}
