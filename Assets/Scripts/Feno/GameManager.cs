/* 
 * Copyright (C) IronEqual SAS
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * File is not exclusive
 * Written by Feno <feno@ironequal.com>, 2017
 */

using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ContainerType
{
	public string Name;
	public int Weight;
	public bool isSuperposable;
	public bool isSensible;
	public bool isDangerous;
	public Material material;
}

public class GameManager : MonoBehaviour
{
	public static GameManager Singleton;

	public GameObject Container;
	public ContainerType[] Containers;

	void Awake ()
	{
		if (GameManager.Singleton == null) {
			GameManager.Singleton = this;
		} else {
			Debug.LogWarning (this.name + " already in the scene. Destroyed.");
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
}
