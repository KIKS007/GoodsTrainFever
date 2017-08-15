using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptiScript : MonoBehaviour
{
	[Tooltip ("Framerate Targeted")]
	public float frameRateAvarageTarget = 30;

	[Tooltip ("Number of Framerate Sample taken before ajusting Resolution")]
	public float frameRateSamples = 9;

	[Tooltip ("Time in sec between each Framerate sample")]
	public float frameRateSamplesTime = 0.2f;



	private float aspectRatio;
	private List<float> framerateSamples = new List<float> ();
	private int deviceHeight;
	private float f = 1;
	private int framerateDeviceHeight;
	// Use this for initialization


	void Start ()
	{
		Debug.Log ("Start");
		aspectRatio = Screen.currentResolution.width / (Screen.currentResolution.height * 1f);
		if (PlayerPrefs.GetInt ("FramerateDeviceHeight", 0) != 0) {
			int tmpDeciveHeight = PlayerPrefs.GetInt ("FramerateDeviceHeight", Screen.currentResolution.height);
			Screen.SetResolution ((int)(tmpDeciveHeight * aspectRatio), tmpDeciveHeight, true);
		} else {
			deviceHeight = Screen.currentResolution.height;
			SaveDeviceRes ();
			Invoke ("ActivateFramerateAnalyser", 2);
		}
	}

	private void Resize ()
	{
		Debug.Log ("Resizing");
		if (f > 0.5f) {
			f -= .1f;
			Debug.Log ("f= " + f);
			framerateDeviceHeight = (int)(deviceHeight * f);
			Screen.SetResolution ((int)(framerateDeviceHeight * aspectRatio), framerateDeviceHeight, true);
			framerateSamples.Clear ();
			Invoke ("ActivateFramerateAnalyser", 1);
		} else {
			SaveCurrentRes ();
		}

	}

	private void ActivateFramerateAnalyser ()
	{
		Debug.Log ("Starting Analyser");
		StopCoroutine ("AnalyseFramerate");
		StartCoroutine (AnalyseFramerate ());
	}

	private void SaveCurrentRes ()
	{
		Debug.Log ("Saving Res");
		StopCoroutine ("AnalyseFramerate");
		framerateSamples.Clear ();
		PlayerPrefs.SetInt ("FramerateDeviceHeight", framerateDeviceHeight);
	}

	private void SaveDeviceRes ()
	{
		Debug.Log ("Saving Default Res");
		PlayerPrefs.SetInt ("DefaultDeviceHeight", deviceHeight);
	}

	public void ResetAndAnalyse ()
	{
		Debug.Log ("Reset Analyse");
		f = 1;
		PlayerPrefs.DeleteKey ("FramerateDeviceHeight");
		Screen.SetResolution ((int)(PlayerPrefs.GetInt ("DefaultDeviceHeight", Screen.height) * aspectRatio), PlayerPrefs.GetInt ("DefaultDeviceHeight", Screen.height), true);
		Invoke ("ActivateFramerateAnalyser", 2f);
	}


	IEnumerator AnalyseFramerate ()
	{
		if (framerateSamples.Count < frameRateSamples) {
			Debug.Log ("Adding Framerate");
			framerateSamples.Add ((Mathf.RoundToInt (1.0f / Time.smoothDeltaTime)));
			yield return new WaitForSeconds (frameRateSamplesTime);
			ActivateFramerateAnalyser ();
		} else {
			Debug.Log ("Calculating...");
			float tmpfloat = 0f;
			foreach (float value in framerateSamples) {
				tmpfloat += value;
			}
			if ((tmpfloat / frameRateSamples) < frameRateAvarageTarget) {
				Resize ();
			} else {
				SaveCurrentRes ();
			}
		}
	}
}
