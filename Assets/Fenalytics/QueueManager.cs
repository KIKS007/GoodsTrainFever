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


namespace FenalyticsScripts
{
	class QueueManager
	{
		public static List<Request> Requests = new List<Request> ();

		private static Thread t;

		private static string _protocol, _hostname;
		private static int _port;

		private static int _refreshRate;

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
			t.Abort ();

			// SAVING DATA HERE
		}

		private static bool stop = false;

		public static void Loop ()
		{
			try {
				while (Thread.CurrentThread.IsAlive) {
					Thread.Sleep (_refreshRate);
					if (Requests.Count > 0) {
						stop = false;
						for (int i = 0; i <= Requests.Count - 1; i++) {
							Request r = Requests [i];
							string model = r.endpoint.Split ('/') [r.endpoint.Split ('/').Length - 1];

							if (model == "sessions") {
								stop = true;
								if (AccountId == null || DeviceId == null || BuildId == null) {
									continue;
								} else {
									FenalyticsSession s = JsonUtility.FromJson<FenalyticsSession> (r.json [0]);

									if (s.name != "session" && !SessionInited) {
										continue;
									}

									if (s.name == "session") {
										SessionInited = true;
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

							Requests.RemoveAt (i);

							WebRequest req = WebRequest.Create (_protocol + "://" + _hostname + ":" + _port + "/" + r.endpoint);
							req.Method = "POST";
							req.ContentType = "application/json";
							//req.Headers.Add ("Content-Encoding: gzip");


							Stream reqStream = req.GetRequestStream ();


							/*
							GZipStream gz = new GZipStream (reqStream, CompressionMode.Compress);

							StreamWriter sw = new StreamWriter (gz, Encoding.ASCII);
						
							if (r.json.Count == 1 && model != "events")
								sw.Write (r.json [0]);
							else
								sw.Write (Newtonsoft.Json.JsonConvert.SerializeObject (r.json));
							


							sw.Close ();
							gz.Close ();
							*/
							Byte[] data;
						
							if (r.json.Count == 1 && model != "events")
								data = Encoding.ASCII.GetBytes (r.json [0]);
							else
								data = Encoding.ASCII.GetBytes (Newtonsoft.Json.JsonConvert.SerializeObject (r.json));

							reqStream.Write (data, 0, data.Length);

							reqStream.Close ();

							WebResponse resp = req.GetResponse ();
							string response = new StreamReader (resp.GetResponseStream ()).ReadToEnd ();


							/*WWW www = new WWW (_protocol + "://" + _hostname + ":" + _port + "/" + r.endpoint);
							yield return www;*/


//							Debug.Log (model + " " + response);

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
								break;
							}
						}
					}
				}
			} catch (Exception e) {
				Debug.Log (e);
			}
		}
	}
}