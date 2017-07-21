using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
