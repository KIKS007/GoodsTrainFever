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
	public Color wagonNormalWeightColor;
	public Color wagonOverweightColor;

	[Header ("Spawn Spots Prefabs")]
	public GameObject spot40SpawnedPrefab;

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
		fpsText.text = ((int)1.0f / Time.smoothDeltaTime).ToString ("##.00");
	}
}
