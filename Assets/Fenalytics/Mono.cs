using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;



namespace FenalyticsScripts
{
	public class Mono : MonoBehaviour
	{
		public string projectId;
		public int refreshRate;
		public bool enable;
		public bool noId;

		[ShowOnly] [SerializeField] private int count;
		[ShowOnly] [SerializeField] private string nextEndpoint;
		[ShowOnly] [SerializeField] private string nextData;

		void Awake ()
		{
			if (enable)
				Core.Init (projectId, refreshRate, gameObject.AddComponent<Core> ());

			if (noId) {
				StartCoroutine (SetAnonAccount ());
			}
		}

		#if UNITY_EDITOR
		void Update ()
		{
			count = QueueManager.Requests.Count;
			if (count > 0) {
				nextEndpoint = QueueManager.Requests [0].endpoint;
				nextData = Newtonsoft.Json.JsonConvert.SerializeObject (QueueManager.Requests [0].json);
			}
		}
		#endif

		IEnumerator SetAnonAccount ()
		{
			yield return new WaitUntil (() => QueueManager.DeviceId != null);
			Core.Account.GetId ("device_id", QueueManager.DeviceId);
		}

		void OnApplicationQuit ()
		{
			QueueManager.Stop ();
			//		FenalyticsRequest.Stop ();
		}
	}

}
