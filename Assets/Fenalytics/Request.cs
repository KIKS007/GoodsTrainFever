using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FenalyticsScripts
{
	[Serializable]
	public class Request
	{
		public string endpoint;
		public List<string> json = new List<string> ();
		public Action<string> action;

		public Request (string _endpoint, string _json)
		{
			endpoint = _endpoint;

			json.Add (_json);
		}
	}
}