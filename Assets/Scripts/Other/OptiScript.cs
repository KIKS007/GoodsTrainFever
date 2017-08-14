using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptiScript : MonoBehaviour
{
	public float aspectRatio;
	public float frameCount = 180;
	public float frameRateAvarageTarget = 30;
	private List<float> StartFramerate = new List<float> ();
	private bool Analyse = false;
	private float deviceHeight;
	private float f = 1;
	private int framerateDeviceHeight;
	// Use this for initialization
	private void Resize ()
	{
		if (f > 0.5f) {
			f -= .1f;
			aspectRatio = Screen.currentResolution.width / (Screen.currentResolution.height * 1f);
			framerateDeviceHeight = (int)(deviceHeight * f);
			Screen.SetResolution ((int)(framerateDeviceHeight * aspectRatio), framerateDeviceHeight, true);
			StartFramerate.Clear ();
			Analyse = true;
		} else {
			StartFramerate.Clear ();
			SaveCurrentRes ();
		}

	}

	void Start ()
	{
		if (PlayerPrefs.GetInt ("FramerateDeviceHeight", 0) != 0) {
			int tmpDeciveHeight = PlayerPrefs.GetInt ("FramerateDeviceHeight", Screen.currentResolution.height);
			aspectRatio = Screen.currentResolution.width / (Screen.currentResolution.height * 1f);
			Screen.SetResolution ((int)(tmpDeciveHeight * aspectRatio), tmpDeciveHeight, true);
		} else {
			deviceHeight = Screen.currentResolution.height;
			Invoke ("ActivateFramerateAnalyser", 2);
			
		}
	}

	void ActivateFramerateAnalyser ()
	{
		Analyse = true;
	}

	void Update ()
	{
		if (Analyse) {
			if (StartFramerate.Count < frameCount) {
				StartFramerate.Add ((Mathf.RoundToInt (1.0f / Time.smoothDeltaTime)));
			} else {
				Analyse = false;
				float tmpfloat = 0f;
				foreach (float value in StartFramerate) {
					tmpfloat += value;
				}
				if ((tmpfloat / frameCount) < frameRateAvarageTarget) {
					Resize ();
				} else {
					SaveCurrentRes ();
					StartFramerate.Clear ();
				}

			}
		}
	}

	private void SaveCurrentRes ()
	{
		PlayerPrefs.SetInt ("FramerateDeviceHeight", framerateDeviceHeight);
	}

	public void ResetAndAnalyse ()
	{
		PlayerPrefs.DeleteKey ("FramerateDeviceHeight");
		aspectRatio = Screen.currentResolution.width / (Screen.currentResolution.height * 1f);
		Screen.SetResolution ((int)(Screen.currentResolution.height * aspectRatio), Screen.currentResolution.height, true);
		Analyse = true;
	}
}
