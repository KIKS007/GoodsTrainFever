using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using FenalyticsScripts.Serialization;
using UnityEngineInternal;


namespace FenalyticsScripts
{
	public class Core : MonoBehaviour
	{
		private const string PROTOCOL = "http";
		private const string HOSTNAME = "89.234.148.158";
		private const int PORT = 3001;

		public static Core _instance;
		private static string _projectId;

		public static class Device
		{
			public static void GetId ()
			{
				FenalyticsDevice device = new FenalyticsDevice (SystemInfo.deviceUniqueIdentifier, SystemInfo.deviceName, SystemInfo.deviceModel, SystemInfo.deviceType.ToString (), SystemInfo.systemMemorySize, 
					                          new FenalyticsGraphicsDevice (SystemInfo.graphicsDeviceName, SystemInfo.graphicsDeviceType.ToString (), SystemInfo.graphicsDeviceVendor, SystemInfo.graphicsMemorySize),
					                          new FenalyticsCentalDevice (SystemInfo.processorType, SystemInfo.processorFrequency, SystemInfo.processorCount),
					                          new FenalyticsDisplay (new Vector2 (Display.main.renderingWidth, Display.main.renderingHeight), new Vector2 (Screen.currentResolution.width, Screen.currentResolution.height), 
						                          Screen.fullScreen, Screen.currentResolution.refreshRate, Screen.orientation.ToString ()),
					                          Display.displays.Length, Application.platform.ToString (), SystemInfo.operatingSystem, SystemInfo.operatingSystemFamily.ToString (), Application.systemLanguage.ToString ());

				QueueManager.Requests.Add (new Request ("/v1/devices", JsonUtility.ToJson (device)));

			}
		}

		public static class Build
		{
			public static void GetId ()
			{
				FenalyticsBuild build = new FenalyticsBuild (_projectId, Application.version, Application.buildGUID, Application.unityVersion);

				QueueManager.Requests.Add (new Request ("/v1/builds", JsonUtility.ToJson (build)));

			}
		}

		public static class Location
		{
			public static void GetId ()
			{
				QueueManager.Requests.Add (new Request ("/v1/locations", "GET"));

			}
		}


		public static class Account
		{
			public static void GetId (string name, string id)
			{
				QueueManager.Requests.Add (new Request ("/v1/accounts", "{\"" + name + "\":\"" + id + "\"}"));
			}

			public static void AddField ()
			{
				throw new NotImplementedException ("The requested feature is not implemented.");
			}
		}

		public static class Event
		{
			public static IEnumerator Send (string name, Dictionary<string, object> data)
			{
				string now = DateTimeOffset.Now.ToString ("o");
				FenalyticsEvent e = new FenalyticsEvent (name, data, now);
				yield return null;
				if (QueueManager.Requests.Count > 0) {
					Request r = QueueManager.Requests [QueueManager.Requests.Count - 1];
					if (r.endpoint == "/v1/events") {
						r.json.Add (Newtonsoft.Json.JsonConvert.SerializeObject (e));
					} else {
						QueueManager.Requests.Add (new Request ("/v1/events", Newtonsoft.Json.JsonConvert.SerializeObject (e)));
					}
				} else {
					QueueManager.Requests.Add (new Request ("/v1/events", Newtonsoft.Json.JsonConvert.SerializeObject (e)));
				}
			}
		}

		public static class Session
		{
			public static void Init ()
			{
				_instance.StartCoroutine (GetId ());
			}

			public static IEnumerator GetId (string name = null)
			{ 
				string now = DateTimeOffset.Now.ToString ("o");
				yield return null;
				string _name = (name != null) ? "session." + name : "session";

				FenalyticsSession session = new FenalyticsSession (_name, now, "", "");

				QueueManager.Requests.Add (new Request ("/v1/sessions", JsonUtility.ToJson (session)));
			}
		}

		public static void Init (string projectId, int refreshRate, Core instance)
		{
			QueueManager.Start (PROTOCOL, HOSTNAME, PORT, refreshRate);
			_projectId = projectId;
			_instance = instance;
			Device.GetId ();
			Build.GetId ();
			Location.GetId ();
			Session.Init ();
		}
	}
}