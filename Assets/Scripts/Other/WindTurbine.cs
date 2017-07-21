using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class WindTurbine : MonoBehaviour 
{
	[Header ("Rotation")]
	public Vector3 rotationDirection;
	public Vector2 rotationSpeed;

	// Use this for initialization
	void Start () 
	{
		transform.DOLocalRotate (rotationDirection, Random.Range (rotationSpeed.x, rotationSpeed.y), RotateMode.LocalAxisAdd).SetLoops (-1, LoopType.Incremental).SetSpeedBased ();
	}
}
