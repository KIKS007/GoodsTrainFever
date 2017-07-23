using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;

public class GlobalVariables : Singleton<GlobalVariables>
{
	[Header ("FPS")]
	public Text fpsText;

	[Header ("Gameplay Parent")]
	public Transform gameplayParent;

	[Header ("Container Color")]
	public Color redColor;
	public Color blueColor;
	public Color yellowColor;
	public Color violetColor;

	[Header ("Weight")]
	public List<ContainerTypeWeight> containersWeight = new List<ContainerTypeWeight> ();
	public List<WagonTypeWeight> wagonsMaxWeight = new List<WagonTypeWeight> ();
	public Color wagonNormalWeightColor;
	public Color wagonOverweightColor;

	[Header ("Spots Prefabs")]
	public GameObject spot20Prefab;
	public GameObject spot40Prefab;
	public GameObject spot40SpawnedPrefab;

	[PropertyOrder (-1)]
	[ButtonAttribute ("Update Is Double Size")]
	public void SetIsDoubleSize ()
	{
		foreach (var c in FindObjectsOfType<Container> ())
			c.SetIsDoubleSize ();

		foreach (var s in FindObjectsOfType<Spot> ())
			s.SetIsDoubleSize ();
	}

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		FPS ();
	}

	void FPS ()
	{
		fpsText.text = ((int)1.0f / Time.smoothDeltaTime).ToString ("##.000");
	}

	[System.Serializable]
	public class ContainerTypeWeight
	{
		public ContainerType containerType;
		public Vector2 weightBounds;
	}

	[System.Serializable]
	public class WagonTypeWeight
	{
		public WagonType wagonType;
		public Vector2 weightBounds;
	}
}
