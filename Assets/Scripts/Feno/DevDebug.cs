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
using UnityEngine.UI;
namespace Feno
{

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class DevDebug : MonoBehaviour
{
	public bool isHidden = false;
	private Image FrameAlert;
	private Text DebugText;

	public string[] tags = new string[2]{ "Spot", "Container" };

	[SerializeField]
	private string DebugString, LogString;

	private List<int> FPS = new List<int> ();
	private float framerateThreshold = 19f;
	// Use this for initialization
	void Awake ()
	{
		foreach (string tag in tags) {
			Debug.Log ("<color=green>" + GameObject.FindGameObjectsWithTag (tag).Length + "</color> " + tag + "(s)");
		}

		FrameAlert = GetComponentInChildren<Image> ();
		DebugText = GetComponentInChildren<Text> ();

		for (int i = 0; i < 60; i++) {
			FPS.Add (30);
		}

		StartCoroutine (Refresh ());
		DebugText.supportRichText = true;

		Application.logMessageReceived += HandleLog;
	}

	// Update is called once per frame
	void Update ()
	{
		FPS.Add (Mathf.FloorToInt (1f / Time.deltaTime));
		FPS.RemoveAt (0);

		if (Input.GetKeyDown (KeyCode.Space)) {
			Time.timeScale = .3f;
		}

		if (Input.GetKeyUp (KeyCode.Space)) {
			Time.timeScale = 1f;
		}

		FrameAlert.enabled = (Time.deltaTime > 1 / framerateThreshold);

		if (isHidden) {
			if (Input.touches.Length <= 1)
				DebugText.enabled = false;
			else
				DebugText.enabled = true;

			if (!Input.GetKey (KeyCode.KeypadEnter))
				DebugText.enabled = false;
			else
				DebugText.enabled = true;
		}

		Color DebugColor = new Color (0, 0, 0, .5f);
		if (FrameAlert.enabled) {
			DebugColor = Color.red;
		}
		DebugText.color = DebugColor;

		DebugText.text = DebugString.ToUpper () + "\n\n" + LogString;
	}

	float GetAvg (List<int> numbers)
	{
		float r = 0;
		foreach (int i in numbers) {
			r += i;
		}
		return r / numbers.Count;
	}

	IEnumerator Refresh ()
	{
		while (true) {
			
			DebugString = "<B>" + Application.productName + " x " + Application.companyName + "</B> // " + Application.unityVersion + " // " + Application.version;
			DebugString += "<size=8>\n" + SystemInfo.deviceModel + "\n" + SystemInfo.operatingSystem + "\n" + SystemInfo.graphicsDeviceName + "\n" + SystemInfo.processorType + "\n</size>";

			DebugString += "\n" + Mathf.Floor (GetAvg (FPS)) + " fps // ";
			DebugString += "" + Object.FindObjectsOfType<Transform> ().Length + " object(s)";
			DebugString += " // " + (SystemInfo.batteryLevel * 100) + "% " + SystemInfo.batteryStatus + "\n";
		


			for (int i = 0; i < tags.Length; i++) {
				string tag = tags [i];
				if (i > 0)
					DebugString += " // ";
				DebugString += (GameObject.FindGameObjectsWithTag (tag).Length + " " + tag + "(s)");
			} 

			yield return new WaitForSeconds (1f);
		}
	}


	void HandleLog (string logString, string stackTrace, LogType type)
	{
		int logStringLength = LogString.Length;
		if (type == LogType.Exception || type == LogType.Error) {
			LogString = "<b><color=red>" + logString.ToString () + "</color><color=#ff0000AA>\n" +
			stackTrace.Split ('\n') [0] + "</color></b>\n" + LogString;
		} else if (type == LogType.Log) {
			LogString = logString + "\n" + LogString;
		}

		StartCoroutine (CutLog (LogString.Length - logStringLength));

		if (logString == "clear") {
			LogString = "";
		}
	}

	IEnumerator CutLog (int size)
	{
		yield return new WaitForSeconds (3f);
		LogString = LogString.Substring (0, LogString.Length - size);
	}

}
#endif

}