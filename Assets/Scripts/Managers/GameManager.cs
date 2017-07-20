﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
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

	// Use this for initialization
	void Start () {
		
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
}
