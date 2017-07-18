using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class OrdersManager : Singleton<OrdersManager> 
{
	[Header ("Fast Forward")]
	public float fastForwardDuration;
	public Ease fastForwardEase = Ease.OutQuad;
	public float xInitialPosition;
	public float wagonLength = 10f;
	public float locomotiveLength = 10f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void FastForwardTrain (Train train)
	{
		train.departed = true;

		float xPosition = xInitialPosition + train.wagons.Count * wagonLength + locomotiveLength;

		train.transform.DOMoveX (xPosition, fastForwardDuration).SetEase (fastForwardEase);
	}

	public Train trainTest;
	[ButtonAttribute ("FastForwardTrain")]
	public void FastForwardTrainTest ()
	{
		FastForwardTrain (trainTest);
	}
}
