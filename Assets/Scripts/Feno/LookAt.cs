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

public class LookAt : MonoBehaviour
{
	public bool mainCamera;

	private Camera cam;
	// Use this for initialization
	void Start ()
	{
		cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (mainCamera) {
			transform.LookAt (cam.transform);
		}
	}
}
