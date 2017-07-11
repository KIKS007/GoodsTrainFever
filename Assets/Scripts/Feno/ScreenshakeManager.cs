/* 
 * Copyright (C) IronEqual SAS
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * File is not exclusive
 * Written by Feno <feno@ironequal.com>, 2017
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScreenshakeManager : MonoBehaviour
{

	public static ScreenshakeManager Singleton;

	void Awake ()
	{
		if (ScreenshakeManager.Singleton == null) {
			ScreenshakeManager.Singleton = this;
		} else {
			Destroy (gameObject);
		}
	}
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void Shake (Vector3 force, int vibrato = 5, float delay = 0, float duration = .2f)
	{
		transform.DOKill (true);
		transform.DOPunchPosition (force, duration, vibrato).SetDelay (delay);
	}
}
