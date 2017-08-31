using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using DG.Tweening;


public class WaterDotween : MonoBehaviour 
{
	public float waterHeight;
	public float waterSpeed;
	public float waterLineDelay;
	public float waterRowDelay;
	public Ease waterEase;
	public List<ListAsset> waterRows = new List<ListAsset> ();

	private float _yPos;
	private float _yPosInitial;

	// Use this for initialization
	void Start () 
	{
		GetWaterBlocks ();
		SetWaterTween ();
	}

	[ButtonAttribute]
	void GetWaterBlocks ()
	{
		waterRows = new List<ListAsset> ();

		var blocks = transform.GetComponentsInChildren<Transform> ().ToList ();
		blocks.Remove (transform);
		blocks = blocks.OrderByDescending (x => x.transform.position.x).ToList (); 

		float xPos = 0;
		xPos = blocks [0].transform.position.x;

		int rowsCount = 0;
		waterRows.Add (new ListAsset ());

		foreach(var w in blocks)
		{
			if (w.transform.position.x == xPos)
				waterRows [rowsCount].waterBlocks.Add (w);
			else
			{
				xPos = w.transform.position.x;
				rowsCount++;
				waterRows.Add (new ListAsset ());
				waterRows [rowsCount].waterBlocks.Add (w);
			}

			w.name = "Water Row " + rowsCount + " Block " + waterRows [rowsCount].waterBlocks.Count;
		}

		foreach (var row in waterRows)
			row.waterBlocks = row.waterBlocks.OrderByDescending (x => x.transform.position.z).ToList ();

		int siblingIndex = 0;
		for(int i = 0; i < waterRows.Count; i++)
		{
			for(int j = 0; j < waterRows[i].waterBlocks.Count; j++)
			{
				waterRows[i].waterBlocks [j].name = "Water Row " + i + " Block " + j; 
				waterRows [i].waterBlocks [j].SetSiblingIndex (siblingIndex);
				siblingIndex++;
			}
		}
	}

	[ButtonAttribute]
	void SetWaterTween ()
	{
		if (_yPosInitial == 0)
			_yPosInitial = waterRows [0].waterBlocks [0].transform.position.y;
		
		_yPos = _yPosInitial - waterHeight;

		float delay = 0;

		for(int i = 0; i < waterRows.Count; i++)
		{
			delay = waterRowDelay * i;

			for(int j = 0; j < waterRows[i].waterBlocks.Count; j++)
			{
				DOTween.Kill (waterRows [i].waterBlocks [j]);
				waterRows [i].waterBlocks [j].transform.position = new Vector3 (waterRows [i].waterBlocks [j].transform.position.x, _yPos, waterRows [i].waterBlocks [j].transform.position.z);

				waterRows [i].waterBlocks [j].DOLocalMoveY (_yPosInitial, waterSpeed).SetEase (waterEase).SetLoops (-1, LoopType.Yoyo).SetDelay (delay + waterLineDelay * j).SetSpeedBased ();
			}
		}
	}

	[System.Serializable]
	public class ListAsset
	{
		public List<Transform> waterBlocks = new List<Transform> ();
	}
}
