using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using System.Net;

// By using this service, you accept that we gather data about your hardware and your play's habits, in order to improve this said service.


/*
public class Fenalytics_old : MonoBehaviour
{
	private const string PROTOCOL = "http";
	private const string HOSTNAME = "localhost";
	private const int PORT = 3001;
	private static Guid _uid;
	private static float _refreshRate;

	private List<string> _session = new List<string> ();

	private static string _device_id = null;
	private static string _account_id = null;



	// Wrapper function from here

	private static Fenalytics _instance;

	public static void To (string sessionName)
	{
		_instance.StartCoroutine (GetSessionId (sessionName));
	}

	public static void Id (string id)
	{
		Debug.Log (id);
	}

	private static IEnumerator GetSessionId (string sessionName)
	{
		yield return new WaitUntil (() => _device_id != null && _account_id != null);
		Debug.Log (sessionName);
	}

	//	SystemInfo.deviceType.ToString (), Application.systemLanguage
	private static void GetDeviceId ()
	{
		FenalyticsDevice device = new FenalyticsDevice (SystemInfo.deviceUniqueIdentifier, SystemInfo.deviceName, SystemInfo.deviceModel, SystemInfo.deviceType.ToString (), SystemInfo.systemMemorySize, 
			                          new FenalyticsGraphicsDevice (SystemInfo.graphicsDeviceName, SystemInfo.graphicsDeviceType.ToString (), SystemInfo.graphicsDeviceVendor, SystemInfo.graphicsMemorySize),
			                          new FenalyticsCentalDevice (SystemInfo.processorType, SystemInfo.processorFrequency, SystemInfo.processorCount),
			                          new FenalyticsDisplay (new Vector2 (Display.main.renderingWidth, Display.main.renderingHeight), new Vector2 (Display.main.systemWidth, Display.main.systemHeight), 
				                          Screen.fullScreen, Screen.currentResolution.refreshRate, Screen.orientation.ToString ()),
			                          Display.displays.Length, Application.platform.ToString (), SystemInfo.operatingSystem, SystemInfo.operatingSystemFamily.ToString (), Application.systemLanguage.ToString ());
//
//		FenalyticsRequest.Requests.Enqueue (new FenalyticsLog ("/v1/devices", JsonUtility.ToJson (device), (key) => {
//			_device_id = key;
//			Debug.Log (_device_id);
//		}));
	}


	public static void Init (string projectId, float refreshRate, Fenalytics instance)
	{
		_instance = instance;

		GetDeviceId ();
//		Fenalytics.Device.GetKey ();

//		FenalyticsRequest.Start (PROTOCOL, HOSTNAME, PORT);
	}

	private static IEnumerator Loop ()
	{
		while (true) {
			
			yield return new WaitForSeconds (1f);
		}
	}

	public static string Request (string endpoint, string json)
	{
		WebRequest req = WebRequest.Create (PROTOCOL + "://" + HOSTNAME + ":" + PORT + "/" + endpoint);
		req.Method = "POST";
		req.ContentType = "application/json";
		req.Headers.Add ("Content-Encoding: gzip");

		Stream reqStream = req.GetRequestStream ();

		GZipStream gz = new GZipStream (reqStream, CompressionMode.Compress);

		StreamWriter sw = new StreamWriter (gz, Encoding.ASCII);
		sw.Write (json);

		sw.Close ();
		gz.Close ();
		reqStream.Close ();


		WebResponse resp = req.GetResponse ();
		return(new StreamReader (resp.GetResponseStream ()).ReadToEnd ());
	}





}


*/