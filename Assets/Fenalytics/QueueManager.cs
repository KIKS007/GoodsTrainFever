using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using System.Net;
using System.Threading;
using UnityEngine.iOS;
using FenalyticsScripts.Serialization;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Reflection;
using System.Net.Cache;


namespace FenalyticsScripts
{
	class QueueManager
	{
		public static List<Request> Requests = new List<Request> ();

		private static Thread t;

		private static string _protocol, _hostname;
		private static int _port;

		private static int _refreshRate;

		private static string RootSession = "";
		private static bool SessionInited = false;
		public static string AccountId = null;
		public static string DeviceId = null;
		public static string BuildId = null;
		public static string SessionId = null;

		public static void Start (string protocol, string hostname, int port, int refreshRate)
		{
			// READING OLD DATA HERE

			_protocol = protocol;
			_hostname = hostname;
			_port = port;
			_refreshRate = refreshRate;
			t = new Thread (new ThreadStart (Loop));
			t.Start ();
		}

		public static void Stop ()
		{
			QueueManager.Requests.Add (new Request ("/v1/session/" + RootSession, JsonUtility.ToJson (new FenalyticsEndAt (DateTimeOffset.Now.ToString ("o")))));
			abort = true;

			// SAVING DATA HERE
		}

		private static bool abort = false;

		public static void Loop ()
		{

			while (Thread.CurrentThread.IsAlive) {
				Thread.Sleep (_refreshRate);
				if (abort) {
					_refreshRate = 1;
					if (Requests.Count == 0) {
						t.Abort ();
					}
				}

				if (Requests.Count > 0) {
					for (int i = 0; i <= Requests.Count - 1; i++) {
						Request r = Requests [i];
						string model = r.endpoint.Split ('/') [r.endpoint.Split ('/').Length - 1];

						if (model == "sessions") {
							if (AccountId == null || DeviceId == null || BuildId == null) {
								continue;
							} else {
								FenalyticsSession s = JsonUtility.FromJson<FenalyticsSession> (r.json [0]);

								if (s.name != "session" && !SessionInited) {
									continue;
								}

								if (s.name == "session") {
									s.device_id = DeviceId;
									s.account_id = AccountId;
									s.build_id = BuildId;
								} else {
									s.prev_id = SessionId;
								}

								r.json [0] = Newtonsoft.Json.JsonConvert.SerializeObject (s);
							}

							if (i != 0)
								continue;
						}

						if (model == "events") {
							if (SessionId == null) {
								continue;
							}

							for (int j = 0; j <= r.json.Count - 1; j++) {
								FenalyticsEvent e = Newtonsoft.Json.JsonConvert.DeserializeObject<FenalyticsEvent> (r.json [j]);
								e.session_id = SessionId;

								r.json [j] = Newtonsoft.Json.JsonConvert.SerializeObject (e);
							}

							if (i != 0)
								continue;
						} 

						// check

						string response = "";
						WebExceptionStatus status = WebExceptionStatus.Success;

						try {
							response = Request (r, "POST");
						} catch (WebException e) {
							status = (e.Status);	
							Debug.Log ("WebRequest ERROR:" + e);
						}

						if (status == WebExceptionStatus.Success) {
							Requests.RemoveAt (i);
						}

						switch (model) {
						case "devices":
							DeviceId = response;
							break;
						case "builds":
							BuildId = response;
							break;
						case "accounts":
							AccountId = response;
							break;
						case "sessions":
							SessionId = response;
							if (!SessionInited) {
								SessionInited = true;
								RootSession = SessionId;
							}
							break;
						}


					}
				}
			}

		}

		public static string Request (Request r, string method)
		{
			WebRequest req = WebRequest.Create (_protocol + "://" + _hostname + ":" + _port + "/" + r.endpoint);

			req.Method = method;
			req.ContentType = "application/json";


			Stream reqStream = req.GetRequestStream ();


			#if UNITY_EDITOR_WIN
			req.Headers.Add ("Content-Encoding: gzip");
			GZipStream gz = new GZipStream (reqStream, CompressionMode.Compress);
			StreamWriter sw = new StreamWriter (gz, Encoding.ASCII);
			sw.Write (Newtonsoft.Json.JsonConvert.SerializeObject (r.json));
			sw.Close ();
			gz.Close ();
			#else
			Byte[] data = Encoding.ASCII.GetBytes (Newtonsoft.Json.JsonConvert.SerializeObject (r.json));

			reqStream.Write (data, 0, data.Length);
			#endif

			reqStream.Close ();

			WebResponse resp = req.GetResponse ();

			string response = new StreamReader (resp.GetResponseStream ()).ReadToEnd ();
			return response;

		}
	}

}