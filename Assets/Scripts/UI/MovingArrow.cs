using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovingArrow : MonoBehaviour
{

	// Use this for initialization
	void OnEnable ()
	{
		this.transform.DOKill ();
		this.transform.localScale = Vector3.one;
		this.transform.DOScale (1.2f, 0.8f).SetLoops (-1, LoopType.Yoyo).SetUpdate (true);
	}
	
	// Update is called once per frame
	void OnDisable ()
	{
		this.transform.localScale = Vector3.one;
		this.transform.DOKill ();
	}
}
