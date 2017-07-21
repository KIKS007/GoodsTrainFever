using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunLight : MonoBehaviour 
{
	// Use this for initialization
	void Awake () 
	{
		if (!Application.isEditor)
			GetComponent<Light> ().shadowResolution = UnityEngine.Rendering.LightShadowResolution.FromQualitySettings;
	}
}
